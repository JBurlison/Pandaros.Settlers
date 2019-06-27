using Newtonsoft.Json;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class ConnectedBlockCalculator
    {
        public static Dictionary<string, IConnectedBlockCalculationType> CalculationTypes { get; } = new Dictionary<string, IConnectedBlockCalculationType>(StringComparer.InvariantCultureIgnoreCase);

        private static RotationAxis[] _blockRotations = (RotationAxis[])Enum.GetValues(typeof(RotationAxis));
        private static BlockRotationDegrees[] _blockRotationDegrees = (BlockRotationDegrees[])Enum.GetValues(typeof(BlockRotationDegrees));

        public static List<ICSType> GetPermutations(ICSType baseBlock)
        {
            Dictionary<List<BlockSide>, ICSType> cSTypes = new Dictionary<List<BlockSide>, ICSType>(new ListComparer<BlockSide>());

            if (baseBlock.ConnectedBlock != null &&
                !string.IsNullOrWhiteSpace(baseBlock.ConnectedBlock.CalculationType) && 
                baseBlock.ConnectedBlock.Connections != null &&
                baseBlock.ConnectedBlock.Connections.Count > 0)
            {
                var itemJson = JsonConvert.SerializeObject(baseBlock, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None });
                PermutateItems(baseBlock, cSTypes, itemJson, baseBlock.ConnectedBlock.Connections);
            }

            return cSTypes.Values.ToList();
        }

        private static void PermutateItems(ICSType baseBlock, Dictionary<List<BlockSide>, ICSType> cSTypes, string itemJson, List<BlockSide> connections)
        {
            foreach (RotationAxis axis in _blockRotations)
                foreach (BlockRotationDegrees rotationDegrees in _blockRotationDegrees)
                {
                    var rotationEuler = new MeshRotationEuler();
                    var rotatedList = new List<BlockSide>();
                    var currentRotation = rotationDegrees;

                    if (baseBlock.meshRotationEuler != null)
                    {
                        rotationEuler.x = baseBlock.meshRotationEuler.x;
                        rotationEuler.y = baseBlock.meshRotationEuler.y;
                        rotationEuler.z = baseBlock.meshRotationEuler.z;
                    }

                    switch (axis)
                    {
                        case RotationAxis.X:
                            rotationEuler.x += (int)rotationDegrees;

                            if (rotationEuler.x > (int)BlockRotationDegrees.TwoSeventy)
                                rotationEuler.x -= 360;

                            currentRotation = (BlockRotationDegrees)rotationEuler.x;
                            break;

                        case RotationAxis.Y:
                            rotationEuler.y += (int)rotationDegrees;

                            if (rotationEuler.y > (int)BlockRotationDegrees.TwoSeventy)
                                rotationEuler.y -= 360;

                            currentRotation = (BlockRotationDegrees)rotationEuler.x;
                            break;

                        case RotationAxis.Z:
                            rotationEuler.z += (int)rotationDegrees;

                            if (rotationEuler.z > (int)BlockRotationDegrees.TwoSeventy)
                                rotationEuler.z -= 360;

                            currentRotation = (BlockRotationDegrees)rotationEuler.x;
                            break;
                    }

                    if (connections.Count() == connections.Distinct().Count())
                        foreach (var side in connections)
                        {
                            Vector3 connectionPoint = side.GetVector();
                            Vector3 eulerRotation = new Vector3(rotationEuler.x, rotationEuler.y, rotationEuler.z);
                            
                            Vector3 rotatedConnectionPoint = Quaternion.Euler(eulerRotation) * connectionPoint;

                            if (rotatedConnectionPoint == Vector3.zero)
                                rotatedConnectionPoint = connectionPoint;

                            rotatedList.Add(rotatedConnectionPoint.GetBlocksideFromVector());
                        }

                    rotatedList.Sort();

                    if (rotatedList.Count != 0 &&
                        !rotatedList.Contains(BlockSide.Invalid) &&
                        rotatedList.Count == rotatedList.Distinct().Count() &&
                        !cSTypes.ContainsKey(rotatedList) &&
                        !rotatedList.All(r => r == rotatedList.First()))
                    {
                        var newItem = JsonConvert.DeserializeObject<CSType>(itemJson);
                        newItem.meshRotationEuler = rotationEuler;
                        newItem.ConnectedBlock = new ConnectedBlock()
                        {
                            BlockType = baseBlock.ConnectedBlock.BlockType,
                            CalculationType = baseBlock.ConnectedBlock.CalculationType,
                            Connections = rotatedList,
                            BlockRotationDegrees = currentRotation,
                            RotationAxis = axis
                        };

                        newItem.name = string.Concat(newItem.name, ".", GetItemName(newItem.ConnectedBlock.Connections));
                        cSTypes[newItem.ConnectedBlock.Connections] = newItem;
                        PermutateItems(newItem, cSTypes, itemJson, connections);
                    }
                }
        }

        private static string GetItemName(List<BlockSide> sides)
        {
            StringBuilder nameBuilder = new StringBuilder();

            foreach (var side in sides)
                nameBuilder.Append(side.ToString());

            return nameBuilder.ToString();
        }
    }
}
