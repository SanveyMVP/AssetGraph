using UnityEngine;
using UnityEditor;
using System.IO;
using AssetBundleGraph;
using System.Collections.Generic;
using System.Linq;
using System;

[CustomEditor(typeof(DefaultAsset))]
public class FolderInspector : Editor {

	private string path;
	private string loaderId = null;	

	private bool IsValid {
		get {
			return Directory.Exists(path);
		}
	}

	protected void OnEnable() {
		path = AssetDatabase.GetAssetPath(target);

		if(IsValid) {
			LoaderSaveData loaderSaveData = LoaderSaveData.LoadFromDisk();
			var loader = loaderSaveData.GetBestLoaderData(path);
			if(loader != null) {
				loaderId = loader.id;
			}
		}
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if(loaderId != null) {
			GUI.enabled = true;
			if(GUILayout.Button("Open Graph")) {

				AssetBundleGraphEditorWindow.SelectAllRelatedTree(loaderId);
			}
		}
	}
}
