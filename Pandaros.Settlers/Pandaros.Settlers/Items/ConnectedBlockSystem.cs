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
                    World.TryChangeBlock(onTryChangeBlockData.Position, ItemId.GetItemId(newBlock.name), new BlockChangeRequestOrigin(onTryChangeBlockData.RequestOrigin.AsPlayer));

                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(1, 0, 0), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(-1, 0, 0), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(0, 1, 0), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(0, -1, 0), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(0, 0, 1), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
                ChangeBlocksForPos(onTryChangeBlockData.Position.Add(0, 0, -1), onTryChangeBlockData.RequestOrigin.AsPlayer, connectedBlock.ConnectedBlock.BlockType);
            }
        }

        public static void ChangeBlocksForPos(Vector3Int pos, Players.Player player, string blockType = null)
        {
            if (World.TryGetTypeAt(pos, out ItemTypes.ItemType itemTypeAtPos) && itemTypeAtPos.Name != ColonyBuiltIn.ItemTypes.AIR)
            {
                if (blockType == null &&
                    _blockLookup.TryGetValue(itemTypeAtPos.Name, out var connectedBlockAtPos))
                    blockType = connectedBlockAtPos.ConnectedBlock.BlockType;

                if (TryGetChangedBlockTypeAtPosition(pos, blockType, out var newBlock))
                    PandaLogger.Log("Change block at {0} To: {1} Result: {2}", pos, newBlock.name, World.TryChangeBlock(pos, ItemId.GetItemId(newBlock.name), new BlockChangeRequestOrigin(player)));
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

            if (World.TryGetTypeAt(centerBlock.Add(-1, 0, 0), out ItemTypes.ItemType xnBlock) &&
                _blockLookup.TryGetValue(xnBlock.Name, out var xnBlockType) &&
                string.Equals(xnBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Xn);

            if (World.TryGetTypeAt(centerBlock.Add(1, 0, 0), out ItemTypes.ItemType xpBlock) &&
                _blockLookup.TryGetValue(xpBlock.Name, out var xpBlockType) &&
                string.Equals(xpBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Xp);

            if (World.TryGetTypeAt(centerBlock.Add(0, -1, 0), out ItemTypes.ItemType ynBlock) &&
                _blockLookup.TryGetValue(ynBlock.Name, out var ynBlockType) &&
                string.Equals(ynBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Yn);

            if (World.TryGetTypeAt(centerBlock.Add(0, 1, 0), out ItemTypes.ItemType ypBlock) &&
                _blockLookup.TryGetValue(ypBlock.Name, out var ypBlockType) &&
                string.Equals(ypBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Yp);

            if (World.TryGetTypeAt(centerBlock.Add(0, 0, -1), out ItemTypes.ItemType znBlock) &&
                _blockLookup.TryGetValue(znBlock.Name, out var znBlockType) &&
                string.Equals(znBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Zn);

            if (World.TryGetTypeAt(centerBlock.Add(0, 0, 1), out ItemTypes.ItemType zpBlock) &&
                _blockLookup.TryGetValue(zpBlock.Name, out var zpBlockType) &&
                string.Equals(zpBlockType.ConnectedBlock.BlockType, blockType, StringComparison.InvariantCultureIgnoreCase))
                connectedBlocks.Add(BlockSides.Zp);

            connectedBlocks.Sort();

            return connectedBlocks;
        }
    }
}
