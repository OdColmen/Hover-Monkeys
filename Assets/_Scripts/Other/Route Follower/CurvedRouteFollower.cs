using System.Collections;
using UnityEngine;

public class CurvedRouteFollower : MonoBehaviour
{
	/* CLASS INFO
	 * This class goes along with IntroRouteFollower and MapManager
	 * There are a total of 4 routes used: 2 for the roads intros, 2 for the roads normal curves.
	 * Left and Right for each one.
	 * RULES FOR CREATING THE ROUTES
	 * The intro routes final points must end on 0 (x coordinate)
	 * The curve routes initial and final point must end on 0 (x coordinate)
	 * The curve routes initial points must be the same as the intro routes final points.
	 * This is necessary for the movement to be smooth
	 */

	#region VARIABLES

	// ----- EVENTS -----

	// Direction has changed
	public delegate void DirectionHasChanged_EventHandler();
	public event DirectionHasChanged_EventHandler DirectionHasChanged;

	// Turning animation is on
	public delegate void TurningAnimationIsOn_EventHandler();
	public event TurningAnimationIsOn_EventHandler TurningAnimationIsOn;

	// Turning animation is off
	public delegate void TurningAnimationIsOff_EventHandler();
	public event TurningAnimationIsOff_EventHandler TurningAnimationIsOff;

	// ----- CAMERA COMPONENT -----

	// Game object
	private GameObject mainCamera;
	private readonly float cameraMovementTime = 3.6f;
	private float cameraLerpValue;
	// Positions
	private Vector3 cameraBase_GoingRight = new Vector3(55, 0, -10);
	private Vector3 cameraBase_GoingLeft = new Vector3(-55, 0, -10);

	// ----- ROUTES -----

	// Routes to follow
	private Transform[] routes_Road;
	// Turning left routes
	private Transform[] turningLeftRoadRoutes;
	// Turning right routes
	private Transform[] turningRightRoadRoutes;

	// ----- ROUTE FOLLOWING VARIABLES -----

	// Current route
	private int currentRouteIndex;
	// Time?
	private float tParam_Road;
	// Moving object position
	private Vector2 position_Road;
	// Speed
	private float speedModifier;
	// coroutineAllowed: it allows the coroutine to run if its true
	private bool coroutineAllowed;
	// End was reached
	private bool endWasReached;

	// ----- ROUTE CHANGING VARIABLES -----

	// Routes vertical distance
	private float verticalDistance_RoadRoutes;
	
	// ----- FLIPPING DIRECTION VARIABLES -----

	private float previousPositionX_RoadRoutes;
	private bool directionHasChanged;
	private bool previousPositionX_RoadRoutes_HasAValue;

	private bool turningAnimationIsOn;

	// ----- OTHER -----

	private bool heroIsTurningLeft;
	private bool cameraMovementIsAllowed;

	#endregion

	#region INITIALIZE
	public void Initialize(Transform [] _turningLeftRoadRoutes, Transform[] _turningRightRoadRoutes, 
		float _verticalDistance_RoadRoutes, float _heroGameSpeed, GameObject _camera)
	{
		// Set camera
		mainCamera = _camera;

		// Set camera movement as not allowed
		cameraMovementIsAllowed = false;

		// Set camera lerp value
		cameraLerpValue = 1;

		// Set coroutine as not allowed 
		coroutineAllowed = false;

		// Set route speed modifier
		speedModifier = _heroGameSpeed * 0.005f;
		
		// ----- ROAD ROUTES -----

		// Get routes
		turningLeftRoadRoutes = _turningLeftRoadRoutes;
		turningRightRoadRoutes = _turningRightRoadRoutes;

		// Set routes vertical distance
		verticalDistance_RoadRoutes = _verticalDistance_RoadRoutes;

		// ----- SET ROUTES ORIGINS AND ENDS -----

		ResetRoutesOriginsAndEnds();
	}
    #endregion

