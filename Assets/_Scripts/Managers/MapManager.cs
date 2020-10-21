using UnityEngine;

public class MapManager : MonoBehaviour
{
	#region VARIABLES

	// ----- EVENTS -----

	// Hero default state started
	public delegate void HeroDefaultStateStarted_EventHandler();
	public event HeroDefaultStateStarted_EventHandler HeroDefaultStateStarted;

	// Map stopped dragging
	public delegate void MapStoppedDragging_EventHandler();
	public event MapStoppedDragging_EventHandler MapStoppedDragging;

	// Curve was entered
	public delegate void CurveWasEntered_EventHandler();
	public event CurveWasEntered_EventHandler CurveWasEntered;

	// Direction has changed
	public delegate void DirectionHasChanged_EventHandler();
	public event DirectionHasChanged_EventHandler DirectionHasChanged;

	// Turning animation is on
	public delegate void TurningAnimationIsOn_EventHandler();
	public event TurningAnimationIsOn_EventHandler TurningAnimationIsOn;

	// Turning animation is off
	public delegate void TurningAnimationIsOff_EventHandler();
	public event TurningAnimationIsOff_EventHandler TurningAnimationIsOff;

	// ----- PREFABS -----

	// Background sections
	[SerializeField] private GameObject IntroRoad1Prefab = null;
	[SerializeField] private GameObject IntroRoad2Prefab = null;
	[SerializeField] private GameObject IntroRoad3Prefab = null;
	[SerializeField] private GameObject IntroRoad4Prefab = null;
	[SerializeField] private GameObject standardRoadPrefab = null;
	[SerializeField] private GameObject curvedRoad1Prefab = null;
	[SerializeField] private GameObject curvedRoad2Prefab = null;

	// Visible gameplay area
	[SerializeField] private GameObject visibleGameplayAreaPrefab = null;

	// Invisible ground
	[SerializeField] private GameObject invisibleGroundPrefab = null;

	// Loop checker for the road
	[SerializeField] private GameObject loopCheckerRoadBgsPrefab = null;

	// Landing area
	[SerializeField] private GameObject landingAreaPrefab = null;

	// ----- BACKGROUND SECTION STRUCT -----

	public struct BgSectionStruct
	{
		public GameObject obj;
		public Rigidbody2D rigidbody;
		public SpriteRenderer renderer;
		public float width;
		public float height;

		public BgSectionStruct(GameObject _obj, Rigidbody2D _rigidbody, float _width, float _height)
		{
			obj = _obj;
			rigidbody = _rigidbody;

			renderer = _obj.GetComponent<SpriteRenderer>();

			width = _width;
			height = _height;
		}
	}

	// ----- GAME OBJECTS -----

	// Road's parent
	GameObject bgParent;

	// Intro road. Curved road used only at beggining of each match
	private BgSectionStruct introRoad1;
	private BgSectionStruct introRoad2;
	private BgSectionStruct introRoad3;
	private BgSectionStruct introRoad4;

	// Standard road bacground sections
	private BgSectionStruct[] standardRoads;
	
	// Curved roads background sections
	// curvedRoad1: road after finishing the standard roads
	private BgSectionStruct curvedRoad1;
	// curvedRoad2: road before starting the standard roads
	private BgSectionStruct curvedRoad2;

	// Visible gameplay area
	private GameObject visibleGameplayArea;

	// Loop checker for the bg roads
	private GameObject loopCheckerRoadBgs;

	// Landing area
	private GameObject landingArea;

	// ----- MAP STATES -----

	private enum MapStates { SERENITY, HERO_ENTERING, HERO_DEFAULT, HERO_TURNING, HERO_DEAD };
	private static MapStates mapState;

	// ----- ROUTE FOLLOWING -----

	CurvedRouteFollower curvedRouteFollower;
	IntroRouteFollower introRouteFollower;

	[SerializeField] private Transform[] roadCurvedRoute_TurningLeft = null;
	[SerializeField] private Transform[] roadCurvedRoute_TurningRight = null;

	[SerializeField] private Transform[] roadIntroRoute_TurningLeft = null;
	[SerializeField] private Transform[] roadIntroRoute_TurningRight = null;

	// ----- ROAD END RELATED -----

	// introRoadStartingPoint: Initial horizontalPosition of introRoad 
	private float introRoadStartingPoint;
	private float introRoadStartingPoint_AbsoluteValue;

	// ----- ROAD END RELATED -----

	// curvedRoadStoppingPoint_ForHeroApproachingCurve: 
	// - It's curvedRoad1's horizontal limit for the end of the floor (when hero starts approaching curve).
	// - Once reached, the floor ends
	private float curvedRoadStoppingPoint_ForHeroApproachingCurve;
	private float curvedRoadStoppingPoint_ForHeroApproachingCurve_AbsoluteValue;

	// ----- OTHER -----

	// Default vertical postition for the backgrounds
	private readonly float bgDefaultPositionY = 0;

	// Game speed
	private float speed;

	// Death drag force
	private Vector2 deathDragForce;

