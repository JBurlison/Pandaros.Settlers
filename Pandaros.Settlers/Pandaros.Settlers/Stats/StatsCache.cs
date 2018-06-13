using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Stats
{
    [ModLoader.ModManager]
    public static class StatsCache
    {
        private static Dictionary<PlayerState, Dictionary<ushort, int>> _itemsPlaced = new Dictionary<PlayerState, Dictionary<ushort, int>>();
        private static Dictionary<PlayerState, Dictionary<ushort, int>> _itemsRemoved = new Dictionary<PlayerState, Dictionary<ushort, int>>();
        private static Dictionary<PlayerState, Dictionary<ushort, int>> _itemsInWorld = new Dictionary<PlayerState, Dictionary<ushort, int>>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructTooltipUI, GameLoader.NAMESPACE + ".Stats.StatsCache.ConstructTooltip")]
        static void ConstructTooltip(ConstructTooltipUIData data)
        {
            if (data.hoverType != Shared.ETooltipHoverType.Item)
                return;

            var ps = PlayerState.GetPlayerState(data.player);
            
            if (_itemsPlaced.ContainsKey(ps) && _itemsPlaced[ps].ContainsKey(data.hoverItem))
            {
                data.menu.Items.Add(new Label(new LabelData("NumberPlaced", UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence)));
                //_itemsPlaced[ps][data.hoverItem];
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Stats.StatsCache.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            var ps = PlayerState.GetPlayerState(d.RequestedByPlayer);
        }
    }
}
