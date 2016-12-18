using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor (typeof(SPH))]
public class SPHEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		SPH sph = target as SPH;

		if (GUILayout.Button ("Restart Simulation")) {
			sph.RestartSimulation ();
		}
	}
}
