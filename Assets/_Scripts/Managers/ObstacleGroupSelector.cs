using System.Collections.Generic;
using UnityEngine;

public class ObstacleGroupSelector : MonoBehaviour
{
	#region VARIABLES

	// ----- FIXED PROJECTILE INITIAL POSITIONS -----

	// Vertical positions
	[SerializeField] private float[] projectileAllowedPositionsY = null;
	private float minionsSpawnPositionY;

	// Default horizontal position
	private float projectileDefaultPositionX;

	// Horizontal offsets
	[SerializeField] private float projectileHorizontalOffset1 = 0;
	[SerializeField] private float projectileHorizontalOffset2 = 0;

	// ----- OTHER -----

	// Random
	System.Random rnd;

	// --------------- INFO ABOUT EACH OBSTACLE GROUP ---------------

	// Obstacle type defines if the obstacle is a (small, big or flawed) projectile or a minion
	public enum ObstacleType { SMALL, BIG, MINION, FLAWED };

	// ----- GROUPS -----

	// ProjectileDataStruct defines a position and a size for each projectile
	public struct ObstacleDataStruct
	{
		public ObstacleType type;
		public Vector2 position;

		public ObstacleDataStruct(ObstacleType _size, Vector2 _position)
		{
			type = _size;
			position = _position;
		}
	}

	// The "groups" array stores several groups of projectiles
	// Each group stores the info about each projectile size and position
	private ObstacleDataStruct[][] groups;

	// Currently selected group
	private int selectedGroup = -1;

	// ----- TYPES -----

	// GroupType defines the different group "types" 
	// TOP_ONE: crouchable group with one rojectile at the highest position
	// TOP_ANY: crouchable group with rojectiles at the top and/or middle position
	// BOTTOM_ONE: non crouchable group with one rojectile at the lowest position
	public enum GroupType { TOP_ONE, TOP_ANY, BOTTOM_ONE, MINION, NONE }
	
	// groupTypes stores the "type" of each group from the variable "groups"
	private GroupType[] groupTypes;

	// previousGroupTypes stores the "type" of the last spawned groups 
	private List<GroupType> previousGroupTypes;

	// ----- SPAWN BLOCKS -----

	// GroupSpawnBlock defines the different "spawn blocks" in which a group can be spawned. 
	// Either the FIRST, SECOND, or THIRD block
	//public enum GroupSpawnBlock { FIRST, SECOND, THIRD, NONE }

	// groupSpawnBlocks stores the "spawn block" of each group from the variable "groups" 
	//private GroupSpawnBlock[] groupSpawnBlocks;

	// ----- BOSS FIRING POSITIONS (VERTICAL) -----

	// Fixed allowed firing vertical positions
	[SerializeField] private float[] bossAllowedFiringPositionsY = null;

	// bossFiringPositions: Indicates where the boss will be placed for each group from the variable "groups"
	private float[] bossFiringPositions;

	// ----- ALLOWED GROUPS PER WAVE -----

	// allowedGroupsPerWave: stores the groups allowed to be spawned on each wave
	private int[][] allowedGroupsPerWave;

	#endregion

	// ---------- INITIALIZATIONS ----------------------------------------------------+

	#region AWAKE
	private void Awake()
	{
		// Initialize random
		int seed = (int)System.DateTime.Now.Ticks & 0x0000FFFF;
		rnd = new System.Random(seed);

		// Initialize "previous group types" list
		previousGroupTypes = new List<GroupType>();

		// Set default horizontal position
		projectileDefaultPositionX = 0;
	}
	#endregion

	#region INITIALIZE
	public void Initialize(float _minionsSpawnPositionY)
	{
		// Set minion's vertical spawn position
		minionsSpawnPositionY = _minionsSpawnPositionY;

		// Initialize groups manually
		InitializeGroupsManually();

		// Allowed groups per wave
		InitializeAllowedGroupsPerWave();
	}
	#endregion

