using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundleGraph {
	public static class TypeUtility {
		public static readonly List<Type> ImporterTypes = new List<Type>{			
			// importers
			typeof(TextureImporter),
			typeof(ModelImporter),
			typeof(AudioImporter),
		};

		public static readonly List<Type> ModifierTypes = new List<Type>{			
			// importers
			typeof(TextureImporter),
			typeof(ModelImporter),
			typeof(AudioImporter),
			typeof(Animation),
			typeof(Animator),
			typeof(AvatarMask),
			typeof(Cubemap),
			typeof(Flare),
			typeof(Font),
			typeof(GUISkin),
			// typeof(LightmapParameters).ToString(),
			typeof(Material),
			typeof(PhysicMaterial),
			typeof(PhysicsMaterial2D),
			typeof(RenderTexture),
			// typeof(SceneAsset).ToString(),
			typeof(Shader),
			typeof(Scene),
		};


		public static readonly List<string> KeyTypes = new List<string>{
			// empty
			AssetBundleGraphSettings.DEFAULT_FILTER_KEYTYPE,
			
			// importers
			typeof(TextureImporter).ToString(),
			typeof(ModelImporter).ToString(),
			typeof(AudioImporter).ToString(),
			
			// others(Assets)
			typeof(Animation).ToString(),
			typeof(Animator).ToString(),
			typeof(AvatarMask).ToString(),
			typeof(Cubemap).ToString(),
			typeof(Flare).ToString(),
			typeof(Font).ToString(),
			typeof(GUISkin).ToString(),
			// typeof(LightmapParameters).ToString(),
			typeof(Material).ToString(),
			typeof(PhysicMaterial).ToString(),
			typeof(PhysicsMaterial2D).ToString(),
			typeof(RenderTexture).ToString(),
			// typeof(SceneAsset).ToString(),
			typeof(Shader).ToString(),
			typeof(Scene).ToString(),
		};
		
		public static readonly Dictionary<string, Type> AssumeTypeBindingByExtension = new Dictionary<string, Type>{
			// others(Assets)
			{".anim", typeof(Animation)},
			{".controller", typeof(Animator)},
			{".mask", typeof(AvatarMask)},
			{".cubemap", typeof(Cubemap)},
			{".flare", typeof(Flare)},
			{".fontsettings", typeof(Font)},
			{".guiskin", typeof(GUISkin)},
			// typeof(LightmapParameters).ToString(),
			{".mat", typeof(Material)},
			{".physicMaterial", typeof(PhysicMaterial)},
			{".physicsMaterial2D", typeof(PhysicsMaterial2D)},
			{".renderTexture", typeof(RenderTexture)},
			// typeof(SceneAsset).ToString(),
			{".shader", typeof(Shader)},
			{".unity", typeof(Scene)},
			{".prefab", typeof(UnityEngine.Object)}

			// {"", typeof(Sprite)},
		};

		public static readonly List<string> IgnoredExtension = new List<string>{
			string.Empty,
			".manifest",
			".assetbundle",
			".sample",
			".cs",
			".sh",
			".json",
			".js",
			".sbsar",
		};

		/**
		 * Get type of asset from give path.
		 */
		public static Type GetTypeOfAsset (string assetPath) {
			if (assetPath.EndsWith(AssetBundleGraphSettings.UNITY_METAFILE_EXTENSION)) return typeof(string);

			var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

			// If asset is null, this asset is not imported yet, or unsupported type of file
			// so we set this to object type.
			if (asset == null) {
				return typeof(object);
			}
			return asset.GetType();
		}

		/**
		 * Get type of asset from give path.
		 */
		public static Type FindTypeOfAsset (string assetPath) {
			// check by asset importer type.
			var importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null) {
				Debug.LogWarning("Failed to assume assetType of asset. The asset will be ignored: " + assetPath);
				return typeof(object);
			}

			var assumedImporterType = importer.GetType();
			var importerTypeStr = assumedImporterType.ToString();
			
			switch (importerTypeStr) {
				case "UnityEditor.TextureImporter":
				case "UnityEditor.ModelImporter":
				case "UnityEditor.AudioImporter": {
					return assumedImporterType;
				}
			}
			
			// not specific type importer. should determine their type by extension.
			var extension = Path.GetExtension(assetPath);
			if (AssumeTypeBindingByExtension.ContainsKey(extension)) {
				return AssumeTypeBindingByExtension[extension];
			}

			if (IgnoredExtension.Contains(extension)) {
				return null;
			}
			
			// unhandled.
			Debug.LogWarning("Unknown file type found:" + extension + "\n. Asset:" + assetPath + "\n Assume 'object'.");
			return typeof(object);
		}			

		public static Type FindIncomingAssetType(List<Asset> assets) {
			if(assets.Any()) {
				Type expectedType = FindTypeOfAsset(assets.First().importFrom);
				return expectedType;
			}

			return null;
		}
	}
}
