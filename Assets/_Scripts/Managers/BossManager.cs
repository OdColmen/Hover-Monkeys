using UnityEngine;

public class BossManager : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    // Boss is firing
    public delegate void BossIsFiring_EventHandler();
    public event BossIsFiring_EventHandler BossIsFiring;

    // Boss is dead
    public delegate void BossIsDead_EventHandler();
    public event BossIsDead_EventHandler BossIsDead;

    // Boss is gone
    public delegate void BossIsGone_EventHandler();
    public event BossIsGone_EventHandler BossIsGone;

    // ----- COMPONENTS AND SCRIPTS -----

    // Rigid body
    private Rigidbody2D bossRigidbody;

    // Gun and hoverboard rigid body
    private Rigidbody2D gunRigidbody;
    private Rigidbody2D hoverboardRigidbody;

    // Boss animator controller
    private BossAnimatorController animatorController;

    // ----- GAME OBJECTS -----

    // Boss prefab
    [SerializeField] private GameObject bossPrefab = null;

    // Game objects
    private GameObject boss;
    private GameObject gun;
    private GameObject hoverboard;

    // Firing area child
    private GameObject firingArea1;
    private GameObject firingArea2;
    private GameObject inputDenier;

    // ----- HERO STATES -----

    private enum BossStates { ENTERING, FIRING, VULNERABLE, DEAD, EXITING, AWAY };
    private static BossStates bossState;
    
    // ----- POSITIONS -----

    // Absolute horizontal position, for spawning and exiting.
    private float spawnAndExit_AbsolutePositionX;

    // Spawn position
    private float spawnPositionX;
    private float spawnPositionY;

    // Exit position
    private float exitPositionX;

    // Absolute horizontal position, for the boss base
    private float baseAbsolutePositionX;

    // Base position
    private float basePositionX;

    // Gun local position
    private Vector2 gunLocalPosition = new Vector2(-1.5f, 0.5f);
    // Hoverboard local position
    private Vector2 hoverboardLocalPosition = new Vector2(0, -3);

    // ----- HORIZONTAL SPEED / FORCE -----

    // Obstacle speed
    private float bossMaxSpeed;

    // Flags
    private bool accelerationAllowed;
    private bool decelerationAllowed;

    // Acceleration force, on hero death
    private Vector2 accelerationForce_OnHeroDeath;
    // Deceleration force, on boss death
    private Vector2 decelerationForce_OnBossDeath;

    // ----- VERTICAL MOVEMENT  -----

    // Firing position
    private float firingPositionY;
    // Vertical direction
    private int bossDirectionY;
    // Flag
    private bool isPreparingToFire;

    // ----- DROP VELOCITIES -----

    private Vector2[] dropVelocities;

    // ----- AWAY OBJECTS -----

    private bool bossIsGone;
    private bool gunIsGone;
    private bool hoverboardIsGone;

    // ----- OTHER -----

    private bool heroIsAlive;

    // ----- HERO DIRECTION -----

    private int heroDirectionX;

    #endregion

    // ---------- INITIALIZATIONS ----------------------------------------------------+

    #region INITIALIZE
    public void Initialize(float _spawnAndExit_AbsolutePositionX, float _spawnPositionY, 
        float _baseAbsolutePositionX, float _bossSpeed, Vector2 _deathAccelerationForce)
    {
        // ----- STATE -----

        bossState = BossStates.AWAY;

        // ----- GAME OBJECTS -----

        // Instantiate and deactivate boss
        boss = Instantiate(bossPrefab);
        boss.SetActive(false);

        // Get firing area & input denier
        firingArea1 = boss.transform.GetChild(0).gameObject;
        firingArea2 = boss.transform.GetChild(1).gameObject;
        inputDenier = boss.transform.GetChild(2).gameObject;

        // Get and deactivate gun
        gun = boss.transform.GetChild(3).gameObject;
        gun.SetActive(false);

        // Instantiate and deactivate hoverboard
        hoverboard = boss.transform.GetChild(4).gameObject;
        hoverboard.SetActive(false);

        // ----- COMPONENTS -----

        // Gravity scale
        float gravityScale = 20;

        // Boss rigidbody
        bossRigidbody = boss.GetComponent<Rigidbody2D>();
        bossRigidbody.gravityScale = gravityScale;

        // Gun rigidbody
        gunRigidbody = gun.GetComponent<Rigidbody2D>();
        gunRigidbody.gravityScale = gravityScale;

        // Hoverboard rigidbody
        hoverboardRigidbody = hoverboard.GetComponent<Rigidbody2D>();
        hoverboardRigidbody.gravityScale = gravityScale;

        // Boss collision system
        CollisionSystem bossCollisions = boss.GetComponent<CollisionSystem>();
        bossCollisions.InitializeBoss(this);

        // Boss collision system
        CollisionSystem gunCollisions = gun.GetComponent<CollisionSystem>();
        gunCollisions.InitializeBoss(this);

        // Hoverboard collision system
        CollisionSystem hoverboardCollisions = hoverboard.GetComponent<CollisionSystem>();
        hoverboardCollisions.InitializeBoss(this);

        // Animator controller
        animatorController = boss.GetComponent<BossAnimatorController>();
        animatorController.StartDefaultAnimation();

        // ----- SET POSITIONS -----

        // Set spawn and exit absolute horizontal position
        spawnAndExit_AbsolutePositionX = _spawnAndExit_AbsolutePositionX;

        // Set spawn real vertical position
        spawnPositionY = _spawnPositionY;

        // Set base's absolute horizontal position
        baseAbsolutePositionX = _baseAbsolutePositionX;

        // ----- SET SPEED & FORCE -----

        // Set forces
        accelerationForce_OnHeroDeath = _deathAccelerationForce;
        decelerationForce_OnBossDeath = _deathAccelerationForce;

        // Set speed
        bossMaxSpeed = _bossSpeed;

        // ----- OTHER -----

        // Deny checking for upcoming bosses
        heroIsAlive = false;

        // Set as not preparing to fire
        isPreparingToFire = false;

        // ----- INITIALIZE DROP VELOCITIES -----

        InitializeDropVelocitiesManually();
    }
    #endregion

    #region INITIALIZE DROP VELOCITIES MANUALLY
    public void InitializeDropVelocitiesManually()
    {
        dropVelocities = new Vector2[9];

        float dropVelocityX = 120;
        float dropVelocityY = 170;

        // ----- MAX HEIGHT (1.0 = 170) -----

        dropVelocities[0] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.85f);
        dropVelocities[1] = new Vector2(dropVelocityX * 0.4f, dropVelocityY * 0.85f);

        // ----- MID HEIGHT (0.7 = 119) -----

        //dropVelocities[5] = new Vector2(dropVelocityX * 0.0f, dropVelocityY * 0.7f);
        dropVelocities[2] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.7f);
        dropVelocities[3] = new Vector2(dropVelocityX * 0.4f, dropVelocityY * 0.7f);
        dropVelocities[4] = new Vector2(dropVelocityX * 0.5f, dropVelocityY * 0.7f);

        // ----- MIN HEIGHT (0.5) -----

        dropVelocities[5] = new Vector2(dropVelocityX * 0.15f, dropVelocityY * 0.5f);
        dropVelocities[6] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.5f);
        dropVelocities[7] = new Vector2(dropVelocityX * 0.5f, dropVelocityY * 0.5f);
        dropVelocities[8] = new Vector2(dropVelocityX * 0.6f, dropVelocityY * 0.5f);
    }
    #endregion

    // ---------- UPDATES ----------------------------------------------------+

    #region UPDATE BOSS
    public void UpdateBoss()
    {
        // ----- UPDATE STATES -----

        UpdateStates();

        // ----- STOP MOVING VERTICALLY -----

        if (isPreparingToFire)
        {
            // If hero passed the base
            if ((bossDirectionY > 0) && (boss.transform.position.y >= firingPositionY) ||
                ((bossDirectionY < 0) && (boss.transform.position.y <= firingPositionY)))
            {
                // Stop moving
                bossRigidbody.velocity = Vector2.zero;

                // Fix position
                boss.transform.position = new Vector2(boss.transform.position.x, firingPositionY);

                // Set as not preparing
                isPreparingToFire = false;
            }
        }
    }
    #endregion

    #region UPDATE STATES
    private void UpdateStates()
    {
        if (bossState == BossStates.ENTERING)
        {
            // If hero passed the base
            if ((heroDirectionX > 0) && (boss.transform.position.x < basePositionX) ||
                ((heroDirectionX < 0) && (boss.transform.position.x > basePositionX)))
            {
                // Stop moving
                bossRigidbody.velocity = Vector2.zero;

                // Fix position at base
                boss.transform.position = new Vector2(basePositionX, boss.transform.position.y);

                if (heroIsAlive)
                {
                    // Set state as "firing"
                    bossState = BossStates.FIRING;

                    // Fire event: Boss is firing
                    BossIsFiring?.Invoke();

                    //Debug.Log("boss started firing");
                }
            }
        }
    }
    #endregion

    #region FIXED UPDATE
    private void FixedUpdate()
    {
        // ----- ACCELERATION -----

        if (accelerationAllowed)
        {
            // Apply force
            bossRigidbody.AddForce(accelerationForce_OnHeroDeath);
        }

        if (decelerationAllowed)
        {
            // Apply force
            bossRigidbody.AddForce(decelerationForce_OnBossDeath);

            if (((heroDirectionX > 0) && (bossRigidbody.velocity.x < bossMaxSpeed)) ||
                ((heroDirectionX < 0) && (bossRigidbody.velocity.x > bossMaxSpeed)))
            {
                decelerationAllowed = false;
            }
        }
    }
    #endregion

    // ---------- DIRECTION ----------------------------------------------------+

    #region SET DIRECTION
    public void SetDirection(int _heroDirectionX)
    {
        // If hero is going right, bosses spawn from the right
        if (_heroDirectionX > 0)
        {
            // Set spawn position
            spawnPositionX = spawnAndExit_AbsolutePositionX;
            // Set exit position
            exitPositionX = spawnAndExit_AbsolutePositionX * (-1);
            // Set base position
            basePositionX = baseAbsolutePositionX;
        }

        // If hero is going left, bosses spawn from the left
        else
        {
            // Set spawn position
            spawnPositionX = spawnAndExit_AbsolutePositionX * (-1);
            // Set exit position
            exitPositionX = spawnAndExit_AbsolutePositionX;
            // Set base position
            basePositionX = baseAbsolutePositionX * (-1);
        }

        // Change speed
        bossMaxSpeed = bossMaxSpeed * _heroDirectionX * (-1);

        // Change forces
        accelerationForce_OnHeroDeath = new Vector2(accelerationForce_OnHeroDeath.x * _heroDirectionX, 
            accelerationForce_OnHeroDeath.y);
        decelerationForce_OnBossDeath = new Vector2(decelerationForce_OnBossDeath.x * _heroDirectionX * (-1), 
            decelerationForce_OnBossDeath.y);

        // Store direction
        heroDirectionX = _heroDirectionX;
    }
    #endregion

    #region FLIP DIRECTION
    public void FlipDirection()
    {
        // Change spawn position
        spawnPositionX = spawnPositionX * (-1);

        // Change exit position
        exitPositionX = exitPositionX * (-1);

        // Change base position
        basePositionX = basePositionX * (-1);

        // Change speed
        bossMaxSpeed = bossMaxSpeed * (-1);

        // Change forces
        accelerationForce_OnHeroDeath = new Vector2(accelerationForce_OnHeroDeath.x * (-1), 
            accelerationForce_OnHeroDeath.y);

        decelerationForce_OnBossDeath = new Vector2(decelerationForce_OnBossDeath.x * (-1), 
            decelerationForce_OnBossDeath.y);

        // Store direction
        heroDirectionX = heroDirectionX * (-1);
    }
    #endregion

    // ---------- RESET ----------------------------------------------------+

    #region RESET VALUES FOR NEW GAME
    public void ResetValuesForNewGame()
    {
        // Set game as running
        heroIsAlive = true;

        // Deactivate boss
        DeactivateBoss(true);
    }
    #endregion

    // ---------- SPAWN & DEACTIVATE ----------------------------------------------------+

    #region SPAWN
    public void Spawn()
    {
        // Activate game object, firing area & input denier
        boss.SetActive(true);
        firingArea1.SetActive(true);
        firingArea2.SetActive(true);
        inputDenier.SetActive(true);
        
        // Deactivate gun & hoverboard
        gun.SetActive(false);
        hoverboard.SetActive(false);

        // Restore tag
        boss.tag = "Boss";

        // ----- SET STATE -----

        bossState = BossStates.ENTERING;

        // ----- SET POSITION PARAMETERS -----

        // Set current positions
        boss.transform.position = new Vector2(spawnPositionX, spawnPositionY);
        gun.transform.localPosition = gunLocalPosition;
        hoverboard.transform.localPosition = hoverboardLocalPosition;

        // Point boss (and children) in the right direction
        boss.transform.localScale = new Vector2(heroDirectionX, 1);
        
        // Reset rotations
        boss.transform.rotation = Quaternion.identity;
        gun.transform.rotation = Quaternion.identity;
        hoverboard.transform.rotation = Quaternion.identity;

        // ----- SET MOVEMENT PARAMETERS -----

        // Set velocity
        bossRigidbody.velocity = Vector2.right * bossMaxSpeed / 4;

        // Set body type, denying automatic physics interaction
        bossRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // ???
        bossRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Un-freeze vertical movement
        bossRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Set gun's movement constraints
        gunRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        // Set hoverboard's movement constraints
        hoverboardRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        // ----- SET FLAGS -----

        // Set flags
        accelerationAllowed = false;
        decelerationAllowed = false;
        isPreparingToFire = false;

        // Gone flags
        bossIsGone = false;
        gunIsGone = false;
        hoverboardIsGone = false;

        // ----- SET DEFAULT ANIMATION -----

        animatorController.StartDefaultAnimation();
    }
    #endregion

    #region DEACTIVATE BOSS
    private void DeactivateBoss(bool _newGame)
    {
        // Deactivate game object
        boss.SetActive(false);
        gun.SetActive(false);
        hoverboard.SetActive(false);

        if (!_newGame)
        {
            // Fire event: Boss is gone
            BossIsGone?.Invoke();
        }

        // Change state
        bossState = BossStates.AWAY;
    }
    #endregion

    #region DEACTIVATE FIRING AREA
    public void DeactivateFiringArea()
    {
        // Deactivate
        firingArea1.SetActive(false);
        firingArea2.SetActive(false);
    }
    #endregion

    #region DEACTIVATE INPUT DENIER
    public void DeactivateInputDenier()
    {
        // Deactivate
        inputDenier.SetActive(false);
    }
    #endregion

    // ---------- ??? ----------------------------------------------------+

    #region START FIRING ANIMATION
    public void StartFiringAnimation()
    {
        // Change animation
        animatorController.StartAttackingAnimation();

        if (bossState == BossStates.VULNERABLE)
        {
            // Change isJammingGunParameter for when the attacking animation ends
            animatorController.PrepareJammingGunAnimation();
        }
    }
    #endregion

    #region SET STATE AS BOSS VULNERABLE
    public void SetStateAs_BossVulnerable()
    {
        // Set state
        bossState = BossStates.VULNERABLE;
    }
    #endregion

    #region SET HERO AS DEAD
    public void SetHeroAsDead()
    {
        // Set hero as not alive
        heroIsAlive = false;
    }
    #endregion

    // ---------- COLLISIONS ----------------------------------------------------+

    #region REACT TO RETURNED SHOT COLLISION
    public void ReactTo_ReturnedShotCollision()
    {
        // Change animation
        animatorController.StartDeadAnimation();

        // Physically react to hit
        PhysicallyReactToHit();

        // Fire event: Boss is dead
        BossIsDead?.Invoke();

        if (bossState == BossStates.VULNERABLE)
        {
            // Set state as "dead"
            bossState = BossStates.DEAD;
        }

        else
        {
            Debug.Log("Error: Previous state should be VULNERABLE");
        }
    }
    #endregion

    #region PHYSICALLY REACT TO HIT
    private void PhysicallyReactToHit()
    {
        // ----- BOSS DROP -----

        // Change body type
        bossRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // ???
        bossRigidbody.interpolation = RigidbodyInterpolation2D.None;

        // Allow all movement
        bossRigidbody.constraints = RigidbodyConstraints2D.None;

        // Set velocity and torque
        SetDropVelocityAndTorque(bossRigidbody);

        // ----- GUN DROP -----

        // Activate object
        gun.SetActive(true);
        // Allow movement
        gunRigidbody.constraints = RigidbodyConstraints2D.None;

        // Set velocity and torque
        SetDropVelocityAndTorque(gunRigidbody);

        // ----- HOVERBOARD DROP -----

        // Activate object
        hoverboard.SetActive(true);
        // Allow movement
        hoverboardRigidbody.constraints = RigidbodyConstraints2D.None;

        // Set velocity and torque
        SetDropVelocityAndTorque(hoverboardRigidbody);
    }
    #endregion

    #region SET DROP VELOCITY AND TORQUE
    private void SetDropVelocityAndTorque(Rigidbody2D _rigidbody)
    {
        // ----- VELOCITY -----

        // Choose random drop velocitiy
        int index = Random.Range(0, dropVelocities.Length);
        int velocityDirection = (Random.Range(0, 2) == 0) ? 1 : -1;

        // Set velocity
        _rigidbody.velocity = new Vector2(dropVelocities[index].x * velocityDirection, dropVelocities[index].y);

        // ----- TORQUE -----

        int torqueDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        int torqueAngle = 45;
        int torqueMultiplier = Random.Range(5, 30);

        _rigidbody.AddTorque(torqueAngle * torqueMultiplier * torqueDirection);
    }
    #endregion

    #region REACT TO OFF LIMIT WALLS COLLISION
    public void ReactTo_OffLimitWallsCollision(bool _bossCollided, bool _gunCollided, bool _hoverboardCollided)
    {
        if (bossState != BossStates.DEAD)
        {
            return;
        }

        // Set the object that collided as gone
        if (_bossCollided)
        {
            bossIsGone = true;
        }
        else if (_gunCollided)
        {
            gunIsGone = true;
        }
        else if (_hoverboardCollided)
        {
            hoverboardIsGone = true;
        }

        // Deactivate boss if all objects are gone
        if (bossIsGone && gunIsGone && hoverboardIsGone)
        {
            DeactivateBoss(false);
        }
    }
    #endregion

    // ---------- HORIZONTAL ACCELERATION ----------------------------------------------------+

    #region START ACCELERATING
    public void StartAccelerating()
    {
        if (bossState == BossStates.DEAD)
        {
            return;
        }

        // Set flag
        heroIsAlive = false;

        // Set body type, allowing automatic physics interaction
        bossRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // Freeze vertical movement
        bossRigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // Set acceleration as allowed
        accelerationAllowed = true;
    }
    #endregion

    #region START DECELERATING
    private void StartDecelerating()
    {
        // Set body type, allowing automatic physics interaction
        bossRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // Freeze vertical movement
        bossRigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // Set deceleration as allowed
        decelerationAllowed = true;
    }
    #endregion

    // ---------- VERTICAL MOVEMENT ----------------------------------------------------+

    #region PREPARE TO FIRE
    public void PrepareToFire(float _firingPositionY)
    {
        // Set firing position
        firingPositionY = _firingPositionY;

        // Set as preparing to fire
        isPreparingToFire = true;

        // ----- START MOVING -----

        // Get vertical direction
        bossDirectionY = ((boss.transform.position.y - _firingPositionY) > 0) ? -1 : 1;

        // Set vertical speed
        float speedY = Mathf.Abs(bossMaxSpeed / 2);

        // Change vertical velocity
        bossRigidbody.velocity = Vector2.up * bossDirectionY * speedY;
    }
    #endregion

    // ---------- OTHER ----------------------------------------------------+

    #region CHECK IF ITS ACTIVE 
    public bool CheckIf_ItsActive()
    {
        return boss.activeSelf;
    }
    #endregion

    #region CHECK IF ITS GONE
    public bool CheckIf_ItsGone()
    {
        // Initialize exit flags
        bool objectExitedThroughLeft;
        bool objectExitedThroughRight;

        // If hero is going right 
        if (heroDirectionX > 0)
        {
            objectExitedThroughLeft = (boss.transform.position.x < exitPositionX);
            objectExitedThroughRight = (boss.transform.position.x > spawnPositionX);
        }

        // if hero is going left
        else
        {
            objectExitedThroughRight = (boss.transform.position.x > exitPositionX);
            objectExitedThroughLeft = (boss.transform.position.x < spawnPositionX);
        }

        return (objectExitedThroughLeft || objectExitedThroughRight);
    }
    #endregion

    #region CHECK IF ITS DEAD
    private bool CheckIf_ItsDead()
    {
        // Return if it's dying 
        return animatorController.CheckIf_Dead();
    }
    #endregion

}
