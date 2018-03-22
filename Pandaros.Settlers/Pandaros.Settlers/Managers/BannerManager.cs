using BlockTypes.Builtin;
using ChatCommands;
using Pandaros.Settlers.Entities;
using Pipliz;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPC;
using Pipliz.Collections;
using UnityEngine;
using Pandaros.Settlers.Research;
using Pandaros.Settlers.AI;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public class BannerManager
    {
        public static Vector3Int MAX_Vector3Int { get; private set; } = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        private static Dictionary<Players.Player, int> _bannerCounts = new Dictionary<Players.Player, int>();
        private static DateTime _nextBannerTime = DateTime.MinValue;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".BannerManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && DateTime.Now > _nextBannerTime)
            {
                EvaluateBanners();
                _nextBannerTime = DateTime.Now + TimeSpan.FromSeconds(10);
            }
        }

        public static void EvaluateBanners()
        {
            if (!GameLoader.WorldLoaded)
                return;

            _bannerCounts.Clear();

            var banners = BannerTracker.GetCount();

            if (banners > 0)
                for (int i = 0; i < banners; i++)
                {
                    if (BannerTracker.TryGetAtIndex(i, out var banner))
                    {
                        if (!_bannerCounts.ContainsKey(banner.Owner))
                            _bannerCounts.Add(banner.Owner, 1);
                        else
                            _bannerCounts[banner.Owner]++;
                    }
                }
            
            foreach (var p in _bannerCounts)
            {
                var ps = PlayerState.GetPlayerState(p.Key);

                if (ps == null)
                    continue;

                var numberOfBanners = p.Key.GetTempValues(true).GetOrDefault(PandaResearch.GetLevelKey(PandaResearch.Settlement), 0) + 1;
                var inv = Inventory.GetInventory(p.Key);
                var sockBanner = Stockpile.GetStockPile(p.Key).AmountContained(BuiltinBlocks.Banner);

                var inventoryBanners = 0;

                if (inv != null)
                {
                    foreach (var item in inv.Items)
                        if (item.Type == BuiltinBlocks.Banner)
                            inventoryBanners = item.Amount;
                }

                var totalBanners = p.Value + sockBanner + inventoryBanners;

#if Debug
                PandaLogger.Log($"Number of research banners: {numberOfBanners}");
                PandaLogger.Log($"Number of banners: {p.Value}");
                PandaLogger.Log($"Number of stockpile banners: {sockBanner}");
                PandaLogger.Log($"Number of Inventory banners: {inventoryBanners}");
                PandaLogger.Log($"Total banners: {totalBanners}");
                PandaLogger.Log($"Add Banner: {totalBanners < numberOfBanners}");
#endif

                if (totalBanners < numberOfBanners)
                {
                    if (!Inventory.GetInventory(p.Key).TryAdd(BuiltinBlocks.Banner))
                    {
                        Stockpile.GetStockPile(p.Key).Add(BuiltinBlocks.Banner);
                    }
                }
            }
        }

    }
}
