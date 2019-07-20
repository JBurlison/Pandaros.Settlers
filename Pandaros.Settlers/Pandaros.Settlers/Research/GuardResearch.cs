using Pandaros.API;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Research;
using Pandaros.Settlers.ColonyManagement;
using System.Collections.Generic;


namespace Pandaros.Settlers.Research
{
    public class AddImprovedSlings : PandaResearch
    {
        public override string name => ColonyBuiltIn.NpcTypes.GUARDSLINGERDAY.Replace("day", "");

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 0.05f;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SLING.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SLINGBULLET.Id, 5)
                }
            }
        };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }
    }

    public class AddImprovedBows : PandaResearch
    {
        public override string name => ColonyBuiltIn.NpcTypes.GUARDBOWDAY.Replace("day", "");

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 0.05f;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BOW.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEARROW.Id, 5)
                }
            }
        };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.ARCHERY
                }
            }
        };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }
    }

    public class AddImprovedCrossbows : PandaResearch
    {
        public override string name => ColonyBuiltIn.NpcTypes.GUARDCROSSBOWDAY.Replace("day", "");

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 0.05f;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOW.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOWBOLT.Id, 5)
                }
            }
        };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.CROSSBOW
                }
            }
        };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }
    }

    public class AddImprovedMatchlockgun : PandaResearch
    {
        public override string name => ColonyBuiltIn.NpcTypes.GUARDMATCHLOCKDAY.Replace("day", "");

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 0.05f;

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.MATCHLOCKGUN.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.LEADBULLET.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GUNPOWDERPOUCH.Id, 2)
                }
            }
        };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.MATCHLOCKGUN
                }
            }
        };

        public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }
    }
}
