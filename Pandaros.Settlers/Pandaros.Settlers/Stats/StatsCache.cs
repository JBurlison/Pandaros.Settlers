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
                data.player.ID.type == NetworkID.IDType.Invalid)
                return;

            var ps = PlayerState.GetPlayerState(data.player);

            if (ps != null)
            {
                BuildMenu(data, ps, ps.ItemsPlaced, "NumberPlaced");
                BuildMenu(data, ps, ps.ItemsRemoved, "NumberRemoved");
                BuildMenu(data, ps, ps.ItemsInWorld, "NumberInWorld");
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
                if (d.TypeNew != BuiltinBlocks.Air)
                {
                    if (!ps.ItemsPlaced.ContainsKey(d.TypeNew))
                        ps.ItemsPlaced.Add(d.TypeNew, 0);

                    if (!ps.ItemsInWorld.ContainsKey(d.TypeNew))
                        ps.ItemsInWorld.Add(d.TypeNew, 0);

                    ps.ItemsPlaced[d.TypeNew]++;
                    ps.ItemsInWorld[d.TypeNew]++;
                }

                if (d.TypeNew == BuiltinBlocks.Air && d.TypeOld != BuiltinBlocks.Air)
                {
                    if (!ps.ItemsRemoved.ContainsKey(d.TypeOld))
                        ps.ItemsRemoved.Add(d.TypeOld, 0);

                    if (!ps.ItemsInWorld.ContainsKey(d.TypeOld))
                        ps.ItemsInWorld.Add(d.TypeOld, 0);
                    else
                        ps.ItemsInWorld[d.TypeOld]--;

                    ps.ItemsRemoved[d.TypeOld]++;
                }
            }
        }

        private static void BuildMenu(ConstructTooltipUIData data, PlayerState ps, Dictionary<ushort, int> dict, string sentenceKey)
        {
            if (!dict.ContainsKey(data.hoverItem))
                dict.Add(data.hoverItem, 0);

            data.menu.Items.Add(new HorizontalSplit(new Label(new LabelData(GameLoader.NAMESPACE + ".inventory." + sentenceKey, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence)),
                                                    new Label(new LabelData(dict[data.hoverItem].ToString())), 30, 0.75f));
            
        }
    }
}
