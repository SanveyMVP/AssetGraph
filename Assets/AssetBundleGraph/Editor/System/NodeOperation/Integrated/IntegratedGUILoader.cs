using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace AssetBundleGraph {
	public class IntegratedGUILoader : INodeOperation {
		public void Setup (BuildTarget target, 
			NodeData node, 
			ConnectionPointData inputPoint,
			ConnectionData connectionToOutput, 
			Dictionary<string, List<Asset>> inputGroupAssets, 
			List<string> alreadyCached, 
			Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output) 
		{
			ValidateLoadPath(
				node.LoaderLoadPath[target],
				node.GetLoaderFullLoadPath(target),
				() => {
					//can be empty
					//throw new NodeException(node.Name + ": Load Path is empty.", node.Id);
				}, 
				() => {
					throw new NodeException(node.Name + ": Directory not found: " + node.GetLoaderFullLoadPath(target), node.Id);
				}
			);

			Load(target, node, connectionToOutput, inputGroupAssets, Output);
		}
		
		public void Run (BuildTarget target, 
			NodeData node, 
			ConnectionPointData inputPoint,
			ConnectionData connectionToOutput, 
			Dictionary<string, List<Asset>> inputGroupAssets, 
			List<string> alreadyCached, 
			Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output) 
		{
			Load(target, node, connectionToOutput, inputGroupAssets, Output);
		}

		void Load (BuildTarget target, 
			NodeData node, 
			ConnectionData connectionToOutput, 
			Dictionary<string, List<Asset>> inputGroupAssets, 
			Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output) 
		{
			// SOMEWHERE_FULLPATH/PROJECT_FOLDER/Assets/
			var assetsFolderPath = Application.dataPath + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR;

			var outputSource = new List<Asset>();
			var targetFilePaths = FileUtility.GetAllFilePathsInFolder(node.GetLoaderFullLoadPath(target));

            var loaderSaveData = LoaderSaveData.LoadFromDisk();
            targetFilePaths.RemoveAll(x => loaderSaveData.GetBestLoaderData(x).id != node.Id);

			foreach (var targetFilePath in targetFilePaths) {

				if(targetFilePath.Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH)) {
					continue;
				}

				// already contained into Assets/ folder.
				// imported path is Assets/SOMEWHERE_FILE_EXISTS.
				if (targetFilePath.StartsWith(assetsFolderPath)) {
					var relativePath = targetFilePath.Replace(assetsFolderPath, AssetBundleGraphSettings.ASSETS_PATH);

					var assetType = TypeUtility.GetTypeOfAsset(relativePath);
					if (assetType == typeof(object)) {
						continue;
					}

					outputSource.Add(Asset.CreateNewAssetFromLoader(targetFilePath, relativePath));
					continue;
				}

				throw new NodeException(node.Name + ": Invalid Load Path. Path must start with Assets/", node.Name);
			}

			var outputDir = new Dictionary<string, List<Asset>> {
				{"0", outputSource}
			};

			Output(connectionToOutput, outputDir, null);
		}


        public void FakeLoad(NodeData node,
            ConnectionData connectionToOutput,
            List<string> fakeAssets,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output,
            AssetImporter preImporter) {

            // SOMEWHERE_FULLPATH/PROJECT_FOLDER/Assets/
            var assetsFolderPath = Application.dataPath + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR;

            var outputSource = new List<Asset>();

            foreach(string targetFilePath in fakeAssets) {

                if(targetFilePath.StartsWith(assetsFolderPath)) {
                    var relativePath = targetFilePath.Replace(assetsFolderPath, AssetBundleGraphSettings.ASSETS_PATH);

                    if(preImporter != null && preImporter.assetPath == relativePath) {
                        outputSource.Add(Asset.CreateNewAssetFromImporter(preImporter));
                        continue;
                    }
                    
                    var assetType = TypeUtility.GetTypeOfAsset(relativePath);
                    if(assetType == typeof(object)) {
                        continue;
                    } 
                    outputSource.Add(Asset.CreateNewAssetFromLoader(targetFilePath, relativePath));
                    
                }else {
                    throw new NodeException(node.Name + ": Invalid Load Path. Path must start with Assets/", node.Name);
                }
            }

            var outputDir = new Dictionary<string, List<Asset>> {
                {"0", outputSource}
            };

            Output(connectionToOutput, outputDir, null);
        }


		public static void ValidateLoadPath (string currentLoadPath, string combinedPath, Action NullOrEmpty, Action NotExist) {
			if (string.IsNullOrEmpty(currentLoadPath)) NullOrEmpty();
			if (!Directory.Exists(combinedPath)) NotExist();
		}
	}
}
