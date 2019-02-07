using Pandaros.Settlers.Items;
using Pipliz;
using Shared;
using System.Collections.Generic;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Help
{
    public enum MenuItemType
    {
        Button,
        Image,
        Text
    }

    public class HelpMenuActivator : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".HelpMenu";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Help.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
        public override StaticItem StaticItemSettings => new StaticItem() { Name = NAME };
        public override OpenMenuSettings OpensMenuSettings => new OpenMenuSettings()
        {
            ActivateClickType = PlayerClickedData.ClickType.Right,
            ItemName = NAME,
            UIUrl = "Wiki.MainMenu"
        };
    }
}
