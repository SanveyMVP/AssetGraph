using UnityEditor;
using System.Collections;
using UnityEngine;
using AssetBundleGraph;
using System.Collections.Generic;
using System;
using System.IO;

public class PreProcessor : AssetPostprocessor {
	private static LoaderSaveData loaderSaveData = null;

	public static LoaderSaveData LoaderData {
		get {
			if(loaderSaveData == null) {
				loaderSaveData = AssetBundleGraph.LoaderSaveData.LoadFromDisk();
			}
			return loaderSaveData;
		}
	}

	private static SaveData fullSaveData = null;

	public static SaveData FullSaveData {
		get {
			if(fullSaveData == null) {
				fullSaveData = SaveData.LoadFromDisk();
			}
			return fullSaveData;
		}
	}
	
	private static List<string> preprocessingAssets = new List<string>();
	
	void OnPreprocessTexture() {
		var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
		if(asset == null) {
			preprocessingAssets.Add(assetImporter.assetPath);
		}
	}

	void OnPreprocessModel() {
		var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
		if(asset == null) {
			preprocessingAssets.Add(assetImporter.assetPath);
		}
	}

	void OnPreprocessAudio() {
		var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
		if(asset == null) {
			preprocessingAssets.Add(assetImporter.assetPath);
		}
	}

	static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
		foreach(string path in imported) {
			GenericProcessing(path, false);
		}

		foreach(string path in moved) {
			GenericProcessing(path, true);
		}

		preprocessingAssets.Clear();
	}



	static void GenericProcessing(string path, bool isMoving) {
		var importer = AssetImporter.GetAtPath(path);
		
		var loader = LoaderData.GetBestLoaderData(path);

		bool execute = false;

		if(loader != null){
			if(loader.isPermanent) {
				execute = true;
			}else if(loader.isPreProcess && !isMoving) {
				execute = preprocessingAssets.Contains(path);
			}
		}

		if(execute) {
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
				var graph = FullSaveData.Graph.GetSubGraph(new NodeData[] { loaderNodeData });

				// perform setup. Fails if any exception raises.
				var streamMap = AssetBundleGraphController.Perform(graph, target, false, errorHandler, null);


				// if there is not error reported, then run
				if(errors.Count == 0) {
					var assetPathList = new List<string>();
					var absPath = Path.GetFullPath(path);

					if(Path.DirectorySeparatorChar != AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR) {
						absPath = absPath.Replace(Path.DirectorySeparatorChar.ToString(), AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR.ToString());
					}
					assetPathList.Add(absPath);

					// run datas.                
					streamMap = AssetBundleGraphController.Perform(graph, target, true, errorHandler, updateHandler, importer.assetPath);
				}

				if(errors.Count > 0) {
					Debug.LogError(errors[0]);
				}

				AssetBundleGraphController.Postprocess(graph, streamMap, true);
			} catch(Exception e) {
				Debug.LogError(e);
			} finally {
				EditorUtility.ClearProgressBar();
			}
		}
	}
}
