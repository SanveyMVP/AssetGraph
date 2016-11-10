using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssetBundleGraph {

	class SaveDataConstants {
		/*
			data key for AssetBundleGraph.json
		*/

		public const string GROUPING_KEYWORD_DEFAULT = "/Group_*/";
		public const string BUNDLECONFIG_BUNDLENAME_TEMPLATE_DEFAULT = "*";

		// by default, AssetBundleGraph's node has only 1 InputPoint. and 
		// this is only one definition of it's label.
		public const string DEFAULT_INPUTPOINT_LABEL = "-";
		public const string DEFAULT_OUTPUTPOINT_LABEL = "+";
		public const string BUNDLECONFIG_BUNDLE_OUTPUTPOINT_LABEL = "bundles";
		public const string BUNDLECONFIG_VARIANTNAME_DEFAULT = "";

		public const string DEFAULT_FILTER_KEYWORD = "keyword";
		public const string DEFAULT_FILTER_KEYTYPE = "Any";

		public const string FILTER_KEYWORD_WILDCARD = "*";

		public const string NODE_INPUTPOINT_FIXED_LABEL = "FIXED_INPUTPOINT_ID";
	}

	/*
	 * Json save data which holds all AssetBundleGraph settings and configurations.
	 */ 
	public class SaveData {

		public const string LASTMODIFIED 	= "lastModified";
		public const string NODES 			= "nodes";
		public const string CONNECTIONS 	= "connections";

		private Dictionary<string, object> m_jsonData;

		private List<NodeData> m_allNodes;
		private List<ConnectionData> m_allConnections;
		private DateTime m_lastModified;

		private LoaderSaveData loaderSaveData = new LoaderSaveData();

		public SaveData() {
			m_lastModified = DateTime.UtcNow;
			m_allNodes = new List<NodeData>();
			m_allConnections = new List<ConnectionData>();
		}

		public SaveData(Dictionary<string, object> jsonData) {
			m_jsonData = jsonData;
			m_allNodes = new List<NodeData>();
			m_allConnections = new List<ConnectionData>();

			m_lastModified = Convert.ToDateTime(m_jsonData[LASTMODIFIED] as string);

			var nodeList = m_jsonData[NODES] as List<object>;
			var connList = m_jsonData[CONNECTIONS] as List<object>;

			foreach(var n in nodeList) {
				m_allNodes.Add(new NodeData(n as Dictionary<string, object>));
			}

			foreach(var c in connList) {
				m_allConnections.Add(new ConnectionData(c as Dictionary<string, object>));
			}
		}

		public SaveData(List<NodeGUI> nodes, List<ConnectionGUI> connections) {
			m_jsonData = null;

			m_lastModified = DateTime.UtcNow;
			m_allNodes = nodes.Select(n => n.Data).ToList();
			m_allConnections = new List<ConnectionData>();

			foreach(var cgui in connections) {
				m_allConnections.Add(new ConnectionData(cgui));
			}
		}

		public DateTime LastModified {
			get {
				return m_lastModified;
			}
		}

		public List<NodeData> Nodes {
			get{ 
				return m_allNodes;
			}
		}

		public List<ConnectionData> Connections {
			get{ 
				return m_allConnections;
			}
		}

		private Dictionary<string, object> ToJsonDictionary() {

			var nodeList = new List<Dictionary<string, object>>();
			var connList = new List<Dictionary<string, object>>();

			foreach(NodeData n in m_allNodes) {
				nodeList.Add(n.ToJsonDictionary());
			}

			foreach(ConnectionData c in m_allConnections) {
				connList.Add(c.ToJsonDictionary());
			}

			return new Dictionary<string, object>{
				{LASTMODIFIED, m_lastModified.ToString()},
				{NODES, nodeList},
				{CONNECTIONS, connList}
			};
		}


		public List<NodeData> CollectAllLeafNodes() {

			var nodesWithChild = new List<NodeData>();
			foreach(var c in m_allConnections) {
				NodeData n = m_allNodes.Find(v => v.Id == c.FromNodeId);
				if(n != null) {
					nodesWithChild.Add(n);
				}
			}
			return m_allNodes.Except(nodesWithChild).ToList();
		}

		public List<NodeData> CollectAllNodes(Predicate<NodeData> condition) { 
			return m_allNodes.FindAll(condition);
		}

		//
		// Save/Load to disk
		//

		public static string SaveDataDirectoryPath {
			get {
				return FileUtility.PathCombine(Application.dataPath, AssetBundleGraphSettings.ASSETNBUNDLEGRAPH_DATA_PATH);
			}
		}

		private static string SaveDataPath {
			get {
				return FileUtility.PathCombine(SaveDataDirectoryPath, AssetBundleGraphSettings.ASSETBUNDLEGRAPH_DATA_NAME);
			}
		}


		public void Save () {
			var dir = SaveDataDirectoryPath;
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			m_lastModified = DateTime.UtcNow;

			var dataStr = Json.Serialize(ToJsonDictionary());
			var prettified = Json.Prettify(dataStr);

			using (var sw = new StreamWriter(SaveDataPath)) {
				sw.Write(prettified);
			}
			
			loaderSaveData.UpdateLoaderData(CollectAllNodes(x=>x.Kind == NodeKind.LOADER_GUI));
			loaderSaveData.Save();

			// reflect change of data.
			AssetDatabase.Refresh();
		}

		private Dictionary<string, object> ToJsonRootNodes() {
			Dictionary<string, object> res = new Dictionary<string, object>();

			var rootNodes = CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI && x.LoaderLoadPath != null);

			foreach(var loaderNode in rootNodes) {
				res.Add(loaderNode.Id, loaderNode.LoaderLoadPath.ToJsonDictionary());
			}

			return res;
		}

		public static bool IsSaveDataAvailableAtDisk() {
			return File.Exists(SaveDataPath);
		}

		/// <summary>
		/// Finds the best suitable loader for the provided asset path
		/// </summary>
		/// <param name="path">Path of the asset</param>
		/// <returns>LoaderData of the nearest LoaderFolder, null if none are suitable</returns>
		public NodeData GetBestLoaderData(string assetPath) {
			NodeData res = null;
			var nodes = CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI);

			foreach(NodeData node in nodes) {
				if(assetPath.Contains(node.LoaderLoadPath.CurrentPlatformValue)) {
					if(res == null || res.LoaderLoadPath.CurrentPlatformValue.Length < node.LoaderLoadPath.CurrentPlatformValue.Length) {
						res = node;
					}
				}
			}

			return res;
		}

		private static SaveData Load() {
			var dataStr = string.Empty;
			using (var sr = new StreamReader(SaveDataPath)) {
				dataStr = sr.ReadToEnd();
			}
			var deserialized = AssetBundleGraph.Json.Deserialize(dataStr) as Dictionary<string, object>;
			return new SaveData(deserialized);
		}

		public static SaveData RecreateDataOnDisk () {
			SaveData newSaveData = new SaveData();
			newSaveData.Save();
			return newSaveData;
		}
			
		public static SaveData LoadFromDisk() {

			if(!IsSaveDataAvailableAtDisk()) {
				return RecreateDataOnDisk ();
			} 

			try {
				SaveData saveData = Load();
				if(!saveData.Validate()) {
					saveData.Save();

					// reload and construct again from disk
					return Load();
				} 
				else {
					return saveData;
				}
			} catch (Exception e) {
				Debug.LogError("Failed to deserialize AssetBundleGraph settings. Error:" + e + " File:" + SaveDataPath);
			}

			return new SaveData();
		}

		/*
		 * Checks deserialized SaveData, and make some changes if necessary
		 * return false if any changes are perfomed.
		 */
		public bool Validate () {
			var changed = false;

			List<NodeData> removingNodes = new List<NodeData>();
			List<ConnectionData> removingConnections = new List<ConnectionData>();

			/*
				delete undetectable node.
			*/
			foreach (var n in m_allNodes) {
				if(!n.Validate(m_allNodes, m_allConnections)) {
					removingNodes.Add(n);
					changed = true;
				}
			}

			foreach (var c in m_allConnections) {
				if(!c.Validate(m_allNodes, m_allConnections)) {
					removingConnections.Add(c);
					changed = true;
				}
			}

			if(changed) {
				Nodes.RemoveAll(n => removingNodes.Contains(n));
				Connections.RemoveAll(c => removingConnections.Contains(c));
				m_lastModified = DateTime.UtcNow;
			}

			return !changed;
		}
	}

	public class LoaderSaveData {
		private const string LOADER_SAVE_DATA = "loaders";
		public class LoaderData {
			private const string LOADER_ID = "id";
			private const string LOADER_PATH = "path";
			private const string LOADER_PREPROCESS = "preprocess";

			public string id;
			public SerializableMultiTargetString paths;
			public bool isPreProcess;

			public LoaderData(string id, SerializableMultiTargetString paths, bool isPreProcess) {
				this.id = id;
				this.paths = paths;
				this.isPreProcess = isPreProcess;
			}

			public LoaderData(Dictionary<string, object> rawData) {
				this.id = rawData[LOADER_ID] as string;
				this.paths = new SerializableMultiTargetString(rawData[LOADER_PATH] as Dictionary<string, object>);
				this.isPreProcess = Convert.ToBoolean(rawData[LOADER_PREPROCESS]);
			}

			public Dictionary<string, object> ToJsonDictionary() {
				Dictionary<string, object> jsonDict = new Dictionary<string, object>();

				jsonDict.Add(LOADER_ID, id);
				jsonDict.Add(LOADER_PATH, paths.ToJsonDictionary());
				jsonDict.Add(LOADER_PREPROCESS, isPreProcess);

				return jsonDict;
			}
		}

		private List<LoaderData> loaders;
		public List<LoaderData> LoaderPaths {
			get {
				return loaders;
			}
		}

		private static string SaveLoaderDataPath {
			get {
				return FileUtility.PathCombine(SaveData.SaveDataDirectoryPath, AssetBundleGraphSettings.ASSETBUNDLEGRAPH_LOADER_DATA_NAME);
			}
		}

		public LoaderSaveData() {
			loaders = new List<LoaderData>();
		}


		public LoaderSaveData(SaveData savedata) {
			var fullLoaders = savedata.CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI);
			UpdateLoaderData(fullLoaders);
		}

		public LoaderSaveData(Dictionary<string, object> rawData) {
			var rawLoaders = rawData[LOADER_SAVE_DATA] as List<object>;
			loaders = new List<LoaderData>();

			foreach(var rawLoader in rawLoaders) {
				loaders.Add(new LoaderData(rawLoader as Dictionary<string, object>));
			}
		}     
		
		public void UpdateLoaderData(List<NodeData> fullLoaders) {
			loaders = fullLoaders.ConvertAll(x => new LoaderData(x.Id, x.LoaderLoadPath, x.PreProcess));
		}

		public void Save() {
			var serializedData = Json.Serialize(ToJsonDictionary());
			var loaderPrettyfied = Json.Prettify(serializedData);

			using(var sw = new StreamWriter(SaveLoaderDataPath)) {
				sw.Write(loaderPrettyfied);
			}
		}

		public Dictionary<string, object> ToJsonDictionary() {
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			List<Dictionary<string, object>> loadersData = new List<Dictionary<string, object>>();

			foreach(var loader in loaders) {
				loadersData.Add(loader.ToJsonDictionary());
			}

			dictionary.Add(LOADER_SAVE_DATA, loadersData);

			return dictionary;
		}
		
		/// <summary>
		/// Finds the best suitable loader for the provided asset path
		/// </summary>
		/// <param name="path">Path of the asset</param>
		/// <returns>LoaderData of the nearest LoaderFolder, null if none are suitable</returns>
		public LoaderData GetBestLoaderData(string assetPath) {
			LoaderData res = null;

			foreach(LoaderData dataPath in loaders) {
				if(assetPath.Contains(dataPath.paths.CurrentPlatformValue)) {                 
					if(res == null || res.paths.CurrentPlatformValue.Length < dataPath.paths.CurrentPlatformValue.Length) {
						res = dataPath;
					}
				}
			}

			return res;
		}

		public static LoaderSaveData RecreateDataOnDisk() {
			LoaderSaveData lSaveData = new LoaderSaveData();
			lSaveData.Save();
			return lSaveData;
		}

		public static LoaderSaveData LoadFromDisk() {
			if(!IsLoaderDataAvailableAtDisk()) {
				return RecreateDataOnDisk();
			}

			try {
				var dataStr = string.Empty;
				using(var sr = new StreamReader(SaveLoaderDataPath)) {
					dataStr = sr.ReadToEnd();
				}
				var deserialized = Json.Deserialize(dataStr) as Dictionary<string, object>;

				return new LoaderSaveData(deserialized);
			} catch(Exception e) {
				Debug.LogError("Failed to deserialize AssetBundleGraph settings. Error:" + e + " File:" + SaveLoaderDataPath);
			}

			return new LoaderSaveData();
		}

		public static bool IsLoaderDataAvailableAtDisk() {
			return File.Exists(SaveLoaderDataPath);
		}
	}
}
