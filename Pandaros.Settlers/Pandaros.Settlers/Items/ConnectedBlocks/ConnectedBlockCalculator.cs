using Newtonsoft.Json;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class ConnectedBlockCalculator
    {
        public static Dictionary<string, IConnectedBlockCalculationType> CalculationTypes { get; } = new Dictionary<string, IConnectedBlockCalculationType>(StringComparer.InvariantCultureIgnoreCase);
        public static Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>> BlockRotations { get; } = new Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>>();

        private static Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>> _blocksideRotations = new Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>();

        private static RotationAxis[] _blockRotations = (RotationAxis[])Enum.GetValues(typeof(RotationAxis));
        private static BlockRotationDegrees[] _blockRotationDegrees = (BlockRotationDegrees[])Enum.GetValues(typeof(BlockRotationDegrees));
        private static ListComparer<BlockSide> _blockSideCompare = new ListComparer<BlockSide>();

        public static void Initialize()
        {
            //_blocksideRotations = JsonConvert.DeserializeObject<Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>>(File.ReadAllText(Path.Combine(GameLoader.MOD_FOLDER, "BlockRotations.json")));
            _blocksideRotations = JsonConvert.DeserializeObject<Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>>(File.ReadAllText(Path.Combine("./BlockRotations.json")));

            foreach (var kvp in CalculationTypes)
            {
                List<List<BlockSide>> blockSides = new List<List<BlockSide>>();

                for (int i = 1; i <= kvp.Value.MaxConnections; i++)
                {
                    var permutations = kvp.Value.AvailableBlockSides.GetPermutations(i);

                    foreach (var permutation in permutations)
                    {
                        var per = permutation.ToList();
                        per.Sort();

                        if (!blockSides.Contains(per, _blockSideCompare))
                            blockSides.Add(per);
                    }
                }

                foreach (BlockRotationDegrees rotationDegrees in _blockRotationDegrees)
                {
                    foreach (var blockSidePermutationList in blockSides)
                    {
                        BlockRotations[kvp.Key] = new Dictionary<List<BlockSide>, MeshRotationEuler>(_blockSideCompare);
                        var rotationEuler = new MeshRotationEuler();
                        var rotatedList = new List<BlockSide>();

                        foreach (var side in blockSidePermutationList)
                        {
                            foreach (RotationAxis axis in _blockRotations)
                                if (_blocksideRotations.TryGetValue(side, out var axisDic) &&
                                    axisDic.TryGetValue(axis, out var rotationDic) &&
                                    rotationDic.TryGetValue(rotationDegrees, out var newBlockSide))
                                {
                                    rotatedList.Add(newBlockSide);

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
                                }
                        }

                        if (rotatedList.Count != 0 && !BlockRotations[kvp.Key].ContainsKey(rotatedList))
                            BlockRotations[kvp.Key][rotatedList] = rotationEuler;
                    }
                }
            }
        }

        public static List<ICSType> GetPermutations(ICSType baseBlock)
        {
            List<ICSType> cSTypes = new List<ICSType>();

            if (baseBlock.ConnectedBlock != null &&
                !string.IsNullOrWhiteSpace(baseBlock.ConnectedBlock.CalculationType) && 
                baseBlock.ConnectedBlock.Connections != null &&
                baseBlock.ConnectedBlock.Connections.Count > 0 &&
                BlockRotations.TryGetValue(baseBlock.ConnectedBlock.CalculationType, out var rotationDic))
            {
                var permutations = baseBlock.ConnectedBlock.Connections.GetPermutations(baseBlock.ConnectedBlock.Connections.Count);
                var itemJson = JsonConvert.SerializeObject(baseBlock);

                foreach (var permutaion in permutations)
                {
                    var newItem = JsonConvert.DeserializeObject<CSType>(itemJson);

                    newItem.ConnectedBlock = new ConnectedBlock()
                    {
                        BlockType = baseBlock.ConnectedBlock.BlockType,
                        CalculationType = baseBlock.ConnectedBlock.CalculationType,
                        Connections = permutaion.ToList()
                    };

                    newItem.name = string.Concat(newItem.name, ".", GetItemName(newItem.ConnectedBlock.Connections));
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
