using UnityEngine;
using System.Collections.Generic;

namespace AssetBundleGraph {
	/*
	 * ScriptableObject helper object to let ConnectionGUI edit from Inspector
	 */
	public class ConnectionGUIInspectorHelper : ScriptableObject {
		public ConnectionGUI connectionGUI;
		public Dictionary<string, List<Asset>> assetGroups;
		public List<bool> foldouts;
		public bool isActive = false;

		public void UpdateInspector (ConnectionGUI con, Dictionary<string, List<Asset>> assetGroups) {
			this.connectionGUI = con;
			this.assetGroups = assetGroups;

			this.foldouts = new List<bool>();
			if(assetGroups != null) {
				for (var i = 0; i < this.assetGroups.Count; i++) {
					foldouts.Add(true);
				}
			}
		}

		public void UpdateAssetGroups(Dictionary<string, List<Asset>> assetGroups) {
			this.assetGroups = assetGroups;
		}
	}
}
