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
        public static Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>> BlockRotations { get; } = new Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>>(StringComparer.InvariantCultureIgnoreCase);

        private static Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>> _blocksideRotations = new Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>();

        private static RotationAxis[] _blockRotations = (RotationAxis[])Enum.GetValues(typeof(RotationAxis));
        private static BlockRotationDegrees[] _blockRotationDegrees = (BlockRotationDegrees[])Enum.GetValues(typeof(BlockRotationDegrees));
        private static ListComparer<BlockSide> _blockSideCompare = new ListComparer<BlockSide>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameLoader.NAMESPACE + ".Items.ConnectedBlockCalculator.Initialize")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterModsLoaded")]
        public static void Initialize(List<ModLoader.ModDescription> list)
        {
            //_blocksideRotations = JsonConvert.DeserializeObject<Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>>(File.ReadAllText(Path.Combine(GameLoader.MOD_FOLDER, "BlockRotations.json")));
            _blocksideRotations = JsonConvert.DeserializeObject<Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>>(File.ReadAllText(Path.Combine("./BlockRotations.json")));

            foreach (var kvp in CalculationTypes)
            {
                List<List<BlockSide>> blockSides = new List<List<BlockSide>>();
                BlockRotations[kvp.Key] = new Dictionary<List<BlockSide>, MeshRotationEuler>(_blockSideCompare);

                for (int i = 1; i <= kvp.Value.MaxConnections; i++)
                {
                    var permutations = kvp.Value.AvailableBlockSides.GetPermutations(i);

                    foreach (var permutation in permutations)
                    {
                        var per = permutation.ToList();
                        per.Sort();

                        if (!per.Contains(BlockSide.Invlaid) && !blockSides.Contains(per, _blockSideCompare))
                            blockSides.Add(per);
                    }
                }

                foreach (var blockSidePermutationList in blockSides)
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

                            foreach (var side in blockSidePermutationList)
                            {
                                if (_blocksideRotations.TryGetValue(side, out var axisDic) &&
                                    axisDic.TryGetValue(axis, out var rotationDic) &&
                                    rotationDic.TryGetValue(rotationDegrees, out var newBlockSide))
                                {
                                    rotatedList.Add(newBlockSide);
                                }
                            }

                            if (rotatedList.Count != 0 && !rotatedList.Contains(BlockSide.Invlaid) && !BlockRotations[kvp.Key].ContainsKey(rotatedList))
                                BlockRotations[kvp.Key][rotatedList] = rotationEuler;
                        }
            }
        }

        private static bool TryGetRotationAndAxis(BlockSide side,
                                                  List<Tuple<RotationAxis, BlockRotationDegrees>> rotationAxesBlacklist,
                                                  out RotationAxis rotationAxis, 
                                                  out BlockRotationDegrees blockRotationDegrees)
        {
            rotationAxis = default(RotationAxis);
            blockRotationDegrees = default(BlockRotationDegrees);

            foreach (var initalBlocksideKvp in _blocksideRotations)
            {
                foreach (var axisKvp in initalBlocksideKvp.Value)
                {
                    foreach (var rotationKvp in axisKvp.Value)
                    {
                        if (rotationAxesBlacklist.Contains(Tuple.Create(axisKvp.Key, rotationKvp.Key)))
                            continue;

                        if (rotationKvp.Value == side)
                        {
                            blockRotationDegrees = rotationKvp.Key;
                            rotationAxis = axisKvp.Key;
                            return true;
                        }
                    }
                }
            }

            return false;
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
                var itemJson = JsonConvert.SerializeObject(baseBlock, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None });

                foreach (var permutaion in permutations)
                {
                    var permutionList = permutaion.ToList();
                    permutionList.Sort();

                    if (rotationDic.TryGetValue(permutionList, out var meshRotationEuler))
                    {
                        var newItem = JsonConvert.DeserializeObject<CSType>(itemJson);
                        newItem.meshRotationEuler = meshRotationEuler;
                        newItem.ConnectedBlock = new ConnectedBlock()
                        {
                            BlockType = baseBlock.ConnectedBlock.BlockType,
                            CalculationType = baseBlock.ConnectedBlock.CalculationType,
                            Connections = permutaion.ToList()
                        };

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
