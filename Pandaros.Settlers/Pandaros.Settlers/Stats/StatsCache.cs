using BlockTypes.Builtin;
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
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructTooltipUI, GameLoader.NAMESPACE + ".Stats.StatsCache.ConstructTooltip")]
        static void ConstructTooltip(ConstructTooltipUIData data)
        {
            if (data.hoverType != Shared.ETooltipHoverType.Item ||
                data.player.ID.type == NetworkID.IDType.Server ||
                data.player.ID.type == NetworkID.IDType.Invalid ||
                !ItemTypes.TryGetType(data.hoverItem, out var item))
                return;

            var ps = PlayerState.GetPlayerState(data.player);

            if (ps != null)
            {
                if (item.IsPlaceable)
                {
                    ushort itemId = GetParentId(data.hoverItem, item);

                    BuildPlaceableMenu(data, itemId, ps, ps.ItemsPlaced, "NumberPlaced");
                    BuildPlaceableMenu(data, itemId, ps, ps.ItemsRemoved, "NumberRemoved");
                    BuildPlaceableMenu(data, itemId, ps, ps.ItemsInWorld, "NumberInWorld");
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Stats.StatsCache.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.RequestedByPlayer == null ||
                d.RequestedByPlayer.ID.type == NetworkID.IDType.Server ||
                d.RequestedByPlayer.ID.type == NetworkID.IDType.Invalid)
                return;

            var ps = PlayerState.GetPlayerState(d.RequestedByPlayer);

            if (ps != null)
            {
                if (d.TypeNew != BuiltinBlocks.Air && ItemTypes.TryGetType(d.TypeNew, out var item))
                {
                    ushort itemId = GetParentId(d.TypeNew, item);

                    if (!ps.ItemsPlaced.ContainsKey(itemId))
                        ps.ItemsPlaced.Add(itemId, 0);

                    if (!ps.ItemsInWorld.ContainsKey(itemId))
                        ps.ItemsInWorld.Add(itemId, 0);

                    ps.ItemsPlaced[itemId]++;
                    ps.ItemsInWorld[itemId]++;
                }

                if (d.TypeNew == BuiltinBlocks.Air && d.TypeOld != BuiltinBlocks.Air && ItemTypes.TryGetType(d.TypeOld, out var itemOld))
                {
                    ushort itemId = GetParentId(d.TypeOld, itemOld);

                    if (!ps.ItemsRemoved.ContainsKey(itemId))
                        ps.ItemsRemoved.Add(itemId, 0);

                    if (!ps.ItemsInWorld.ContainsKey(itemId))
                        ps.ItemsInWorld.Add(itemId, 0);
                    else
                        ps.ItemsInWorld[itemId]--;

                    ps.ItemsRemoved[itemId]++;
                }
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

        private static void BuildPlaceableMenu(ConstructTooltipUIData data, ushort item, PlayerState ps, Dictionary<ushort, int> dict, string sentenceKey)
        {
            if (!dict.ContainsKey(item))
                dict.Add(item, 0);

            data.menu.Items.Add(new HorizontalSplit(new Label(new LabelData(GameLoader.NAMESPACE + ".inventory." + sentenceKey, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence)),
                                                    new Label(new LabelData(dict[item].ToString())), 30, 0.75f));
            
        }
    }
}
