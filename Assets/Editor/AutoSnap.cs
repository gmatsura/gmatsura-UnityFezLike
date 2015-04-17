using UnityEngine;
using UnityEditor;
using System.Collections;
public class AutoSnap : EditorWindow
{
	private Vector3 prevPosition;
	private bool doSnap = true;
	private float snapValue = 1;
	private bool Initi = false;
	[MenuItem( "Edit/Auto Snap %_l" )]
	static void Init()
	{

		var window = (AutoSnap)EditorWindow.GetWindow( typeof( AutoSnap ) );
		window.maxSize = new Vector2( 200, 100 );
	}
	void Start()
	{


	}
	public void OnGUI()
	{
		if(!Initi)
		{
			SceneView.onSceneGUIDelegate += SceneGUI;
			Initi = true;
		}
		doSnap = EditorGUILayout.Toggle( "Auto Snap", doSnap );
		snapValue = EditorGUILayout.FloatField( "Snap Value", snapValue );
	}
	public void SceneGUI(SceneView sceneView)
	{
		if ( doSnap
		    && !EditorApplication.isPlaying
		    && Selection.transforms.Length > 0
		    && Selection.transforms[0].position != prevPosition )
		{
			Snap();
			prevPosition = Selection.transforms[0].position;
		}
	}
	private void Snap()
	{
		foreach ( var transform in Selection.transforms )
		{
			var t = transform.transform.position;
			t.x = Round( t.x );
			t.y = Round( t.y );
			t.z = Round( t.z );
			transform.transform.position = t;
		}
	}
	private float Round( float input )
	{
		return snapValue * Mathf.Round( ( input / snapValue ) );
	}
} 