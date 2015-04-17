using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Fez manager creates invisible cubes for the player to move on. The world is based in 3D to allow rotation, this means
/// there are varying levels of depth that the player could be at, these may not line up with the physical platforms we create.
/// The player is moving in 2D, so it looks like a platform is present where one may not be, depending on the depth of the platform
/// and the player. If they are different, we will create invisible cubes that the player can move on to fake the player being on
/// a 2D platform. When we have the chance, we will move the player's depth to the closest platform so when they next rotate
/// it will not disorient them.
/// </summary>
public class FezManager : MonoBehaviour {
	//Script that controls the player sprite movement and animation control
	private FezMove fezMove;
	
	//Keeps track of the direction our player is oriented
	public FacingDirection facingDirection;
	
	//Access to the player gameObject, useful for getting spacial coordinates
	public GameObject Player;
	
	//Used to tell the FezMove script how much to rotate 90 or -90 degrees depending on input
	private float degree = 0;
	
	//Access to the Transform containing our Level Data - Platforms we can walk on
	public Transform Level;
	
	//Access to the Transform containing our Building Data - There for asthetics but we don't plan to move on it
	public Transform Building;
	
	//For simplicity we will use a cube with a collider that has the mesh renderer disabled. This will allow us to
	//create places for the player to walk when the player depth is different than the platform on which they
	//appear to be standing.
	public GameObject InvisiCube;
	
	//Holds our InvisiCubes so we can look at their locations or delete them
	private List<Transform> InvisiList = new List<Transform>();
	
	//Keeps track of the facing direction from the last frame, helps prevent us from needlessly re-building the location
	//of our Invisicubes
	private FacingDirection lastfacing;
	//Keeps track of the player depth from the last frame, helps prevent us from needlessly re-building the location
	//of our Invisicubes
	private float lastDepth = 0f;
	
	//Dimensions of cubes used - so far only tested with 1. This could potentially be updated if cubes of a different
	//size are needed - Note: All cubes must be same size
	public float WorldUnits = 1.000f;
	
	// Use this for initialization
	void Start () {
		
		//Define our facing direction, must be the same as built in inspector
		//Cache our fezMove script located on the player and update our level data (create invisible cubes)
		facingDirection = FacingDirection.Front;
		fezMove = Player.GetComponent<FezMove> ();
		UpdateLevelData (true);
	}
	
