using UnityEngine;

public class HeroManager : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    // Hero jumped event
    public delegate void HeroJumped_EventHandler();
    public event HeroJumped_EventHandler HeroJumped;

    // Hero landed event
    //public delegate void HeroLanded_EventHandler();
    //public event HeroLanded_EventHandler HeroLanded;

    // Hero started dodging event
    public delegate void HeroStartedDodging_EventHandler();
    public event HeroStartedDodging_EventHandler HeroStartedDodging;

    // Hero stopped dodging event
    public delegate void HeroStoppedRolling_EventHandler();
    public event HeroStoppedRolling_EventHandler HeroStoppedDodging;

    // Hero was hit event
    public delegate void HeroWasHit_EventHandler();
    public event HeroWasHit_EventHandler HeroWasHit;

    // Game is over
    public delegate void GameIsOver_EventHandler();
    public event GameIsOver_EventHandler GameIsOver;

    // Obstacle wall was hit
    public delegate void ObstacleWallWasHit_EventHandler();
    public event ObstacleWallWasHit_EventHandler ObstacleWallWasHit;

    // ----- COMPONENTS AND SCRIPTS -----

    // Hero rigidbody
    private Rigidbody2D heroRigidbody;

    // Hero Colliders
    // fullCollider will be active ONLY when the hero IS NOT sliding
    private PolygonCollider2D fullCollider;
    // bottomCollider will be active ONLY when the hero IS sliding
    private CapsuleCollider2D bottomCollider;

    // Hero animator controller
    private HeroAnimatorController animatorController;

    // Sword and hoverboard rigid body
    private Rigidbody2D swordRigidbody;
    private Rigidbody2D hoverboardRigidbody;

    // ----- MAIN GAME OBJECTS -----

    // Prefabs
    [SerializeField] private GameObject heroPrefab = null;

    // Game objects
    private GameObject hero;
    private GameObject sword;
    private GameObject hoverboard;

    // ----- HERO STATES -----

    private enum HeroStates { AWAY, ENTERING, DEFAULT, DEAD, GAME_OVER, TURNING }
    private static HeroStates heroState;

    // ----- POSITION -----

    // Base position
    private Vector2 basePosition;

    // Sword local position
    private Vector2 swordLocalPosition = new Vector2(-1, 1);
    // Hoverboard local position
    private Vector2 hoverboardLocalPosition = new Vector2(0, -3);

    // ----- GRAVITY -----

    // Jump speed
    private readonly float jumpSpeed = 160;

    // Normal gravity scale
    private readonly float normalGravityScale = 40;
    // Death gravity scale
    private readonly float deathGravityScale = 20;

    // ----- DROP VELOCITIES -----

    private Vector2[] dropVelocities;

    // ----- OTHER -----

    // Jump-dodge cooldown duration
    private readonly float jumpDodgeCooldownDuration = 0.2f; //=0.4f;
    private float jumpStartTime;

    // Floor layer mask
    // This variable is used for Physics2D.Raycast
    [SerializeField] private LayerMask floorLayerMask = 0;

    // Flags
    private bool playerIsDodging;
    private bool heroIsDodging;
    private bool heroIsJumping;
    private bool playerInputDenied;
    private bool playerJumpDenied;

    // ----- HERO DIRECTION -----

    private int heroDirectionX;

    #endregion

    // ---------- INITIALIZATIONS AND UPDATE ----------------------------------------------------+

    #region INITIALIZE
    public void Initialize(float _basePositionY, float _speed)
    {
        // Allow player input
        playerInputDenied = true;
        playerJumpDenied = true;

        // ----- STATE -----

        heroState = HeroStates.AWAY;

        // ----- GAME OBJECTS -----

        // Instantiate and activate hero
        hero = Instantiate(heroPrefab);
        hero.SetActive(true);

        // Get and deactivate sword
        sword = hero.transform.GetChild(0).gameObject;
        sword.SetActive(false);
        
        // Get and deactivate hoverboard
        hoverboard = hero.transform.GetChild(1).gameObject;
        hoverboard.SetActive(false);

        // ----- COMPONENTS -----

        // Rigidbody
        heroRigidbody = hero.GetComponent<Rigidbody2D>();

        // Polygon collider
        fullCollider = hero.GetComponent<PolygonCollider2D>();
        fullCollider.isTrigger = false;
        // Capsule collider
        bottomCollider = hero.GetComponent<CapsuleCollider2D>();
        bottomCollider.isTrigger = false;

        // Collision system
        CollisionSystem heroCollisions = hero.GetComponent<CollisionSystem>();
        heroCollisions.InitializeHero(this);

        // Animator controller
        animatorController = hero.GetComponent<HeroAnimatorController>();

        // Sword rigidbody
        swordRigidbody = sword.GetComponent<Rigidbody2D>();
        swordRigidbody.gravityScale = deathGravityScale;

        // Hoverboard rigidbody
        hoverboardRigidbody = hoverboard.GetComponent<Rigidbody2D>();
        hoverboardRigidbody.gravityScale = deathGravityScale;

        // ----- SET DEFAULT ANIMATION (running) -----

        // Start iddle animation
        animatorController.StartDefaultAnimation();

        // ----- SET BASE POSITION -----

        basePosition = new Vector2(0, _basePositionY);

        // Set hero position
        hero.transform.position = basePosition;

        // ----- RESET JUMPING FLAGS -----

        heroIsJumping = false;

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

        //dropVelocities[0] = new Vector2(dropVelocityX * 0.0f, dropVelocityY * 1.0f);
        //dropVelocities[1] = new Vector2(dropVelocityX * 0.15f, dropVelocityY * 1.0f);
        //dropVelocities[2] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 1.0f);

        // ----- SECOND HEIGHT (0.85 = 144.5) -----

        dropVelocities[0] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.85f);
        dropVelocities[1] = new Vector2(dropVelocityX * 0.4f, dropVelocityY * 0.85f);

        // ----- THIRD HEIGHT (0.7 = 119) -----

        //dropVelocities[5] = new Vector2(dropVelocityX * 0.0f, dropVelocityY * 0.7f);
        dropVelocities[2] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.7f);
        dropVelocities[3] = new Vector2(dropVelocityX * 0.4f, dropVelocityY * 0.7f);
        dropVelocities[4] = new Vector2(dropVelocityX * 0.5f, dropVelocityY * 0.7f);

        // ----- FOURTH HEIGHT (0.5) -----

        dropVelocities[5] = new Vector2(dropVelocityX * 0.15f, dropVelocityY * 0.5f);
        dropVelocities[6] = new Vector2(dropVelocityX * 0.3f, dropVelocityY * 0.5f);
        dropVelocities[7] = new Vector2(dropVelocityX * 0.5f, dropVelocityY * 0.5f);
        dropVelocities[8] = new Vector2(dropVelocityX * 0.6f, dropVelocityY * 0.5f);

        // ----- MIN HEIGHT (0.3) -----

        //dropVelocities[9] = new Vector2(dropVelocityX * 0.0f, dropVelocityY * 0.3f);
    }
    #endregion

    // ---------- UPDATES ----------------------------------------------------+

    #region UPDATE HERO
    public void UpdateHero()
    {
        // Update living hero
        if ((heroState != HeroStates.DEAD) && (heroState != HeroStates.GAME_OVER))
        {
            // Check if player is dodging
            if (playerIsDodging)
            {
                // Make hero dodge
                StartDodging();
            }
            else
            {
                // Make hero stop dodging
                StopDodging(false);
            }
        }
    }
    #endregion

    // ---------- DIRECTION ----------------------------------------------------+

    #region SET DIRECTION
    public void SetDirection(int _heroDirectionX)
    {
        // Set local scale on hero and children
        hero.transform.localScale = new Vector2(_heroDirectionX, 1);

        // Set direction
        heroDirectionX = _heroDirectionX;
    }
    #endregion

    #region FLIP DIRECTION
    public void FlipDirection()
    {
        // ----- FLIP DIRECTION -----

        // Change local scale on hero and children
        hero.transform.localScale = new Vector2(heroDirectionX * (-1), 1);

        // Store new direction
        heroDirectionX = heroDirectionX * (-1);
    }
    #endregion

    // ---------- SPAWNS ----------------------------------------------------+

    #region SPAWN NEW HERO
    public void SpawnNewHero()
    {
        // Activate hero
        hero.SetActive(true);

        // Deactivate sword and hoverboard
        sword.SetActive(false);
        hoverboard.SetActive(false);

        // Deny player input
        playerInputDenied = false;
        playerJumpDenied = false;

        // ----- STATE -----

        // Change state
        heroState = HeroStates.ENTERING;

        // Change animator controller
        animatorController.SetAnimatorController(true);

        // ----- RESET JUMPING FLAGS -----

        heroIsJumping = false;

        // ----- RESET DODGING FLAGS -----

        // Reset playerIsDodging flag
        playerIsDodging = false;

        // Stop dodging hero
        if (heroIsDodging)
        {
            StopDodging(false);
        }

        // Enable top collider only
        fullCollider.enabled = true;
        bottomCollider.enabled = false;

        // Set collider as non trigger
        fullCollider.isTrigger = false;

        // ----- RESET ANIMATIONS -----

        // Reset hero animation
        animatorController.StopDeadAnimation();
        
        // ----- RESET TRANSFORM PARAMETERS -----

        // Set positions
        hero.transform.position = basePosition;
        sword.transform.localPosition = swordLocalPosition;
        hoverboard.transform.localPosition = hoverboardLocalPosition;

        // Reset rorations
        hero.transform.rotation = Quaternion.identity;
        sword.transform.rotation = Quaternion.identity;
        hoverboard.transform.rotation = Quaternion.identity;

        // ----- RESET RIGIDBODIES PARAMETERS -----

        // Reset hero's gravity
        heroRigidbody.gravityScale = normalGravityScale;
        // Set hero's movement constraints
        heroRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // Set sword's movement constraints
        swordRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        // Set hoverboard's movement constraints
        hoverboardRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        // ----- RESET OTHER PARAMETERS -----

        // Set the hero jumpStartTime to a previous time, so the player can dodge from the start
        jumpStartTime = Time.time - jumpDodgeCooldownDuration * 2;

        // Restore tags
        hero.tag = "Hero";
    }
    #endregion

    #region START HERO DEFAULT STATE
    public void StartHeroDefaultState()
    {
        if ((heroState == HeroStates.ENTERING) || (heroState == HeroStates.TURNING))
        {
            // Allow player input
            playerInputDenied = false;
            playerJumpDenied = false;

            // Set state as default
            heroState = HeroStates.DEFAULT;
        }
    }
    #endregion

    #region CHANGE ANIMATION CONTROLLER
    public void ChangeAnimationController(bool _isTurning)
    {
        // Change animator controller
        animatorController.SetAnimatorController(_isTurning);
    }
    #endregion

    // ---------- KEYBOARD CONTROLS ----------------------------------------------------+

    #region UPDATE
    private void Update()
    {
        DetectKeyboardInput();
    }
    #endregion

    #region DETECT KEYBOARD INPUT
    private void DetectKeyboardInput()
    {
        // ----- GAME INPUT -----

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteJumpButton_OnPointerDown();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            ExecuteDodgeButton_OnPointerDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            ExecuteDodgeButton_OnPointerUp();
        }

        // ----- OTHER -----

        // For debugging
        if (Input.GetKeyUp(KeyCode.D))
        {
            Debug.Log("Start debugging");
        }

        if ((heroState == HeroStates.DEFAULT) && !playerInputDenied)
        {
            // Force projectile hit
            if (Input.GetKeyUp(KeyCode.H))
            {
                ReactTo_ProjectileCollision();
            }
        }

        // Slow motion
        if (Input.GetKeyUp(KeyCode.S))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0.1f;
            }
            
            else
            {
                Time.timeScale = 1;
            }
        }

        // Attack show-off
        if (Input.GetKeyUp(KeyCode.A))
        {
            animatorController.StartAttackingAnimation();
        }
    }
    #endregion

    // ---------- CHECK IF HERO IS GROUNDED ----------------------------------------------------+

    #region IS GROUNDED
    private bool IsGrounded()
    {
        // Return if hero is grounded on any collider
        return (IsGroundedUsingPolygonCollider(fullCollider) || IsGroundedUsingCapsuleCollider(bottomCollider));
    }
    #endregion

    #region IS GROUNDED USING CAPSULE COLLIDER
    private bool IsGroundedUsingCapsuleCollider(CapsuleCollider2D _collider)
    {
        // Set extra height
        float extraHeight = 1f;

        // ----- CHECK RAYCAST HIT -----

        RaycastHit2D raycastHit = Physics2D.Raycast(_collider.bounds.center, Vector2.down,
            _collider.bounds.extents.y + extraHeight, floorLayerMask);

        // ----- DRAW RAYCAST -----

        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(_collider.bounds.center, Vector2.down *
            (_collider.bounds.extents.y + extraHeight), rayColor);

        // ----- RETURN IF RAYCAST HIT SOMETHING -----

        return raycastHit.collider != null;
    }
    #endregion

    #region IS GROUNDED USING POLYGON COLLIDER
    private bool IsGroundedUsingPolygonCollider(PolygonCollider2D _collider)
    {
        // Set extra height
        float extraHeight = 1f;

        // ----- CHECK RAYCAST HIT -----

        RaycastHit2D raycastHit = Physics2D.Raycast(_collider.bounds.center, Vector2.down,
            _collider.bounds.extents.y + extraHeight, floorLayerMask);

        // ----- DRAW RAYCAST -----

        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(_collider.bounds.center, Vector2.down *
            (_collider.bounds.extents.y + extraHeight), rayColor);

        // ----- RETURN IF RAYCAST HIT SOMETHING -----

        return raycastHit.collider != null;
    }
    #endregion

    // ---------- JUMP AND DODGE ACTIONS ----------------------------------------------------+

    #region EXECUTE JUMP BUTTON - ON POINTER DOWN
    public void ExecuteJumpButton_OnPointerDown()
    {
        if (playerInputDenied || playerJumpDenied)
        {
            return;
        }

        if (IsGrounded())
        {
            // Stop dodging
            StopDodging(true);

            // Set starting time
            jumpStartTime = Time.time;

            // Start jumping animation
            animatorController.StartJumpingAnimation();

            // Start moving up
            heroRigidbody.velocity = Vector2.up * jumpSpeed;

            // Set hero as jumping
            heroIsJumping = true;

            // Fire event: Hero jumped
            HeroJumped?.Invoke();
        }
    }
    #endregion

    #region EXECUTE DODGE BUTTON - ON POINTER DOWN
    public void ExecuteDodgeButton_OnPointerDown()
    {
        if (playerInputDenied)
        {
            return;
        }

        playerIsDodging = true;
    }
    #endregion

    #region EXECUTE DODGE BUTTON - ON POINTER UP
    public void ExecuteDodgeButton_OnPointerUp()
    {
        if (playerInputDenied)
        {
            return;
        }

        playerIsDodging = false;
    }
    #endregion

    #region START DODGING
    public void StartDodging()
    {
        if (heroIsDodging || (Time.time - jumpStartTime < jumpDodgeCooldownDuration))
        {
            return;
        }

        // Enable bottom collider only
        fullCollider.enabled = false;
        bottomCollider.enabled = true;

        // Start dodging animation
        animatorController.StartDodgingAnimation();

        // Change falling speed
        heroRigidbody.velocity = Vector2.down * jumpSpeed;

        // Set hero as dodging
        heroIsDodging = true;

        if (!heroIsJumping)
        {
            // Fire event: Hero started dodging
            //HeroStartedDodging?.Invoke();
        }

        HeroStartedDodging?.Invoke();
    }
    #endregion

    #region STOP DODGING
    private void StopDodging(bool _playerIsJumping)
    {
        if (!heroIsDodging)
        {
            return;
        }

        // If hero is NOT dying or dead
        if ((heroState != HeroStates.DEAD) && (heroState != HeroStates.GAME_OVER))
        {
            // Enable full
            fullCollider.enabled = true;
            bottomCollider.enabled = false;
        }

        // Stop dodging animation
        animatorController.StopDodgingAnimation();

        // Set hero as not dodging
        heroIsDodging = false;

        if (!_playerIsJumping)
        {
            // Fire event: Hero stopped dodging
            HeroStoppedDodging?.Invoke();
        }
    }
    #endregion

    // ---------- COLLISIONS AND OTHER ACTIONS ----------------------------------------------------+

    #region REACT TO PROJECTILE COLLISION
    public void ReactTo_ProjectileCollision()
    {
        // Do not react to projectiles if hero is dead
        if ((heroState == HeroStates.DEAD) || (heroState == HeroStates.GAME_OVER))
        {
            return;
        }

        // ----- CHANGE ANIMATIONS -----

        // Start getting hit animation
        animatorController.StartDeadAnimation();

        // Deny player input
        DenyPlayerInput();

        // Stop dodging
        StopDodging(true);

        // Stop jumping animation if possible
        if (!IsGrounded())
        {
            animatorController.StopJumpingAnimation();
        }

        // Start VFX animation
        //vfxTakeHit.StartAnimationCycle();

        // ----- OTHER ACTIONS -----

        // Physically react to hit
        PhysicallyReactToHit();

        // Set collider as trigger
        fullCollider.isTrigger = true;

        // Change tag
        hero.tag = "Hero Dying";

        // Set match state as "hero dying"
        heroState = HeroStates.DEAD;

        // Fire event: Hero was hit
        HeroWasHit?.Invoke();

        // Add slow mo
        Time.timeScale = 0.75f;
    }
    #endregion

    #region PHYSICALLY REACT TO HIT
    private void PhysicallyReactToHit()
    {
        // Notes: currently swordGravityScale and heroDyingGravityScale is the same (20)

        // ----- HERO DROP -----

        // Change gravity
        heroRigidbody.gravityScale = deathGravityScale;
        // Allow all movement
        heroRigidbody.constraints = RigidbodyConstraints2D.None;

        // Set velocity and torque
        SetDropVelocityAndTorque(heroRigidbody);

        // ----- SWORD DROP -----

        // Activate object
        sword.SetActive(true);
        // Allow movement
        swordRigidbody.constraints = RigidbodyConstraints2D.None;

        // Set velocity and torque
        SetDropVelocityAndTorque(swordRigidbody);

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

    #region REACT TO PROJECTILE WALL COLLISION
    public void ReactTo_ProjectileWallCollision(GameObject _wall)
    {
        // Do not react to wall if hero is dead
        if ((heroState == HeroStates.DEAD) || (heroState == HeroStates.GAME_OVER))
        {
            return;
        }

        // Remove tag from wall
        _wall.tag = "Untagged";

        // Fire event: Obstacle wall was hit
        ObstacleWallWasHit?.Invoke();
    }
    #endregion

    #region REACT TO OFF LIMIT WALLS COLLISION
    public void ReactTo_OffLimitWallsCollision()
    {
        if (heroState != HeroStates.DEAD)
        {
            return;
        }

        // Change state
        heroState = HeroStates.GAME_OVER;

        // ----- EVENT -----

        // Fire event: Hero is dead
        GameIsOver?.Invoke();
    }
    #endregion

    #region REACT TO FLOOR COLLISION
    public void ReactTo_FloorCollision()
    {
        // Stop jumping only when the rigidbody's velocity has stabilized to 0
        if (heroRigidbody.velocity.y == 0)
        {
            if (heroIsJumping)
            {
                // Stop jumping animation
                animatorController.StopJumpingAnimation();

                // Set hero as not jumping
                heroIsJumping = false;
            }
        }
    }
    #endregion

    #region HERO REACT TO LANDING AREA COLLISION
    public void Hero_ReactTo_LandingAreaCollision()
    {
        // Do not react if hero is dead
        if ((heroState == HeroStates.DEAD) || (heroState == HeroStates.GAME_OVER))
        {
            return;
        }

        if (heroRigidbody.velocity.y < 0)
        {
            // Fire event: Hero landed
            //HeroLanded?.Invoke();
        }
    }
    #endregion

    #region REACT TO "FLAWED PROJECTILE" COLLISION
    public void ReactTo_FlawedProjectile_Collision()
    {
        // Start giving shot animation
        animatorController.StartAttackingAnimation();
    }
    #endregion

    #region RESPOND TO "INPUT DENIER WAS HIT" EVENT
    public void RespondTo_InputDenierWasHit_Event()
    {
        // Deny player jump
        playerJumpDenied = true;
    }
    #endregion

    // ---------- OTHER ----------------------------------------------------+

    #region DENY PLAYER INPUT
    private void DenyPlayerInput()
    {
        // Deny player input
        playerInputDenied = true;

        // Set player as not dodging
        playerIsDodging = false;
    }
    #endregion

    #region ALLOW PLAYER JUMP
    public void AllowPlayerJump()
    {
        // Allow player jump
        playerJumpDenied = false;
    }
    #endregion

    #region START TURNING
    public void StartTurning()
    {
        // ----- DENY PLAYER INPUT -----

        //DenyPlayerInput();

        // ----- CHANGE STATE & MORE -----

        // Change state
        heroState = HeroStates.TURNING;
    }
    #endregion
}
