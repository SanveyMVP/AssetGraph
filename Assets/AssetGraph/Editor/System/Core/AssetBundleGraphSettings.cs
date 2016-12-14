using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AssetBundleGraph {
	public class AssetBundleGraphSettings {
		/*
			if true, ignore .meta files inside AssetBundleGraph.
		*/
		public const bool IGNORE_META = true;

		public const string GUI_TEXT_MENU_OPEN = "Window/AssetGraph/Open Graph Editor";
		public const string GUI_TEXT_MENU_BUILD = "Window/AssetGraph/Build Bundles for Current Platform";
		public const string GUI_TEXT_MENU_GENERATE = "Window/AssetGraph/Create Node Script";
		public const string GUI_TEXT_MENU_GENERATE_MODIFIER = GUI_TEXT_MENU_GENERATE + "/Modifier Script";
		public const string GUI_TEXT_MENU_GENERATE_PREFABBUILDER = GUI_TEXT_MENU_GENERATE + "/PrefabBuilder Script";
		public const string GUI_TEXT_MENU_GENERATE_VALIDATOR = GUI_TEXT_MENU_GENERATE + "/Validator Script";
		public const string GUI_TEXT_MENU_GENERATE_CUITOOL = "Window/AssetGraph/Create CUI Tool";

		public const string GUI_TEXT_MENU_GENERATE_POSTPROCESS = GUI_TEXT_MENU_GENERATE + "/Postprocess Script";
		public const string GUI_TEXT_MENU_DELETE_CACHE = "Window/AssetGraph/Clear Build Cache";
		
		public const string GUI_TEXT_MENU_DELETE_IMPORTSETTING_SETTINGS = "Window/AssetGraph/Clear Saved ImportSettings";

		public const string ASSETNBUNDLEGRAPH_DATA_PATH = "AssetGraph/SettingFiles";
		public const string ASSETBUNDLEGRAPH_DATA_NAME = "AssetGraph.json";
		public const string ASSETBUNDLEGRAPH_LOADER_DATA_NAME = "LoaderFolders.json";

		public const string ASSETS_PATH = "Assets/";
		public const string ASSETBUNDLEGRAPH_PATH = ASSETS_PATH + "AssetGraph/";
		public const string APPLICATIONDATAPATH_CACHE_PATH = ASSETBUNDLEGRAPH_PATH + "Cache/";
		public const string USERSPACE_PATH = ASSETBUNDLEGRAPH_PATH + "Generated/Editor/";
		public const string CUISPACE_PATH = ASSETBUNDLEGRAPH_PATH + "Generated/CUI/";

		public const string PREFABBUILDER_CACHE_PLACE	= APPLICATIONDATAPATH_CACHE_PATH + "Prefabs";
		public const string BUNDLEBUILDER_CACHE_PLACE	= APPLICATIONDATAPATH_CACHE_PATH + "AssetBundles";

		public const string IMPORTER_SETTINGS_PLACE		= ASSETBUNDLEGRAPH_PATH + "SavedSettings/ImportSettings";

		public const string UNITY_METAFILE_EXTENSION = ".meta";
		public const string UNITY_LOCAL_DATAPATH = "Assets";
		public const string DOTSTART_HIDDEN_FILE_HEADSTRING = ".";
		public const string MANIFEST_FOOTER = ".manifest";
		public const string IMPORTER_RECORDFILE = ".importedRecord";
		public const char UNITY_FOLDER_SEPARATOR = '/';// Mac/Windows/Linux can use '/' in Unity.

		public const char KEYWORD_WILDCARD = '*';

		public struct BuildAssetBundleOption {
			public readonly BuildAssetBundleOptions option;
			public readonly string description;
			public BuildAssetBundleOption(string desc, BuildAssetBundleOptions opt) {
				option = opt;
				description = desc;
			}
		}

		public static List<BuildAssetBundleOption> BundleOptionSettings = new List<BuildAssetBundleOption> {
			new BuildAssetBundleOption("Uncompressed AssetBundle", BuildAssetBundleOptions.UncompressedAssetBundle),
			new BuildAssetBundleOption("Disable Write TypeTree", BuildAssetBundleOptions.DisableWriteTypeTree),
			new BuildAssetBundleOption("Deterministic AssetBundle", BuildAssetBundleOptions.DeterministicAssetBundle),
			new BuildAssetBundleOption("Force Rebuild AssetBundle", BuildAssetBundleOptions.ForceRebuildAssetBundle),
			new BuildAssetBundleOption("Ignore TypeTree Changes", BuildAssetBundleOptions.IgnoreTypeTreeChanges),
			new BuildAssetBundleOption("Append Hash To AssetBundle Name", BuildAssetBundleOptions.AppendHashToAssetBundleName),
			new BuildAssetBundleOption("ChunkBased Compression", BuildAssetBundleOptions.ChunkBasedCompression),
#if UNITY_5_4_OR_NEWER
			new BuildAssetBundleOption("Strict Mode", BuildAssetBundleOptions.StrictMode),
			new BuildAssetBundleOption("Omit Class Versions", BuildAssetBundleOptions.OmitClassVersions)
#endif
		};

		//public const string PLATFORM_DEFAULT_NAME = "Default";
		//public const string PLATFORM_STANDALONE = "Standalone";

		public const float WINDOW_SPAN = 20f;

		/*
			node generation from GUI
		*/
		public const string MENU_LOADER_NAME = "Loader";
		public const string MENU_FILTER_NAME = "Filter";
		public const string MENU_IMPORTSETTING_NAME = "ImportSetting";
		public const string MENU_MODIFIER_NAME = "Modifier";
		public const string MENU_GROUPING_NAME = "Grouping";
		public const string MENU_PREFABBUILDER_NAME = "PrefabBuilder";
		public const string MENU_BUNDLECONFIG_NAME = "BundleConfig";
		public const string MENU_BUNDLEBUILDER_NAME = "BundleBuilder";
		public const string MENU_EXPORTER_NAME = "Exporter";
		public const string MENU_WARP_NAME = "Warp";
		public const string MENU_VALIDATOR_NAME = "Validator";

		public static Dictionary<string, NodeKind> GUI_Menu_Item_TargetGUINodeDict = new Dictionary<string, NodeKind>{
			{"Create " + MENU_LOADER_NAME + " Node", NodeKind.LOADER_GUI},
			{"Create " + MENU_FILTER_NAME + " Node", NodeKind.FILTER_GUI},
			{"Create " + MENU_IMPORTSETTING_NAME + " Node", NodeKind.IMPORTSETTING_GUI},
			{"Create " + MENU_MODIFIER_NAME + " Node", NodeKind.MODIFIER_GUI},
			{"Create " + MENU_VALIDATOR_NAME + " Node", NodeKind.VALIDATOR_GUI },
			{"Create " + MENU_GROUPING_NAME + " Node", NodeKind.GROUPING_GUI},
			{"Create " + MENU_PREFABBUILDER_NAME + " Node", NodeKind.PREFABBUILDER_GUI},
			{"Create " + MENU_BUNDLECONFIG_NAME + " Node", NodeKind.BUNDLECONFIG_GUI},
			{"Create " + MENU_BUNDLEBUILDER_NAME + " Node", NodeKind.BUNDLEBUILDER_GUI},
			{"Create " + MENU_EXPORTER_NAME + " Node", NodeKind.EXPORTER_GUI},
			{"Create " + MENU_WARP_NAME + " Node", NodeKind.WARP_IN }
		};

		public static Dictionary<NodeKind, string> DEFAULT_NODE_NAME = new Dictionary<NodeKind, string>{
			{NodeKind.LOADER_GUI, "Loader"},
			{NodeKind.FILTER_GUI, "Filter"},
			{NodeKind.IMPORTSETTING_GUI, "ImportSetting"},
			{NodeKind.MODIFIER_GUI, "Modifier"},
			{NodeKind.GROUPING_GUI, "Grouping"},
			{NodeKind.PREFABBUILDER_GUI, "PrefabBuilder"},
			{NodeKind.BUNDLECONFIG_GUI, "BundleConfig"},
			{NodeKind.BUNDLEBUILDER_GUI, "BundleBuilder"},
			{NodeKind.EXPORTER_GUI, "Exporter"},
			{NodeKind.WARP_IN, "In"},
			{NodeKind.WARP_OUT, "Out"},
			{NodeKind.VALIDATOR_GUI, "Validator"}
		};

		public static Dictionary<Type, string> PLACEHOLDER_FILE = new Dictionary<Type, string>{
			{typeof(TextureImporter),  "config.png"},
			{typeof(ModelImporter),  "config.fbx"},
			{typeof(AudioImporter),  "config.wav"}
		};

		/*
			data key for AssetBundleGraph.json
		*/

		public const string GROUPING_KEYWORD_DEFAULT = "/Group_*/";
		public const string BUNDLECONFIG_BUNDLENAME_TEMPLATE_DEFAULT = "bundle_*";

		// by default, AssetBundleGraph's node has only 1 InputPoint. and 
		// this is only one definition of it's label.
		public const string DEFAULT_INPUTPOINT_LABEL = "-";
		public const string DEFAULT_OUTPUTPOINT_LABEL = "+";
		public const string BUNDLECONFIG_BUNDLE_OUTPUTPOINT_LABEL = "bundles";
		public const string BUNDLECONFIG_VARIANTNAME_DEFAULT = "";

		public const string DEFAULT_FILTER_NAME = "";
		public const string DEFAULT_FILTER_KEYWORD = "";
		public const string DEFAULT_FILTER_KEYTYPE = "Any";
		public const bool DEFAULT_FILTER_EXCLUSION = false;

		public const string FILTER_KEYWORD_WILDCARD = "*";

		public const string NODE_INPUTPOINT_FIXED_LABEL = "FIXED_INPUTPOINT_ID";

		public static NodeKind NodeKindFromString (string val) {
			return (NodeKind)Enum.Parse(typeof(NodeKind), val);
		}

        [InitializeOnLoad]
        public static class AssetGraphRelativePaths {
            
            private const string ARROW_NAME = "AssetGraph_Arrow";
            //This asset will be at path *****/Editor/GUI/GraphicResources/AssetGraph_Arrow" so we go three times up to get the Editor/ root folder
            // This initialization needs to be done here because static constructors are executed after static field initialization.
            public static readonly string RELATIVE_FOLDER = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(ARROW_NAME)[0])))) + "/";
            public static readonly string SCRIPT_TEMPLATE_PATH = RELATIVE_FOLDER + "ScriptTemplate/";
            public static readonly string ASSET_PLACEHOLDER_FOLDER = RELATIVE_FOLDER + "AssetPlaceholders/";
            public static readonly string RESOURCE_BASEPATH = RELATIVE_FOLDER + "GUI/GraphicResources/";
            public static readonly string RESOURCE_NODEPATH = RESOURCE_BASEPATH + "Nodes/";

            public static readonly float NODE_BASE_WIDTH = 120f;
            public static readonly float NODE_BASE_HEIGHT = 40f;

            public static readonly float NODE_WARP_WIDTH = 40f;

            public static readonly float CONNECTION_ARROW_WIDTH = 12f;
            public static readonly float CONNECTION_ARROW_HEIGHT = 15f;

            public static readonly float INPUT_POINT_WIDTH = 21f;
            public static readonly float INPUT_POINT_HEIGHT = 29f;

            public static readonly float OUTPUT_POINT_WIDTH = 10f;
            public static readonly float OUTPUT_POINT_HEIGHT = 23f;

            public static readonly float FILTER_OUTPUT_SPAN = 32f;

            public static readonly float CONNECTION_POINT_MARK_SIZE = 19f;

            public static readonly float CONNECTION_CURVE_LENGTH = 10f;

            public static readonly float TOOLBAR_HEIGHT = 20f;

            public static readonly string RESOURCE_ARROW = RESOURCE_BASEPATH + ARROW_NAME +".png";

            public static readonly string RESOURCE_CONNECTIONPOINT_ENABLE = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_EnableMark.png";
            public static readonly string RESOURCE_CONNECTIONPOINT_INPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_InputMark.png";
            public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark.png";
            public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT_CONNECTED = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark_Connected.png";

            public static readonly string RESOURCE_INPUT_BG = RESOURCE_BASEPATH + "AssetGraph_InputBG.png";
            public static readonly string RESOURCE_OUTPUT_BG = RESOURCE_BASEPATH + "AssetGraph_OutputBG.png";

            public static readonly string RESOURCE_SELECTION = RESOURCE_BASEPATH + "AssetGraph_Selection.png";


            public static readonly string RESOURCE_NODE_GREY = RESOURCE_NODEPATH + "grey.png";
            public static readonly string RESOURCE_NODE_GREY_ON = RESOURCE_NODEPATH + "grey_on.png";
            public static readonly string RESOURCE_NODE_GREY_HIGHLIGHT = RESOURCE_NODEPATH + "grey_highlight.png";
            public static readonly string RESOURCE_NODE_BLUE = RESOURCE_NODEPATH + "blue.png";
            public static readonly string RESOURCE_NODE_BLUE_ON = RESOURCE_NODEPATH + "blue_on.png";
            public static readonly string RESOURCE_NODE_BLUE_HIGHLIGHT = RESOURCE_NODEPATH + "blue_highlight.png";
            public static readonly string RESOURCE_NODE_AQUA = RESOURCE_NODEPATH + "aqua.png";
            public static readonly string RESOURCE_NODE_AQUA_ON = RESOURCE_NODEPATH + "aqua_on.png";
            public static readonly string RESOURCE_NODE_AQUA_HIGHLIGHT = RESOURCE_NODEPATH + "aqua_highlight.png";
            public static readonly string RESOURCE_NODE_ORANGE = RESOURCE_NODEPATH + "orange.png";
            public static readonly string RESOURCE_NODE_ORANGE_ON = RESOURCE_NODEPATH + "orange_on.png";
            public static readonly string RESOURCE_NODE_ORANGE_HIGHLIGHT = RESOURCE_NODEPATH + "orange_highlight.png";
            public static readonly string RESOURCE_NODE_RED = RESOURCE_NODEPATH + "red.png";
            public static readonly string RESOURCE_NODE_RED_ON = RESOURCE_NODEPATH + "red_on.png";
            public static readonly string RESOURCE_NODE_RED_HIGHLIGHT = RESOURCE_NODEPATH + "red_highlight.png";
            public static readonly string RESOURCE_NODE_YELLOW = RESOURCE_NODEPATH + "yellow.png";
            public static readonly string RESOURCE_NODE_YELLOW_ON = RESOURCE_NODEPATH + "yellow_on.png";
            public static readonly string RESOURCE_NODE_YELLOW_HIGHLIGHT = RESOURCE_NODEPATH + "yellow_highlight.png";
            public static readonly string RESOURCE_NODE_GREEN = RESOURCE_NODEPATH + "green.png";
            public static readonly string RESOURCE_NODE_GREEN_ON = RESOURCE_NODEPATH + "green_on.png";
            public static readonly string RESOURCE_NODE_GREEN_HIGHLIGHT = RESOURCE_NODEPATH + "green_highlight.png";

        }
	}
}