	// Update is called once per frame
	void Update () {
		
		//Logic to control the player depth
		//If we're on an invisible platform, move to a physical platform, this comes in handy to make rotating possible
		//Try to move us to the closest platform to the camera, will help when rotating to feel more natural
		//If we changed anything, update our level data which pertains to our inviscubes
		if(!fezMove._jumping)
		{
			bool updateData = false;
			if(OnInvisiblePlatform())
				if(MovePlayerDepthToClosestPlatform())
					updateData = true;
			if(MoveToClosestPlatformToCamera())
				updateData = true;
			if(updateData)
				UpdateLevelData(false);
			
			
		}
		
		
		//Handle Player input for rotation command
		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			//If we rotate while on an invisible platform we must move to a physical platform
			//If we don't, then we could be standing in mid air after the rotation
			if(OnInvisiblePlatform())
			{
				//MoveToClosestPlatform();
				MovePlayerDepthToClosestPlatform();
				
			}
			lastfacing = facingDirection;
			facingDirection = RotateDirectionRight();
			degree-=90f;
			UpdateLevelData(false);
			fezMove.UpdateToFacingDirection(facingDirection, degree);
			
			
			
		}
		else if( Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if(OnInvisiblePlatform())
			{
				//MoveToClosestPlatform();
				MovePlayerDepthToClosestPlatform();
				
			}
			lastfacing = facingDirection;
			facingDirection = RotateDirectionLeft();
			degree+=90f;
			UpdateLevelData(false);
			fezMove.UpdateToFacingDirection(facingDirection, degree);
			
			
		}
	}
	/// <summary>
	/// Destroy current invisible platforms
	/// Create new invisible platforms taking into account the
	/// player's facing direction and the orthographic view of the
	/// platforms
	/// </summary>
	private void UpdateLevelData(bool forceRebuild)
	{
		//If facing direction and depth havent changed we do not need to rebuild
		if(!forceRebuild)
			if (lastfacing == facingDirection && lastDepth == GetPlayerDepth ())
				return;
		foreach(Transform tr in InvisiList)
		{
			//Move obsolete invisicubes out of the way and delete
			
			tr.position = Vector3.zero;
			Destroy(tr.gameObject);
			
		}
		InvisiList.Clear ();
		float newDepth = 0f;
		
		newDepth = GetPlayerDepth ();
		CreateInvisicubesAtNewDepth (newDepth);
		
		
	}
	/// <summary>
	/// Returns true if the player is standing on an invisible platform
	/// </summary>
	private bool OnInvisiblePlatform()
	{
		foreach(Transform item in InvisiList)
		{
			
			if(Mathf.Abs(item.position.x - fezMove.transform.position.x) < WorldUnits && Mathf.Abs(item.position.z - fezMove.transform.position.z) < WorldUnits)
				if(fezMove.transform.position.y - item.position.y <= WorldUnits + 0.2f && fezMove.transform.position.y - item.position.y >0)
					return true;
			
			
			
		}
		return false;
	}
	/// <summary>
	/// Moves the player to the closest platform with the same height to the camera
	/// Only supports Unity cubes of size (1x1x1)
	/// </summary>
	private bool MoveToClosestPlatformToCamera()
	{
		bool moveCloser = false;
		foreach(Transform item in Level)
		{
			if(facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
			{
				
				//When facing Front, find cubes that are close enough in the x position and the just below our current y value
				//This would have to be updated if using cubes bigger or smaller than (1,1,1)
				if(Mathf.Abs(item.position.x - fezMove.transform.position.x) < WorldUnits +0.1f)
				{
					
					if(fezMove.transform.position.y - item.position.y <= WorldUnits + 0.2f && fezMove.transform.position.y - item.position.y >0 && !fezMove._jumping)
					{
						if(facingDirection == FacingDirection.Front && item.position.z < fezMove.transform.position.z)
							moveCloser = true;
						
						if(facingDirection == FacingDirection.Back && item.position.z > fezMove.transform.position.z)
							moveCloser = true;
						
						
						if(moveCloser)
						{
							
							fezMove.transform.position = new Vector3(fezMove.transform.position.x, fezMove.transform.position.y, item.position.z);
							return true;
						}
					}
					
				}
				
			}
			else{
				if(Mathf.Abs(item.position.z - fezMove.transform.position.z) < WorldUnits + 0.1f)
				{
					if(fezMove.transform.position.y - item.position.y <= WorldUnits + 0.2f && fezMove.transform.position.y - item.position.y >0 && !fezMove._jumping)
					{
						if(facingDirection == FacingDirection.Right && item.position.x > fezMove.transform.position.x)
							moveCloser = true;
						
						if(facingDirection == FacingDirection.Left && item.position.x < fezMove.transform.position.x)
							moveCloser = true;
						
						if(moveCloser)
						{
							fezMove.transform.position = new Vector3(item.position.x, fezMove.transform.position.y, fezMove.transform.position.z);
							return true;
						}
						
					}
					
				}
			}
			
			
		}
		return false;
	}
	
	
	/// <summary>
	/// Looks for an invisicube in InvisiList at position 'cube'
	/// </summary>
	/// <returns><c>true</c>, if transform invisi list was found, <c>false</c> otherwise.</returns>
	/// <param name="cube">Cube position.
	private bool FindTransformInvisiList(Vector3 cube)
	{
		foreach(Transform item in InvisiList)
		{
			if(item.position == cube)
				return true;
		}
		return false;
		
	}
	/// <summary>
	/// Looks for a physical (visible) cube in our level data at position 'cube'
	/// </summary>
	/// <returns><c>true</c>, if transform level was found, <c>false</c> otherwise.</returns>
	/// <param name="cube">Cube.
	private bool FindTransformLevel(Vector3 cube)
	{
		foreach(Transform item in Level)
		{
			if(item.position == cube)
				return true;
			
		}
		return false;
		
	}
	/// <summary>
	/// Determines if any building cubes are between the "cube"
	/// and the camera
	/// </summary>
	/// <returns><c>true</c>, if transform building was found, <c>false</c> otherwise.</returns>
	/// <param name="cube">Cube.
	private bool FindTransformBuilding(Vector3 cube)
	{
		foreach(Transform item in Building)
		{
			if(facingDirection == FacingDirection.Front )
			{
				if(item.position.x == cube.x && item.position.y == cube.y && item.position.z < cube.z)
					return true;
			}
			else if(facingDirection == FacingDirection.Back )
			{
				if(item.position.x == cube.x && item.position.y == cube.y && item.position.z > cube.z)
					return true;
			}
			else if(facingDirection == FacingDirection.Right )
			{
				if(item.position.z == cube.z && item.position.y == cube.y && item.position.x > cube.x)
					return true;
				
			}
			else
			{
				if(item.position.z == cube.z && item.position.y == cube.y && item.position.x < cube.x)
					return true;
				
			}
		}
		return false;
		
	}
	
	/// <summary>
	/// Moves player to closest platform with the same height
	/// Intended to be used when player jumps onto an invisible platform
	/// </summary>
	private bool MovePlayerDepthToClosestPlatform()
	{
		foreach(Transform item in Level)
		{
			
			if(facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
			{
				if(Mathf.Abs(item.position.x - fezMove.transform.position.x) < WorldUnits + 0.1f)
					if(fezMove.transform.position.y - item.position.y <= WorldUnits + 0.2f && fezMove.transform.position.y - item.position.y >0)
				{
					
					fezMove.transform.position = new Vector3(fezMove.transform.position.x, fezMove.transform.position.y, item.position.z);
					return true;
					
				}
			}
			else
			{
				if(Mathf.Abs(item.position.z - fezMove.transform.position.z) < WorldUnits + 0.1f)
					if(fezMove.transform.position.y - item.position.y <= WorldUnits + 0.2f && fezMove.transform.position.y - item.position.y >0)
				{
					
					fezMove.transform.position = new Vector3(item.position.x, fezMove.transform.position.y, fezMove.transform.position.z);
					return true;
				}
			}
		}
		return false;
		
	}
	/// <summary>
	/// Creates an invisible cube at position
	/// Invisicubes are used as a place to land because our current
	/// depth level in 3 dimensions may not be aligned with a physical platform
	/// </summary>
	/// <returns>The invisicube.</returns>
	/// <param name="position">Position.
	private Transform CreateInvisicube(Vector3 position)
	{
		GameObject go = Instantiate (InvisiCube) as GameObject;
		
		go.transform.position = position;
		
		return go.transform;
		
	}
	/// <summary>
	/// Creates invisible cubes for the player to move on
	/// if the physical cubes that make up a platform
	/// are on a different depth
	/// </summary>
	/// <param name="newDepth">New depth.
	private void CreateInvisicubesAtNewDepth(float newDepth)
	{
		
		Vector3 tempCube = Vector3.zero;
		foreach(Transform child in Level)
		{
			
			if(facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
			{
				tempCube = new Vector3(child.position.x, child.position.y, newDepth);
				if(!FindTransformInvisiList(tempCube) && !FindTransformLevel(tempCube) && !FindTransformBuilding(child.position))
				{
					
					Transform go = CreateInvisicube(tempCube);
					InvisiList.Add(go);
				}
				
			}
			//z and y must match a level cube
			else if(facingDirection == FacingDirection.Right || facingDirection == FacingDirection.Left)
			{
				tempCube = new Vector3(newDepth, child.position.y, child.position.z);
				if(!FindTransformInvisiList(tempCube) && !FindTransformLevel(tempCube) && !FindTransformBuilding(child.position))
				{
					
					Transform go = CreateInvisicube(tempCube);
					InvisiList.Add(go);
				}
				
			}
			
			
		}
		
		
	}
	/// <summary>
	/// Any actions required if player returns to start
	/// </summary>
	public void ReturnToStart()
	{
		
		UpdateLevelData (true);
	}
	/// <summary>
	/// Returns the player depth. Depth is how far from or close you are to the camera
	/// If we're facing Front or Back, this is Z
	/// If we're facing Right or Left it is X
	/// </summary>
	/// <returns>The player depth.</returns>
	private float GetPlayerDepth()
	{
		float ClosestPoint = 0f;
		
		if(facingDirection == FacingDirection.Front || facingDirection == FacingDirection.Back)
		{
			ClosestPoint = fezMove.transform.position.z;
			
		}
		else if(facingDirection == FacingDirection.Right || facingDirection == FacingDirection.Left)
		{
			ClosestPoint = fezMove.transform.position.x;
		}
		
		
		return Mathf.Round(ClosestPoint);
		
	}
	
	
	/// <summary>
	/// Determines the facing direction after we rotate to the right
	/// </summary>
	/// <returns>The direction right.</returns>
	private FacingDirection RotateDirectionRight()
	{
		int change = (int)(facingDirection);
		change++;
		//Our FacingDirection enum only has 4 states, if we go past the last state, loop to the first
		if (change > 3)
			change = 0;
		return (FacingDirection) (change);
	}
	/// <summary>
	/// Determines the facing direction after we rotate to the left
	/// </summary>
	/// <returns>The direction left.</returns>
	private FacingDirection RotateDirectionLeft()
	{
		int change = (int)(facingDirection);
		change--;
		//Our FacingDirection enum only has 4 states, if we go below the first, go to the last state
		if (change < 0)
			change = 3;
		return (FacingDirection) (change);
	}
	
}
//Used frequently to keep track of the orientation of our player and camera
public enum FacingDirection
{
	Front = 0,
	Right = 1,
	Back = 2,
	Left = 3
	
}