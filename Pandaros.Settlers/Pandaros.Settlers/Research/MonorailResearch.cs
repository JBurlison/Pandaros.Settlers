using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Transportation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Research
{
    public class MonorailResearch : PandaResearch
    {
        public override string name => GameLoader.NAMESPACE + ".ImprovedMonorailTrainDistance";

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => .5f;

        public override int BaseIterationCount => 50;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Id, 3)
                    }
                }
            };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.ARTIFICER1
                    }
                }
            };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (Train.TrainTransports.TryGetValue("Monorail", out var trainTransports))
                foreach (var t in trainTransports)
                {
                    t.ManaCostPerBlock = t.TrainType.TrainConfiguration.ManaCostPerBlock / (e.Research.Value + 1);
                }
        }
    }

    public class MonorailSpeedResearch : PandaResearch
    {
        public override string name => GameLoader.NAMESPACE + ".ImprovedMonorailTrainSpeed";

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 65f;

        public override int BaseIterationCount => 50;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Id, 3)
                    }
                }
            };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.ARTIFICER1
                    }
                }
            };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (Train.TrainTransports.TryGetValue("Monorail", out var trainTransports))
                foreach (var t in trainTransports)
                {
                    t.Delay = t.TrainType.TrainConfiguration.MoveTimePerBlockMs - (int)e.Research.Value;
                }
        }
    }
}
