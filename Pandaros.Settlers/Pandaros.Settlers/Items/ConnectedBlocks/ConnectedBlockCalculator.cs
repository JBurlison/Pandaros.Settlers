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
            List<ICSType> cSTypes = new List<ICSType>();

            if (baseBlock.ConnectedBlock != null &&
                !string.IsNullOrWhiteSpace(baseBlock.ConnectedBlock.CalculationType) && 
                baseBlock.ConnectedBlock.Connections != null &&
                baseBlock.ConnectedBlock.Connections.Count > 0)
            {
                var itemJson = JsonConvert.SerializeObject(baseBlock, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None });

                foreach (BlockRotationDegrees rotationDegrees in _blockRotationDegrees)
                    foreach (RotationAxis axis in _blockRotations)
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

                        foreach (var side in baseBlock.ConnectedBlock.Connections)
                        {
                            if (_blocksideRotations.TryGetValue(side, out var axisDic) &&
                                axisDic.TryGetValue(axis, out var rotationDic) &&
                                rotationDic.TryGetValue(rotationDegrees, out var newBlockSide))
                            {
                                rotatedList.Add(newBlockSide);
                            }
                        }

                        if (rotatedList.Count != 0 && !rotatedList.Contains(BlockSide.Invalid))
                        {
                            var newItem = JsonConvert.DeserializeObject<CSType>(itemJson);
                            newItem.meshRotationEuler = rotationEuler;
                            newItem.ConnectedBlock = new ConnectedBlock()
                            {
                                BlockType = baseBlock.ConnectedBlock.BlockType,
                                CalculationType = baseBlock.ConnectedBlock.CalculationType,
                                Connections = rotatedList.ToList()
                            };
                            newItem.ConnectedBlock.Connections.Sort();
                            newItem.name = string.Concat(newItem.name, ".", GetItemName(newItem.ConnectedBlock.Connections));
                            cSTypes.Add(newItem);
                        }
                    }
            }

            return cSTypes;
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
