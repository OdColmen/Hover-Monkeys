using System.Collections;
using UnityEngine;

public class IntroRouteFollower : MonoBehaviour
{
	/* CLASS INFO
	 * This class goes along with CurveRouteFollower and MapManager
	 * There are a total of 4 routes used: 2 for the roads intros and 2 for the roads normal curves.
	 * Left and Right for each one.
	 * RULES FOR CREATING THE ROUTES
	 * The intro routes final points must end on 0 (x coordinate)
	 * The curve routes initial and final point must end on 0 (x coordinate)
	 * The curve routes initial points must be the same as the intro routes final points.
	 * This is necessary for the movement to be smooth
	 */

	#region VARIABLES

	// ----- EVENTS -----

	// Turning animation is off
	public delegate void TurningAnimationIsOff_EventHandler();
	public event TurningAnimationIsOff_EventHandler TurningAnimationIsOff;

	// ----- CAMERA COMPONENT -----

	// Game object
	private GameObject mainCamera;
	private readonly float cameraMovementTime = 3f;
	private float cameraLerpValue;
	// Positions
	private Vector3 cameraLerpStart;
	private Vector3 cameraLerpEnd_GoingRight = new Vector3(55, 0, -10);
	private Vector3 cameraLerpEnd_GoingLeft = new Vector3(-55, 0, -10);
	private Vector3 cameraSerenityStateOffset_GoingRight = new Vector3(-20, 90, -10);
	private Vector3 cameraSerenityStateOffset_GoingLeft = new Vector3(20, 90, -10);

	// ----- ROUTES -----

	// Routes to follow (road)
	private Transform[] roadRoutes;
	// Turning left routes (road)
	private Transform[] turningLeftRoadRoutes;
	// Turning right routes (road)
	private Transform[] turningRightRoadRoutes;

	// ----- ROUTE FOLLOWING VARIABLES -----

	// Current route
	private int currentRouteIndex;
	// Time?
	private float tParam_Road;
	// Speed
	private float speedModifier;
	// coroutineAllowed: it allows the coroutine to run if its true
	private bool coroutineAllowed;
	// End was reached
	private bool endWasReached;

	// ----- OTHER -----

	private bool turningAnimationIsOn;

	private bool heroIsTurningLeft;

	private bool cameraMovementIsAllowed;

	#endregion

	#region INITIALIZE
	public void Initialize(Transform[] _turningLeftRoadRoutes, Transform[] _turningRightRoadRoutes, 
		float _heroGameSpeed, GameObject _camera)
	{
		// Set camera
		mainCamera = _camera;

		// Set camera movement as not allowed
		cameraMovementIsAllowed = false;

		// Set camera lerp value
		cameraLerpValue = 1;

		// Set coroutine as not allowed
		coroutineAllowed = false;

		// Set road route speed modifier
		speedModifier = _heroGameSpeed * 0.01f;

		// ----- ROAD ROUTES -----

		// Get road routes
		turningLeftRoadRoutes = _turningLeftRoadRoutes;
		turningRightRoadRoutes = _turningRightRoadRoutes;

		// ----- SET ROUTES ORIGINS AND ENDS -----

		SetRoutesOriginsAndEnds();
	}
	#endregion

	#region SET ROAD PARENTS POSITION, AT ZERO-ZERO
	public void SetRoadParentsPosition_AtZeroZero()
	{
		// Set road parent position
		transform.position = Vector2.zero;
	}
	#endregion

	#region SET ROAD PARENTS POSITION, AT ROUTES ORIGINS
	public void SetRoadParentsPosition_AtRoutesOrigins(int _heroDirectionX)
	{
		int routeNumber = 0;

		// If hero is turning right
		if (_heroDirectionX > 0)
		{
			// Set road parent position
			transform.position = turningRightRoadRoutes[routeNumber].GetChild(0).position;
		}

		// If hero is turning left
		else
		{
			// Set road parent position
			transform.position = turningLeftRoadRoutes[routeNumber].GetChild(0).position;
		}
	}
	#endregion

	#region RESET CAMERA POSITION
	public void ResetCameraPosition(int _heroDirection)
	{
		// Reset camera position
		mainCamera.transform.position = (_heroDirection > 0) ? transform.position + cameraSerenityStateOffset_GoingRight :
			transform.position + cameraSerenityStateOffset_GoingLeft;

		// Set camera movement as not allowed
		cameraMovementIsAllowed = false;
	}
	#endregion

