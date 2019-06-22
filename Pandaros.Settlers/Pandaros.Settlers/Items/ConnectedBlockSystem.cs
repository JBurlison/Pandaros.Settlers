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
        private static Dictionary<string, Dictionary<List<BlockSides>, ICSType>> _connectedBlockLookup = new Dictionary<string, Dictionary<List<BlockSides>, ICSType>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, ICSType> _blockLookup = new Dictionary<string, ICSType>(StringComparer.InvariantCultureIgnoreCase);
        private static BlockSides[] _blockTypes = (BlockSides[])Enum.GetValues(typeof(BlockSides));

        public static void AddConnectedBlock(ICSType cSType)
        {
            if (cSType.ConnectedBlock != null && !string.IsNullOrEmpty(cSType.ConnectedBlock.BlockType))
            {
                cSType.ConnectedBlock.Connections.Sort();
                _blockLookup[cSType.name] = cSType;

                if (!_connectedBlockLookup.ContainsKey(cSType.ConnectedBlock.BlockType))
                    _connectedBlockLookup.Add(cSType.ConnectedBlock.BlockType, new Dictionary<List<BlockSides>, ICSType>(new ListComparer<BlockSides>()));

                _connectedBlockLookup[cSType.ConnectedBlock.BlockType][cSType.ConnectedBlock.Connections] = cSType;

                if (cSType.ConnectedBlock.Connections.Count == 2 &&
                    ((cSType.ConnectedBlock.Connections.Contains(BlockSides.Xn) && cSType.ConnectedBlock.Connections.Contains(BlockSides.Xp)) ||
                    (cSType.ConnectedBlock.Connections.Contains(BlockSides.Yn) && cSType.ConnectedBlock.Connections.Contains(BlockSides.Yp)) ||
                    (cSType.ConnectedBlock.Connections.Contains(BlockSides.Zn) && cSType.ConnectedBlock.Connections.Contains(BlockSides.Zp))))
                    foreach (var side in cSType.ConnectedBlock.Connections)
                    {
                        var newBlockList = new List<BlockSides>() { side };
                        _connectedBlockLookup[cSType.ConnectedBlock.BlockType][newBlockList] = cSType;
                    }
            }
        }

        public static bool TryGetConnectingBlock(string blockType, List<BlockSides> neededSides, out ICSType connectedBlock)
        {
            connectedBlock = null;
            return _connectedBlockLookup.ContainsKey(blockType) && _connectedBlockLookup[blockType].TryGetValue(neededSides, out connectedBlock);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnChangedBlock, GameLoader.NAMESPACE + ".Items.ConnectedBlockSystem.OnChangedBlock")]
        public static void OnChangedBlock(ModLoader.OnTryChangeBlockData onTryChangeBlockData)
        {
            var connectedBlock = default(ICSType);

            if (onTryChangeBlockData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player && 
                (_blockLookup.TryGetValue(onTryChangeBlockData.TypeNew.Name, out connectedBlock) ||
                _blockLookup.TryGetValue(onTryChangeBlockData.TypeOld.Name, out connectedBlock)))
            {
                if (onTryChangeBlockData.TypeNew.Name != ColonyBuiltIn.ItemTypes.AIR &&
                    TryGetChangedBlockTypeAtPosition(onTryChangeBlockData.Position, connectedBlock.ConnectedBlock.BlockType, out var newBlock))
                    ServerManager.TryChangeBlock(onTryChangeBlockData.Position, ItemId.GetItemId(newBlock.name));

                foreach (var block in _blockTypes)
                    ChangeBlocksForPos(onTryChangeBlockData.Position.GetBlockOffset(block), connectedBlock.ConnectedBlock.BlockType);
            }
        }

        public static void ChangeBlocksForPos(Vector3Int pos, string blockType = null)
        {
            if (World.TryGetTypeAt(pos, out ItemTypes.ItemType itemTypeAtPos) && _blockLookup.ContainsKey(itemTypeAtPos.Name))
            {
                if (blockType == null &&
                    _blockLookup.TryGetValue(itemTypeAtPos.Name, out var connectedBlockAtPos))
                    blockType = connectedBlockAtPos.ConnectedBlock.BlockType;

                if (TryGetChangedBlockTypeAtPosition(pos, blockType, out var newBlock))
                     ServerManager.TryChangeBlock(pos, ItemId.GetItemId(newBlock.name));
            }
        }

        public static bool TryGetChangedBlockTypeAtPosition(Vector3Int centerBlock, string blockType, out ICSType newBlock)
        {
            List<BlockSides> connectedBlocks = GetConnectedBlocks(centerBlock, blockType);
            return TryGetConnectingBlock(blockType, connectedBlocks, out newBlock);
        }

        public static List<BlockSides> GetConnectedBlocks(Vector3Int centerBlock, string blockType)
        {
            List<BlockSides> connectedBlocks = new List<BlockSides>();

            foreach (var block in _blockTypes)
                SetBlock(centerBlock, blockType, block, connectedBlocks);

            connectedBlocks.Sort();

            return connectedBlocks;
        }

        private static void SetBlock(Vector3Int centerBlock, string blockType, BlockSides blockSide, List<BlockSides> connectedBlocks)
        {
            if (World.TryGetTypeAt(centerBlock.GetBlockOffset(blockSide), out ItemTypes.ItemType blockAtLocation) &&
                _blockLookup.TryGetValue(blockAtLocation.Name, out var existingBlockType) &&
                string.Equals(existingBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(blockSide);
        }

    }
}
