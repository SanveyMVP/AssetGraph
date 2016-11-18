using UnityEditor;
using System.Collections;
using UnityEngine;
using AssetBundleGraph;
using System.Collections.Generic;
using System;
using System.IO;

public class PreProcessor : AssetPostprocessor {
	private LoaderSaveData loaderSaveData = null;

	public LoaderSaveData LoaderData {
		get {
			if(loaderSaveData == null) {
				loaderSaveData = AssetBundleGraph.LoaderSaveData.LoadFromDisk();
			}
			return loaderSaveData;
		}
	}

	private SaveData fullSaveData = null;

	public SaveData FullSaveData {
		get {
			if(fullSaveData == null) {
				fullSaveData = SaveData.LoadFromDisk();
			}
			return fullSaveData;
		}
	}

	void GenericPreProcessing() {
		var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

		if(asset == null) { // This means it is the first time importing this asset.
			var loader = LoaderData.GetBestLoaderData(assetPath);
			if(loader != null && loader.isPreProcess) {
				try {
					var currentCount = 0.00f;
					var totalCount = FullSaveData.Graph.Nodes.Count * 1f;
					Action<NodeData, float> updateHandler = (node, progress) => {
						var progressPercentage = ((currentCount / totalCount) * 100).ToString();
						if(progressPercentage.Contains(".")) progressPercentage = progressPercentage.Split('.')[0];

						if(0 < progress) {
							currentCount = currentCount + 1f;
						}

						EditorUtility.DisplayProgressBar("AssetBundleGraph Processing... ", "Processing " + node.Name + ": " + progressPercentage + "%", currentCount / totalCount);
					};
					List<NodeException> errors = new List<NodeException>();
					Action<NodeException> errorHandler = (NodeException e) => {
						errors.Add(e);
					};

					var target = EditorUserBuildSettings.activeBuildTarget;

					var loaderNodeData = FullSaveData.Graph.Nodes.Find(x => x.Id == loader.id);
					var graph = FullSaveData.Graph.GetSubGraph(loaderNodeData);

					// perform setup. Fails if any exception raises.
					AssetBundleGraphController.Perform(graph, target, false, errorHandler, null);

					// if there is not error reported, then run
					if(errors.Count == 0) {
						var fakeLoaders = new Dictionary<string, List<string>>();
						var assetPathList = new List<string>();
						var absPath = Path.GetFullPath(assetPath);

						if(Path.DirectorySeparatorChar != AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR) {
							absPath = absPath.Replace(Path.DirectorySeparatorChar.ToString(), AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR.ToString());
						}
						assetPathList.Add(absPath);

						fakeLoaders.Add(loaderNodeData.Id, assetPathList);
						// run datas.                
						AssetBundleGraphController.Perform(graph, target, true, errorHandler, updateHandler, fakeLoaders, assetImporter);
					}

					if(errors.Count > 0) {
						Debug.LogError(errors[0]);
					}
				} catch(Exception e) {
					Debug.LogError(e);
				} finally {
					EditorUtility.ClearProgressBar();
				}
			}
		}
	}

	void OnPreprocessTexture() {
		GenericPreProcessing();
	}

	void OnPreprocessModel() {
		GenericPreProcessing();
	}

	void OnPreprocessAudio() {
		GenericPreProcessing();
	}
}
