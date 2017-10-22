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

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public class BannerManager
    {
        public static Vector3Int MAX_Vector3Int { get; private set; } = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".BannerManager.OnUpdate")]
        public static void OnUpdate()
        {
            //if (SettlerManager.RUNNING)
            //{
            //    var banners = BannerTracker.GetBanners();

            //    if (banners != null)
            //        for (int i = 0; i < banners.Count; i++)
            //        {
            //            var banner = banners.GetValueAtIndex(i);
            //            var ps = PlayerState.GetPlayerState(banner.Owner);
                        
            //        }
            //}
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
