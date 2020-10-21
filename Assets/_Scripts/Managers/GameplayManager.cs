using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    // Game started or continued event
    public delegate void GameStartedOrContinued_EventHandler(bool _newGame);
    public event GameStartedOrContinued_EventHandler GameStartedOrContinued;

    // Hero jumped event
    public delegate void HeroJumped_EventHandler();
    public event HeroJumped_EventHandler HeroJumped;

    // Hero started dodging event
    public delegate void HeroStartedDodging_EventHandler();
    public event HeroStartedDodging_EventHandler HeroStartedDodging;

    // Hero stopped dodging event
    public delegate void HeroStoppedDodging_EventHandler();
    public event HeroStoppedDodging_EventHandler HeroStoppedDodging;

    // Hero was hit event
    public delegate void HeroWasHit_EventHandler();
    public event HeroWasHit_EventHandler HeroWasHit;

    // Hero is dead event
    public delegate void HeroIsDead_EventHandler(int _currentScore);
    public event HeroIsDead_EventHandler HeroIsDead;

    // Hero attacked
    public delegate void HeroAttacked_EventHandler();
    public event HeroAttacked_EventHandler HeroAttacked;

    // Boss entered
    public delegate void BossEntered_EventHandler();
    public event BossEntered_EventHandler BossEntered;

    // Boss fired
    public delegate void BossFired_EventHandler(bool _oneProjectileIsBig);
    public event BossFired_EventHandler BossFired;

    // Boss fired jammed gun
    public delegate void BossFired_JammedGun_EventHandler();
    public event BossFired_JammedGun_EventHandler BossFired_JammedGun;

    // Boss is dead
    public delegate void BossIsDead_EventHandler();
    public event BossIsDead_EventHandler BossIsDead;

    // Boss is gone
    public delegate void BossIsGone_EventHandler();
    public event BossIsGone_EventHandler BossIsGone;

    // Current score has changed event
    public delegate void CurrentScore_HasChanged_EventHandler(int _currentScore);
    public event CurrentScore_HasChanged_EventHandler CurrentScore_HasChanged;

    // Current wave has changed event
    public delegate void CurrentWave_HasChanged_EventHandler(int _wave);
    public event CurrentWave_HasChanged_EventHandler CurrentWave_HasChanged;

    // ----- GAME ELEMENTS MANAGERS -----

    // Camera
    [SerializeField] private GameObject mainCamera = null;

    // Hero direction manager
    [SerializeField] private HeroDirectionManager heroDirectionManager = null;

    // Map, hero, obstacle and boss manager
    [SerializeField] private MapManager mapManager = null;
    [SerializeField] private HeroManager heroManager = null;
    [SerializeField] private ObstacleManager obstacleManager = null;
    [SerializeField] private BossManager bossManager = null;

    // ----- GAME STATES -----

    private enum GameStates { TITLE_SCREEN, RUNNING, PAUSED, GAME_OVER };
    private static GameStates gameState;

    // --------------- GAMEPLAY DATA ---------------

    // Hero direction
    private int heroDirectionX;

    // Match points
    private int currentScore;

    // ----- SPEED & FORCE -----

    // Game speed: real speed used by game elements
    private readonly float gameSpeed = 105;

    // Death drag force: force applied to slow down elements, when hero is dead
    private Vector2 deathDragForce = new Vector2(50, 0);

    // Death acceleration force: force applied to speed up elements, when hero is dead
    private Vector2 deathAccelerationForce = new Vector2(100, 0);

    // ----- POSITIONS -----

    // Absolute horizontal position, for spawning and exiting the screen. For every object
    private readonly float spawnAndExit_AbsolutePositionX = 145;

    // Real vertical position, for spawning the hero & enemies
    private readonly float charactersSpawnPositionY = -21.29f; //- 20.2f;

    // Absolute horizontal position, for the boss base
    private readonly float bossBase_AbsolutePositionX = 110;

    #endregion

    // ---------- INITIALIZATIONS ON AWAKE ----------------------------------------------------+

    #region AWAKE
    private void Awake()
    {
        // ----- GAME STATE -----

        gameState = GameStates.TITLE_SCREEN;

        // ----- EVENT SUBSCRIPTIONS -----

        SubscribeToEvents();

        // ----- GAME ELEMENTS INITIALIZATIONS -----

        // Obstacles and Bosses will have normal game speed
        // Map and hero's speed will be 0.25% slower

        // Obstacles' drag force will be 0.5% slower than the map's

        // Initialize map
        mapManager.Initialize(gameSpeed * 3 / 4, deathDragForce, mainCamera);

        // Initialize hero
        heroManager.Initialize(charactersSpawnPositionY, gameSpeed * 3 / 4);

        // Initialize obstacles
        obstacleManager.Initialize(spawnAndExit_AbsolutePositionX, charactersSpawnPositionY, 
            gameSpeed, deathDragForce / 2);

        // Initialize bosses
        bossManager.Initialize(spawnAndExit_AbsolutePositionX,
            charactersSpawnPositionY, bossBase_AbsolutePositionX, gameSpeed, deathAccelerationForce);

        // ----- SET HERO DIRECTION -----

        // Set hero direction
        SetHeroDirection();

        // Set serenity state
        mapManager.SetSerenityState();
    }
    #endregion

    #region SUBSCRIBE TO EVENTS
    private void SubscribeToEvents()
    {
        // ----- PROJECTILE RELATED -----

        // Subscribe "projectile was spawned" event
        obstacleManager.ProjectileWasSpawned += RespondTo_ProjectileWasSpawned_Event;

        // ----- HERO-PROJECTILE RELATED -----

        // Subscribe to "input denier was hit" event
        obstacleManager.InputDenierWasHit += RespondTo_InputDenierWasHit_Event;

        // Subscribe to "flawed projectile collided with hero" event
        obstacleManager.FlawedProjectile_CollidedWithHero += RespondTo_FlawedProjectile_CollidedWithHero_Event;

        // Subscribe to "obstacle wall was hit" event
        heroManager.ObstacleWallWasHit += RespondTo_ObstacleWallWasHit_Event;

        // Subscribe to "hero was hit" event
        heroManager.HeroWasHit += RespondTo_HeroWasHit_Event;

        // Subscribe to "game is over" event
        heroManager.GameIsOver += RespondTo_GameIsOver_Event;

        // ----- BOSS RELATED -----

        // Subscribe to "boss is ready to spawn" event
        obstacleManager.BossIsReadyToSpawn += RespondTo_BossIsReadyToSpawn_Event;

        // Subscribe to "boss is firing" event
        bossManager.BossIsFiring += RespondTo_BossIsFiring_Event;

        // Subscribe to "boss fired" event
        obstacleManager.BossFired += RespondTo_BossFired_Event;

        // Subscribe to "boss fired jammed gun" event
        obstacleManager.BossFired_JammedGun += RespondTo_BossFired_JammedGun_Event;

        // Subscribe to "boss is vulnerable" event
        obstacleManager.BossIsVulnerable += RespondTo_BossIsVulnerable_Event;

        // Subscribe to "boss is dead" event
        bossManager.BossIsDead += RespondTo_BossIsDead_Event;

        // Subscribe to "boss is gone" event
        bossManager.BossIsGone += RespondTo_BossIsGone_Event;

        // ----- MAP RELATED -----

        // Subscribe to "hero default state started" event
        mapManager.HeroDefaultStateStarted += RespondTo_HeroDefaultStateStarted_Event;

        // Subscribe to "curve was entered" event
        mapManager.CurveWasEntered += RespondTo_CurveWasEntered_Event;

        // Subscribe to "direction has changed" event
        mapManager.DirectionHasChanged += RespondTo_DirectionHasChanged_Event;

        // Subscribe to "turning animation is on" event
        mapManager.TurningAnimationIsOn += RespondTo_TurningAnimationIsOn_Event;

        // Subscribe to "turning animation is of" event
        mapManager.TurningAnimationIsOff += RespondTo_TurningAnimationIsOff_Event;

        // Subscribe to "map stopped dragging" event
        mapManager.MapStoppedDragging += RespondTo_MapStoppedDragging_Event;

        // ----- HERO RELATED -----

        // Subscrite to hero actions events
        heroManager.HeroJumped += RespondTo_HeroJumped_Event;
        heroManager.HeroStartedDodging += RespondTo_HeroStartedDodging_Event;
        heroManager.HeroStoppedDodging += RespondTo_HeroStoppedDodging_Event;

        // ----- WAVE RELATED -----

        // 
        obstacleManager.CurrentWave_HasChanged += RespondTo_CurrentWave_HasChanged_Event;
    }
    #endregion

    #region SET HERO DIRECTION
    private void SetHeroDirection()
    {
        // ----- DIRECTION STORED ON JSON FILE -----

        // Initialize hero direction manager
        heroDirectionManager.InitializeDirection();

        // Get current hero direction
        heroDirectionX = heroDirectionManager.GetDirection();

        // Exception
        if (heroDirectionX == 0)
        {
            Debug.Log("ERROR: Hero's direction can't be zero");
        }

        // ----- DIRECTION ON GAME MANAGERS -----

        mapManager.SetDirection(heroDirectionX);
        heroManager.SetDirection(heroDirectionX);
        obstacleManager.SetDirection(heroDirectionX);
        bossManager.SetDirection(heroDirectionX);
    }
    #endregion

    // ---------- UPDATE ----------------------------------------------------+

    #region UPDATE
    public void Update()
    {
        // Do not update if game is paused or haven't started once
        if (gameState == GameStates.TITLE_SCREEN ||
            gameState == GameStates.PAUSED)
        {
            return;
        }

        // ----- UPDATE HERO -----

        heroManager.UpdateHero();

        // ----- UPDATE OBSTACLES -----

        obstacleManager.UpdateObstacles();

        // ----- UPDATE BOSSES -----

        bossManager.UpdateBoss();
    }
    #endregion

    // ---------- PUBLIC METHODS ----------------------------------------------------+

    #region START OR CONTINUE GAME
    // Continue the game if it was paused, start new game otherwise
    public void StartOrContinueGame()
    {
        // If game was over
        if (gameState == GameStates.GAME_OVER)
        {
            // Clear active obstacles from previous match
            obstacleManager.ClearObstacles();

            // Set serenity state
            mapManager.SetSerenityState();
        }

        // If new match
        if (gameState == GameStates.TITLE_SCREEN ||
            gameState == GameStates.GAME_OVER)
        {
            // Reset match UI
            CurrentScore_HasChanged?.Invoke(0);
            CurrentWave_HasChanged?.Invoke(1);

            // Spawn new hero
            heroManager.SpawnNewHero();

            // Reset obstacles
            obstacleManager.ResetValuesForNewGame();

            // Reset bosses
            bossManager.ResetValuesForNewGame();

            // Start moving map. 
            mapManager.StartHeroEnteringState();

            // Reset dodged obstacles
            currentScore = 0;
        }

        // Fire event: Game started or continued
        GameStartedOrContinued?.Invoke(gameState != GameStates.PAUSED);

        // Set game as running or intro
        //gameState = GameStates.INTRO;
        gameState = GameStates.RUNNING;

        // Unpause game
        Time.timeScale = 1.0f; // 1
    }
    #endregion

    #region PAUSE GAME
    public void PauseGame()
    {
        // Pause game
        Time.timeScale = 0;

        // Set game as paused
        gameState = GameStates.PAUSED;
    }
    #endregion

    // ---------- RESPOND TO [PROJECTILE RELATED] EVENTS ----------------------------------------------------+

    #region RESPOND TO "PROJECTILE WAS SPAWNED" EVENT
    private void RespondTo_ProjectileWasSpawned_Event(float _bossFiringPositionY)
    {
        bossManager.PrepareToFire(_bossFiringPositionY);
    }
    #endregion

    // ---------- RESPOND TO [HERO-PROJECTILE RELATED] EVENTS ----------------------------------------------------+

    #region RESPOND TO "INPUT DENIER WAS HIT" EVENT
    private void RespondTo_InputDenierWasHit_Event()
    {
        // Deactivate input denier
        heroManager.RespondTo_InputDenierWasHit_Event();
    }
    #endregion

    #region RESPOND TO "FLAWED PROJECTILE COLLIDED WITH HERO" EVENT
    private void RespondTo_FlawedProjectile_CollidedWithHero_Event()
    {
        // Make hero react to flawed projectile collision
        heroManager.ReactTo_FlawedProjectile_Collision();

        // Deactivate input denier
        bossManager.DeactivateInputDenier();

        // Deactivate boss's firing area
        bossManager.DeactivateFiringArea();

        // Allow player input
        heroManager.AllowPlayerJump();

        // Fire event: Hero attacked
        HeroAttacked?.Invoke();
    }
    #endregion

    #region RESPOND TO "OBSTACLE WALL WAS HIT" EVENT
    private void RespondTo_ObstacleWallWasHit_Event()
    {
        IncreaseScore(false);
    }
    #endregion

    #region RESPOND TO "HERO WAS HIT" EVENT
    private void RespondTo_HeroWasHit_Event()
    {
        // Fire event: Hero was hit
        HeroWasHit?.Invoke();

        // Set hero as dead on boss manager
        bossManager.SetHeroAsDead();

        // Set state as serenity
        obstacleManager.SetStateAs_Serenity();
    }
    #endregion

    #region RESPOND TO "GAME IS OVER" EVENT
    private void RespondTo_GameIsOver_Event()
    {
        // Start dragging map
        mapManager.StartDragging();
        // Change map state
        mapManager.SetHeroDeadState();

        // Start dragging obstacles
        obstacleManager.StartDragging();

        // Speed up bosses
        bossManager.StartAccelerating();

        // Set game as over
        gameState = GameStates.GAME_OVER;

        // Fire event: Hero is dead
        HeroIsDead?.Invoke(currentScore);
    }
    #endregion

    #region INCREASE SCORE
    private void IncreaseScore(bool _bossWasDefeated)
    {
        int points;

        if (_bossWasDefeated)
        {
            points = 50;
        }
        else
        {
            points = 10;
        }

        // Increase current score
        currentScore += points;

        // Fire event: Current score has changed
        CurrentScore_HasChanged?.Invoke(currentScore);
    }
    #endregion

    // ---------- RESPOND TO [BOSS RELATED] EVENTS ----------------------------------------------------+

    #region RESPOND TO "BOSS IS READY TO SPAWN" EVENT
    private void RespondTo_BossIsReadyToSpawn_Event()
    {
        // Spawn boss
        bossManager.Spawn();

        // Fire event: Boss entered
        BossEntered?.Invoke();
    }
    #endregion

    #region RESPOND TO "BOSS IS FIRING" EVENT
    private void RespondTo_BossIsFiring_Event()
    {
        // Set state as boss firing
        obstacleManager.SetStateAs_BossFiring();
    }
    #endregion

    #region RESPOND TO "BOSS FIRED" EVENT
    private void RespondTo_BossFired_Event(bool _oneProjectileIsBig)
    {
        // Change boss animation
        bossManager.StartFiringAnimation();

        // Fire event: Boss fired
        BossFired?.Invoke(_oneProjectileIsBig);
    }
    #endregion

    #region RESPOND TO "BOSS FIRED" EVENT
    private void RespondTo_BossFired_JammedGun_Event()
    {
        // Change boss animation
        bossManager.StartFiringAnimation();

        // Fire event: Boss fired jammed gun
        BossFired_JammedGun?.Invoke();
    }
    #endregion

    #region RESPOND TO "BOSS IS VULNERABLE" EVENT
    private void RespondTo_BossIsVulnerable_Event()
    {
        // Set state as boss vulnerable
        bossManager.SetStateAs_BossVulnerable();
    }
    #endregion

    #region RESPOND TO "BOSS IS DEAD" EVENT
    private void RespondTo_BossIsDead_Event()
    {
        // Set state as serenity
        obstacleManager.SetStateAs_Serenity();

        // Activate curved road 1
        mapManager.RespondTo_BossIsDead_Event();

        // Fire event: Boss is dead
        BossIsDead?.Invoke();
    }
    #endregion

    #region RESPOND TO "BOSS IS GONE" EVENT
    private void RespondTo_BossIsGone_Event()
    {
        // Fire event: Boss is gone
        BossIsGone?.Invoke();

        // Fire event: Increase score
        IncreaseScore(true);
    }
    #endregion

    // ---------- RESPOND TO [MAP RELATED] EVENTS ----------------------------------------------------+

    #region RESPOND TO "HERO DEFAULT STATE STARTED" EVENT
    // Start normal game loop after the hero exits a curve
    private void RespondTo_HeroDefaultStateStarted_Event()
    {
        // Start hero default state
        heroManager.StartHeroDefaultState();

        // Start spawning obstacles
        obstacleManager.StartSpawningState();
    }
    #endregion

    #region RESPOND TO "FLOOR END IS CLOSE" EVENT
    private void RespondTo_FloorEndIsClose_Event()
    {
        
    }
    #endregion

    #region RESPOND TO "CURVE WAS ENTERED" EVENT
    private void RespondTo_CurveWasEntered_Event()
    {
        heroManager.StartTurning();
    }
    #endregion

    #region RESPOND TO "DIRECTION HAS CHANGED" EVENT
    private void RespondTo_DirectionHasChanged_Event()
    {
        // Flip direction on hero manager
        heroManager.FlipDirection();
        
        // Flip direction on obstacle manager
        obstacleManager.FlipDirection();

        // Flip direction on enemy boss manager
        bossManager.FlipDirection();

        // Change direction of hero
        heroDirectionX = heroDirectionX * (-1);

        // Save direction on file
        heroDirectionManager.SaveNewDirection(heroDirectionX);
    }
    #endregion

    #region RESPOND TO "TURNING ANIMATION IS ON" EVENT
    private void RespondTo_TurningAnimationIsOn_Event()
    {
        // Set turning animator controller
        heroManager.ChangeAnimationController(true);
    }
    #endregion

    #region RESPOND TO "TURNING ANIMATION IS OFF" EVENT
    private void RespondTo_TurningAnimationIsOff_Event()
    {
        // Set normal animator controller
        heroManager.ChangeAnimationController(false);
    }
    #endregion

    #region RESPOND TO "MAP STOPPED DRAGGING" EVENT
    private void RespondTo_MapStoppedDragging_Event()
    {
        obstacleManager.StopDragging();
    }
    #endregion

    // ---------- RESPOND TO [HERO ACTIONS] EVENTS ----------------------------------------------------+

    #region RESPOND TO "HERO JUMPED" EVENT
    private void RespondTo_HeroJumped_Event()
    {
        HeroJumped?.Invoke();
    }
    #endregion

    #region RESPOND TO "HERO STARTED DODGING" EVENT
    private void RespondTo_HeroStartedDodging_Event()
    {
        HeroStartedDodging?.Invoke();
    }
    #endregion

    #region RESPOND TO "HERO STOPPED DODGING" EVENT
    private void RespondTo_HeroStoppedDodging_Event()
    {
        HeroStoppedDodging?.Invoke();
    }
    #endregion

    // ---------- RESPOND TO [WAVE RELATED] EVENTS ----------------------------------------------------+

    #region RESPOND TO "CURRENT WAVE HAS CHANGED" EVENT
    private void RespondTo_CurrentWave_HasChanged_Event(int _wave)
    {
        // Fire event: Current wave has changed
        CurrentWave_HasChanged?.Invoke(_wave);
    }
    #endregion
}
