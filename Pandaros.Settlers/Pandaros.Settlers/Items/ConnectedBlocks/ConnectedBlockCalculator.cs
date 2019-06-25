using Newtonsoft.Json;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class ConnectedBlockCalculator
    {
        public static Dictionary<string, IConnectedBlockCalculationType> CalculationTypes { get; } = new Dictionary<string, IConnectedBlockCalculationType>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>> _blocksideRotations = new Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>();

        private static RotationAxis[] _blockRotations = (RotationAxis[])Enum.GetValues(typeof(RotationAxis));
        private static BlockRotationDegrees[] _blockRotationDegrees = (BlockRotationDegrees[])Enum.GetValues(typeof(BlockRotationDegrees));

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameLoader.NAMESPACE + ".Items.ConnectedBlockCalculator.Initialize")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterModsLoaded")]
        public static void Initialize(List<ModLoader.ModDescription> list)
        {
            _blocksideRotations = JsonConvert.DeserializeObject<Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>>(File.ReadAllText(Path.Combine(GameLoader.MOD_FOLDER, "BlockRotations.json")));
        }

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

                foreach (var connection in baseBlock.ConnectedBlock.Connections.GetAllCombos())
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

                    switch (axis)
                    {
                        case RotationAxis.X:
                            rotationEuler.x = (int)rotationDegrees;
                            break;

                        case RotationAxis.Y:
                            rotationEuler.y = (int)rotationDegrees;
                            break;

                        case RotationAxis.Z:
                            rotationEuler.z = (int)rotationDegrees;
                            break;
                    }

                    foreach (var side in connections)
                    {
                        if (_blocksideRotations.TryGetValue(side, out var axisDic) &&
                            axisDic.TryGetValue(axis, out var rotationDic) &&
                            rotationDic.TryGetValue(rotationDegrees, out var newBlockSide))
                        {
                            rotatedList.Add(newBlockSide);
                        }
                    }

                    StoreItem(baseBlock, cSTypes, itemJson, rotationEuler, rotatedList);
                }
        }

        private static void StoreItem(ICSType baseBlock, Dictionary<List<BlockSide>, ICSType> cSTypes, string itemJson, MeshRotationEuler rotationEuler, List<BlockSide> rotatedList)
        {
            rotatedList.Sort();

            if (rotatedList.Count != 0 && !rotatedList.Contains(BlockSide.Invalid) && !cSTypes.ContainsKey(rotatedList))
            {
                var newItem = JsonConvert.DeserializeObject<CSType>(itemJson);
                newItem.meshRotationEuler = rotationEuler;
                newItem.ConnectedBlock = new ConnectedBlock()
                {
                    BlockType = baseBlock.ConnectedBlock.BlockType,
                    CalculationType = baseBlock.ConnectedBlock.CalculationType,
                    Connections = rotatedList
                };

                newItem.name = string.Concat(newItem.name, ".", GetItemName(newItem.ConnectedBlock.Connections));
                cSTypes[newItem.ConnectedBlock.Connections] = newItem;
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
