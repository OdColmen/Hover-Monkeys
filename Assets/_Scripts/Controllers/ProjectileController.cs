using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    public delegate void ProjectileHitFiringArea_EventHandler(GameObject _projectile);
    public event ProjectileHitFiringArea_EventHandler ProjectileHitFiringArea;

    // Input denier was hit
    public delegate void InputDenierWasHit_EventHandler();
    public event InputDenierWasHit_EventHandler InputDenierWasHit;


    public delegate void FlawedProjectile_CollidedWithHero_EventHandler();
    public event FlawedProjectile_CollidedWithHero_EventHandler FlawedProjectile_CollidedWithHero;

    // ----- COMPONENTS -----

    // Rigid body
    private Rigidbody2D myRigidbody;

    // Animator
    private Animator animator;

    // Renderer
    private SpriteRenderer myRenderer;

    // ----- POSITIONS -----

    // Absolute horizontal position, for spawning and exiting the screen.
    private float spawnAndExit_AbsolutePositionX;

    // Spawn BASE horizontal position. Same value for every projectile.
    // The final position will be set on every spawn. Different value for every projectile.
    private float spawnPositionX_Base;
    
    // Exit REAL horizontal position. Same value for every projectile.
    private float exitPositionX_Real;

    // ----- SPEED / FORCE -----

    // Obstacle speed
    private float maxSpeed;

    // Flags
    private bool dragAllowed;

    // Death drag force (for dead hero)
    private Vector2 deathDragForce;

    // ----- FLAWED -----

    // Flags
    private bool isFlawed;
    private bool isReturning;

    // Projectile speed modifier
    private float projectileSpeedModifier = 2;

    // ----- OTHER -----

    // flag
    private bool isBig;

    // ----- HERO DIRECTION -----

    private float heroDirectionX;

    #endregion

    // ---------- INITIALIZATION & UPDATE ----------------------------------------------------+

    #region INITIALIZE
    public void Initialize(GameObject _parent, float _spawnAndExit_AbsolutePositionX, float _speed,
        Vector2 _deathDragForce, bool _isBig, bool _isFlawed)
    {
        // Deactivate game object
        gameObject.SetActive(false);

        // Set parent
        gameObject.transform.parent = _parent.transform;

        // ----- GET COMPONENTS -----

        // Rigidbody, animator & sprite renderer
        myRigidbody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        myRenderer = gameObject.GetComponent<SpriteRenderer>();

        // Collision system
        CollisionSystem collisions = GetComponent<CollisionSystem>();
        collisions.InitializeProjectile(this, _isBig);

        // ----- SET ABSOLUTE POSITION -----

        // Set spawn and exit absolute horizontal position
        spawnAndExit_AbsolutePositionX = _spawnAndExit_AbsolutePositionX;

        // ----- SET SPEED & FORCE -----

        // Set speed
        maxSpeed = _speed;

        // Set death drag force
        deathDragForce = _deathDragForce;

        // ----- SET FLAWED -----

        isBig = _isBig;

        // ----- SET FLAWED -----

        isFlawed = _isFlawed;
    }
    #endregion

    #region FIXED UPDATE
    private void FixedUpdate()
    {
        // ----- DEATH DRAG FORCE -----

        if (dragAllowed)
        {
            // Apply force
            myRigidbody.AddForce(deathDragForce);
        }
    }
    #endregion

    // ---------- DIRECTION ----------------------------------------------------+

    #region SET DIRECTION
    public void SetDirection(int _heroDirectionX)
    {
        // If hero is going right, projectiles spawn from the right
        if (_heroDirectionX > 0)
        {
            // Set spawn position
            spawnPositionX_Base = spawnAndExit_AbsolutePositionX;
            // Set exit position
            exitPositionX_Real = spawnAndExit_AbsolutePositionX * (-1);
        }

        // If hero is going left, projectiles spawn from the left
        else
        {
            // Set spawn position
            spawnPositionX_Base = spawnAndExit_AbsolutePositionX * (-1);
            // Set exit position
            exitPositionX_Real = spawnAndExit_AbsolutePositionX;
        }

        // Change speed
        maxSpeed = maxSpeed * _heroDirectionX;

        // Change death drag force
        deathDragForce = new Vector2(deathDragForce.x * _heroDirectionX, deathDragForce.y);

        // Set direction
        heroDirectionX = _heroDirectionX;
    }
    #endregion

    #region FLIP DIRECTION
    public void FlipDirection()
    {
        // Change initial position
        spawnPositionX_Base = spawnPositionX_Base * (-1);

        // Change final position
        exitPositionX_Real = exitPositionX_Real * (-1);

        // Change speed
        maxSpeed = maxSpeed * (-1);

        // Change death drag force
        deathDragForce = new Vector2(deathDragForce.x * (-1), deathDragForce.y);

        // Store direction
        heroDirectionX = heroDirectionX * (-1);
    }
    #endregion

    // ---------- SPAWN & DEACTIVATE ----------------------------------------------------+

    #region SPAWN
    public void Spawn(Vector2 _position, RuntimeAnimatorController _animatorController, bool _bossIsFiring)
    {
        // Activate game object
        gameObject.SetActive(true);

        // ----- SET POSITION PARAMETERS -----

        // Point image in the right direction
        transform.localScale = new Vector2(heroDirectionX, 1);

        // Set current position
        transform.position = Vector2.zero;
        transform.localPosition = new Vector2(spawnPositionX_Base - _position.x * heroDirectionX, _position.y);

        // ----- SET MOVEMENT PARAMETERS -----

        // Set body type, denying automatic physics interaction
        myRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // Set velocity
        myRigidbody.velocity = Vector2.left * maxSpeed;

        // ----- SET FLAGS -----

        // Set flags
        dragAllowed = false;

        // ----- SET ANIMATION -----

        // Set animator controller
        SetAnimatorController(_animatorController);

        // Set animation
        animator.SetBool("isDead", false);

        // ----- HIDE -----

        if (_bossIsFiring)
        {
            myRenderer.enabled = false;
        }
        else
        {
            myRenderer.enabled = true;
        }

        // ----- SET TAG -----

        gameObject.tag = "Projectile";
    }
    #endregion

    #region SPAWN
    public void Spawn(Vector2 _position)
    {
        // Activate game object
        gameObject.SetActive(true);

        // ----- SET POSITION PARAMETERS -----

        // Point image in the right direction
        transform.localScale = new Vector2(heroDirectionX, 1);

        transform.position = Vector2.zero;
        transform.localPosition = new Vector2(spawnPositionX_Base - _position.x * heroDirectionX, _position.y);

        // ----- SET MOVEMENT PARAMETERS -----

        // Set body type, denying automatic physics interaction
        myRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // Set velocity
        myRigidbody.velocity = Vector2.left * maxSpeed;

        // ----- SET FLAGS -----

        // Set flags
        dragAllowed = false;
        isReturning = false;

        // ----- SET ANIMATION -----

        // Set animation
        animator.SetBool("isDead", false);
        animator.SetBool("isReturning", false);

        // ----- HIDE -----

        myRenderer.enabled = false;

        // ----- SET TAG -----
        
        gameObject.tag = "Projectile";
    }
    #endregion

    #region SET ANIMATOR CONTROLLER
    private void SetAnimatorController(RuntimeAnimatorController _animatorController)
    {
        animator.runtimeAnimatorController = _animatorController;
    }
    #endregion

    #region SHOW HIDDEN, CHANGE SPEED
    // Show hidden projectile when fired by boss
    public void ShowHidden_ChangeSpeed()
    {
        // Show projectile
        myRenderer.enabled = true;

        // Reduce velocity
        myRigidbody.velocity = Vector2.left * maxSpeed / projectileSpeedModifier;
    }
    #endregion

    #region SHOW HIDDEN, CHANGE TAG
    // Show hidden projectile and change tag
    public void ShowHidden_ChangeTag()
    {
        // Show projectile
        myRenderer.enabled = true;

        // Change tag
        gameObject.tag = "Projectile 2";
    }
    #endregion

    #region DEACTIVATE
    public void Deactivate()
    {
        // Deactivate game object
        gameObject.SetActive(false);
    }
    #endregion

    // ---------- COLLISIONS ----------------------------------------------------+

    #region REACT TO FIRING AREA COLLISION
    public void ReactTo_FiringAreaCollision()
    {
        // Fire event: Projectile hit firing area
        ProjectileHitFiringArea?.Invoke(gameObject);
    }
    #endregion

    #region REACT TO "PROJECTILE INPUT DENIER" COLLISION
    public void ReactTo_Projectile_InputDenier_Collision()
    {
        if (isFlawed)
        {
            // Fire event: Input denier was hit
            InputDenierWasHit?.Invoke();
        }
    }
    #endregion

    #region REACT TO HERO COLLISION
    public void ReactTo_HeroCollision()
    {
        if (isFlawed)
        {
            // Change animation
            animator.SetBool("isReturning", true);

            // Change tag
            gameObject.tag = "Projectile Returning";

            // Return (change direction and speed)
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x * (-1) * projectileSpeedModifier,
                myRigidbody.velocity.y);

            // Set as returning
            isReturning = true;

            // Fire event: Flawed projectile collided with hero
            FlawedProjectile_CollidedWithHero?.Invoke();
        }

        else
        {
            // Change animation
            animator.SetBool("isDead", true);

            // Reduce speed
            myRigidbody.velocity = Vector2.left * maxSpeed / 1.5f;
        }
    }
    #endregion

    #region REACT TO BOSS COLLISION
    public void ReactTo_BossCollision()
    {
        // Change tag
        gameObject.tag = "Untagged";

        if (isFlawed && isReturning)
        {
            // Change animation
            animator.SetBool("isDead", true);
        }
    }
    #endregion

    // ---------- DRAGGING ----------------------------------------------------+

    #region START DRAGGING
    public void StartDragging()
    {
        // Set body type, allowing automatic physics interaction
        myRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // Freeze vertical movement
        myRigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // Set drag as allowed
        dragAllowed = true;
    }
    #endregion

    #region STOP DRAGGING
    public void StopDragging()
    {
        // Set body type, denying automatic physics interaction
        myRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // Set drag as not allowed
        dragAllowed = false;
    }
    #endregion

    // ---------- OTHERS ----------------------------------------------------+

    #region CHECK IF ITS ACTIVE 
    public bool CheckIf_ItsActive()
    {
        return gameObject.activeSelf;
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
            objectExitedThroughLeft = (transform.localPosition.x < exitPositionX_Real);
            objectExitedThroughRight = (transform.localPosition.x > spawnPositionX_Base);
        }

        // if hero is going left
        else
        {
            objectExitedThroughRight = (transform.localPosition.x > exitPositionX_Real);
            objectExitedThroughLeft = (transform.localPosition.x < spawnPositionX_Base);
        }

        return (objectExitedThroughLeft || objectExitedThroughRight);
    }
    #endregion

    #region CHECK IF ITS BIG
    public bool CheckIf_ItsBig()
    {
        return isBig;
    }
    #endregion
}