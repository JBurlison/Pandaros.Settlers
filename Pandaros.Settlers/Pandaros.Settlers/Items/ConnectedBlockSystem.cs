using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    class ListComparer<T> : IEqualityComparer<List<T>>
    {
        public bool Equals(List<T> x, List<T> y)
        {
            foreach (T t in x)
                if (!y.Contains(t))
                    return false;

            foreach (T t in y)
                if (!x.Contains(t))
                    return false;

            return true;
        }

        public int GetHashCode(List<T> obj)
        {
            int hashcode = 0;
            foreach (T t in obj)
            {
                hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
    
    [ModLoader.ModManager]
    public static class ConnectedBlockSystem
    {
        private static Dictionary<string, Dictionary<List<BlockSides>, ICSType>> _connectedBlockLookup = new Dictionary<string, Dictionary<List<BlockSides>, ICSType>>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, ICSType> _blockLookup = new Dictionary<string, ICSType>(StringComparer.InvariantCultureIgnoreCase);

        public static void AddConnectedBlock(ICSType cSType)
        {
            if (cSType.ConnectedBlock != null && !string.IsNullOrEmpty(cSType.ConnectedBlock.BlockType))
            {
                _blockLookup[cSType.name] = cSType;

                if (!_connectedBlockLookup.ContainsKey(cSType.ConnectedBlock.BlockType))
                    _connectedBlockLookup.Add(cSType.ConnectedBlock.BlockType, new Dictionary<List<BlockSides>, ICSType>(new ListComparer<BlockSides>()));

                _connectedBlockLookup[cSType.ConnectedBlock.BlockType][cSType.ConnectedBlock.Connections] = cSType;
            }
        }

        public static bool TryGetConnectingBlock(string blockType, List<BlockSides> neededSides, out ICSType connectedBlock)
        {
            connectedBlock = null;
            return _connectedBlockLookup.ContainsKey(blockType) && _connectedBlockLookup[blockType].TryGetValue(neededSides, out connectedBlock);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Items.ConnectedBlockSystem.OnTryChangeBlock")]
        public static void OnTryChangeBlock(ModLoader.OnTryChangeBlockData onTryChangeBlockData)
        {
            if (onTryChangeBlockData.CallbackOrigin == ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual && 
                _blockLookup.TryGetValue(onTryChangeBlockData.TypeNew.Name, out var connectedBlock))
            {

            }
        }
    }
}
