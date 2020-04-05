using BlockEntities.Implementations;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.API;
using Pandaros.API.localization;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public class Permission
    {
        static LocalizationHelper _LocalizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Permission");
        static List<Players.Player> _warnedPlayers = new List<Players.Player>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.Permission.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (SettlersConfiguration.GetorDefault("AntigriefEnabled", true) && 
                ServerManager.BlockEntityTracker.BannerTracker.TryGetClosest(userData.Position, out BannerTracker.Banner existingBanner, ServerManager.ServerSettings.Colony.ExclusiveRadius))
            {
                if (userData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player && 
                    userData.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Steam &&
                    !PermissionsManager.HasPermission(userData.RequestOrigin.AsPlayer, new PermissionsManager.Permission(GameLoader.NAMESPACE + ".Permissions.Antigrief")) &&
                    !PermissionsManager.HasPermission(userData.RequestOrigin.AsPlayer, new PermissionsManager.Permission("god")) &&
                    !existingBanner.Colony.Owners.Contains(userData.RequestOrigin.AsPlayer))
                {
                    PandaChat.SendThrottle(userData.RequestOrigin.AsPlayer, _LocalizationHelper, _LocalizationHelper.LocalizeOrDefault("NotYourColony",userData.RequestOrigin.AsPlayer) + string.Join(", ", existingBanner.Colony.Owners.Select(o => o.Name)) , ChatColor.red);
                    userData.InventoryItemResults.Clear();
                    userData.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
                    userData.CallbackConsumedResult = EServerChangeBlockResult.CancelledByCallback;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".ColonyManager.Permission.OnHit")]
        public static void OnHit(NPC.NPCBase npc, ModLoader.OnHitData data)
        {
           if ((data.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
                data.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile) && !npc.Colony.Owners.Contains((Players.Player)data.HitSourceObject))
            {
                var p = (Players.Player)data.HitSourceObject;

                if (_warnedPlayers.Contains(p))
                {
                    ServerManager.Disconnect(p);
                }
                else
                {
                    NetworkMenu menu = new NetworkMenu();
                    menu.LocalStorage.SetAs("header", _LocalizationHelper.LocalizeOrDefault("warning", p));
                    menu.Width = 800;
                    menu.Height = 600;
                    menu.ForceClosePopups = true;
                    menu.Items.Add(new Label(new LabelData(_LocalizationHelper.LocalizeOrDefault("KillingColonists", p), UnityEngine.Color.black)));
                    NetworkMenuManager.SendServerPopup(p, menu);
                }

                data.HitDamage = 0;
                data.ResultDamage = 0;
            }
        }
    }
}
