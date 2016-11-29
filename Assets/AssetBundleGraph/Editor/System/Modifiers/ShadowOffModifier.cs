using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomModifier("ShadowOffModifier", typeof(ModelImporter))]
public class ShadowOffModifier : AssetBundleGraph.IModifier {

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
		EditorGUILayout.HelpBox("This modifier disables shadows in the model GameObject", MessageType.Info);
	}

	// serialize this class to JSON 
	public string Serialize() {
		return JsonUtility.ToJson(this);
	}
}
