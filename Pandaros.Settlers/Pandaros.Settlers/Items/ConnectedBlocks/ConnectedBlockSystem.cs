using Pandaros.Settlers.Models;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class ConnectedBlockSystem
    {
        private static Dictionary<string, Dictionary<List<BlockSide>, ICSType>> _connectedBlockLookup = new Dictionary<string, Dictionary<List<BlockSide>, ICSType>>(StringComparer.InvariantCultureIgnoreCase);
        public static Dictionary<string, ICSType> BlockLookup { get; private set; } = new Dictionary<string, ICSType>(StringComparer.InvariantCultureIgnoreCase);
        private static BlockSideComparer _blockSideComparer = new BlockSideComparer();

        public static void AddConnectedBlock(ICSType cSType)
        {
            if (cSType.ConnectedBlock != null && 
                !string.IsNullOrEmpty(cSType.ConnectedBlock.BlockType) && 
                cSType.ConnectedBlock.AutoChange &&
                 !string.IsNullOrEmpty(cSType.ConnectedBlock.CalculationType))
            {
                cSType.ConnectedBlock.Connections.Sort();
                BlockLookup[cSType.name] = cSType;

                if (!_connectedBlockLookup.ContainsKey(cSType.ConnectedBlock.BlockType))
                    _connectedBlockLookup.Add(cSType.ConnectedBlock.BlockType, new Dictionary<List<BlockSide>, ICSType>(new ListComparer<BlockSide>()));

                if (!_connectedBlockLookup[cSType.ConnectedBlock.BlockType].ContainsKey(cSType.ConnectedBlock.Connections))
                    _connectedBlockLookup[cSType.ConnectedBlock.BlockType][cSType.ConnectedBlock.Connections] = cSType;

                if (cSType.ConnectedBlock.Connections.Count == 2 &&
                   ((cSType.ConnectedBlock.Connections.Contains(BlockSide.Xn) && cSType.ConnectedBlock.Connections.Contains(BlockSide.Xp)) ||
                   (cSType.ConnectedBlock.Connections.Contains(BlockSide.Yn) && cSType.ConnectedBlock.Connections.Contains(BlockSide.Yp)) ||
                   (cSType.ConnectedBlock.Connections.Contains(BlockSide.Zn) && cSType.ConnectedBlock.Connections.Contains(BlockSide.Zp))))
                    foreach (var side in cSType.ConnectedBlock.Connections)
                    {
                        var newBlockList = new List<BlockSide>() { side };
                        _connectedBlockLookup[cSType.ConnectedBlock.BlockType][newBlockList] = cSType;
                    }
            }
        }

        public static bool TryGetConnectingBlock(string blockType, List<BlockSide> neededSides, out ICSType connectedBlock)
        {
            connectedBlock = null;

            if (_connectedBlockLookup.ContainsKey(blockType))
            {
                if (!_connectedBlockLookup[blockType].TryGetValue(neededSides, out connectedBlock))
                {
                    IEnumerable<KeyValuePair<List<BlockSide>, ICSType>> lookup = _connectedBlockLookup[blockType];

                    if (neededSides.Contains(BlockSide.XpYp) ||
                        neededSides.Contains(BlockSide.XnYp) ||
                        neededSides.Contains(BlockSide.ZpYp) ||
                        neededSides.Contains(BlockSide.ZnYp))
                        lookup = lookup.Reverse();

                    foreach (var kvp in lookup)
                    {
                        if (_blockSideComparer.Equals(kvp.Key, neededSides))
                        {
                            connectedBlock = kvp.Value;
                            break;
                        }
                    }
                }

                return connectedBlock != null;
            }

            return false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnChangedBlock, GameLoader.NAMESPACE + ".Items.ConnectedBlockSystem.OnChangedBlock")]
        public static void OnChangedBlock(ModLoader.OnTryChangeBlockData onTryChangeBlockData)
        {
            var connectedBlock = default(ICSType);

            if (onTryChangeBlockData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player &&
                (BlockLookup.TryGetValue(onTryChangeBlockData.TypeNew.Name, out connectedBlock) ||
                BlockLookup.TryGetValue(onTryChangeBlockData.TypeOld.Name, out connectedBlock)) &&
                ConnectedBlockCalculator.CalculationTypes.TryGetValue(connectedBlock.ConnectedBlock.CalculationType, out var connectedBlockCalculationType))
            {
                ChangeBlocksForPos(onTryChangeBlockData.Position, connectedBlock.ConnectedBlock.BlockType, connectedBlockCalculationType);

                foreach (var block in connectedBlockCalculationType.AvailableBlockSides)
                    ChangeBlocksForPos(onTryChangeBlockData.Position.GetBlockOffset(block), connectedBlock.ConnectedBlock.BlockType, connectedBlockCalculationType);
            }
        }

        public static void ChangeBlocksForPos(Vector3Int pos, string blockType, IConnectedBlockCalculationType calculationType)
        {
            if (World.TryGetTypeAt(pos, out ItemTypes.ItemType itemTypeAtPos) && 
                BlockLookup.TryGetValue(itemTypeAtPos.Name, out var existingBlock) && 
                existingBlock.ConnectedBlock.BlockType == blockType)
            {
                if (calculationType != null && TryGetChangedBlockTypeAtPosition(pos, blockType, calculationType, out var newBlock) && newBlock.ConnectedBlock.AutoChange)
                     ServerManager.TryChangeBlock(pos, ItemId.GetItemId(newBlock.name));
            }
        }

        public static bool TryGetChangedBlockTypeAtPosition(Vector3Int centerBlock, string blockType, IConnectedBlockCalculationType calculationType, out ICSType newBlock)
        {
            List<BlockSide> connectedBlocks = GetConnectedBlocks(centerBlock, blockType, calculationType);
            return TryGetConnectingBlock(blockType, connectedBlocks, out newBlock);
        }

        public static List<BlockSide> GetConnectedBlocks(Vector3Int centerBlock, string blockType, IConnectedBlockCalculationType calculationType)
        {
            List<BlockSide> connectedBlocks = new List<BlockSide>();

            foreach(var block in calculationType.AvailableBlockSides)
                if (World.TryGetTypeAt(centerBlock.GetBlockOffset(block), out ItemTypes.ItemType blockAtLocation) &&
                    ItemCache.CSItems.TryGetValue(blockAtLocation.Name, out var existingBlockType) &&
                    string.Equals(existingBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                        connectedBlocks.Add(block);

            connectedBlocks.Sort();

            return connectedBlocks;
        }



    }
}
