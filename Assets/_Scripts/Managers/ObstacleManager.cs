using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleManager : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    // Projectile was spawned
    public delegate void ProjectileWasSpawned_EventHandler(float _bossFiringPositionY);
    public event ProjectileWasSpawned_EventHandler ProjectileWasSpawned;

    // Boss is ready to spawn
    public delegate void BossIsReadyToSpawn_EventHandler();
    public event BossIsReadyToSpawn_EventHandler BossIsReadyToSpawn;

    // Boss fired
    public delegate void BossFired_EventHandler(bool _oneProjectileIsBig);
    public event BossFired_EventHandler BossFired;

    // Boss fired jammed gun
    public delegate void BossFired_JammedGun_EventHandler();
    public event BossFired_JammedGun_EventHandler BossFired_JammedGun;

    // Boss is vulnerable
    public delegate void BossIsVulnerable_EventHandler();
    public event BossIsVulnerable_EventHandler BossIsVulnerable;

    // Input denier was hit
    public delegate void InputDenierWasHit_EventHandler();
    public event InputDenierWasHit_EventHandler InputDenierWasHit;

    // Flawed projectile collided with hero
    public delegate void FlawedProjectile_CollidedWithHero_EventHandler();
    public event FlawedProjectile_CollidedWithHero_EventHandler FlawedProjectile_CollidedWithHero;

    // Current wave has changed event
    public delegate void CurrentWave_HasChanged_EventHandler(int _wave);
    public event CurrentWave_HasChanged_EventHandler CurrentWave_HasChanged;

    // ----- OBSTACLES' PARENT -----

    private GameObject obstaclesParent;

    // ----- PROJECTILES' CONTROLLERS -----

    private ProjectileController[] smallProjectilePool;
    private ProjectileController[] bigProjectilePool;
    private List<ProjectileController> currentProjectiles;

    // ----- PROJECTILE GROUPS -----

    private List<List<ProjectileController>> currentProjectileGroups;

    // ----- FLAWED PROJECTILE -----

    private ProjectileController flawedProjectile;
    
    // ----- PROJECTILE WALLS' CONTROLLER -----

    private ProjectileWallController[] obstacleWalls;

    // ----- VARIABLES FOR COMPONENTS AND SCRIPTS -----

    // Obstacle group selector
    // This class decides what types of obstacles to spawn and how to group the projectiles
    private ObstacleGroupSelector obstacleGroupSelector;

    // Animator controller pools
    [SerializeField] private RuntimeAnimatorController[] animatorControllerPool_ProjectileSmall = null;
    [SerializeField] private RuntimeAnimatorController[] animatorControllerPool_ProjectileBig = null;

    // ----- STATES -----

    private enum ObstacleStates { 
        SERENITY, SPAWNING, BOSS_NEAR, BOSS_FIRING, BOSS_FIRING_JAMMED_GUN, 
        BOSS_VULNERABLE, BOSS_ENTERING };
    private static ObstacleStates state;

    // ----- PREFABS -----

    [SerializeField] private GameObject projectileSmallPrefab = null;
    [SerializeField] private GameObject projectileBigPrefab = null;
    [SerializeField] private GameObject projectileFlawedPrefab = null;
    [SerializeField] private GameObject projectileWallPrefab = null;

    // ----- WAVES -----

    [SerializeField] private int[] obstaclesPerWave = { };
    [SerializeField] private float[] spawnGapsPerWave = { };

    private int spawnedObstacles_Total;
    private int spawnedObstacles_CurrentWave;

    private int currentWave;
    private int totalObstacles_CurrentWave;
    private float spawnGap_CurrentWave;

    private int totalObstacles_PerBossGap_CurrentWave;
    private int totalShotsPerBoss_CurrentWave;

    private readonly int increaseConstant_ObstaclesPerBossGap = 0;//5;
    private readonly int increaseConstant_ShotsPerBoss = 0;//5;

    private readonly int maxObstaclesPerBossGap = 40;
    private readonly int maxShotsPerBoss = 20;

    private bool minimumSpawnGap_WasReached;

    // ----- OTHERS -----

    // Previous spawn time
    private float previousSpawnTime = 0;

    // Obstacle pool size
    private readonly int obstaclePoolSize = 15;

    // Debug variable for viewing data on screen
    [SerializeField] private Text currentSpawnCooldown_Text = null;

    // Random
    System.Random rnd;

    // ----- HERO DIRECTION -----

    private int heroDirectionX;

    #endregion

    // ---------- INITIALIZATIONS ----------------------------------------------------+

    #region INITIALIZE
    public void Initialize(float _spawnAndExit_AbsolutePositionX, float _minionsSpawnPositionY,
        float _obstacleSpeed, Vector2 _deathDragForce)
    {
        // Initialize state
        state = ObstacleStates.SERENITY;

        // Initialize random
        int seed = (int)System.DateTime.Now.Ticks & 0x0000FFFF;
        rnd = new System.Random(seed);

        // Note: obstacle's deathDragForce is half of the one for the map

        // Initialize projectilesParent
        obstaclesParent = new GameObject();
        obstaclesParent.name = "Obstacles";

        // Initialize obstacles
        InitializeObstacles(_spawnAndExit_AbsolutePositionX, _minionsSpawnPositionY, _obstacleSpeed, _deathDragForce);

        // Initialize projectile walls
        InitializeProjectileWalls();
    }
    #endregion

    #region INITIALIZE OBSTACLES
    private void InitializeObstacles(float _spawnAndExit_AbsolutePositionX, float _minionsSpawnPositionY,
        float _obstacleSpeed, Vector2 _deathDragForce)
    {
        // ----- INITIALIZE PROJECTILES AND MINION POOLS -----

        smallProjectilePool = new ProjectileController[obstaclePoolSize];
        bigProjectilePool = new ProjectileController[obstaclePoolSize];

        for (int i = 0; i < obstaclePoolSize; i++)
        {
            // ----- INITIALIZATIONS -----

            // Small projectile
            GameObject projectile = Instantiate(projectileSmallPrefab);
            smallProjectilePool[i] = projectile.GetComponent<ProjectileController>();
            smallProjectilePool[i].Initialize(obstaclesParent, _spawnAndExit_AbsolutePositionX, _obstacleSpeed, 
                _deathDragForce, false, false);

            // Subscribe to event
            smallProjectilePool[i].ProjectileHitFiringArea += ShowHiddenProjectiles;

            // Big projectile
            projectile = Instantiate(projectileBigPrefab);
            bigProjectilePool[i] = projectile.GetComponent<ProjectileController>();
            bigProjectilePool[i].Initialize(obstaclesParent, _spawnAndExit_AbsolutePositionX, _obstacleSpeed,
                _deathDragForce, true, false);

            // Subscribe to event
            bigProjectilePool[i].ProjectileHitFiringArea += ShowHiddenProjectiles;
        }

        // ----- INITIALIZE FLAWED PROJECTILE -----

        // Initialize
        GameObject flawed = Instantiate(projectileFlawedPrefab);
        flawedProjectile = flawed.GetComponent<ProjectileController>();
        flawedProjectile.Initialize(obstaclesParent, _spawnAndExit_AbsolutePositionX, _obstacleSpeed,
            _deathDragForce, false, true);

        // Subscribe to events
        flawedProjectile.ProjectileHitFiringArea += ShowHiddenProjectiles;
        flawedProjectile.InputDenierWasHit += RespondTo_InputDenierWasHit_Event;
        flawedProjectile.FlawedProjectile_CollidedWithHero += RespondTo_FlawedProjectile_CollidedWithHero_Event;

        // ----- OTHER INITIALIZATIONS -----

        // Initialize current projectile list
        currentProjectiles = new List<ProjectileController>();

        // Initialize current projectile groups list
        currentProjectileGroups = new List<List<ProjectileController>>();

        // Initialize projectile group selector
        obstacleGroupSelector = gameObject.GetComponent<ObstacleGroupSelector>();
        obstacleGroupSelector.Initialize(_minionsSpawnPositionY);
    }
    #endregion

    #region INITIALIZE PROJECTILE WALLS
    private void InitializeProjectileWalls()
    {
        // Initialize array
        obstacleWalls = new ProjectileWallController[obstaclePoolSize];

        for (int i = 0; i < obstaclePoolSize; i++)
        {
            // ----- INITIALIZATIONS -----

            GameObject wall = Instantiate(projectileWallPrefab);
            obstacleWalls[i] = wall.GetComponent<ProjectileWallController>();
            obstacleWalls[i].Initialize(obstaclesParent);
        }
    }
    #endregion

    #region RESET VALUES FOR NEW GAME
    public void ResetValuesForNewGame()
    {
        // Set previous spawn time
        previousSpawnTime = Time.time;

        // Reset current spawn cooldown
        //obstacleSpawnGap = initial_ObstacleSpawnGap;

        // Reset "previous group types" list
        obstacleGroupSelector.ResetPreviousGroupTypes();

        // ----- RESET WAVES -----

        spawnedObstacles_Total = 0;
        spawnedObstacles_CurrentWave = 0;

        currentWave = 0;
        totalObstacles_CurrentWave = obstaclesPerWave[currentWave];
        spawnGap_CurrentWave = spawnGapsPerWave[currentWave];

        totalObstacles_PerBossGap_CurrentWave = 0;
        totalShotsPerBoss_CurrentWave = 0;

        minimumSpawnGap_WasReached = false;

        // ----- RESET DEBUG VARIABLE -----

        currentSpawnCooldown_Text.text = "Spawn Cooldown: " + spawnGap_CurrentWave.ToString("F2");
    }
    #endregion

    // ---------- DIRECTION ----------------------------------------------------+

    #region SET DIRECTION
    public void SetDirection(int _heroDirectionX)
    {
        // ----- OBSTACLES -----

        // Set direction on small projectile
        for (int i = 0; i < smallProjectilePool.Length; i++)
        {
            smallProjectilePool[i].SetDirection(_heroDirectionX);
        }

        // Set direction on big rojectiles
        for (int i = 0; i < bigProjectilePool.Length; i++)
        {
            bigProjectilePool[i].SetDirection(_heroDirectionX);
        }

        // Set direction on flawed projectile
        flawedProjectile.SetDirection(_heroDirectionX);

        // Set direction on rojectile walls
        for (int i = 0; i < obstacleWalls.Length; i++)
        {
            obstacleWalls[i].SetDirection(_heroDirectionX);
        }

        // ----- STORE DIRECTION -----

        heroDirectionX = _heroDirectionX;
    }
    #endregion

    #region FLIP DIRECTION, STOP MAP TRANSITIONING
    public void FlipDirection()
    {
        // ----- OBSTACLES -----

        // Set direction on small projectiles
        for (int i = 0; i < smallProjectilePool.Length; i++)
        {
            smallProjectilePool[i].FlipDirection();
        }

        // Set direction on big projectiles
        for (int i = 0; i < bigProjectilePool.Length; i++)
        {
            bigProjectilePool[i].FlipDirection();
        }

        // Set direction on flawed projectile
        flawedProjectile.FlipDirection();

        // Set direction on projectile walls
        for (int i = 0; i < obstacleWalls.Length; i++)
        {
            obstacleWalls[i].FlipDirection();
        }

        // ----- DIRECTION -----

        heroDirectionX = heroDirectionX * (-1);
    }
    #endregion

    // ---------- UPDATE ----------------------------------------------------+

    #region UPDATE OBSTACLES
    public void UpdateObstacles()
    {
        // ----- DEACTIVATE PROJECTILES -----

        DeactivateObstacles(false);

        // ----- SPAWN ANYTHING -----

        SpawnAnything();
    }
    #endregion

    // ---------- SPAWNS ----------------------------------------------------+

    #region SPAWN ANYTHING
    private void SpawnAnything()
    {
        if (Time.time - previousSpawnTime > spawnGap_CurrentWave)
        {
            // ----- SPAWN BOSS -----

            if (state == ObstacleStates.BOSS_NEAR)
            {
                // Fire event: Boss is ready to spawn
                BossIsReadyToSpawn?.Invoke();

                // Set state as "boss entering"
                state = ObstacleStates.BOSS_ENTERING;
            }

            // ----- SPAWN OBSTACLE -----

            if ((state == ObstacleStates.SPAWNING) || (state == ObstacleStates.BOSS_FIRING) ||
                (state == ObstacleStates.BOSS_FIRING_JAMMED_GUN))
            {
                // Spawn obstacle
                SpawnObstacleFromPool();

                //Debug.Log(Time.time);

                // Set previous spawn time
                previousSpawnTime = Time.time;

                // Fire projectile was spawned event, only if the boss is on screen
                //if (state == ObstacleStates.BOSS_FIRING)
                if ((state == ObstacleStates.BOSS_FIRING) || (state == ObstacleStates.BOSS_FIRING_JAMMED_GUN))
                {
                    // Get boss firing position
                    float firingPosition = obstacleGroupSelector.GetBossFiringPositionY();

                    // Fire event: Projectile was spawned
                    ProjectileWasSpawned?.Invoke(firingPosition);
                }

                // Increment dificulty
                IncrementDifficulty();
            }
        }
    }
    #endregion

    #region SPAWN OBSTACLE FROM POOL
    private void SpawnObstacleFromPool()
    {
        bool bossIsFiring = (state == ObstacleStates.BOSS_FIRING);
        bool bossIsFiringJammedGun = (state == ObstacleStates.BOSS_FIRING_JAMMED_GUN);

        // Create temporary list in case the obstacles spawned are projectiles (not minion)
        List<ProjectileController> recentProjectiles = new List<ProjectileController>();

        // Select group
        obstacleGroupSelector.SelectGroup(currentWave, bossIsFiring, bossIsFiringJammedGun);

        // Get number of projectiles in group
        int totalProjectilesInGroup = obstacleGroupSelector.GetTotalObstacles();

        for (int a = 0; a < totalProjectilesInGroup; a++)
        {
            // ----- GET PROJECTILE DATA FROM GROUP -----

            int type = obstacleGroupSelector.GetObstacleType(a);
            Vector2 projectilePosition = obstacleGroupSelector.GetProjectilePosition(a);

            // ----- IF OBSTACLE IS SMALL PROJECTILE -----

            if (type == 0)
            {
                // Loop pool
                for (int b = 0; b < smallProjectilePool.Length; b++)
                {
                    if (!smallProjectilePool[b].CheckIf_ItsActive())
                    {
                        // Spawn projectile
                        smallProjectilePool[b].Spawn(projectilePosition, GetAnimatorController(true), bossIsFiring);

                        // Add new projectile to list
                        currentProjectiles.Add(smallProjectilePool[b]);

                        // Add new projectile to temporary list
                        recentProjectiles.Add(smallProjectilePool[b]);

                        // Spawn projectile wall
                        if (a == 0)
                        {
                            SpawnProjectileWall(smallProjectilePool[b].gameObject);
                        }

                        break;
                    }
                }
            }

            // ----- IF OBSTACLE IS BIG PROJECTILE -----
            
            else if (type == 1)
            {
                // Loop pool
                for (int b = 0; b < bigProjectilePool.Length; b++)
                {
                    if (!bigProjectilePool[b].CheckIf_ItsActive())
                    {
                        // Spawn projectile
                        bigProjectilePool[b].Spawn(projectilePosition, GetAnimatorController(false), bossIsFiring);

                        // Add new projectile to list
                        currentProjectiles.Add(bigProjectilePool[b]);

                        // Add new projectile to temporary list
                        recentProjectiles.Add(bigProjectilePool[b]);

                        // Spawn projectile wall
                        if (a == 0)
                        {
                            SpawnProjectileWall(bigProjectilePool[b].gameObject);
                        }

                        break;
                    }
                }
            }

            // ----- IF OBSTACLE IS MINION -----

            else if (type == 2)
            {
                
            }

            // ----- IF OBSTACLE IS FLAWED PROJECTILE -----

            else
            {
                // Spawn
                flawedProjectile.Spawn(projectilePosition);
            }
        }

        // ----- ADD RECENT PROJECTILES TO "CURRENT PROJECTILE GROUPS" -----

        currentProjectileGroups.Add(recentProjectiles);
    }
    #endregion

    #region SPAWN PROJECTILE WALL
    private void SpawnProjectileWall(GameObject _projectileParent)
    {
        for (int i = 0; i < obstacleWalls.Length; i++)
        {
            // If this wall is not active
            if (!obstacleWalls[i].CheckIf_ItsActive())
            {
                // Spawn wall
                obstacleWalls[i].Spawn(_projectileParent);

                break;
            }
        }
    }
    #endregion

    // ---------- SHOW HIDDEN PROJECTILES ----------------------------------------------------+

    #region SHOW HIDDEN PROJECTILES
    private void ShowHiddenProjectiles(GameObject _firstProjectile)
    {
        // Local flag
        bool oneProjectileIsBig = false;

        // ----- IF BOSS IS VULNERABLE -----

        if (state == ObstacleStates.BOSS_VULNERABLE)
        {
            // Show hidden flawed projectile
            flawedProjectile.ShowHidden_ChangeSpeed();

            // Fire event: Boss fired jammed gun
            BossFired_JammedGun?.Invoke();
        }

        // ----- IF BOSS IS NOT VULNERABLE -----

        else if (state != ObstacleStates.SERENITY)
        {
            // Local flag
            bool secondProjectileIsBig = false;

            // Loop "current projectile groups" list
            for (int i = 0; i < currentProjectileGroups.Count; i++)
            {
                // Local flag
                bool firstProjectileWasFound = false;

                // Loop projectiles in current group
                for (int j = 0; j < currentProjectileGroups[i].Count; j++)
                {
                    // If projectile is found: Show all hidden projectiles at the same time
                    if (currentProjectileGroups[i][j].gameObject == _firstProjectile)
                    //if (GameObject.ReferenceEquals(currentProjectileGroups[i][j], _firstProjectile))
                    {
                        // ----- VALIDATE THAT "IF FIRST PROJECTILE IS SMALL -> SECOND PROJECTILE IS NOT BIG" -----

                        // Check if first projectile is small
                        if (!currentProjectileGroups[i][j].CheckIf_ItsBig())
                        {
                            // Loop same cycle as j
                            for (int k = 0; k < currentProjectileGroups[i].Count; k++)
                            {
                                // Check if second projectile is big
                                if ((k != j) && currentProjectileGroups[i][k].CheckIf_ItsBig())
                                {
                                    secondProjectileIsBig = true;
                                    break;
                                }
                            }
                        }

                        if (secondProjectileIsBig)
                        {
                            break;
                        }

                        // ----- SHOW HIDDEN PROJECTILES IF THE VALIDATION WAS CLEARED -----

                        // Loop same cycle as j
                        for (int k = 0; k < currentProjectileGroups[i].Count; k++)
                        {
                            // Show hidden projectile when fired by boss
                            currentProjectileGroups[i][k].ShowHidden_ChangeTag();

                            // Set local flag
                            if (currentProjectileGroups[i][k].CheckIf_ItsBig())
                            {
                                oneProjectileIsBig = true;
                            }
                        }

                        firstProjectileWasFound = true;
                        break;
                    }
                }

                if (firstProjectileWasFound || secondProjectileIsBig)
                {
                    break;
                }
            }

            if (!secondProjectileIsBig)
            {
                // Fire event: Boss fired
                BossFired?.Invoke(oneProjectileIsBig);
            }
        }
    }
    #endregion

    // ---------- INCREMENT DIFFICULTY ----------------------------------------------------+

    #region INCREMENT DIFFICULTY
    private void IncrementDifficulty()
    {
        if ((state == ObstacleStates.SPAWNING) || (state == ObstacleStates.BOSS_FIRING))
        {
            // Increase total spawned obstacles
            spawnedObstacles_Total++;

            // Increase spawned obstacles on current wave
            spawnedObstacles_CurrentWave++;

            // Local flag
            bool waveChanged = false;

            // ----- IF MINIMUM "SPAWN GAP" WAS REACHED -----

            if (minimumSpawnGap_WasReached)
            {
                // 
                if ((state == ObstacleStates.SPAWNING) &&
                    (spawnedObstacles_CurrentWave >= totalObstacles_PerBossGap_CurrentWave))
                {
                    // 
                    waveChanged = true;

                    // Increase wave
                    currentWave++;

                    // Reset spawned obstacles
                    spawnedObstacles_CurrentWave = 0;

                    // If shots per boss can increase
                    if (totalShotsPerBoss_CurrentWave < maxShotsPerBoss)
                    {
                        // Set total shots for current wave
                        totalShotsPerBoss_CurrentWave =
                            totalShotsPerBoss_CurrentWave + increaseConstant_ShotsPerBoss;

                        //Debug.Log("W: " + (currentWave + 1) + ". O: " + totalShotsPerBoss_CurrentWave);
                    }
                }

                //
                else if ((state == ObstacleStates.BOSS_FIRING) &&
                    (spawnedObstacles_CurrentWave >= totalShotsPerBoss_CurrentWave))
                {
                    // 
                    waveChanged = true;

                    // Increase wave
                    currentWave++;

                    // Reset spawned obstacles
                    spawnedObstacles_CurrentWave = 0;

                    // If "obstacles per boss gap" can increase
                    if (totalObstacles_PerBossGap_CurrentWave < maxObstaclesPerBossGap)
                    {
                        // Set total obstacles for current wave
                        totalObstacles_PerBossGap_CurrentWave =
                            totalObstacles_PerBossGap_CurrentWave + increaseConstant_ObstaclesPerBossGap;

                        //Debug.Log("W: " + (currentWave + 1) + ". O: " + totalObstacles_PerBossGap_CurrentWave);
                    }
                }
                
            }

            // ----- IF MINIMUM "SPAWN GAP" WAS NOT REACHED -----

            else
            {
                if (spawnedObstacles_CurrentWave >= totalObstacles_CurrentWave)
                {
                    // 
                    waveChanged = true;
                    
                    // ----- INCREASE WAVE -----

                    // Increase wave
                    currentWave++;

                    // Reset spawned obstacles
                    spawnedObstacles_CurrentWave = 0;

                    // ----- IF MINIMUM "SPAWN GAP" WAS REACHED -----

                    if (currentWave >= obstaclesPerWave.Length)
                    {
                        // If next state is "spawning"
                        if (state == ObstacleStates.BOSS_FIRING)
                        {
                            // Set obstacles current wave (+increment)
                            totalObstacles_PerBossGap_CurrentWave =
                                obstaclesPerWave[currentWave - 2] + increaseConstant_ObstaclesPerBossGap;

                            // Set obstacles current wave
                            totalShotsPerBoss_CurrentWave = obstaclesPerWave[currentWave - 1];

                            Debug.Log("W: " + (currentWave + 1) + ". O: " + totalObstacles_PerBossGap_CurrentWave);
                        }

                        // If next state is "boss entering"
                        else if (state == ObstacleStates.SPAWNING)
                        {
                            // Set obstacles current wave (+increment)
                            totalShotsPerBoss_CurrentWave =
                                obstaclesPerWave[currentWave - 2] + increaseConstant_ShotsPerBoss;

                            // Set obstacles current wave 
                            totalObstacles_PerBossGap_CurrentWave = obstaclesPerWave[currentWave - 1];

                            Debug.Log("W: " + (currentWave + 1) + ". O: " + totalShotsPerBoss_CurrentWave);
                        }

                        // Set flag
                        minimumSpawnGap_WasReached = true;
                    }

                    // ----- IF MINIMUM "SPAWN GAP" WAS NOT REACHED -----

                    else 
                    {
                        //Debug.Log(totalObstacles_CurrentWave + ", " + currentWave + ", " + obstaclesPerWave.Length);
                        
                        // Set obstacles current wave
                        totalObstacles_CurrentWave = obstaclesPerWave[currentWave];

                        // Set current spawn gap
                        spawnGap_CurrentWave = spawnGapsPerWave[currentWave];

                        //Debug.Log("W: " + (currentWave + 1 ) + ". O: " + totalObstacles_CurrentWave);

                        // Update debug variable
                        currentSpawnCooldown_Text.text = "Spawn Cooldown: " + spawnGap_CurrentWave.ToString("F2");
                    }
                }
            }

            // ----- CHANGE STATE -----

            if (waveChanged)
            {
                // If spawning, change state to boss near
                if (state == ObstacleStates.SPAWNING)
                {
                    state = ObstacleStates.BOSS_NEAR;
                }

                // If boss firing, change jammed gun
                else if (state == ObstacleStates.BOSS_FIRING)
                {
                    // Set state as "boss firing jammed gun"
                    state = ObstacleStates.BOSS_FIRING_JAMMED_GUN;
                }

                // Fire event: Current wave has changed
                CurrentWave_HasChanged?.Invoke(currentWave + 1);
            }
        }

        else if (state == ObstacleStates.BOSS_FIRING_JAMMED_GUN)
        {
            // ----- CHANGE STATE -----

            // Set state as "boss vulnerable"
            state = ObstacleStates.BOSS_VULNERABLE;

            // Fire event: Boss is vulnerable
            BossIsVulnerable?.Invoke();
        }
    }
    #endregion

    // ---------- ASDF ----------------------------------------------------+

    #region RESPOND TO "INPUT DENIER WAS HIT" EVENT
    private void RespondTo_InputDenierWasHit_Event()
    {
        // Fire event: Input denier was hit
        InputDenierWasHit?.Invoke();
    }
    #endregion

    #region RESPOND TO "FLAWED PROJECTILE COLLIDED WITH HERO" EVENT
    private void RespondTo_FlawedProjectile_CollidedWithHero_Event()
    {
        FlawedProjectile_CollidedWithHero?.Invoke();
    }
    #endregion

    // ---------- DEACTIVATES ----------------------------------------------------+

    #region CLEAR OBSTACLES
    // Used when restarting game
    public void ClearObstacles()
    {
        DeactivateObstacles(true);
    }
    #endregion

    #region DEACTIVATE OBSTACLES
    private void DeactivateObstacles(bool _newGame)
    {
        // ----- DEACTIVATE PROJECTILES -----

        // Loop CURRENT PROJECTILES list in reverse in order to remove elements safely
        for (int i = currentProjectiles.Count - 1; i >= 0; i--)
        {
            // If projectile is eligible for deactivating
            if ( _newGame ||
                (currentProjectiles[i].CheckIf_ItsActive() && currentProjectiles[i].CheckIf_ItsGone()) )
            {
                // ----- REMOVE FROM "CURRENT PROJECTILE GROUPS" -----

                // Loop "current projectile groups" list
                for (int a = currentProjectileGroups.Count - 1; a >= 0; a--)
                {
                    // Loop projectiles in current group
                    for (int b = currentProjectileGroups[a].Count - 1; b >= 0; b--)
                    {
                        // If projectile is found
                        if (currentProjectileGroups[a][b] == currentProjectiles[i])
                        {
                            // Remove from list
                            currentProjectileGroups[a].RemoveAt(b);
                        }
                    }

                    // Remove child list from parent list if child list is empty
                    if (currentProjectileGroups[a].Count == 0)
                    {
                        currentProjectileGroups.RemoveAt(a);
                    }
                }

                // ----- DEACTIVATE -----

                // Deactivate obstacle wall child
                DeactivateObstacleWallChild(currentProjectiles[i].gameObject);

                // Deactivate projectile
                currentProjectiles[i].Deactivate();

                // ----- REMOVE FROM "CURRENT PROJECTILES" -----

                // Remove from list
                currentProjectiles.RemoveAt(i);
            }
        }

        // ----- DEACTIVATE MINIONS -----


        // ----- DEACTIVATE FLAWED PROJECTILES -----

        if (_newGame || (flawedProjectile.CheckIf_ItsActive() && flawedProjectile.CheckIf_ItsGone()))
        {
            flawedProjectile.Deactivate();
        }
    }
    #endregion

    #region DEACTIVATE OBSTACLE WALL CHILD
    private void DeactivateObstacleWallChild(GameObject _projectile)
    {
        for (int i = 0; i < obstacleWalls.Length; i++)
        {
            // If the osbtacle is this wall's parent
            if (_projectile == obstacleWalls[i].GetParent())
            {
                // Deactivate wall and remove parent
                obstacleWalls[i].Deactivate(obstaclesParent);

                break;
            }
        }
    }
    #endregion

    // ---------- STATES ----------------------------------------------------+

    #region SET STATE AS SERENITY
    public void SetStateAs_Serenity()
    {
        // Set state
        state = ObstacleStates.SERENITY;
    }
    #endregion

    #region SET STATE AS SPAWNING
    public void StartSpawningState()
    {
        // Set state
        state = ObstacleStates.SPAWNING;
    }
    #endregion

    /*
    #region SET STATE AS BOSS NEAR
    public void SetStateAs_BossNear()
    {
        // Set state
        state = ObstacleStates.BOSS_NEAR;
    }
    #endregion
    */

    #region SET STATE AS BOSS FIRING
    public void SetStateAs_BossFiring()
    {
        // Set state
        state = ObstacleStates.BOSS_FIRING;
    }
    #endregion

    /*
    #region SET STATE AS BOSS FIRING  JAMMED GUN
    public void SetStateAs_BossFiring_JammedGun()
    {
        // Set state
        state = ObstacleStates.BOSS_FIRING_JAMMED_GUN;
    }
    #endregion
    */

    // ---------- DEAD HERO ----------------------------------------------------+

    #region START DRAGGING
    public void StartDragging()
    {
        // Start dragging projectiles
        for (int i = 0; i < currentProjectiles.Count; i++)
        {
            currentProjectiles[i].StartDragging();
        }
    }
    #endregion

    #region STOP DRAGGING
    public void StopDragging()
    {
        // Stop dragging projectiles
        for (int i = 0; i < currentProjectiles.Count; i++)
        {
            currentProjectiles[i].StopDragging();
        }
    }
    #endregion

    // ---------- ANIMATOR CONTROLLERS ----------------------------------------------------+

    #region GET ANIMATOR CONTROLLER
    private RuntimeAnimatorController GetAnimatorController(bool _enemyIsSmall)
    {
        // Get random animator controler from the SMALL pool
        if (_enemyIsSmall)
        {
            // Get random controler index
            int selectedAnimatorIndex = rnd.Next(0, animatorControllerPool_ProjectileSmall.Length);

            // Return controller
            return animatorControllerPool_ProjectileSmall[selectedAnimatorIndex];
        }

        // Get random controller from the BIG pool
        else
        {
            // Get random controller index
            int selectedAnimatorIndex = rnd.Next(0, animatorControllerPool_ProjectileBig.Length);

            // Return controller
            return animatorControllerPool_ProjectileBig[selectedAnimatorIndex];
        }
    }
    #endregion
}
