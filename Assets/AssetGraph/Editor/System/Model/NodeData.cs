using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AssetBundleGraph {

	public enum NodeKind : int {
		LOADER_GUI,
		FILTER_GUI,
		IMPORTSETTING_GUI,
		MODIFIER_GUI,

		GROUPING_GUI,
		PREFABBUILDER_GUI,
		BUNDLECONFIG_GUI,
		BUNDLEBUILDER_GUI,

		EXPORTER_GUI,

		WARP_IN,
		WARP_OUT,

		VALIDATOR_GUI
	}

	public enum ExporterExportOption : int {
		ErrorIfNoExportDirectoryFound,
		AutomaticallyCreateIfNoExportDirectoryFound,
		DeleteAndRecreteExportDirectory
	}

	[Serializable]
	public class FilterEntry {

		[SerializeField] private string m_name;
		[SerializeField] private string m_filterKeyword;
		[SerializeField] private string m_filterKeytype;
		[SerializeField] private bool m_isExclusion;
		[SerializeField] private ConnectionPointData m_point;

		public FilterEntry(string name, string keyword, string keytype, bool isExclusion, ConnectionPointData point) {
			m_name = name;
			m_filterKeyword = keyword;
			m_filterKeytype = keytype;
			m_isExclusion = isExclusion;
			m_point = point;
			m_point.Label = name;
			m_point.LabelColor = isExclusion ? NodeData.FILTER_EXCLUDED_OUTPUT : NodeData.DEFAULT_COLOR;
		}

		public string Name {
			get {
				return m_name;
			}
			set {
				m_name = value;
				m_point.Label = value;
			}
		}
		public string FilterKeyword {
			get {
				return m_filterKeyword;
			}
			set {
				m_filterKeyword = value;
			}
		}
		public bool IsExclusion{
			get{
				return m_isExclusion;
			}
			set{
				m_isExclusion = value;
				if(value){
					m_point.LabelColor = NodeData.FILTER_EXCLUDED_OUTPUT;
				}
				else{
					m_point.LabelColor = NodeData.DEFAULT_COLOR;
				}
			}
		}
		public string FilterKeytype {
			get {
				return m_filterKeytype; 
			}
			set {
				m_filterKeytype = value;
			}
		}
		public ConnectionPointData ConnectionPoint {
			get {
				return m_point; 
			}
		}
		public string Hash {
			get {
				return m_filterKeyword+m_filterKeytype+m_isExclusion;
			}
		}
	}

	[Serializable]
	public class Variant {
		[SerializeField] private string m_name;
		[SerializeField] private ConnectionPointData m_point;

		public Variant(string name, ConnectionPointData point) {
			m_name = name;
			m_point = point;
		}

		public string Name {
			get {
				return m_name;
			}
			set {
				m_name = value;
				m_point.Label = value;
			}
		}
		public ConnectionPointData ConnectionPoint {
			get {
				return m_point; 
			}
		}
	}

	/*
	 * node data saved in/to Json
	 */
	[Serializable]
	public class NodeData {

		private const string NODE_NAME = "name";
		private const string NODE_ID = "id";
		private const string NODE_KIND = "kind";
		private const string NODE_POS = "pos";
		private const string NODE_POS_X = "x";
		private const string NODE_POS_Y = "y";

		private const string NODE_INPUTPOINTS = "inputPoints";
		private const string NODE_OUTPUTPOINTS = "outputPoints";

		//loader settings
		private const string NODE_LOADER_LOAD_PATH = "loadPath";
		private const string NODE_LOADER_PREPROCESS = "preProcess";
		private const string NODE_LOADER_PERMANENT = "permanent";

		//exporter settings
		private const string NODE_EXPORTER_EXPORT_PATH = "exportTo";
		private const string NODE_EXPORTER_EXPORT_OPTION = "exportOption";

		//related
		private const string NODE_RELATED_ID = "relatedNode";

		//filter settings
		private const string NODE_FILTER = "filter";
		private const string NODE_FILTER_NAME = "name";
		private const string NODE_FILTER_KEYWORD = "keyword";
		private const string NODE_FILTER_KEYTYPE = "keytype";
		private const string NODE_FILTER_EXCLUSION = "excludes";
		private const string NODE_FILTER_POINTID = "pointId";

		//group settings
		private const string NODE_GROUPING_KEYWORD = "groupingKeyword";

		//mofidier/prefabBuilder settings
		private const string NODE_SCRIPT_CLASSNAME 		= "scriptClassName";
		private const string NODE_SCRIPT_INSTANCE_DATA  = "scriptInstanceData";

		//bundleconfig settings
		private const string NODE_BUNDLECONFIG_BUNDLENAME_TEMPLATE = "bundleNameTemplate";
		private const string NODE_BUNDLECONFIG_VARIANTS 		 = "variants";
		private const string NODE_BUNDLECONFIG_VARIANTS_NAME 	 = "name";
		private const string NODE_BUNDLECONFIG_VARIANTS_POINTID = "pointId";
		private const string NODE_BUNDLECONFIG_USE_GROUPASVARIANTS = "useGroupAsVariants";
		private const string NODE_BUNDLECONFIG_SET_BUNDLE_NAME = "setBundleNameAndVariant";

		//bundlebuilder settings
		private const string NODE_BUNDLEBUILDER_ENABLEDBUNDLEOPTIONS = "enabledBundleOptions";

		public static readonly Color DEFAULT_COLOR = new Color(0.705f, 0.705f, 0.705f,1);
		public static readonly Color LOADER_PREPROCESS_COLOR = Color.yellow * 0.9f;
		public static readonly Color LOADER_PERMANENT_COLOR = Color.red * 0.9f;
		public static readonly Color FILTER_EXCLUDED_OUTPUT = DEFAULT_COLOR;

		[SerializeField] private string m_name;		
		[SerializeField] private string m_id;
		[SerializeField] private NodeKind m_kind;
		[SerializeField] private float m_x;
		[SerializeField] private float m_y;
		[SerializeField] private string m_scriptClassName;
		[SerializeField] private bool m_isPreProcess;
		[SerializeField] private bool m_isPermanent;
		[SerializeField] private string relatedNodeId;
		[SerializeField] private List<FilterEntry> m_filter;
		[SerializeField] private List<ConnectionPointData> 	m_inputPoints; 
		[SerializeField] private List<ConnectionPointData> 	m_outputPoints;
		[SerializeField] private SerializableMultiTargetString m_loaderLoadPath;
		[SerializeField] private SerializableMultiTargetString m_exporterExportPath;		
		[SerializeField] private SerializableMultiTargetString m_groupingKeyword;
		[SerializeField] private SerializableMultiTargetString m_bundleConfigBundleNameTemplate;
		[SerializeField] private SerializableMultiTargetString m_scriptInstanceData;
		[SerializeField] private List<Variant> m_variants;
		[SerializeField] private bool m_bundleConfigUseGroupAsVariants;
		[SerializeField] private bool m_SetBundleNameAndVariant;
		[SerializeField] private SerializableMultiTargetInt m_bundleBuilderEnabledBundleOptions;
		[SerializeField] private SerializableMultiTargetInt m_exporterExportOption;

		[SerializeField] private bool m_isNodeOperationPerformed;

		[SerializeField] private Color m_name_color;

		/*
		 * Properties
		 */

		public string Name {
			get {
				return m_name;
			}
			set {
				m_name = value;
			}
		}

		public Color NameColor {
			get {
				return m_name_color;
			}
			set {
				m_name_color = value;
			}
		}

		public string Id {
			get {
				return m_id;
			}
		}
		public NodeKind Kind {
			get {
				return m_kind;
			}
		}
		public string ScriptClassName {
			get {
				ValidateAccess(
					NodeKind.PREFABBUILDER_GUI,
					NodeKind.MODIFIER_GUI,
					NodeKind.VALIDATOR_GUI
				);
				return m_scriptClassName;
			}
			set {
				ValidateAccess(
					NodeKind.PREFABBUILDER_GUI,
					NodeKind.MODIFIER_GUI,
					NodeKind.VALIDATOR_GUI
				);
				m_scriptClassName = value;
			}
		}

		public float X {
			get {
				return m_x;
			}
			set {
				m_x = value;
			}
		}

		public float Y {
			get {
				return m_y;
			}
			set {
				m_y = value;
			}
		}

		public bool PreProcess {
			get {
				ValidateAccess(NodeKind.LOADER_GUI);
				return m_isPreProcess;
			}
			set {
				ValidateAccess(NodeKind.LOADER_GUI);
				m_isPreProcess = value;
				if(value) {
					if(!m_isPermanent) {
						m_name_color = LOADER_PREPROCESS_COLOR;
					}
				} else {
					if(!m_isPermanent) {
						m_name_color = DEFAULT_COLOR;
					}
				}
			}
		}

		public bool Permanent {
			get {
				ValidateAccess(NodeKind.LOADER_GUI);
				return m_isPermanent;
			}
			set {
				ValidateAccess(NodeKind.LOADER_GUI);
				m_isPermanent = value;
				if(value) {
					m_name_color = LOADER_PERMANENT_COLOR;
				} else {
					if(m_isPreProcess) {
						m_name_color = LOADER_PREPROCESS_COLOR;
					} else {
						m_name_color = DEFAULT_COLOR;
					}
				}
			}
		}

		public string RelatedNodeId {
			get {
				ValidateAccess(NodeKind.WARP_IN, NodeKind.WARP_OUT);
				return relatedNodeId;
			}
			set {
				ValidateAccess(NodeKind.WARP_IN, NodeKind.WARP_OUT);
				relatedNodeId = value;
			}
		}

		public List<ConnectionPointData> InputPoints {
			get {
				return m_inputPoints;
			}
		}

		public List<ConnectionPointData> OutputPoints {
			get {
				return m_outputPoints;
			}
		}

		public SerializableMultiTargetString LoaderLoadPath {
			get {
				ValidateAccess(
					NodeKind.LOADER_GUI 
				);
				return m_loaderLoadPath;
			}
		}

		public SerializableMultiTargetString ExporterExportPath {
			get {
				ValidateAccess(
					NodeKind.EXPORTER_GUI 
				);
				return m_exporterExportPath;
			}
		}

		public SerializableMultiTargetString GroupingKeywords {
			get {
				ValidateAccess(
					NodeKind.GROUPING_GUI 
				);
				return m_groupingKeyword;
			}
		}

		public SerializableMultiTargetString BundleNameTemplate {
			get {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				return m_bundleConfigBundleNameTemplate;
			}
		}

		public bool BundleConfigUseGroupAsVariants {
			get {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				return m_bundleConfigUseGroupAsVariants;
			}
			set {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				m_bundleConfigUseGroupAsVariants = value;
			}
		}

		public bool SetBundleNameAndVariant {
			get {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				return m_SetBundleNameAndVariant;
			}
			set {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				m_SetBundleNameAndVariant = value;
			}
		}

		public SerializableMultiTargetString InstanceData {
			get {
				ValidateAccess(
					NodeKind.PREFABBUILDER_GUI,
					NodeKind.MODIFIER_GUI,
					NodeKind.VALIDATOR_GUI
				);
				return m_scriptInstanceData;
			}
		}

		public List<Variant> Variants {
			get {
				ValidateAccess(
					NodeKind.BUNDLECONFIG_GUI 
				);
				return m_variants;
			}
		}

		public SerializableMultiTargetInt BundleBuilderBundleOptions {
			get {
				ValidateAccess(
					NodeKind.BUNDLEBUILDER_GUI 
				);
				return m_bundleBuilderEnabledBundleOptions;
			}
		}

		public SerializableMultiTargetInt ExporterExportOption {
			get {
				ValidateAccess(
					NodeKind.EXPORTER_GUI 
				);
				return m_exporterExportOption;
			}
		}

		public List<FilterEntry> FilterConditions {
			get {
				ValidateAccess(
					NodeKind.FILTER_GUI
				);
				return m_filter;
			}
		}

		private Dictionary<string, object> _SafeGet(Dictionary<string, object> jsonData, string key) {
			if(jsonData.ContainsKey(key)) {
				return jsonData[key] as Dictionary<string, object>;
			} else {
				return new Dictionary<string, object>();
			}
		}

		/*
		 *  Create NodeData from JSON
		 */ 
		public NodeData(Dictionary<string, object> jsonData) {

			m_name = jsonData[NODE_NAME] as string;
			m_id = jsonData[NODE_ID]as string;
			m_kind = AssetBundleGraphSettings.NodeKindFromString(jsonData[NODE_KIND] as string);

			var pos = jsonData[NODE_POS] as Dictionary<string, object>;
			m_x = (float)Convert.ToDouble(pos[NODE_POS_X]);
			m_y = (float)Convert.ToDouble(pos[NODE_POS_Y]);

			var inputs  = jsonData[NODE_INPUTPOINTS] as List<object>;
			var outputs = jsonData[NODE_OUTPUTPOINTS] as List<object>;
			m_inputPoints  = new List<ConnectionPointData>();
			m_outputPoints = new List<ConnectionPointData>();
			m_name_color = DEFAULT_COLOR;

			foreach(var obj in inputs) {
				var pDic = obj as Dictionary<string, object>;
				m_inputPoints.Add(new ConnectionPointData(pDic, this, true));
			}

			foreach(var obj in outputs) {
				var pDic = obj as Dictionary<string, object>;
				m_outputPoints.Add(new ConnectionPointData(pDic, this, false));
			}


			switch (m_kind) {
			case NodeKind.IMPORTSETTING_GUI:
				// nothing to do
				break;
			case NodeKind.PREFABBUILDER_GUI:
			case NodeKind.MODIFIER_GUI:
			case NodeKind.VALIDATOR_GUI:
				{
					if(jsonData.ContainsKey(NODE_SCRIPT_CLASSNAME)) {
						m_scriptClassName = jsonData[NODE_SCRIPT_CLASSNAME] as string;
					}
					if(jsonData.ContainsKey(NODE_SCRIPT_INSTANCE_DATA)) {
						m_scriptInstanceData = new SerializableMultiTargetString(_SafeGet(jsonData, NODE_SCRIPT_INSTANCE_DATA));
					}
				}
				break;
			case NodeKind.LOADER_GUI:
				{
					m_loaderLoadPath = new SerializableMultiTargetString(_SafeGet(jsonData, NODE_LOADER_LOAD_PATH));
					if(jsonData.ContainsKey(NODE_LOADER_PREPROCESS)) {
						m_isPreProcess = Convert.ToBoolean(jsonData[NODE_LOADER_PREPROCESS]);
						if(m_isPreProcess) {
							m_name_color = LOADER_PREPROCESS_COLOR;
						}
					}
					if(jsonData.ContainsKey(NODE_LOADER_PERMANENT)) {
						m_isPermanent = Convert.ToBoolean(jsonData[NODE_LOADER_PERMANENT]);
						if(m_isPermanent) {
							m_name_color = LOADER_PERMANENT_COLOR;
						}
					}
				}
				break;
			case NodeKind.FILTER_GUI:
				{
					var filters = jsonData[NODE_FILTER] as List<object>;

					m_filter = new List<FilterEntry>();

					for(int i=0; i<filters.Count; ++i) {
						var f = filters[i] as Dictionary<string, object>;


						var name = string.Empty;
						if(f.ContainsKey(NODE_FILTER_NAME)) {
							name = f[NODE_FILTER_NAME] as string;
						}
						var keyword = f[NODE_FILTER_KEYWORD] as string;
						var keytype = f[NODE_FILTER_KEYTYPE] as string;
						bool isExclusion = false;
						if(f.ContainsKey(NODE_FILTER_EXCLUSION)) {
							isExclusion = Convert.ToBoolean(f[NODE_FILTER_EXCLUSION]);
						}
						var pointId = f[NODE_FILTER_POINTID] as string;

						var point = m_outputPoints.Find(p => p.Id == pointId);
						UnityEngine.Assertions.Assert.IsNotNull(point, "Output point not found for " + keyword);
						m_filter.Add(new FilterEntry(name, keyword, keytype, isExclusion, point));
					}
				}
				break;
			case NodeKind.GROUPING_GUI:
				{
					m_groupingKeyword = new SerializableMultiTargetString(_SafeGet(jsonData, NODE_GROUPING_KEYWORD));
				}
				break;
			case NodeKind.BUNDLECONFIG_GUI:
				{
					m_bundleConfigBundleNameTemplate = new SerializableMultiTargetString(_SafeGet(jsonData, NODE_BUNDLECONFIG_BUNDLENAME_TEMPLATE));
					if(jsonData.ContainsKey(NODE_BUNDLECONFIG_USE_GROUPASVARIANTS)) {
						m_bundleConfigUseGroupAsVariants = Convert.ToBoolean(jsonData[NODE_BUNDLECONFIG_USE_GROUPASVARIANTS]);
					}
					if(jsonData.ContainsKey(NODE_BUNDLECONFIG_SET_BUNDLE_NAME)) {
						m_SetBundleNameAndVariant = Convert.ToBoolean(jsonData[NODE_BUNDLECONFIG_SET_BUNDLE_NAME]);
					}
					m_variants = new List<Variant>();
					if(jsonData.ContainsKey(NODE_BUNDLECONFIG_VARIANTS)){
						var variants = jsonData[NODE_BUNDLECONFIG_VARIANTS] as List<object>;

						for(int i=0; i<variants.Count; ++i) {
							var v = variants[i] as Dictionary<string, object>;

							var name    = v[NODE_BUNDLECONFIG_VARIANTS_NAME] as string;
							var pointId = v[NODE_BUNDLECONFIG_VARIANTS_POINTID] as string;

							var point = m_inputPoints.Find(p => p.Id == pointId);
							UnityEngine.Assertions.Assert.IsNotNull(point, "Input point not found for " + name);
							m_variants.Add(new Variant(name, point));
						}
					}
				}
				break;
			case NodeKind.BUNDLEBUILDER_GUI:
				{
					m_bundleBuilderEnabledBundleOptions = new SerializableMultiTargetInt(_SafeGet(jsonData, NODE_BUNDLEBUILDER_ENABLEDBUNDLEOPTIONS));
				}
				break;
			case NodeKind.EXPORTER_GUI:
				{
					m_exporterExportPath = new SerializableMultiTargetString(_SafeGet(jsonData, NODE_EXPORTER_EXPORT_PATH));
					m_exporterExportOption = new SerializableMultiTargetInt(_SafeGet(jsonData, NODE_EXPORTER_EXPORT_OPTION));
				}
				break;

			case NodeKind.WARP_IN:
				if(jsonData.ContainsKey(NODE_RELATED_ID)) {
					relatedNodeId = jsonData[NODE_RELATED_ID] as string;
				}
				break;
			case NodeKind.WARP_OUT:
				if(jsonData.ContainsKey(NODE_RELATED_ID)) {
					relatedNodeId = jsonData[NODE_RELATED_ID] as string;
				}
				break;
				default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/*
		 * Constructor used to create new node from GUI
		 */ 
		public NodeData(string name, NodeKind kind, float x, float y) {

			m_id = Guid.NewGuid().ToString();
			m_name = name;
			m_x = x;
			m_y = y;
			m_kind = kind;
			m_name_color = DEFAULT_COLOR;

			m_inputPoints  = new List<ConnectionPointData>();
			m_outputPoints = new List<ConnectionPointData>();


			// adding defalut input point.
			// Loader does not take input
			if(kind != NodeKind.LOADER_GUI) {
				m_inputPoints.Add(new ConnectionPointData(AssetBundleGraphSettings.DEFAULT_INPUTPOINT_LABEL, this, true, kind == NodeKind.WARP_OUT));
			}

			// adding default output point.
			// Filter and Exporter does not have output.
			if(kind != NodeKind.FILTER_GUI && kind != NodeKind.EXPORTER_GUI) {
				m_outputPoints.Add(new ConnectionPointData(AssetBundleGraphSettings.DEFAULT_OUTPUTPOINT_LABEL, this, false, kind == NodeKind.WARP_IN));
			}

			switch(m_kind) {
			case NodeKind.PREFABBUILDER_GUI:
			case NodeKind.MODIFIER_GUI:
			case NodeKind.VALIDATOR_GUI:
				m_scriptClassName 	= String.Empty;
				m_scriptInstanceData = new SerializableMultiTargetString();
				break;
			
			case NodeKind.IMPORTSETTING_GUI:
				break;

			case NodeKind.FILTER_GUI:
				m_filter = new List<FilterEntry>();
				break;

			case NodeKind.LOADER_GUI:
				m_loaderLoadPath = new SerializableMultiTargetString();
				m_isPreProcess = false;
				m_isPermanent = false;
				break;

			case NodeKind.GROUPING_GUI:
				m_groupingKeyword = new SerializableMultiTargetString(AssetBundleGraphSettings.GROUPING_KEYWORD_DEFAULT);
				break;

			case NodeKind.BUNDLECONFIG_GUI:
				m_bundleConfigBundleNameTemplate = new SerializableMultiTargetString(AssetBundleGraphSettings.BUNDLECONFIG_BUNDLENAME_TEMPLATE_DEFAULT);
				m_bundleConfigUseGroupAsVariants = false;
				m_SetBundleNameAndVariant = false;
				m_variants = new List<Variant>();
				break;

			case NodeKind.BUNDLEBUILDER_GUI:
				m_bundleBuilderEnabledBundleOptions = new SerializableMultiTargetInt();
				break;

			case NodeKind.EXPORTER_GUI:
				m_exporterExportPath = new SerializableMultiTargetString();
				m_exporterExportOption = new SerializableMultiTargetInt();
				break;
			case NodeKind.WARP_IN:
				break;
			case NodeKind.WARP_OUT:
				break;

			default:
				throw new AssetBundleGraphException("[FATAL]Unhandled nodekind. unimplmented:"+ m_kind);
			}
		}

		/**
		 * Duplicate this node with new guid.
		 */ 
		public NodeData Duplicate () {

			var newData = new NodeData(m_name, m_kind, m_x, m_y);

			switch(m_kind) {
			case NodeKind.IMPORTSETTING_GUI:
				IntegratedGUIImportSetting.CopySampleFile(this,newData);
				break;
			case NodeKind.PREFABBUILDER_GUI:
			case NodeKind.MODIFIER_GUI:
			case NodeKind.VALIDATOR_GUI:
				newData.m_scriptClassName = m_scriptClassName;
				newData.m_scriptInstanceData = new SerializableMultiTargetString(m_scriptInstanceData);
				break;

			case NodeKind.FILTER_GUI:
				foreach(var f in m_filter) {
					newData.AddFilterCondition(f.Name, f.FilterKeyword, f.FilterKeytype, f.IsExclusion);
				}
				break;

			case NodeKind.LOADER_GUI:
				newData.m_loaderLoadPath = new SerializableMultiTargetString(m_loaderLoadPath);
				newData.m_isPreProcess = m_isPreProcess;
				newData.m_isPermanent = m_isPermanent;
				break;

			case NodeKind.GROUPING_GUI:
				newData.m_groupingKeyword = new SerializableMultiTargetString(m_groupingKeyword);
				break;

			case NodeKind.BUNDLECONFIG_GUI:
				newData.m_bundleConfigBundleNameTemplate = new SerializableMultiTargetString(m_bundleConfigBundleNameTemplate);
				newData.m_bundleConfigUseGroupAsVariants = m_bundleConfigUseGroupAsVariants;
				newData.m_SetBundleNameAndVariant = m_SetBundleNameAndVariant;
				foreach(var v in m_variants) {
					newData.AddVariant(v.Name);
				}
				break;

			case NodeKind.BUNDLEBUILDER_GUI:
				newData.m_bundleBuilderEnabledBundleOptions = new SerializableMultiTargetInt(m_bundleBuilderEnabledBundleOptions);
				break;

			case NodeKind.EXPORTER_GUI:
				newData.m_exporterExportPath = new SerializableMultiTargetString(m_exporterExportPath);
				newData.m_exporterExportOption = new SerializableMultiTargetInt(m_exporterExportOption);
				break;

			default:
				throw new AssetBundleGraphException("[FATAL]Unhandled nodekind. unimplmented:"+ m_kind);
			}

			return newData;
		}

		public ConnectionPointData AddInputPoint(string label) {
			var p = new ConnectionPointData(label, this, true);
			m_inputPoints.Add(p);
			return p;
		}

		public ConnectionPointData AddOutputPoint(string label) {
			var p = new ConnectionPointData(label, this, false);
			m_outputPoints.Add(p);
			return p;
		}

		public ConnectionPointData FindInputPoint(string id) {
			return m_inputPoints.Find(p => p.Id == id);
		}

		public ConnectionPointData FindOutputPoint(string id) {
			return m_outputPoints.Find(p => p.Id == id);
		}

		public ConnectionPointData FindConnectionPoint(string id) {
			var v = FindInputPoint(id);
			if(v != null) {
				return v;
			}
			return FindOutputPoint(id);
		}

		public string GetLoaderFullLoadPath(BuildTarget g) {
			return FileUtility.PathCombine(Application.dataPath, LoaderLoadPath[g]);
		}

		public bool ValidateOverlappingFilterCondition(bool throwException) {
			ValidateAccess(NodeKind.FILTER_GUI);

			var conditionGroup = FilterConditions.Select(v => v).GroupBy(v => v.Hash).ToList();
			var overlap = conditionGroup.Find(v => v.Count() > 1);

			if( overlap != null && throwException ) {
				var element = overlap.First();
				throw new NodeException(String.Format("Duplicated filter condition found for [Keyword:{0} Type:{1}]", element.FilterKeyword, element.FilterKeytype), Id);
			}
			return overlap != null;
		}

		public void AddFilterCondition(string name, string keyword, string keytype, bool isExclusion) {
			ValidateAccess(
				NodeKind.FILTER_GUI
			);
			var point = new ConnectionPointData(keyword, this, false);
			m_outputPoints.Add(point);
			var newEntry = new FilterEntry(name, keyword, keytype, isExclusion, point);
			m_filter.Add(newEntry);
		}

		public void RemoveFilterCondition(FilterEntry f) {
			ValidateAccess(
				NodeKind.FILTER_GUI
			);

			m_filter.Remove(f);
			m_outputPoints.Remove(f.ConnectionPoint);
		}

		public void AddVariant(string name) {
			ValidateAccess(
				NodeKind.BUNDLECONFIG_GUI
			);

			var point = new ConnectionPointData(name, this, true);
			m_inputPoints.Add(point);
			var newEntry = new Variant(name, point);
			m_variants.Add(newEntry);
		}

		public void RemoveVariant(Variant v) {
			ValidateAccess(
				NodeKind.BUNDLECONFIG_GUI
			);

			m_variants.Remove(v);
			m_inputPoints.Remove(v.ConnectionPoint);
		}

		private void ValidateAccess(params NodeKind[] allowedKind) {
			foreach(var k in allowedKind) {
				if (k == m_kind) {
					return;
				}
			}
			throw new AssetBundleGraphException(m_name + ": Tried to access invalid method or property.");
		}

		public bool Validate (List<NodeData> allNodes, List<ConnectionData> allConnections) {

			switch(m_kind) {
			case NodeKind.BUNDLEBUILDER_GUI:
				{
					foreach(var v in m_bundleBuilderEnabledBundleOptions.Values) {
						bool isDisableWriteTypeTreeEnabled  = 0 < (v.value & (int)BuildAssetBundleOptions.DisableWriteTypeTree);
						bool isIgnoreTypeTreeChangesEnabled = 0 < (v.value & (int)BuildAssetBundleOptions.IgnoreTypeTreeChanges);

						// If both are marked something is wrong. Clear both flag and save.
						if(isDisableWriteTypeTreeEnabled && isIgnoreTypeTreeChangesEnabled) {
							int flag = ~((int)BuildAssetBundleOptions.DisableWriteTypeTree + (int)BuildAssetBundleOptions.IgnoreTypeTreeChanges);
							v.value = v.value & flag;
							Debug.LogWarning(m_name + ": DisableWriteTypeTree and IgnoreTypeTreeChanges can not be used together. Settings overwritten.");
						}
					}
				}
				break;
			}

			return true;
		}

		private bool TestCreateScriptInstance() {
			if(string.IsNullOrEmpty(ScriptClassName)) {
				return false;
			}
			var nodeScriptInstance = Assembly.GetExecutingAssembly().CreateInstance(m_scriptClassName);
			return nodeScriptInstance != null;
		}

		/**
		 * Serialize to JSON dictionary
		 */ 
		public Dictionary<string, object> ToJsonDictionary() {
			var nodeDict = new Dictionary<string, object>();

			nodeDict[NODE_NAME] = m_name;
			nodeDict[NODE_ID] 	= m_id;
			nodeDict[NODE_KIND] = m_kind.ToString();

			var inputs  = new List<object>();
			var outputs = new List<object>();

			foreach(var p in m_inputPoints) {
				inputs.Add( p.ToJsonDictionary() );
			}

			foreach(var p in m_outputPoints) {
				outputs.Add( p.ToJsonDictionary() );
			}

			nodeDict[NODE_INPUTPOINTS]  = inputs;
			nodeDict[NODE_OUTPUTPOINTS] = outputs;

			nodeDict[NODE_POS] = new Dictionary<string, object>() {
				{NODE_POS_X, m_x},
				{NODE_POS_Y, m_y}
			};
				
			switch (m_kind) {
			case NodeKind.PREFABBUILDER_GUI:
			case NodeKind.MODIFIER_GUI:
			case NodeKind.VALIDATOR_GUI:
				nodeDict[NODE_SCRIPT_CLASSNAME] = m_scriptClassName;
				nodeDict[NODE_SCRIPT_INSTANCE_DATA] = m_scriptInstanceData.ToJsonDictionary();
				break;

			case NodeKind.LOADER_GUI:
				nodeDict[NODE_LOADER_LOAD_PATH] = m_loaderLoadPath.ToJsonDictionary();
				nodeDict[NODE_LOADER_PREPROCESS] = m_isPreProcess;
				nodeDict[NODE_LOADER_PERMANENT] = m_isPermanent;
				break;

			case NodeKind.FILTER_GUI:
				var filterDict = new List<Dictionary<string, object>>();
				foreach(var f in m_filter) {
					var df = new Dictionary<string, object>();
					df[NODE_FILTER_NAME] = f.Name;
					df[NODE_FILTER_KEYWORD] = f.FilterKeyword;
					df[NODE_FILTER_KEYTYPE] = f.FilterKeytype;
					df[NODE_FILTER_EXCLUSION] = f.IsExclusion;
					df[NODE_FILTER_POINTID] = f.ConnectionPoint.Id;
					filterDict.Add(df);
				}
				nodeDict[NODE_FILTER] = filterDict;
				break;

			case NodeKind.GROUPING_GUI:
				nodeDict[NODE_GROUPING_KEYWORD] = m_groupingKeyword.ToJsonDictionary();
				break;

			case NodeKind.BUNDLECONFIG_GUI:
				nodeDict[NODE_BUNDLECONFIG_BUNDLENAME_TEMPLATE] = m_bundleConfigBundleNameTemplate.ToJsonDictionary();
				nodeDict[NODE_BUNDLECONFIG_USE_GROUPASVARIANTS] = m_bundleConfigUseGroupAsVariants;
				nodeDict[NODE_BUNDLECONFIG_SET_BUNDLE_NAME] = m_SetBundleNameAndVariant;
				var variantsDict = new List<Dictionary<string, object>>();
				foreach(var v in m_variants) {
					var dv = new Dictionary<string, object>();
					dv[NODE_BUNDLECONFIG_VARIANTS_NAME] 	= v.Name;
					dv[NODE_BUNDLECONFIG_VARIANTS_POINTID] = v.ConnectionPoint.Id;
					variantsDict.Add(dv);
				}
				nodeDict[NODE_BUNDLECONFIG_VARIANTS] = variantsDict;
				break;

			case NodeKind.BUNDLEBUILDER_GUI:
				nodeDict[NODE_BUNDLEBUILDER_ENABLEDBUNDLEOPTIONS] = m_bundleBuilderEnabledBundleOptions.ToJsonDictionary();
				break;

			case NodeKind.EXPORTER_GUI:
				nodeDict[NODE_EXPORTER_EXPORT_PATH] = m_exporterExportPath.ToJsonDictionary();
				nodeDict[NODE_EXPORTER_EXPORT_OPTION] = m_exporterExportOption.ToJsonDictionary();
				break;

			case NodeKind.IMPORTSETTING_GUI:
				// nothing to do
				break;

			case NodeKind.WARP_IN:
			case NodeKind.WARP_OUT:
				nodeDict[NODE_RELATED_ID] = relatedNodeId;
				break;
				
			default:
				throw new ArgumentOutOfRangeException ();
			}

			return nodeDict;
		}

		/**
		 * Serialize to JSON string
		 */ 
		public string ToJsonString() {
			return AssetBundleGraph.Json.Serialize(ToJsonDictionary());
		}
	}
}
