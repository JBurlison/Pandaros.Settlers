using Pandaros.Settlers.Models;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class DoorManager
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.DoorManager.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (boxedData.item1.clickType == PlayerClickedData.ClickType.Right &&
                boxedData.item1.rayCastHit.rayHitType == RayHitType.Block &&
               ItemTypes.TryGetType(boxedData.item1.typeHit, out ItemTypes.ItemType itemHit))
            {
                var baseType = itemHit.GetRootParentType();

                if (baseType.Categories != null &&
                baseType.Categories.Count != 0 &&
                baseType.Categories.Contains("door", StringComparer.CurrentCultureIgnoreCase))
                {
                    ushort replacement = ColonyBuiltIn.ItemTypes.AIR;

                    if (itemHit.Name == baseType.RotatedXMinus)
                        replacement = ItemId.GetItemId(baseType.RotatedZMinus);
                    else if (itemHit.Name == baseType.RotatedZMinus)
                        replacement = ItemId.GetItemId(baseType.RotatedXMinus);
                    else if (itemHit.Name == baseType.RotatedXPlus)
                        replacement = ItemId.GetItemId(baseType.RotatedZPlus);
                    else if (itemHit.Name == baseType.RotatedZPlus)
                        replacement = ItemId.GetItemId(baseType.RotatedXPlus);

                    if (replacement != ColonyBuiltIn.ItemTypes.AIR)
                        ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, replacement, new BlockChangeRequestOrigin(player));
                }
            }
        }
    }
}
