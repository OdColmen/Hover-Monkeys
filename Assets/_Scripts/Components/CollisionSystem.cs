using UnityEngine;

public class CollisionSystem : MonoBehaviour
{
	#region VARIABLES

    private BossManager bossManager;
    private ProjectileController projectileController;

    private MapManager mapManager;
    private HeroManager heroManager;

    private bool isBigProjectile;

    #endregion

    #region INITIALIZE PROJECTILE
    public void InitializeProjectile(ProjectileController _projectileController, bool _isBig)
    {
        projectileController = _projectileController;
        isBigProjectile = _isBig;
    }
    #endregion

    #region INITIALIZE BOSS
    public void InitializeBoss(BossManager _bossManager)
    {
        bossManager = _bossManager;
    }
    #endregion

    #region INITIALIZE MAP MANAGER
    public void InitializeMapManager(MapManager _mopMonogor)
    {
        mapManager = _mopMonogor;
    }
    #endregion

    #region INITIALIZE HERO
    public void InitializeHero(HeroManager _heroManager)
    {
        heroManager = _heroManager;
    }
    #endregion

    #region ON TRIGGER ENTER 2D
    public void OnTriggerEnter2D(Collider2D col)
    {
        // ----- HERO - PROJECTILE -----

        // Colision hero->projectile
        if (gameObject.CompareTag("Hero") && col.gameObject.CompareTag("Projectile"))
        {
            heroManager.ReactTo_ProjectileCollision();
        }

        // Colision projectile->hero
        if (gameObject.CompareTag("Projectile") && col.gameObject.CompareTag("Hero"))
        {
            projectileController.ReactTo_HeroCollision();
        }

        // ----- HERO - PROJECTILE 2 -----

        // Note: projectile 2's are projectiles being fired by boss that are "unhidden"
        // This tag was created to make sure each projectile got unhidden only once
        // (so there wouldn't be more sfx than needed)

        // Colision hero->"projectile 2"
        if (gameObject.CompareTag("Hero") && col.gameObject.CompareTag("Projectile 2"))
        {
            heroManager.ReactTo_ProjectileCollision();
        }

        // Colision "projectile 2"->hero
        if (gameObject.CompareTag("Projectile 2") && col.gameObject.CompareTag("Hero"))
        {
            projectileController.ReactTo_HeroCollision();
        }

        // ----- HERO - ENEMY -----

        // Colision hero->enemy
        if (gameObject.CompareTag("Hero") && col.gameObject.CompareTag("Enemy"))
        {
            //heroManager.ReactTo_FlawedProjectile_Collision();
        }

        // ----- HERO - PROJECTILE WALL -----

        // Collision hero->"projectile wall"
        if (gameObject.CompareTag("Hero") && col.gameObject.CompareTag("Projectile Wall"))
        {
            heroManager.ReactTo_ProjectileWallCollision(col.gameObject);
        }

        // ----- PROJECTILE - BOSS -----

        // Collision "projectile returning"->boss
        if (gameObject.CompareTag("Projectile Returning") && col.gameObject.CompareTag("Boss"))
        {
            projectileController.ReactTo_BossCollision();
        }

        // Collision boss->"projectile returning"
        if (gameObject.CompareTag("Boss") && col.gameObject.CompareTag("Projectile Returning"))
        {
            bossManager.ReactTo_ReturnedShotCollision();
        }

        // ----- PROJECTILE - ENEMY FIRING AREA (1 & 2) -----

        // Collision projectile->"enemy firing area"
        if (gameObject.CompareTag("Projectile") && col.gameObject.CompareTag("Enemy Firing Area"))
        {
            if (!isBigProjectile)
            {
                projectileController.ReactTo_FiringAreaCollision();
            }            
        }

        // Collision projectile->"enemy firing area 2"
        if (gameObject.CompareTag("Projectile") && col.gameObject.CompareTag("Enemy Firing Area 2"))
        {
            if (isBigProjectile)
            {
                projectileController.ReactTo_FiringAreaCollision();
            }
        }

        // ----- PROJECTILE - INPUT DENIER-----

        // Collision projectile->"input denier"
        if (gameObject.CompareTag("Projectile") && col.gameObject.CompareTag("Input Denier"))
        {
            projectileController.ReactTo_Projectile_InputDenier_Collision();
        }

        // ----- BACKGROUND LOOP CHECKER - BACKGROUND -----

        // Collision "background loop checker"->background
        if (gameObject.CompareTag("Background Loop Checker") && col.gameObject.CompareTag("Background"))
        {
            mapManager.LoopCheckerRoadsBg_ReactTo_RoadCollision(col.gameObject);
        }

        // ----- BG LOOP CHECKER CITY - BG CITY (1 & 2) -----

        // Collision "backgroung loop checker city"->"bg city"
        if (gameObject.CompareTag("Background Loop Checker City") && col.gameObject.CompareTag("Bg City"))
        {
            //mapManager.HorizontalLoopCheckerCityBg_ReactTo_CityCollision(col.gameObject);
        }

        // Collision "backgroung loop checker city 2"->"bg city"
        if (gameObject.CompareTag("Background Loop Checker City 2") && col.gameObject.CompareTag("Bg City"))
        {
            //mapManager.VerticalLoopCheckerCityBg_ReactTo_CityCollision(col.gameObject);
        }

        // ----- HERO - LANDING AREA -----

        // Collision hero->"landing area"
        if (gameObject.CompareTag("Hero") && col.gameObject.CompareTag("Landing Area"))
        {
            heroManager.Hero_ReactTo_LandingAreaCollision();
        }

        // ----- HERO - OFF LIMIT WALLS -----

        // Colision hero->"bg off limit walls"
        if ((gameObject.CompareTag("Hero") || gameObject.CompareTag("Hero Dying"))
            && col.gameObject.CompareTag("Bg Off Limit Walls"))
        {
            heroManager.ReactTo_OffLimitWallsCollision();
        }

        // ----- BOSS - OFF LIMIT WALLS -----

        // Collision boss->"bg off limit walls"
        if (gameObject.CompareTag("Boss") && col.gameObject.CompareTag("Bg Off Limit Walls"))
        {
            bossManager.ReactTo_OffLimitWallsCollision(true, false, false);
        }

        // Collision "boss gun"->"bg off limit walls"
        if (gameObject.CompareTag("Boss Gun") && col.gameObject.CompareTag("Bg Off Limit Walls"))
        {
            bossManager.ReactTo_OffLimitWallsCollision(false, true, false);
        }

        // Collision "boss hoverboard"->"bg off limit walls"
        if (gameObject.CompareTag("Boss Hoverboard") && col.gameObject.CompareTag("Bg Off Limit Walls"))
        {
            bossManager.ReactTo_OffLimitWallsCollision(false, false, true);
        }
    }
    #endregion

    #region ON COLLISION ENTER 2D
    public void OnCollisionEnter2D(Collision2D col)
    {
        // Colision hero->floor
        if ((gameObject.CompareTag("Hero") || gameObject.CompareTag("Hero Dying")) 
            && col.gameObject.CompareTag("Floor"))
        {
            heroManager.ReactTo_FloorCollision();
        }

        // Colision "hero dying"->floor
        else if (gameObject.CompareTag("Hero Dying") && col.gameObject.CompareTag("Floor"))
        {
            heroManager.ReactTo_FloorCollision();
        }
    }
    #endregion
}
