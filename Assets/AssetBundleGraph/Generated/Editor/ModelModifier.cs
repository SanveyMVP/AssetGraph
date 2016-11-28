using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomModifier("ModelModifier", typeof(ModelImporter))]
public class MyModifier : AssetBundleGraph.IModifier {

	[SerializeField]
	private bool disableShadows = true;

	// Test if asset is different from intended configuration 
	public bool IsModified(object asset) {
		return asset is GameObject && ((GameObject)asset).GetComponent<MeshRenderer>() != null;
	}

	// Actually change asset configurations. 
	public void Modify(object asset) {
		var meshRenderer = ((GameObject)asset).GetComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		meshRenderer.useLightProbes = false;
		meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
	}

	// Draw inspector gui 
	public void OnInspectorGUI(Action onValueChanged) {
		GUILayout.Label("Model Modifiers", EditorStyles.largeLabel);
		EditorGUILayout.Space();

		var newValue = EditorGUILayout.Toggle("Disable Mesh Renderer Shadows", disableShadows);
		if(newValue != disableShadows) {
			disableShadows = newValue;
			onValueChanged();
		}
	}

	// serialize this class to JSON 
	public string Serialize() {
		return JsonUtility.ToJson(this);
	}
}