    #region RESET CAMERA POSITION
	public void ResetCameraPosition(int _heroDirection)
	{
		mainCamera.transform.position = (_heroDirection > 0) ? cameraBase_GoingRight : cameraBase_GoingLeft;
	}
	#endregion

	#region DENY CAMERA MOVEMENT
	public void DenyCameraMovement()
	{
		cameraMovementIsAllowed = false;
	}
	#endregion

	#region START FOLLOWING ROUTE
	public void StartFollowingRoute(bool _isTurningLeft)
	{
		// Set which routes to follow
		if (_isTurningLeft)
		{
			routes_Road = turningLeftRoadRoutes;
		}
		else
		{
			routes_Road = turningRightRoadRoutes;
		}

		// Set coroutine initial values
		currentRouteIndex = 0;
		tParam_Road = 0f;

		// Allow the coroutine to be started
		coroutineAllowed = true;

		// Set direciton as not changed
		directionHasChanged = false;

		// Set flags
		previousPositionX_RoadRoutes_HasAValue = false;
		turningAnimationIsOn = false;

		// Set end as not reached
		endWasReached = false;

		// Set turning direction
		heroIsTurningLeft = _isTurningLeft;

		// Camera movement is allowed
		cameraMovementIsAllowed = true;

		// Set camera lerp value
		cameraLerpValue = 0;
	}
    #endregion

    #region UPDATE
    private void Update()
	{
		if (coroutineAllowed)
		{
			// Start coroutine
			StartCoroutine(FollowRoute(currentRouteIndex));
		}

		// ----- CAMERA LERP -----

		if (cameraMovementIsAllowed)
		{
			// Increase lerp value
			cameraLerpValue += Time.deltaTime / cameraMovementTime;

			// Move right
			if (heroIsTurningLeft)
			{
				mainCamera.transform.position = Vector3.Lerp(cameraBase_GoingRight, 
					cameraBase_GoingLeft, cameraLerpValue);

				if (mainCamera.transform.position == cameraBase_GoingLeft)
				{
					cameraMovementIsAllowed = false;
				}
			}

			// Move left
			else
			{
				mainCamera.transform.position = Vector3.Lerp(cameraBase_GoingLeft, 
					cameraBase_GoingRight, cameraLerpValue);

				if (mainCamera.transform.position == cameraBase_GoingRight)
				{
					cameraMovementIsAllowed = false;
				}
			}
		}
		
	}
	#endregion

	#region FOLLOW ROUTE (IEnumerator)
	private IEnumerator FollowRoute (int routeNumber)
	{
		// ----- ROUTE FOLLOWING -----

		// Deny another coroutine to run
		coroutineAllowed = false;

		// Get road route points
		Vector2 p0_Road = routes_Road[routeNumber].GetChild(0).position;
		Vector2 p1_Road = routes_Road[routeNumber].GetChild(1).position;
		Vector2 p2_Road = routes_Road[routeNumber].GetChild(2).position;
		Vector2 p3_Road = routes_Road[routeNumber].GetChild(3).position;

		// Calculate current character position
		while (tParam_Road < 1)
		{
			// ----- ROAD -----

			tParam_Road += Time.deltaTime * speedModifier;

			position_Road = Mathf.Pow(1 - tParam_Road, 3) * p0_Road +
				3 * Mathf.Pow(1 - tParam_Road, 2) * tParam_Road * p1_Road +
				3 * (1 - tParam_Road) * Mathf.Pow(tParam_Road, 2) * p2_Road +
				Mathf.Pow(tParam_Road, 3) * p3_Road;

			// Set object position
			transform.position = position_Road;

			// Check if turning animation is on
			CheckIf_TurningAnimationIsOn(p0_Road, p3_Road);

			// Check if direction has changed
			CheckIf_DirectionHasChanged();

			// Yield
			yield return new WaitForEndOfFrame();
		}

		// ----- ROUTE FOLLOWING, END -----

		// Prepare for next route, if any

		// Set tParam to 0
		tParam_Road = 0f;
		// Increase the route index
		currentRouteIndex += 1;

		// Reset index to first route, if needed
		if (currentRouteIndex > routes_Road.Length - 1)
		{
			currentRouteIndex = 0;
		}

		// Set end as reached
		endWasReached = true;

		// Allow next coroutine to run (NOT ON MY GAME)
		//coroutineAllowed = true;
	}
	#endregion