	// Drag allowed
	private bool dragAllowed;

	// ----- HERO DIRECTION -----

	private int heroDirectionX;

	#endregion

	// ---------- MAIN INITIALIZATIONS ----------------------------------------------------+

	#region INITIALIZE
	public void Initialize(float _speed, Vector2 _deathDragForce, GameObject _camera)
	{
		// ----- SPEED, FORCE & PARENTS -----

		// Set game speed
		speed = _speed;

		// Set death drag force
		deathDragForce = _deathDragForce;

		// Create parent of road backgrounds
		bgParent = new GameObject("Roads Backgrounds");

		// ----- INITIALIZE INTRO ROADS -----

		// Intro road 1
		GameObject intro1 = Instantiate(IntroRoad1Prefab);
		introRoad1 = new BgSectionStruct();
		InitializeBackgroundImage(ref introRoad1, bgParent.transform, intro1);

		// Intro road 2
		GameObject intro2 = Instantiate(IntroRoad2Prefab);
		introRoad2 = new BgSectionStruct();
		InitializeBackgroundImage(ref introRoad2, bgParent.transform, intro2);

		// Intro road 3
		GameObject intro3 = Instantiate(IntroRoad3Prefab);
		introRoad3 = new BgSectionStruct();
		InitializeBackgroundImage(ref introRoad3, bgParent.transform, intro3);

		// Intro road 4
		GameObject intro4 = Instantiate(IntroRoad4Prefab);
		introRoad4 = new BgSectionStruct();
		InitializeBackgroundImage(ref introRoad4, bgParent.transform, intro4);

		// ----- INITIALIZE STANDARD ROADS -----

		int totalStandardRoads = 7;

		// Initialize Array
		standardRoads = new BgSectionStruct[totalStandardRoads];

		// Loop array
		for (int i = 0; i < standardRoads.Length; i++)
		{
			GameObject standardObj = Instantiate(standardRoadPrefab);
			standardRoads[i] = new BgSectionStruct();
			InitializeBackgroundImage(ref standardRoads[i], bgParent.transform, standardObj);
		}

		// ----- INITIALIZE CURVED ROADS -----

		// Initialize curved road 1
		GameObject curved1 = Instantiate(curvedRoad1Prefab);
		curvedRoad1 = new BgSectionStruct();
		InitializeBackgroundImage(ref curvedRoad1, bgParent.transform, curved1);
		
		// Initialize curved road 2
		GameObject curved2 = Instantiate(curvedRoad2Prefab);
		curvedRoad2 = new BgSectionStruct();
		InitializeBackgroundImage(ref curvedRoad2, bgParent.transform, curved2);

		// ----- INITIALIZE OTHER BACKGROUND OBJECTS -----

		// Create parent of extra elements
		GameObject bgParent2 = new GameObject("Background Extra Elements");
		
		// Initialize visible gameplay area
		visibleGameplayArea = Instantiate(visibleGameplayAreaPrefab);
		// Set parent
		visibleGameplayArea.transform.parent = bgParent2.transform;
		// Set position
		visibleGameplayArea.transform.position = Vector2.zero;

		// Initialize ground
		GameObject invisibleGround = Instantiate(invisibleGroundPrefab);
		// Set parent
		invisibleGround.transform.parent = bgParent2.transform;
		
		// Initialize landing area
		landingArea = Instantiate(landingAreaPrefab);
		// Set parent
		landingArea.transform.parent = bgParent2.transform;
		// Set position
		landingArea.transform.position = new Vector2(0, -8f);

		// ----- INITIALIZE LOOP CHECKER (ROAD BACKGROUNDS) -----

		// Initialize
		loopCheckerRoadBgs = Instantiate(loopCheckerRoadBgsPrefab);
		// Set parent
		loopCheckerRoadBgs.transform.parent = bgParent2.transform;
		// Initialize collision system
		loopCheckerRoadBgs.GetComponent<CollisionSystem>().InitializeMapManager(this);
		// Deactivate?
		loopCheckerRoadBgs.SetActive(true);

		// ----- ???? -----

		SetIntroRoadStartPoint_AbsoluteValue();
		SetCurvedRoadStoppingPoint_AbsoluteValue();

		// ----- INITIALIZE ROUTE FOLLOWERS -----

		// Get "route follower"
		introRouteFollower = bgParent.AddComponent<IntroRouteFollower>();
		// Initialize script
		introRouteFollower.Initialize(roadIntroRoute_TurningLeft, roadIntroRoute_TurningRight, _speed, _camera);

		// Get "route follower"
		curvedRouteFollower = bgParent.AddComponent<CurvedRouteFollower>();

		// Initialize script
		curvedRouteFollower.Initialize(roadCurvedRoute_TurningLeft, roadCurvedRoute_TurningRight, 
			standardRoads[0].height, _speed, _camera);

		// Subscribe to events
		curvedRouteFollower.DirectionHasChanged += RespondTo_DirectionHasChanged_Event;
		curvedRouteFollower.TurningAnimationIsOn += RespondTo_TurningAnimationIsOn_Event;
		curvedRouteFollower.TurningAnimationIsOff += RespondTo_TurningAnimationIsOff_Event;
		introRouteFollower.TurningAnimationIsOff += RespondTo_TurningAnimationIsOff_Event;
	}
	#endregion

