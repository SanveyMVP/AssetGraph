using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("TriCountValidator", typeof(ModelImporter))]
public class TriCountValidator : AssetBundleGraph.IValidator {

	[SerializeField] public int maxTriangleCount;

	private bool isSkinned;
	private int triangleCount;
	private string offendingMesh;

	// Tells the validator if this object should be validated or is an exception.	
	public bool ShouldValidate(object asset) {
		var target = (GameObject)asset;

		var staticMesh = target.GetComponent<MeshFilter>();

		isSkinned = staticMesh == null;

		return !isSkinned || target.GetComponentInChildren<SkinnedMeshRenderer>() != null;
	}


	// Validate things. 
	public bool Validate (object asset) {
		var target = (GameObject)asset;
		bool exceedsMaximum = false;

		if(isSkinned) {
			foreach(SkinnedMeshRenderer skinnedMesh in target.GetComponentsInChildren<SkinnedMeshRenderer>()) {
				triangleCount = skinnedMesh.sharedMesh.triangles.Length / 3;
				exceedsMaximum = triangleCount > maxTriangleCount;
				offendingMesh = skinnedMesh.name;

				if(exceedsMaximum) {
					break;
				}
			}
		} else {
			triangleCount = target.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
			offendingMesh = target.name;
			exceedsMaximum = triangleCount > maxTriangleCount;
		}

		return !exceedsMaximum;
	}

	
	//When the validation fails you can try to recover in here and return if it is recovered
	public bool TryToRecover(object asset) {
		return false;
	}

	
	// When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
	public string ValidationFailed(object asset) {
		var target = (GameObject)asset;

		return "The mesh " + offendingMesh + " of " + AssetDatabase.GetAssetPath(target) + " has " + triangleCount + " triangles, this exceeds the maximum threshold of " + maxTriangleCount;
	}


	// Draw inspector gui 
	public void OnInspectorGUI (Action onValueChanged) {
		GUILayout.Label("Model Triangles Count Validator", EditorStyles.largeLabel);
		EditorGUILayout.Space();

		var newValue = EditorGUILayout.IntField("Max Triangles Count", maxTriangleCount);
		if(newValue != maxTriangleCount) {
			maxTriangleCount = newValue;
			onValueChanged();
		}
	}

	// serialize this class to JSON 
	public string Serialize() {
		return JsonUtility.ToJson(this);
	}
}
