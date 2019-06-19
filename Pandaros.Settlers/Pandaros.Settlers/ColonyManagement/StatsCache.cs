using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Entities;
using System.Collections.Generic;

namespace Pandaros.Settlers.ColonyManagement
{
    // TODO: More stats?
    [ModLoader.ModManager]
    public static class StatsCache
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructTooltipUI, GameLoader.NAMESPACE + ".ColonyManager.StatsCache.ConstructTooltip")]
        static void ConstructTooltip(ConstructTooltipUIData data)
        {
            if (data.hoverType != Shared.ETooltipHoverType.Item ||
                data.player.ID.type == NetworkID.IDType.Server ||
                data.player.ID.type == NetworkID.IDType.Invalid ||
                !ItemTypes.TryGetType(data.hoverItem, out var item))
                return;

            var ps = PlayerState.GetPlayerState(data.player);
            var cs = ColonyState.GetColonyState(ps.Player.ActiveColony);

            if (ps != null)
            {
                if (item.IsPlaceable)
                {
                    ushort itemId = GetParentId(data.hoverItem, item);

                    BuildPlaceableMenu(data, itemId, ps.ItemsPlaced, "PlayerNumberPlaced");
                    BuildPlaceableMenu(data, itemId, ps.ItemsRemoved, "PlayerNumberRemoved");
                    BuildPlaceableMenu(data, itemId, ps.ItemsInWorld, "PlayerNumberInWorld");
                }
            }

            if (cs != null)
            {
                if (item.IsPlaceable)
                {
                    ushort itemId = GetParentId(data.hoverItem, item);

                    BuildPlaceableMenu(data, itemId, cs.ItemsPlaced, "ColonyNumberPlaced");
                    BuildPlaceableMenu(data, itemId, cs.ItemsRemoved, "ColonyNumberRemoved");
                    BuildPlaceableMenu(data, itemId, cs.ItemsInWorld, "ColonyNumberInWorld");
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.StatsCache.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.RequestOrigin.AsPlayer == null ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Server ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Invalid)
                return;

            var ps = PlayerState.GetPlayerState(d.RequestOrigin.AsPlayer);
            var cs = ColonyState.GetColonyState(ps.Player.ActiveColony);

            if (ps != null)
                AddToCount(d, ps.ItemsPlaced, ps.ItemsInWorld, ps.ItemsRemoved);

            if (cs != null)
                AddToCount(d, cs.ItemsPlaced, cs.ItemsInWorld, cs.ItemsRemoved);
        }

        private static void AddToCount(ModLoader.OnTryChangeBlockData d, Dictionary<ushort, int> ItemsPlaced, Dictionary<ushort, int> ItemsInWorld, Dictionary<ushort, int> ItemsRemoved)
        {
            if (d.TypeNew.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id && ItemTypes.TryGetType(d.TypeNew.ItemIndex, out var item))
            {
                ushort itemId = GetParentId(d.TypeNew.ItemIndex, item);

                if (!ItemsPlaced.ContainsKey(itemId))
                    ItemsPlaced.Add(itemId, 0);

                if (!ItemsInWorld.ContainsKey(itemId))
                    ItemsInWorld.Add(itemId, 0);

                ItemsPlaced[itemId]++;
                ItemsInWorld[itemId]++;
            }

            if (d.TypeNew.ItemIndex == ColonyBuiltIn.ItemTypes.AIR.Id && d.TypeOld.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id && ItemTypes.TryGetType(d.TypeOld.ItemIndex, out var itemOld))
            {
                ushort itemId = GetParentId(d.TypeOld.ItemIndex, itemOld);

                if (!ItemsRemoved.ContainsKey(itemId))
                    ItemsRemoved.Add(itemId, 0);

                if (!ItemsInWorld.ContainsKey(itemId))
                    ItemsInWorld.Add(itemId, 0);
                else
                    ItemsInWorld[itemId]--;

                ItemsRemoved[itemId]++;
            }
        }

        private static ushort GetParentId(ushort siblingType, ItemTypes.ItemType itemOld)
        {
            var itemId = siblingType;
            var parent = itemOld.GetRootParentType();

            if (parent != null)
                itemId = parent.ItemIndex;

            return itemId;
        }

        private static void BuildPlaceableMenu(ConstructTooltipUIData data, ushort item, Dictionary<ushort, int> dict, string sentenceKey)
        {
            if (!dict.ContainsKey(item))
                dict.Add(item, 0);

            data.menu.Items.Add(new HorizontalSplit(new Label(new LabelData(GameLoader.NAMESPACE + ".inventory." + sentenceKey, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence)),
                                                    new Label(new LabelData(dict[item].ToString())), 30, 0.75f));
            
        }
    }
}
