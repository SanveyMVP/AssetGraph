using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AssetBundleGraph {
	public class IntegratedGUIValidator : INodeOperation {
		public void Setup(BuildTarget target, NodeData nodeData, ConnectionPointData inputPoint, ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, List<string> alreadyCached, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output) {
			var incomingAssets = inputGroupAssets.SelectMany(v => v.Value).ToList();

			ValidateValidator(nodeData, target, incomingAssets,
				(Type expectedType, Type foundType, Asset foundAsset) => {
					throw new NodeException(string.Format("{3} :Validator expect {0}, but different type of incoming asset is found({1} {2})",
						expectedType.FullName, foundType.FullName, foundAsset.fileNameAndExtension, nodeData.Name), nodeData.Id);
				},
				() => {
					throw new NodeException(nodeData.Name + " :Validator is not configured. Please configure from Inspector.", nodeData.Id);
				},
				() => {
					throw new NodeException(nodeData.Name + " :Failed to create Validator from settings. Please fix settings from Inspector.", nodeData.Id);
				},
				(Type expected, Type incoming) => {
					throw new NodeException(string.Format("{0} :Incoming asset type is does not match with this Validator (Expected type:{1}, Incoming type:{2}).",
						nodeData.Name, (expected != null) ? expected.FullName : "null", (incoming != null) ? incoming.FullName : "null"), nodeData.Id);
				}
			);

			// Modifier does not add, filter or change structure of group, so just pass given group of assets
			Output(connectionToOutput, inputGroupAssets, null);
		}

		public void Run(BuildTarget target, NodeData nodeData, ConnectionPointData inputPoint, ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, List<string> alreadyCached, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output) {
			Output(connectionToOutput, inputGroupAssets, null);
		}
		
		public static void ValidateValidator(
			NodeData node,
			BuildTarget target,
			List<Asset> incomingAssets,
			Action<Type, Type, Asset> multipleAssetTypeFound,
			Action noValidatorData,
			Action failedToCreateValidator,
			Action<Type, Type> incomingTypeMismatch
		) {
			Type expectedType = TypeUtility.FindIncomingAssetType(incomingAssets);
			if(expectedType != null) {
				foreach(var a in incomingAssets) {
					Type assetType = TypeUtility.FindTypeOfAsset(a.importFrom);
					if(assetType != expectedType) {
						multipleAssetTypeFound(expectedType, assetType, a);
					}
				}
			}

			if(string.IsNullOrEmpty(node.InstanceData[target])) {
				noValidatorData();
			}

			var validator = ValidatorUtility.CreateValidator(node, target);

			if(null == validator) {
				failedToCreateValidator();
			}

			// if there is no incoming assets, there is no way to check if 
			// right type of asset is coming in - so we'll just skip the test
			if(incomingAssets.Any()) {
				var targetType = ValidatorUtility.GetValidatorTargetType(validator);
				if(targetType != expectedType) {
					incomingTypeMismatch(targetType, expectedType);
				}
			}
		}
	}
}