	#region CHECK IF DIRECTION HAS CHANGED
	public void CheckIf_DirectionHasChanged()
	{
		if (!directionHasChanged)
		{
			if (!previousPositionX_RoadRoutes_HasAValue)
			{
				// Set first previous position x
				previousPositionX_RoadRoutes = transform.position.x;

				// Set flag
				previousPositionX_RoadRoutes_HasAValue = true;
			}

			if ( (heroIsTurningLeft && (previousPositionX_RoadRoutes < transform.position.x)) ||
				(!heroIsTurningLeft && (previousPositionX_RoadRoutes > transform.position.x)))
			{
				// Set direction as changed
				directionHasChanged = true;

				// Fire event: Direction has changed
				DirectionHasChanged?.Invoke();
			}

			// Set previous position;
			previousPositionX_RoadRoutes = transform.position.x;
		}
	}
	#endregion

	#region CHECK IF TURNING ANIMATION IS ON
	public void CheckIf_TurningAnimationIsOn(Vector2 _point0, Vector2 _point3)
	{
		float bottomLimit = _point0.y - 2;
		float topLimit = _point3.y + 10;

		// If turning animation is off
		if (!turningAnimationIsOn)
		{
			if ((transform.position.y <= bottomLimit) && (transform.position.y > topLimit))
			{
				// Set turning animation as on
				turningAnimationIsOn = true;

				// Fire event: Turning animation is on
				TurningAnimationIsOn?.Invoke();
			}
		}

		// If turning animation is on
		else
		{
			if (transform.position.y <= topLimit)
			{
				// Set turning animation as off
				turningAnimationIsOn = false;

				// Fire event: Turning animation is off
				TurningAnimationIsOff?.Invoke();
			}
		}
	}
	#endregion

	#region CHECK IF END WAS REACHED
	public bool CheckIf_EndWasReached()
	{
		return endWasReached;
	}
	#endregion

	#region ADJUST ROUTES ORIGINS & ENDS
	// VERTICAL ADJUSTMENT
	// The route's values need to be adjusted vertically everytime the "floor" changes.
	// By adjusting each route vertically after each route end, we're making sure that they are 
	// prepared for the next route movement.
	// Basically we're adjusting vertically the origin and end (pt 1 & 2). But also pt 3 & 4.
	public void AdjustRoutesOriginsAndEnds()
	{
		// Adjust road routes vertically
		for (int i = 0; i < turningLeftRoadRoutes.Length; i++)
		{
			turningLeftRoadRoutes[i].position = new Vector2(turningLeftRoadRoutes[i].position.x,
				turningLeftRoadRoutes[i].position.y - verticalDistance_RoadRoutes);

			turningRightRoadRoutes[i].position = new Vector2(turningRightRoadRoutes[i].position.x,
				turningRightRoadRoutes[i].position.y - verticalDistance_RoadRoutes);
		}
	}
	#endregion

	#region RESET ROUTES ORIGINS & ENDS
	// The route's values need to be reset every new match.
	// By resetting each route to (0,0) we're making sure each point of each route
	// is set to it's original value.
	public void ResetRoutesOriginsAndEnds()
	{
		for (int i = 0; i < turningLeftRoadRoutes.Length; i++)
		{
			turningLeftRoadRoutes[i].position = Vector2.zero;

			turningRightRoadRoutes[i].position = Vector2.zero;
		}
	}
	#endregion
}