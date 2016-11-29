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
	private LoaderSaveData.LoaderData loader = null;

	private bool IsValid {
		get {
			return Directory.Exists(path) && !(path + "/").Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH);
		}
	}


	protected void OnEnable() {
		path = AssetDatabase.GetAssetPath(target);

		if(IsValid) {
			CheckForLoader();
		}
	}

	private void CheckForLoader() {
		LoaderSaveData loaderSaveData = LoaderSaveData.LoadFromDisk();
		loader = loaderSaveData.GetBestLoaderData(path);
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if(IsValid) {
			bool perfectMatch = false;

			if(loader != null) {
				GUI.enabled = true;
				var folderConfigured = loader.paths.CurrentPlatformValue;
				if(folderConfigured == string.Empty) {
					folderConfigured = "Global";
				} else {
					folderConfigured = "Assets/" + folderConfigured;
				}

				perfectMatch = folderConfigured == path;

				string message = perfectMatch ? "This folder is configured to use the Graph Importer" : "This folder is inheriting a Graph Importer configuration from " + folderConfigured;
				string buttonMsg = perfectMatch ? "Open Folder Graph" : "Open Inherited graph from " + folderConfigured;

				EditorGUILayout.HelpBox(message, MessageType.Info);
				EditorGUILayout.Space();


				if(GUILayout.Button(buttonMsg)) {
					AssetBundleGraphEditorWindow.SelectAllRelatedTree(new string[] { loader.id });
				}

				if(GUILayout.Button("Run this Subgraph")) {
					var nodeIDs = new List<string>();
					nodeIDs.Add(loader.id);
					AssetBundleGraphEditorWindow.OpenAndRunSelected(new string[] { loader.id });
				}

			}

			if(!perfectMatch) {
				if(GUILayout.Button("Setup Graph Loader for this folder")){
					AssetBundleGraphEditorWindow.OpenAndCreateLoader(path);
					CheckForLoader();
				}
			}
		}		
	}
}
