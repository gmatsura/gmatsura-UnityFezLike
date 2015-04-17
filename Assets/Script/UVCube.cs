using UnityEngine;
using System.Collections;

public class UVCube : MonoBehaviour {
	private MeshFilter mf;
	public float tileSize = 0.25f;
	
	
	// Use this for initialization
	void Start () {
		
		ApplyTexture ();
		
	}
	
	public void ApplyTexture()
	{
		mf = gameObject.GetComponent<MeshFilter> ();
		if(mf)
		{
			Mesh mesh = mf.sharedMesh;
			if(mesh)
			{
				
				Vector2[] uvs = mesh.uv;
				//FRBLUD - Freeblood
				
				
				// Front
				uvs[0] = new Vector2(0f, 0f); //Bottom Left
				uvs[1] = new Vector2(tileSize, 0f); //Bottom Right
				uvs[2] = new Vector2(0f, 1f); //Top Left
				uvs[3] = new Vector2(tileSize, 1f); // Top Right
				
				// Right
				uvs[20] = new Vector2(tileSize * 1.001f, 0f);
				uvs[22] = new Vector2(tileSize * 2.001f, 0f);
				uvs[23] = new Vector2(tileSize * 1.001f, 1f);
				uvs[21] = new Vector2(tileSize * 2.001f, 1f);
				
				
				// Back
				uvs[10] = new Vector2((tileSize * 2.001f), 1f);
				uvs[11] = new Vector2((tileSize * 3.001f), 1f);
				uvs[6] = new Vector2((tileSize * 2.001f), 0f);
				uvs[7] = new Vector2((tileSize * 3.001f), 0f);
				
				// Left
				uvs[16] = new Vector2(tileSize * 3.001f, 0f);
				uvs[18] = new Vector2(tileSize * 4.001f, 0f);
				uvs[19] = new Vector2(tileSize * 3.001f, 1f);
				uvs[17] = new Vector2(tileSize * 4.001f, 1f);
				
				// Up
				uvs[8] = new Vector2(tileSize * 4.001f, 0f);
				uvs[9] = new Vector2(tileSize * 5.001f, 0f);
				uvs[4] = new Vector2(tileSize * 4.001f, 1f);
				uvs[5] = new Vector2(tileSize * 5.001f, 1f);
				
				
				// Down
				uvs[12] = new Vector2(tileSize * 5.001f, 0f);
				uvs[14] = new Vector2(tileSize * 6.001f, 0f);
				uvs[15] = new Vector2(tileSize * 5.001f, 1f);
				uvs[13] = new Vector2(tileSize * 6.001f, 1f);
				
				
				mesh.uv = uvs;
				
				
			}
		}
		else
			Debug.Log("No mesh filter attached");
		
	}
}
