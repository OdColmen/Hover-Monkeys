using UnityEngine;

public class ProjectileWallController : MonoBehaviour
{
	#region VARIABLES

	// ----- OTHER -----

	// Position references
	private float initialPositionY = 5;
	private float projectileOffsetX = 23;

	#endregion

	#region INITIALIZE
	public void Initialize(GameObject _generalProjectilesParent)
	{
		// Deactivate game object
		gameObject.SetActive(false);

		// Set parent
		transform.parent = _generalProjectilesParent.transform;
	}
	#endregion

	#region SET DIRECTION
	public void SetDirection(int _heroDirectionX)
	{
		// Change offset, so the wall is correctly place behind the projectile
		projectileOffsetX = projectileOffsetX * _heroDirectionX;
	}
	#endregion

	#region FLIP DIRECTION
	public void FlipDirection()
	{
		// Change offset, so the wall is correctly place behind the projectile
		projectileOffsetX = projectileOffsetX * (-1);
	}
	#endregion

	#region SPAWN
	public void Spawn(GameObject _specificProjectileParent)
	{
		// Activate game object
		gameObject.SetActive(true);

		// Set tag
		gameObject.tag = "Projectile Wall";

		if (_specificProjectileParent.transform.childCount > 1)
		{
			Debug.Log("Error: Projectile has more than one child");
		}

		// Set parent
		transform.parent = _specificProjectileParent.transform;

		// Set position
		transform.position = new Vector2(_specificProjectileParent.transform.position.x + projectileOffsetX, initialPositionY);
	}
	#endregion

	#region DEACTIVATE
	public void Deactivate(GameObject _generalProjectileParent)
	{
		// Change parent
		transform.parent = _generalProjectileParent.transform;

		// Deactivate game object
		gameObject.SetActive(false);
	}
	#endregion

	#region GET PARENT
	public GameObject GetParent()
	{
		return (transform.parent == null) ? null : transform.parent.gameObject;
	}
	#endregion

	#region CHECK IF ITS ACTIVE
	public bool CheckIf_ItsActive()
	{
		return gameObject.activeSelf;
	}
	#endregion
}
