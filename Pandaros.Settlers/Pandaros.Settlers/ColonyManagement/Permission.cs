using BlockEntities.Implementations;
using Pandaros.API;
using Pandaros.API.localization;
using System.Linq;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public class Permission
    {
        static LocalizationHelper _LocalizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Permission");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.Permission.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (SettlersConfiguration.GetorDefault("AntigriefEnabled", true) && 
                ServerManager.BlockEntityTracker.BannerTracker.TryGetClosest(userData.Position, out BannerTracker.Banner existingBanner, ServerManager.ServerSettings.Colony.ExclusiveRadius))
            {
                if (userData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player && 
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
    }
}
