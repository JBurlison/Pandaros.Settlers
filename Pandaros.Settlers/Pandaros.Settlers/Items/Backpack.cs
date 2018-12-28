using BlockTypes;
using Pandaros.Settlers.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items
{
    public class SchematicToolResearch : IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
        {
            { BuiltinBlocks.ScienceBagBasic, 1 },
        };

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public List<string> Dependancies => new List<string>()
            {
                ColonyBuiltIn.Research.ScienceBagBasic
            };

        public int BaseIterationCount => 50;
        public bool AddLevelToName => true;
        public string Name => "Backpack";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (ItemTypes.IndexLookup.IndexLookupTable.TryGetItem(Backpack.NAME, out var item) &&
                !e.Manager.Colony.Stockpile.Contains(item.ItemIndex))
                e.Manager.Colony.Stockpile.Add(item.ItemIndex);
        }
    }

    public class Backpack : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".Backpack";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Backpack.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };

        public override StaticItem StaticItemSettings => new StaticItem()
        {
            Name = NAME,
            RequiredScience = NAME + 1
        };
    }

    [ModLoader.ModManager]
    public class BackpackCallbacks
    {
    }
}
