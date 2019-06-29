using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Research;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class AddImprovedManaCapacity : PandaResearch
    {
        public override string name => GameLoader.NAMESPACE + ".ImprovedManaCapacity";

        public override string IconDirectory => GameLoader.ICON_PATH;

        public override float BaseValue => 0.1f;

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
            RoamingJobState.SetActionsMaxEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", e.Manager.Colony, "ManaPipe", RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }
    }

    public class ManaTankObjective : IRoamingJobObjective
    {
        public float WorkTime => 6;

        public ItemId ItemIndex => SettlersBuiltIn.ItemTypes.MANATANK;

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks => new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            {
                GameLoader.NAMESPACE + ".ManaMachineRepair",
                new ManaMachineRepairingAction()
            },
            {
                GameLoader.NAMESPACE + ".ManaTankRefill",
                new ManaTankRefill()
            }
        };

        public string ObjectiveCategory => "ManaMachine";

        public string name => SettlersBuiltIn.ItemTypes.MANATANK;

        public void DoWork(Colony colony, RoamingJobState state)
        {
            if (!state.ActionEnergy.ContainsKey(GameLoader.NAMESPACE + ".ManaTankRefill"))
                state.ActionEnergy.Add(GameLoader.NAMESPACE + ".ManaTankRefill", 0);
        }
    }

    public class ManaTankRefill : IRoamingJobObjectiveAction
    {
        public float TimeToPreformAction => 15;

        public string AudioKey => "Pandaros.Settlers.ManaPour";

        public ItemId ObjectiveLoadEmptyIcon => SettlersBuiltIn.ItemTypes.MANA;

        public string name => GameLoader.NAMESPACE + ".ManaTankRefill";

        public ItemId PreformAction(Colony colony, RoamingJobState state)
        {
            var retval = ItemId.GetItemId(GameLoader.NAMESPACE + ".Refuel");

            if (state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill") < .70f)
            {
                var requiredForFix = new List<InventoryItem>();
                var stockpile = colony.Stockpile;
                var energy = state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");
                var maxMana = RoamingJobState.GetActionsMaxEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", colony, state.RoamingJobSettings.ObjectiveCategory);

                var manaCost = (int)Math.Round((maxMana - energy) / .02, 0);

                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Id, manaCost));

                if (stockpile.TryRemove(requiredForFix))
                {
                    state.ResetActionToMaxLoad(GameLoader.NAMESPACE + ".ManaTankRefill");
                }
                else
                {
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = ItemId.GetItemId(item.Type);
                            break;
                        }
                }

                energy = state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");

                if (energy > .90)
                    ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankFull"));
                else if (energy > .75)
                    ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankThreeQuarter"));
                else if (energy > .50)
                    ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankHalf"));
                else if (energy > .25)
                    ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankQuarter"));
                else
                    ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".ManaTank"));
            }

            return retval;
        }
    }

    public class ManaTankRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 6),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 10),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 20),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ManaTank")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".ManaTank";
    }

    public class ManaTankTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".ManaTank";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Tank_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Tank_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Tank_Normal.png");
    }


    public class ManaTankBase : CSType
    {
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "ManaTank.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".ManaTank";
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, GameLoader.NAMESPACE + ".ManaTank")
        };
    }


    public class ManaTankGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankFull";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_Full.obj");
    }

    public class ManaTankEmptyGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaTank";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_empty.obj");
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
    }

    public class ManaTankQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_quarter.obj");
    }

    public class ManaTankThreeQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankThreeQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_3quarter.obj");
    }

    public class ManaTankHalfGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankHalf";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_half.obj");
    }
}