	#region INITIALIZE BACKGROUND IMAGE
	private void InitializeBackgroundImage(ref BgSectionStruct _bgSection, Transform _parent, 
		GameObject _newRoad)
	{
		// Set parent
		_newRoad.transform.parent = _parent;

		// ----- CREATE BACKGOUND SECTION -----

		// Get rigidbody
		Rigidbody2D rb = _newRoad.GetComponent<Rigidbody2D>();

		// Get image dimensions
		float width = _newRoad.GetComponent<SpriteRenderer>().bounds.size.x;
		float height = _newRoad.GetComponent<SpriteRenderer>().bounds.size.y;

		// Create bg section
		_bgSection = new BgSectionStruct(_newRoad, rb, width, height);
	}
	#endregion

	#region SET INTRO ROAD STARTING POINT, ABSOLUTE VALUE
	// This method calculates the intro road starting point (absolute value)
	// When the hero is going right, introRoad needs to be aligned with the left side of the screen
	private void SetIntroRoadStartPoint_AbsoluteValue()
	{
		// Get image width
		float introRoadWidth = introRoad1.width;

		// Get screen width
		float screenWidth = GetScreenWidth_InWorldUnits();

		// Get the adjustment distance for aligning the image to the left
		// This is done by first getting the width difference between image and screen,
		// and then getting the half of that value.
		screenWidth = 160;
		float adjustmentDistance = (introRoadWidth - screenWidth) / 2;

		// Set starting point (absolute value)
		// The starting point is the adjustment distance (which is the distance from the center, that
		// the image needs to be moved in order to be aligned to the left)
		introRoadStartingPoint_AbsoluteValue = adjustmentDistance + 55;
	}
	#endregion
	
	#region SET CURVED ROAD STOPPING POINT, ABSOLUTE VALUE
	// This method calculates the curved road (curvedRoad1) stopping point (absolute value)
	//   The stopping point indicates the end of the floor (when hero starts approaching curve).
	// When the hero is going right, curvedRoad1 needs to stop when the left side of the image 
	// is aligned with the left side of the screen
	private void SetCurvedRoadStoppingPoint_AbsoluteValue()
	{
		// Get image width
		float curvedRoadWidth = curvedRoad1.width;

		// Get screen width
		float screenWidth = GetScreenWidth_InWorldUnits();

		// Get the adjustment distance for aligning the image to the left
		// This is done by first getting the width difference between image and screen,
		// and then getting the half of that value.
		screenWidth = 160;
		float adjustmentDistance = (curvedRoadWidth - screenWidth) / 2;

		// Set stopping point (absolute value)
		// The stopping point is the adjustment distance (which is the distance from the center, that
		// the image needs to be moved in order to be aligned to the left)
		curvedRoadStoppingPoint_ForHeroApproachingCurve_AbsoluteValue = adjustmentDistance + 55;
	}
	#endregion

	// ---------- UPDATE ----------------------------------------------------+
	
	#region FIXED UPDATE
	private void FixedUpdate()
	{
		// Do not update if the state is serenity
		if (mapState == MapStates.SERENITY)
		{
			return;
		}

		// ----- CURVE RELATED -----

		// Check if the hero exited the intro road
		CheckIf_IntroRoadWasExited();

		// Check beggining of curve
		CheckIf_CurveWasReached();

		// Check curve exit
		CheckIf_CurveWasExited();

		// ----- MAP STOPPING -----

		// Stop map if possible
		StopMap();
	}
	#endregion

	// ---------- DIRECTION ----------------------------------------------------+

	#region SET DIRECTION
	public void SetDirection(int _heroDirectionX)
	{
		// ----- SET CURVED ROAD STOPPING POINT -----

		// If hero is going right
		if (_heroDirectionX > 0)
		{
			// Set starting point
			introRoadStartingPoint = introRoadStartingPoint_AbsoluteValue;

			// Set stopping point
			// If hero is going RIGHT, the curved road must stop when
			// it's LEFT side is aligned with the LEFT side of the screen
			curvedRoadStoppingPoint_ForHeroApproachingCurve = 
				curvedRoadStoppingPoint_ForHeroApproachingCurve_AbsoluteValue;
		}

		// If hero is going left
		else if (_heroDirectionX < 0)
		{
			// Set starting point
			introRoadStartingPoint = introRoadStartingPoint_AbsoluteValue * (-1);

			// Set stopping point
			// If hero is going LEFT, the curved road must stop when
			// it's RIGHT side is aligned with the RIGHT side of the screen
			curvedRoadStoppingPoint_ForHeroApproachingCurve =
				curvedRoadStoppingPoint_ForHeroApproachingCurve_AbsoluteValue * (-1);
		}

		// If hero has no direction
		else
		{
			Debug.Log("Error: hero has no direction (just like the game developer)");
		}

		// ----- SET GAME OBJECTS' LOCAL SCALES -----

		introRoad1.obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		introRoad2.obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		introRoad3.obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		introRoad4.obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		}
		curvedRoad1.obj.transform.localScale = new Vector2(_heroDirectionX, 1);
		curvedRoad2.obj.transform.localScale = new Vector2(_heroDirectionX, 1);

