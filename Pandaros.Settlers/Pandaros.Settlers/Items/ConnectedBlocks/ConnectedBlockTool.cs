using Pandaros.Settlers.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items.ConnectedBlocks
{
    public class ConnectedBlockTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".ConnectedBlockTool";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "ConnectedBlockTool.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameLoader.NAMESPACE + ".ConnectedBlockTool" };
    }

    [ModLoader.ModManager]
    public class ConnectedBlockToolRegister
    {

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.ConnectedBlocks.ConnectedBlockTool.ConnectedBlockInfo")]
        public static void ConnectedBlockInfo(Players.Player player, PlayerClickedData playerClickData)
        {
            //Only launch on RIGHT click
            if (player == null || playerClickData.ClickType != PlayerClickedData.EClickType.Right || player.ActiveColony == null)
                return;

            if (playerClickData.HitType == PlayerClickedData.EHitType.Block &&
                ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".ConnectedBlockTool", out var toolItem) &&
                playerClickData.TypeSelected == toolItem &&
                ConnectedBlockSystem.BlockLookup.TryGetValue(ItemId.GetItemId(playerClickData.GetVoxelHit().TypeHit), out var cSType))
            {
                PandaChat.Send(player, "Side Hit: " + playerClickData.GetVoxelHit().SideHit);
                PandaChat.Send(player, "Origin Sides: " + string.Join(", ", cSType.ConnectedBlock.Origin.Select(c => c.ToString()).ToArray()));
                PandaChat.Send(player, "BlockRotationDegrees: " + cSType.ConnectedBlock.BlockRotationDegrees.ToString());
                PandaChat.Send(player, "RotationAxis: " + cSType.ConnectedBlock.RotationAxis.ToString());
            }
        }
    }
}