	#region INITIALIZE GROUPS MANUALLY
	private void InitializeGroupsManually()
	{
		// Variables to keep track of each group and each projectile
		int currentGroup = -1;
		int totalProjectiles = -1;
		int currentProjectile = -1;

		// Set number of groups
		int totalGroups = 15;

		// Initialize projectileGroups array (first dimention)
		groups = new ObstacleDataStruct[totalGroups][];

		// Group types
		groupTypes = new GroupType[totalGroups];

		// Spawning blocks
		//groupSpawnBlocks = new GroupSpawnBlock[totalGroups];

		// Boss firing positions
		bossFiringPositions = new float[totalGroups];

		// --------------- ONE PROJECTILE --------------------------------------------- +

		// ----- GROUP 0 (one small proj at height 0) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[0]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[0], GroupType.TOP_ONE);

		// ----- GROUP 1 (one small proj at height 1) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[1]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.TOP_ANY);

		// ----- GROUP 2 (one small proj at height 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[2], GroupType.BOTTOM_ONE);

		// ----- GROUP 3 (one BIG proj at height 3) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.BIG, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[3]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[3], GroupType.TOP_ANY);

		// ----- GROUP 4 (one BIG proj at height 4) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.BIG, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[4]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[4], GroupType.NONE);

		// --------------- TWO PROJECTILES --------------------------------------------- +

		// ----- GROUP 5 (two small - height 0 & 1) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[0]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[1]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[5], GroupType.TOP_ANY);

		// ----- GROUP 6 (two small - height 1 & 0) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[0]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[1]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[5], GroupType.TOP_ANY);

		// ----- GROUP 7 (two small - height 1 & 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[1]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[6], GroupType.NONE);

		// ----- GROUP 8 (two small - height 2 & 1) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[1]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[6], GroupType.NONE);

		// ----- GROUP 9 (one BIG one small - height 4 & 0) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.BIG, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[4]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset2, projectileAllowedPositionsY[0]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.NONE);

		// ----- GROUP 10 (one BIG one small - height 3 & 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 2;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.BIG, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[3]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset2, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.NONE);

		// --------------- THREE PROJECTILES --------------------------------------------- +

		// ----- GROUP 11 (three small - height 0, 1 & 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 3;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[0]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[1]));

		// Projectile 2
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.NONE);

		// ----- GROUP 12 (three small - height 0, 1 & 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 3;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[0]));

