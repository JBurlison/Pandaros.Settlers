using BlockEntities.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public class Permission
    {
        static localization.LocalizationHelper _LocalizationHelper = new localization.LocalizationHelper("Permission");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.Permission.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (ServerManager.BlockEntityTracker.BannerTracker.TryGetClosest(userData.Position, out BannerTracker.Banner existingBanner, ServerManager.ServerSettings.Colony.ExclusiveRadius * 2))
            {
                if (userData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player && !existingBanner.Colony.Owners.Contains(userData.RequestOrigin.AsPlayer))
                {
                    PandaChat.SendThrottle(userData.RequestOrigin.AsPlayer, _LocalizationHelper.LocalizeOrDefault("NotYourColony",userData.RequestOrigin.AsPlayer) + string.Join(", ", existingBanner.Colony.Owners.Select(o => o.Name)) , ChatColor.red);
                    userData.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
                    userData.CallbackConsumedResult = EServerChangeBlockResult.CancelledByCallback;
                }
            }
        }
    }
}