	#region START FOLLOWING ROUTE
	public void StartFollowingRoute(bool _isTurningLeft)
	{
		// Set which routes to follow
		if (_isTurningLeft)
		{
			roadRoutes = turningLeftRoadRoutes;
		}
		else
		{
			roadRoutes = turningRightRoadRoutes;
		}

		// Set coroutine initial values
		currentRouteIndex = 0;
		tParam_Road = 0f;

		// Allow the coroutine to be started
		coroutineAllowed = true;

		// Set end as not reached
		endWasReached = false;

		// Set turning direction
		heroIsTurningLeft = _isTurningLeft;

		// Set camera lerp value
		cameraLerpValue = 0;

		// 
		turningAnimationIsOn = true;
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
				float newPositionX = 
					Mathf.Lerp(cameraLerpStart.x, cameraLerpEnd_GoingLeft.x, cameraLerpValue);
				mainCamera.transform.position = 
					new Vector3(newPositionX, mainCamera.transform.position.y, -10);
				
				if (mainCamera.transform.position == cameraLerpEnd_GoingLeft)
				{
					cameraMovementIsAllowed = false;
				}
			}

			// Move left
			else
			{
				float newPositionX = 
					Mathf.Lerp(cameraLerpStart.x, cameraLerpEnd_GoingRight.x, cameraLerpValue);
				mainCamera.transform.position = 
					new Vector3(newPositionX, mainCamera.transform.position.y, -10);

				if (mainCamera.transform.position == cameraLerpEnd_GoingRight)
				{
					cameraMovementIsAllowed = false;
				}
			}
		}
	}
	#endregion

	#region FOLLOW ROUTE (IEnumerator)
	private IEnumerator FollowRoute(int routeNumber)
	{
		// ----- ROUTE FOLLOWING -----

		// Deny another coroutine to run
		coroutineAllowed = false;

		// Get road route points
		Vector2 p0_Road = roadRoutes[routeNumber].GetChild(0).position;
		Vector2 p1_Road = roadRoutes[routeNumber].GetChild(1).position;
		Vector2 p2_Road = roadRoutes[routeNumber].GetChild(2).position;
		Vector2 p3_Road = roadRoutes[routeNumber].GetChild(3).position;

		// Calculate current positions
		while (tParam_Road < 1)
		{
			// ----- ROAD -----

			tParam_Road += Time.deltaTime * speedModifier;

			if (tParam_Road > 1)
			{
				tParam_Road = 1;
			}
			
			Vector2 position_Road = Mathf.Pow(1 - tParam_Road, 3) * p0_Road +
				3 * Mathf.Pow(1 - tParam_Road, 2) * tParam_Road * p1_Road +
				3 * (1 - tParam_Road) * Mathf.Pow(tParam_Road, 2) * p2_Road +
				Mathf.Pow(tParam_Road, 3) * p3_Road;

			// Get roadBgs vertical distance traveled
			float distanceTraveledY = position_Road.y - transform.position.y;
			float distanceTraveledX = position_Road.x - transform.position.x;

			// Set object position
			transform.position = position_Road;

			// ----- CAMERA -----

			// Move camera with map
			if (transform.position.y > (p3_Road.y + 20))
			{
				mainCamera.transform.position = new Vector3(
					mainCamera.transform.position.x + distanceTraveledX,
					mainCamera.transform.position.y + distanceTraveledY, -10);
			}
			
			else
			{
				// Move camera VERTICALLY with map
				if (transform.position.y > p3_Road.y)
				{
					mainCamera.transform.position = new Vector3(
						mainCamera.transform.position.x,
						mainCamera.transform.position.y + distanceTraveledY, -10);
				}

				// Allow camera to move HORIZONTALLY on update
				if (!cameraMovementIsAllowed)
				{
					cameraLerpStart = mainCamera.transform.position;

					cameraMovementIsAllowed = true;
				}
			}

			// Check if hero animation must change
			CheckIf_TurningAnimationIsOn(p3_Road);

			// ----- YIELD -----

			yield return new WaitForEndOfFrame();
		}

		// ----- ROUTE FOLLOWING, END -----

		// Prepare for next route, if any

		// Set tParam to 0
		tParam_Road = 0f;
		// Increase the route index
		currentRouteIndex += 1;

		// Reset index to first route, if needed
		if (currentRouteIndex > roadRoutes.Length - 1)
		{
			currentRouteIndex = 0;
		}

		// Set end as reached
		endWasReached = true;

		// Allow next coroutine to run (NOT ON MY GAME)
		//coroutineAllowed = true;
	}
	#endregion

	#region CHECK IF TURNING ANIMATION IS ON
	public void CheckIf_TurningAnimationIsOn(Vector2 _point3)
	{
		float limit = _point3.y + 10;

		// If turning animation is off
		if (turningAnimationIsOn)
		{
			if (transform.position.y <= limit)
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

	#region SET ROUTES ORIGINS AND ENDS
	// By setting each route to (0,0) we're making sure each point of each route
	// is set to it's original value.
	private void SetRoutesOriginsAndEnds()
	{
		// Set every route on position zero
		for (int i = 0; i < turningLeftRoadRoutes.Length; i++)
		{
			turningLeftRoadRoutes[i].position = Vector2.zero;
			turningRightRoadRoutes[i].position = Vector2.zero;
		}
	}
	#endregion
}