		// ----- SET LOOP CHECKER ROAD BGS -----

		// Get screen width
		float screenWidth = GetScreenWidth_InWorldUnits();
		screenWidth = 160;

		// Get width
		float width = loopCheckerRoadBgs.GetComponent<SpriteRenderer>().bounds.size.x;
		// Get distance needed, for aligning object to the left
		float distanceToMove_ForLeftAlignment = (screenWidth - width) / 2;
		// Set position, at 7 spaces away from center
		loopCheckerRoadBgs.transform.position = new Vector2(
			distanceToMove_ForLeftAlignment * 10 * _heroDirectionX * (-1), bgDefaultPositionY);

		// ----- RESET CAMERA POSITION -----

		// Reset camera position
		curvedRouteFollower.ResetCameraPosition(_heroDirectionX);
		
		// ----- SET SPEED, FORCE AND HERO DIRECTION -----

		// Set speed
		speed = speed * _heroDirectionX;

		// Set death drag force
		deathDragForce = new Vector2(deathDragForce.x * _heroDirectionX, deathDragForce.y);

		// Set hero direction
		heroDirectionX = _heroDirectionX;
	}
	#endregion

	#region FLIP DIRECTION
	public void FlipDirection()
	{
		// ----- FLIP CURVED ROAD STOPPING POINT -----

		// Flip starting point
		introRoadStartingPoint = introRoadStartingPoint * (-1);

		// Flip stopping point
		curvedRoadStoppingPoint_ForHeroApproachingCurve = curvedRoadStoppingPoint_ForHeroApproachingCurve * (-1);

		// ----- FLIP BACKGROUND LOOP CHECKERS -----

		loopCheckerRoadBgs.transform.position = new Vector2(
			loopCheckerRoadBgs.transform.position.x * (-1), bgDefaultPositionY);

		// ----- FLIP SPEED, FORCE AND HERO DIRECTION -----

		// Flip speed
		speed = speed * (-1);

		// Flip death drag force
		deathDragForce = new Vector2(deathDragForce.x * (-1), deathDragForce.y);

		// Flip hero direction
		heroDirectionX = heroDirectionX * (-1);
	}
	#endregion

	#region SET DIRECTION SINGLE ROAD
	private void SetDirection_SingleRoad(GameObject _road)
	{
		_road.transform.localScale = new Vector2(heroDirectionX, 1);
	}
	#endregion

	#region FLIP SINGLE ROAD
	private void FlipSingleRoad(GameObject _road)
	{
		_road.transform.localScale = new Vector2(_road.transform.localScale.x * (-1), 1);
	}
	#endregion

	// ---------- SERENITY STATE ----------------------------------------------------+

    #region SET SERENITY STATE
	public void SetSerenityState()
	{
		// ----- SET STATE & FLAGS -----

		// Set map state
		mapState = MapStates.SERENITY;

		// Set drag as not allowed
		dragAllowed = false;

		// ----- ACTIVATE INTRO ROADS -----

		introRoad1.obj.SetActive(true);
		introRoad2.obj.SetActive(true);
		introRoad3.obj.SetActive(true);
		introRoad4.obj.SetActive(true);

		// ----- RESET BODY TYPES -----

		// Change body type of intro roads
		introRoad1.rigidbody.bodyType = RigidbodyType2D.Kinematic;
		introRoad2.rigidbody.bodyType = RigidbodyType2D.Kinematic;
		introRoad3.rigidbody.bodyType = RigidbodyType2D.Kinematic;
		introRoad4.rigidbody.bodyType = RigidbodyType2D.Kinematic;
		// Change body type of standard roads
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.bodyType = RigidbodyType2D.Kinematic;
		}
		// Change body type of curved roads
		curvedRoad1.rigidbody.bodyType = RigidbodyType2D.Kinematic;
		curvedRoad2.rigidbody.bodyType = RigidbodyType2D.Kinematic;

		// ----- FREEZE & ALLOW BACKGROUND MOVEMENT -----

		// Freeze background's movement
		introRoad1.rigidbody.velocity = Vector2.zero;
		introRoad2.rigidbody.velocity = Vector2.zero;
		introRoad3.rigidbody.velocity = Vector2.zero;
		introRoad4.rigidbody.velocity = Vector2.zero;
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.velocity = Vector2.zero;
		}
		curvedRoad1.rigidbody.velocity = Vector2.zero;
		curvedRoad2.rigidbody.velocity = Vector2.zero;

		// Freeze background's movement
		introRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		introRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		introRoad3.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		introRoad4.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		}
		curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

		// Allow background's movement
		introRoad1.rigidbody.constraints = RigidbodyConstraints2D.None;
		introRoad2.rigidbody.constraints = RigidbodyConstraints2D.None;
		introRoad3.rigidbody.constraints = RigidbodyConstraints2D.None;
		introRoad4.rigidbody.constraints = RigidbodyConstraints2D.None;
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.None;
		}
		curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.None;
		curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.None;

		// ----- RESET SERENITY POSITIONS -----

		// Set background parents positions
		introRouteFollower.SetRoadParentsPosition_AtZeroZero();

		// Set background positions, on serenity state
		SetBackgroundPositions_OnSerenityState();

		// Set background parents positions
		introRouteFollower.SetRoadParentsPosition_AtRoutesOrigins(heroDirectionX);
		
		// Deny camera movement on curved routes
		curvedRouteFollower.DenyCameraMovement();

		// Reset curved route start and end point
		curvedRouteFollower.ResetRoutesOriginsAndEnds();

		// Set camera position
		introRouteFollower.ResetCameraPosition(heroDirectionX);
	}
	#endregion

	// ---------- MAP MOVEMENT ----------------------------------------------------+

	#region START HERO ENTERING STATE
	public void StartHeroEnteringState()
	{
		// Start following intro route
		introRouteFollower.StartFollowingRoute(heroDirectionX < 0);

		// Set state as entering
		mapState = MapStates.HERO_ENTERING;
	}
    #endregion

    #region START HERO DEFAULT STATE
    // This method gets called when any route is exited
    private void StartHeroDefaultState()
	{
		// Adjust curved route start and ending point
		curvedRouteFollower.AdjustRoutesOriginsAndEnds();

		// ----- FREEZE VERTICAL MOVEMENT -----

		if (mapState == MapStates.HERO_TURNING || mapState == MapStates.HERO_ENTERING)
		{
			introRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			introRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			introRoad3.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			introRoad4.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			for (int i = 0; i < standardRoads.Length; i++)
			{
				standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			}
			curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
			curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
		}
		
		// ----- SET BACKGROUNDS' VELOCITIES -----

		// Move intro road if this is a new match
		if (mapState == MapStates.HERO_ENTERING)
		{
			introRoad1.rigidbody.velocity = Vector2.left * speed;
			introRoad2.rigidbody.velocity = Vector2.left * speed;
			introRoad3.rigidbody.velocity = Vector2.left * speed;
			introRoad4.rigidbody.velocity = Vector2.left * speed;
		}
		// Move other roads
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.velocity = Vector2.left * speed;
		}
		curvedRoad1.rigidbody.velocity = Vector2.left * speed;
		curvedRoad2.rigidbody.velocity = Vector2.left * speed;

		// Hide curved roads
		if (mapState == MapStates.HERO_ENTERING)
		{
			// Hide roads
			curvedRoad1.renderer.enabled = false;
			curvedRoad2.renderer.enabled = false;
		}

		// ----- START DEFAULT STATE -----

		// Set state
		mapState = MapStates.HERO_DEFAULT;

		// Fire event: Hero default state started
		HeroDefaultStateStarted?.Invoke();
	}
    #endregion

    #region STOP MAP
	private void StopMap()
	{
		// Apply death drag force if the hero is dead 
		if (dragAllowed)
		{
			// ----- ADD DRAG FORCE -----

			// Add force on intro roads
			introRoad1.rigidbody.AddForce(deathDragForce);
			introRoad2.rigidbody.AddForce(deathDragForce);
			introRoad3.rigidbody.AddForce(deathDragForce);
			introRoad4.rigidbody.AddForce(deathDragForce);
			// Add force on standard roads
			for (int i = 0; i < standardRoads.Length; i++)
			{
				standardRoads[i].rigidbody.AddForce(deathDragForce);
			}
			// Add force on curved roads
			curvedRoad1.rigidbody.AddForce(deathDragForce);
			curvedRoad2.rigidbody.AddForce(deathDragForce);

			bool heroGoingRight_And_AnyBackgroundVelocityChangedDirection =
				(heroDirectionX > 0) && (standardRoads[0].rigidbody.velocity.x > 0);
			bool heroGoingLeft_And_AnyBackgroundVelocityChangedDirection =
				(heroDirectionX < 0) && (standardRoads[0].rigidbody.velocity.x < 0);

			if (heroGoingRight_And_AnyBackgroundVelocityChangedDirection ||
				heroGoingLeft_And_AnyBackgroundVelocityChangedDirection)
			{
				// ----- STOP MOVING MAP -----

				// Freeze intro roads
				introRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				introRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				introRoad3.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				introRoad4.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				// Freeze curved roads
				curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				// Freeze standard roads
				for (int i = 0; i < standardRoads.Length; i++)
				{
					standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
				}

				dragAllowed = false;

				// Fire event: Map stopped dragging
				MapStoppedDragging?.Invoke();
			}
		}
	}
    #endregion

    // ---------- INTRO ROUTE FOLLOWING ----------------------------------------------------+

    #region CHECK IF INTRO ROAD WAS EXITED
    private void CheckIf_IntroRoadWasExited()
	{
		// Check ONLY if hero is entering
		if (mapState == MapStates.HERO_ENTERING)
		{
			// If the route end was reached
			if (introRouteFollower.CheckIf_EndWasReached())
			{
				//SetBackgroundPositions_OnExitingIntroRoad();

				// Start moving map
				StartHeroDefaultState();
			}
		}
	}
	#endregion

	// ---------- CURVED ROUTE FOLLOWING ----------------------------------------------------+

	#region CHECK IF CURVE WAS REACHED
	private void CheckIf_CurveWasReached()
	{
		// Check ONLY if hero is going straight & the curved road is active
		if ((mapState == MapStates.HERO_DEFAULT) && curvedRoad1.renderer.enabled)
		{
			// Get road position
			float curvedRoad1PosX = curvedRoad1.obj.transform.position.x;

			// Enter curve if floor end was reached
			if (((heroDirectionX > 0) && (curvedRoad1PosX < curvedRoadStoppingPoint_ForHeroApproachingCurve)) ||
				((heroDirectionX < 0) && (curvedRoad1PosX > curvedRoadStoppingPoint_ForHeroApproachingCurve)))
			{
				EnterCurve();
			}
		}
	}
	#endregion

	#region ENTER CURVE
	private void EnterCurve()
	{
		// ----- ACTIVATE / DEACTIVATE OBJECTS -----

		// Deactivate background loop checker
		loopCheckerRoadBgs.SetActive(false);

		// Show curved road 2
		curvedRoad2.renderer.enabled = true;

		// ----- STOP RIGID BODIES -----

		// Standard movement stop (velocity)
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.velocity = Vector2.zero;
		}
		curvedRoad1.rigidbody.velocity = Vector2.zero;
		curvedRoad2.rigidbody.velocity = Vector2.zero;

		// Forced movement stop (constraints)
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		}
		curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

		// ----- FLIP AND RESET BACKGROUND POSITIONS -----

		FlipAndResetBackgroundPositions_OnReachingCurve();

		// ----- SET CONSTRAINTS FOR NEW MOVEMENT -----

		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.constraints = RigidbodyConstraints2D.None;
		}
		curvedRoad1.rigidbody.constraints = RigidbodyConstraints2D.None;
		curvedRoad2.rigidbody.constraints = RigidbodyConstraints2D.None;

		// ----- START FOLLOWING CURVE ROUTE -----

		curvedRouteFollower.StartFollowingRoute(heroDirectionX > 0);

		// ----- CHANGE STATE & FIRE EVENT -----

		// Change state
		mapState = MapStates.HERO_TURNING;

		// Fire event: curve was entered
		CurveWasEntered?.Invoke();
	}
	#endregion

	#region RESPOND TO "DIRECTION HAS CHANGED" EVENT
	private void RespondTo_DirectionHasChanged_Event()
	{
		// Fire event: Direction has changed
		DirectionHasChanged?.Invoke();
	}
	#endregion

	#region RESPOND TO "TURNING ANIMATION IS ON" EVENT
	private void RespondTo_TurningAnimationIsOn_Event()
	{
		// Fire event: Turning animation is on
		TurningAnimationIsOn?.Invoke();
	}
	#endregion

	#region RESPOND TO "TURNING ANIMATION IS OFF" EVENT
	private void RespondTo_TurningAnimationIsOff_Event()
	{
		// Fire event: Turning animation is off
		TurningAnimationIsOff?.Invoke();
	}
	#endregion

	#region CHECK CURVE EXIT
	private void CheckIf_CurveWasExited()
	{
		// Check ONLY if hero is turning
		if (mapState == MapStates.HERO_TURNING)
		{
			// If the route end was reached
			if (curvedRouteFollower.CheckIf_EndWasReached())
			{
				// Flip direction
				FlipDirection();

				// Activate background loop checker
				loopCheckerRoadBgs.SetActive(true);

				// Hide curved road 1
				curvedRoad1.renderer.enabled = false;

				// Start moving map
				StartHeroDefaultState();
			}
		}
	}
	#endregion

	// ---------- BACKGROUND POSITIONS / BACKGROUND FLIPPING ----------------------------------------------------+

	#region SET BACKGROUND POSITIONS, ON SERENITY STATE
	private void SetBackgroundPositions_OnSerenityState()
	{
		// ----- INTRO ROADS -----

		// Place intro road 1
		SetDirection_SingleRoad(introRoad1.obj);
		introRoad1.obj.transform.position = new Vector2(introRoadStartingPoint, bgDefaultPositionY);

		// Place intro road 2
		SetDirection_SingleRoad(introRoad2.obj);
		PlaceRoad_AlignedToAnother(introRoad2, introRoad1, true, -1);

		// Place intro road 3
		SetDirection_SingleRoad(introRoad3.obj);
		PlaceRoad_AlignedToAnother(introRoad3, introRoad2, false, 1);

		// Place intro road 4
		SetDirection_SingleRoad(introRoad4.obj);
		PlaceRoad_AlignedToAnother(introRoad4, introRoad3, true, 1);

		// ----- STANDARD ROADS -----

		// Place standard road 0 next to intro road
		SetDirection_SingleRoad(standardRoads[0].obj);
		PlaceRoad_AlignedToAnother(standardRoads[0], introRoad4, true, 1);

		// Place standard road 1 next to standard road 2
		SetDirection_SingleRoad(standardRoads[1].obj);
		PlaceRoad_AlignedToAnother(standardRoads[1], standardRoads[0], true, 1);

		// Place standard road 2 next to standard road 1
		SetDirection_SingleRoad(standardRoads[2].obj);
		PlaceRoad_AlignedToAnother(standardRoads[2], standardRoads[1], true, 1);

		// Place standard road 3 next to standard road 2
		SetDirection_SingleRoad(standardRoads[3].obj);
		PlaceRoad_AlignedToAnother(standardRoads[3], standardRoads[2], true, 1);

		// Place standard road 4 next to standard road 3
		SetDirection_SingleRoad(standardRoads[4].obj);
		PlaceRoad_AlignedToAnother(standardRoads[4], standardRoads[3], true, 1);

		// Place standard road 5 next to standard road 4
		SetDirection_SingleRoad(standardRoads[5].obj);
		PlaceRoad_AlignedToAnother(standardRoads[5], standardRoads[4], true, 1);

		// Place standard road 6 next to standard road 5
		SetDirection_SingleRoad(standardRoads[6].obj);
		PlaceRoad_AlignedToAnother(standardRoads[6], standardRoads[5], true, 1);

		// ----- CURVED ROADS -----

		// Place curved road 1 where it can't be seen
		SetDirection_SingleRoad(curvedRoad1.obj);
		PlaceRoad_AlignedToAnother(curvedRoad1, introRoad2, true, -1);

		// Place curved road 2 where it can't be seen
		SetDirection_SingleRoad(curvedRoad2.obj);
		PlaceRoad_AlignedToAnother(curvedRoad2, curvedRoad1, true, -1);
	}
	#endregion

	#region FLIP AND RESET BACKGROUND POSITIONS, ON REACHING CURVE
	private void FlipAndResetBackgroundPositions_OnReachingCurve()
	{
		// Place curved road 1 on stopping point
		curvedRoad1.obj.transform.position = new Vector2(
			curvedRoadStoppingPoint_ForHeroApproachingCurve, bgDefaultPositionY);

		// Place curved road 2 on top of curved road 1
		PlaceRoad_AlignedToAnother(curvedRoad2, curvedRoad1, false, 1);

		// Place standard road 0 next to curved road 1
		PlaceRoad_AlignedToAnother(standardRoads[0], curvedRoad1, true, -1);

		// Place standard road 1 next to standard road 0
		PlaceRoad_AlignedToAnother(standardRoads[1], standardRoads[0], true, -1);

		// Flip standard road 2
		FlipSingleRoad(standardRoads[2].obj);
		// Place standard road 2 next to curved road 2
		PlaceRoad_AlignedToAnother(standardRoads[2], curvedRoad2, true, -1);

		// Flip standard road 3
		FlipSingleRoad(standardRoads[3].obj);
		// Place standard road 3 next to standard road 2
		PlaceRoad_AlignedToAnother(standardRoads[3], standardRoads[2], true, -1);

		// Flip standard road 4
		FlipSingleRoad(standardRoads[4].obj);
		// Place standard road 4 next to standard road 3
		PlaceRoad_AlignedToAnother(standardRoads[4], standardRoads[3], true, -1);

		// Flip standard road 5
		FlipSingleRoad(standardRoads[5].obj);
		// Place standard road 5 next to standard road 4
		PlaceRoad_AlignedToAnother(standardRoads[5], standardRoads[4], true, -1);

		// Flip standard road 6
		FlipSingleRoad(standardRoads[6].obj);
		// Place standard road 6 next to standard road 5
		PlaceRoad_AlignedToAnother(standardRoads[6], standardRoads[5], true, -1);
	}
	#endregion

	#region PLACE ROAD ALIGNED TO ANOTHER
	private void PlaceRoad_AlignedToAnother(BgSectionStruct _road1, BgSectionStruct _road2, bool _alignHorizontally,
		int _roadDirection)
	{
		// Align horizontally
		if (_alignHorizontally)
		{
			float distanceToMove = (_road1.width + _road2.width) / 2; 
			float positionX = _road2.obj.transform.position.x + distanceToMove * heroDirectionX * _roadDirection;
			//float positionY = bgDefaultPositionY;
			float positionY = _road2.obj.transform.position.y;
			_road1.obj.transform.position = new Vector2(positionX, positionY);
		}

		// Align vertically
		else
		{
			// The distance to move is not calculated here, because I'm asuming same height for every background
			float positionX = _road2.obj.transform.position.x;
			float positionY = _road2.obj.transform.position.y + _road2.height * _roadDirection;
			_road1.obj.transform.position = new Vector2(positionX, positionY);
		}
	}
	#endregion

	#region RESPOND TO BOSS IS DEAD EVENT
	public void RespondTo_BossIsDead_Event()
	{
		// Activate road
		//curvedRoad1.obj.SetActive(true);

		// Show road
		curvedRoad1.renderer.enabled = true;
	}
	#endregion

	// ---------- COLLISIONS ----------------------------------------------------+

	#region LOOP CHECKER (ROADS BG) REACT TO ROAD COLLISION
	public void LoopCheckerRoadsBg_ReactTo_RoadCollision(GameObject _collider)
	{
		if (!curvedRoad1.renderer.enabled)
		{
			for (int i = 0; i < standardRoads.Length; i++)
			{
				// If collider is standardRoad1 AND curvedRoad1 IS NOT active
				if (standardRoads[i].obj == _collider)
				{
					PlaceRoad_AlignedToAnother(standardRoads[i], GetFurthestAwayRoad(), true, 1);
					PlaceRoad_AlignedToAnother(curvedRoad1, standardRoads[i], true, 1);
				}
			}
		}

		// If collider is curvedRoad2 && curvedRoad2 is enabled
		if ((mapState == MapStates.HERO_DEFAULT) && (curvedRoad2.renderer.enabled))
		{
			// Flip curved roads
			FlipSingleRoad(curvedRoad1.obj);
			FlipSingleRoad(curvedRoad2.obj);

			// Hide curved road 2
			curvedRoad2.renderer.enabled = false;

			// Flip and place standard roads 0 & 1
			FlipSingleRoad(standardRoads[0].obj);
			FlipSingleRoad(standardRoads[1].obj);
			PlaceRoad_AlignedToAnother(standardRoads[0], GetFurthestAwayRoad(), true, 1);
			PlaceRoad_AlignedToAnother(standardRoads[1], standardRoads[0], true, 1);
		}

		// If collider is introRoad
		else if (introRoad1.obj == _collider)
		{
			introRoad1.obj.SetActive(false);
			introRoad2.obj.SetActive(false);
			introRoad3.obj.SetActive(false);
			introRoad4.obj.SetActive(false);
		}
	}
	#endregion

	// ---------- OTHER ----------------------------------------------------+

	#region SET HERO DEAD STATE
	public void SetHeroDeadState()
	{
		// Set map state
		mapState = MapStates.HERO_DEAD;
	}
	#endregion

    #region START DRAGGING
    public void StartDragging()
	{
		// Set map state
		mapState = MapStates.HERO_DEAD;

		// Set drag as allowed
		dragAllowed = true;

		// Change body type of intro roads
		introRoad1.rigidbody.bodyType = RigidbodyType2D.Dynamic;
		introRoad2.rigidbody.bodyType = RigidbodyType2D.Dynamic;
		introRoad3.rigidbody.bodyType = RigidbodyType2D.Dynamic;
		introRoad4.rigidbody.bodyType = RigidbodyType2D.Dynamic;

		// Change body type of curved roads
		curvedRoad1.rigidbody.bodyType = RigidbodyType2D.Dynamic;
		curvedRoad2.rigidbody.bodyType = RigidbodyType2D.Dynamic;

		// Change body type of standard roads
		for (int i = 0; i < standardRoads.Length; i++)
		{
			standardRoads[i].rigidbody.bodyType = RigidbodyType2D.Dynamic;
		}
	}
	#endregion

	#region GET SCREEN WIDTH IN WORLD UNITS
	private float GetScreenWidth_InWorldUnits()
	{
		// Get world units height
		float wuHeight = Camera.main.orthographicSize * 2;
		// Get world units width
		float wuWidth = wuHeight * Camera.main.aspect;

		// Return width
		return wuWidth;
	}
	#endregion

	#region GET FURTHEST AWAY ROAD
	private BgSectionStruct GetFurthestAwayRoad()
	{
		float furthestLeftPosition = 0;
		int furthestLeftIndex = -1;

		float furthestRightPosition = 0;
		int furthestRightIndex = -1;

		for (int i = 0; i < standardRoads.Length; i++)
		{
			if (i == 0)
			{
				furthestLeftPosition = standardRoads[i].obj.transform.position.x;
				furthestLeftIndex = i;

				furthestRightPosition = standardRoads[i].obj.transform.position.x;
				furthestRightIndex = i;
			}

			else if ((heroDirectionX > 0) && (standardRoads[i].obj.transform.position.x > furthestRightPosition))
			{
				furthestRightPosition = standardRoads[i].obj.transform.position.x;
				furthestRightIndex = i;
			}

			else if ((heroDirectionX < 0) && (standardRoads[i].obj.transform.position.x < furthestLeftPosition))
			{
				furthestLeftPosition = standardRoads[i].obj.transform.position.x;
				furthestLeftIndex = i;
			}
		}

		if (heroDirectionX > 0)
		{
			return standardRoads[furthestRightIndex];
		}

		else
		{
			return standardRoads[furthestLeftIndex];
		}
	}
	#endregion
}