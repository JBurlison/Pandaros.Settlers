using BlockTypes.Builtin;
using ChatCommands;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Pipliz.BlockNPCs.Implementations;
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
        private static double _nextBannerTime = TimeCycle.TotalTime + 1;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".BannerManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (SettlerManager.RUNNING && TimeCycle.TotalTime > _nextBannerTime)
            {
                EvaluateBanners();
                _nextBannerTime = TimeCycle.TotalTime + 1;
            }
        }

        public static void EvaluateBanners()
        {
            _bannerCounts.Clear();
            var banners = BannerTracker.GetBanners();

            if (banners != null)
                for (int i = 0; i < banners.Count; i++)
                {
                    var banner = banners.GetValueAtIndex(i);

                    if (banner != null)
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
                var numberOfBanners = p.Key.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.Settlement), 0f) + 1f;
                var inv = Inventory.GetInventory(p.Key);
               
                var inventoryBanners = 0;

                if (inv != null)
                {
                    var items = inv.GetInventory();

                    if (items != null)
                    foreach (var item in items)
                        if (item.Type == BuiltinBlocks.Banner)
                            inventoryBanners = item.Amount;
                }

                PandaLogger.Log($"Number of research banners: {numberOfBanners}");
                PandaLogger.Log($"Number of banners: {p.Value}");
                PandaLogger.Log($"Number of stockpile banners: {Stockpile.GetStockPile(p.Key).AmountContained(BuiltinBlocks.Banner)}");
                PandaLogger.Log($"Number of Inventory banners: {inventoryBanners}");

                if (p.Value + Stockpile.GetStockPile(p.Key).AmountContained(BuiltinBlocks.Banner) + inventoryBanners < numberOfBanners)
                    Stockpile.GetStockPile(p.Key).Add(BuiltinBlocks.Banner);
            }
        }

        public static Banner GetClosestBanner(Players.Player p, Vector3Int currentPos)
        {
            Banner closest = null;
            float distance = float.MaxValue;

            var banners = BannerTracker.GetBanners();

            for (int i = 0; i < banners.Count; i++)
            {
                var banner = banners.GetValueAtIndex(i);

                if (banner.Owner == p)
                {
                    var bannerDistance = Vector3.Distance(currentPos.Vector, banner.KeyLocation.Vector);

                    if (closest == null)
                    {
                        closest = banner;
                        distance = bannerDistance;
                    }
                    else
                    {
                        if (bannerDistance < distance)
                        {
                            closest = banner;
                            distance = bannerDistance;
                        }
                    }
                }
            }
            
            return closest;
        }
    }
}
