using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public class Footman
    {

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Footman.PlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            var click = boxedData.item1;

            if (click.rayCastHit.rayHitType == Shared.RayHitType.Block && click.rayCastHit.voxelSideHit == VoxelSide.yPlus)
            {
                var inv = Inventory.GetInventory(player).GetInventory();
                
            }
        }
    }
}