		// Projectile 1
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX + projectileHorizontalOffset1, projectileAllowedPositionsY[1]));

		// Projectile 2
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.SMALL, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.NONE);

		// ----- GROUP 13 (one flawed shot at height 2) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.FLAWED, new Vector3(projectileDefaultPositionX, projectileAllowedPositionsY[2]));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[2], GroupType.NONE);

		// ----- GROUP 14 (one enemy minion at only height allowed) -----

		currentGroup++;
		currentProjectile = -1;
		totalProjectiles = 1;

		// Initialize projectileGroups array (second dimention)
		groups[currentGroup] = new ObstacleDataStruct[totalProjectiles];

		// Projectile 0
		currentProjectile++;
		groups[currentGroup][currentProjectile] =
			new ObstacleDataStruct(ObstacleType.MINION, new Vector3(projectileDefaultPositionX, minionsSpawnPositionY));

		// Extra data
		SetGroupExtraData(currentGroup, bossAllowedFiringPositionsY[1], GroupType.MINION);
	}
	#endregion

	#region SET GROUP EXTRA DATA
	private void SetGroupExtraData(int _currentGroup, float _bossFiringPosition, GroupType _groupType)
	{
		// Set boss firing position
		bossFiringPositions[_currentGroup] = _bossFiringPosition;

		// Set type
		groupTypes[_currentGroup] = _groupType;
	}
	#endregion

	#region RESET PREVIOUS GROUP TYPES LIST
	public void ResetPreviousGroupTypes()
	{
		previousGroupTypes.Clear();
	}
	#endregion

	#region INITIALIZE ALLOWED GROUPS PER WAVE
	public void InitializeAllowedGroupsPerWave()
	{
		int _includedWaves = 32;

		// Set array size
		allowedGroupsPerWave = new int[_includedWaves][];

		// ----- INITIALIZE ARRAY -----

		// Floor 0, wave 0 & 1
		allowedGroupsPerWave[0] = allowedGroupsPerWave[1] = new int[] { 1, 5, 6, 7, 8, 11, 12 };

		// test
		allowedGroupsPerWave[0] = allowedGroupsPerWave[1] = new int[] { 5, 6, 7, 8, 11, 12, 3, 4, 9, 10 }; 

		// Floor 1, wave 2 & 3
		allowedGroupsPerWave[2] = allowedGroupsPerWave[3] = new int[] { 1, 2, 5, 6, 7, 8, 11, 12 };

		// Floor 2, wave 4 & 5
		allowedGroupsPerWave[4] = allowedGroupsPerWave[5] = new int[] { 1, 2, 5, 7, 8, 11, 12 };

		// Floor 3, wave 6 & 7
		allowedGroupsPerWave[6] = allowedGroupsPerWave[7] = new int[] { 1, 2, 5, 7, 11, 12, 3, 4 };

		// Floor 4, wave 8 & 9
		allowedGroupsPerWave[8] = allowedGroupsPerWave[9] = new int[] { 1, 2, 5, 7, 11, 3, 4 };

		// Floor 5, wave 10 & 11
		allowedGroupsPerWave[10] = allowedGroupsPerWave[11] = new int[] { 1, 2, 5, 6, 7, 8, 11, 3, 4 };

		// Floor 6, wave 12 & 13
		allowedGroupsPerWave[12] = allowedGroupsPerWave[13] = new int[] { 0, 1, 2, 5, 6, 7, 8, 11, 3, 4 };
		// Floor 7, wave 14 & 15
		allowedGroupsPerWave[14] = allowedGroupsPerWave[15] = new int[] { 0, 1, 2, 5, 6, 7, 8, 11, 3, 4 };

		// Floor 8, wave 16 & 17
		allowedGroupsPerWave[16] = allowedGroupsPerWave[17] = new int[] { 0, 1, 2, 5, 6, 7, 8, 11, 3, 4, 9 };
		// Floor 9, wave 18 & 19
		allowedGroupsPerWave[18] = allowedGroupsPerWave[19] = new int[] { 0, 1, 2, 5, 6, 7, 8, 11, 3, 4, 9 };

		// Floor 10, wave 20 & 21
		allowedGroupsPerWave[20] = allowedGroupsPerWave[21] = new int[] { 0, 1, 2, 5, 6, 7, 8, 11, 12, 3, 4, 9, 10 };

		// Floor 11, wave 22 & 23
		allowedGroupsPerWave[22] = allowedGroupsPerWave[23] = new int[] { 1, 2, 5, 6, 7, 8, 11, 12, 3, 4, 9, 10 };

		// Floor 12, wave 24 & 25
		allowedGroupsPerWave[24] = allowedGroupsPerWave[25] = new int[] { 5, 6, 7, 8, 11, 12, 3, 4, 9, 10 };

		// Floor 13, wave 26 & 27
		allowedGroupsPerWave[26] = allowedGroupsPerWave[27] = new int[] { 7, 8, 11, 12, 4, 9, 10 };
		// Floor 14, wave 28 & 29
		allowedGroupsPerWave[28] = allowedGroupsPerWave[29] = new int[] { 7, 8, 11, 12, 4, 9, 10 };

		// Floor 15, wave 30 & 31
		allowedGroupsPerWave[30] = allowedGroupsPerWave[31] = new int[] { 11, 12, 9, 10 };
	}
	#endregion

	// ---------- RETURN INFO ABOUT PROJECTILES OR ENEMIES ----------------------------------------------------+

	#region SELECT GROUP 
	public void SelectGroup(int _wave, bool _bossIsFiring, bool _bossIsFiringJammedGun)
	{
		int maxItemsOnList = 3;
		
		// ----- FIRE A FLAWED PROJECTILE -----

		if (_bossIsFiringJammedGun)
		{
			selectedGroup = 13;
		}

		// ----- SEARCH GROUP UNTIL A CORRECT ONE IS FOUND -----

		else
		{
			bool searchAgain;
			do
			{
				searchAgain = false;

				// ----- SELECT RANDOM GROUP -----

				_wave = (_wave < allowedGroupsPerWave.Length) ? _wave : (allowedGroupsPerWave.Length - 1);

				int selectedIndex = rnd.Next(0, allowedGroupsPerWave[_wave].Length);
				selectedGroup = allowedGroupsPerWave[_wave][selectedIndex];

				// ----- CHECK CORRECT TYPE -----

				// Count previous groups with same type
				int repeated_TOP_ONE = 0;
				int repeated_TOP_ANY = 0;
				int repeated_BOTTOM_ONE = 0;

				foreach (GroupType type in previousGroupTypes)
				{
					repeated_TOP_ONE += (type == GroupType.TOP_ONE) ? 1 : 0;
					repeated_TOP_ANY += (type == GroupType.TOP_ANY) ? 1 : 0;
					repeated_BOTTOM_ONE += (type == GroupType.BOTTOM_ONE) ? 1 : 0;
				}

				// Type TOP_ANY is allowed if there aren't three dodgable obstacles in a row
				if (groupTypes[selectedGroup] == GroupType.TOP_ANY)
				{
					if (repeated_TOP_ANY == 2 || (repeated_TOP_ANY == 1 && repeated_TOP_ONE == 1))
					{
						searchAgain = true;
					}
				}

				// Type TOP_ONE is allowed once every four obstacles
				// Type TOP_ONE is allowed if there aren't three dodgable obstacles in a row
				else if (groupTypes[selectedGroup] == GroupType.TOP_ONE)
				{
					if (repeated_TOP_ONE == 1 || repeated_TOP_ANY == 2)
					{
						searchAgain = true;
					}
				}

				// Type BOTTOM_ONE is allowed once every four obstacles
				// Exception: It is always allowed on wave 0
				else if ((groupTypes[selectedGroup] == GroupType.BOTTOM_ONE) && (_wave > 0))
				{
					if (repeated_BOTTOM_ONE == 1)
					{
						searchAgain = true;
					}

				}

			} while (/*false && */searchAgain);
		}

		// ----- UPDATE LIST -----

		// Add group to list
		previousGroupTypes.Add(groupTypes[selectedGroup]);

		// Remove first item from list
		if (previousGroupTypes.Count > maxItemsOnList)
		{
			previousGroupTypes.RemoveAt(0);
		}
	}
	#endregion

	#region CHECK CORRECT GROUP, ACCORDING TO WAVE
	public bool CheckCorrectGroup_AccordingToWave(int _group, int _wave)
	{
		if (_wave < allowedGroupsPerWave.Length)
		{
			// Loop groups in wave
			for (int i = 0; i < allowedGroupsPerWave[_wave].Length; i++)
			{
				if (allowedGroupsPerWave[_wave][i] == _group)
				{
					// Return true if group is allowed in current wave
					return true;
				}
			}
		}

		else
		{
			int finalWave = allowedGroupsPerWave.Length - 1;
			
			// Loop groups in final wave 
			for (int i = 0; i < allowedGroupsPerWave[finalWave].Length; i++)
			{
				if (allowedGroupsPerWave[finalWave][i] == _group)
				{
					// Return true if group is allowed in current wave
					return true;
				}
			}
		}
		
		// Return false if group is not allowed in current wave
		return false;
	}
	#endregion

	#region GET TOTAL OBSTACLES
	public int GetTotalObstacles()
	{
		return groups[selectedGroup].Length;
	}
	#endregion

	#region GET BOSS FIRING POSITION Y
	public float GetBossFiringPositionY()
	{
		return bossFiringPositions[selectedGroup];
	}
	#endregion

	#region GET OBSTACLE TYPE
	public int GetObstacleType(int _projIndex)
	{
		if (groups[selectedGroup][_projIndex].type == ObstacleType.SMALL)
		{
			return 0;
		}

		else if (groups[selectedGroup][_projIndex].type == ObstacleType.BIG)
		{
			return 1;
		}

		else if (groups[selectedGroup][_projIndex].type == ObstacleType.MINION)
		{
			return 2;
		}

		else
		{
			return 3;
		}
	}
	#endregion

	#region GET PROJECTILE POSITION
	public Vector2 GetProjectilePosition(int _projIndex)
	{
		return groups[selectedGroup][_projIndex].position;
	}
	#endregion

	#region GET BOSS VERTICAL FIRING POSITION
	public float GeBossFiringPositionY()
	{
		return projectileAllowedPositionsY[selectedGroup];

	}
	#endregion
}
