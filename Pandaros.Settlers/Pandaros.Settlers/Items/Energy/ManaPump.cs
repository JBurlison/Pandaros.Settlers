using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using Pipliz;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaPumpObjective : IRoamingJobObjective
    {
        private List<BlockSide> _applicableBlockSides = new List<BlockSide>()
        {
            BlockSide.Xp,
            BlockSide.Xn,
            BlockSide.Zn,
            BlockSide.Zp,
            BlockSide.Yp,
            BlockSide.Yn
        };

        public float WorkTime => 6;

        public ItemId ItemIndex => SettlersBuiltIn.ItemTypes.MANAPUMP;

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks => new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            {
                GameLoader.NAMESPACE + ".ManaMachineRepair",
                new ManaMachineRepairingAction()
            }
        };

        public string ObjectiveCategory => "ManaMachine";

        public string name => SettlersBuiltIn.ItemTypes.MANAPUMP;

        public void DoWork(Colony colony, RoamingJobState state)
        {
            var soundUpdate = state.TempValues.GetOrDefault("SoundUpdate", Time.SecondsSinceStartDouble);

            if (soundUpdate < Time.SecondsSinceStartDouble)
            {
                AudioManager.SendAudio(state.Position.Vector, "Pandaros.Settlers.ManaPump");
                state.TempValues.Set("SoundUpdate", Time.SecondsSinceStartDouble + 16);
            }
            
            if (state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair") > 0 &&
                state.NextTimeForWork < TimeCycle.TotalTime.Value.TotalMinutes &&
                RoamingJobManager.Objectives.TryGetValue(colony, out var catDic) &&
                catDic.TryGetValue(state.RoamingJobSettings.ObjectiveCategory, out var locDic))
            {
                List<RoamingJobState> manaTanks = new List<RoamingJobState>();
                HashSet<Vector3Int> exploredPos = new HashSet<Vector3Int>();

                foreach (var side in _applicableBlockSides)
                {
                    var offset = state.Position.GetBlockOffset(side);

                    if (locDic.TryGetValue(offset, out var roamingJobState) && roamingJobState.RoamingJobSettings.ItemIndex.Name == SettlersBuiltIn.ItemTypes.MANATANK)
                        manaTanks.Add(roamingJobState);
                }

                if (manaTanks.Count > 0)
                    foreach (var side in _applicableBlockSides)
                    {
                        var offset = state.Position.GetBlockOffset(side);
                        Queue<Vector3Int> explore = new Queue<Vector3Int>();
                        explore.Enqueue(offset);
                        var maxMana = RoamingJobState.GetActionsMaxEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", colony, state.RoamingJobSettings.ObjectiveCategory);

                        // walk mana pipes and find machines
                        while (explore.Count > 0)
                        {
                            offset = explore.Dequeue();

                            foreach (var exploreSide in _applicableBlockSides)
                            {
                                var exploreOffset = offset.GetBlockOffset(exploreSide);

                                if (!exploredPos.Contains(exploreOffset) &&
                                    World.TryGetTypeAt(exploreOffset, out ItemTypes.ItemType exploredItem))
                                {
                                    if (ItemCache.CSItems.TryGetValue(exploredItem.Name, out var csExploredItem) &&
                                        csExploredItem.ConnectedBlock != null &&
                                        csExploredItem.ConnectedBlock.BlockType == "ManaPipe")
                                    {
                                        explore.Enqueue(exploreOffset);
                                        exploredPos.Add(exploreOffset);

                                        if (locDic.TryGetValue(exploreOffset, out var exploredJobState))
                                        {
                                            var existingEnergyDeficit = maxMana - exploredJobState.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");

                                            foreach (var tank in manaTanks)
                                            {
                                                var energy = tank.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");

                                                if (energy >= existingEnergyDeficit)
                                                {
                                                    tank.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", existingEnergyDeficit);
                                                    exploredJobState.ResetActionToMaxLoad(GameLoader.NAMESPACE + ".ManaTankRefill");
                                                    break;
                                                }
                                                else
                                                {
                                                    tank.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", energy);
                                                    existingEnergyDeficit = existingEnergyDeficit - energy;
                                                }

                                                state.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair", .05f);
                                                energy = tank.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");

                                                if (energy > .90)
                                                    ServerManager.TryChangeBlock(tank.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankFull"));
                                                else if (energy > .75)
                                                    ServerManager.TryChangeBlock(tank.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankThreeQuarter"));
                                                else if (energy > .50)
                                                    ServerManager.TryChangeBlock(tank.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankHalf"));
                                                else if (energy > .25)
                                                    ServerManager.TryChangeBlock(tank.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".TankQuarter"));
                                                else
                                                    ServerManager.TryChangeBlock(tank.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".ManaTank"));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                state.NextTimeForWork = TimeCycle.TotalTime.Value.TotalMinutes + 10;
            }
        }
    }

    public class ManaPumpTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".ManaPump";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Manapump_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Manapump_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Manapump_Normal.png");
    }

    public class ManaPumpRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 6),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 10),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 3),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ManaPump")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".ManaPump";
    }

    public class ManaPumpGenerate : CSGenerateType
    {
        public override string generateType { get; set; } = "rotateBlock";
        public override string typeName { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override ICSType baseType { get; set; } = new CSType()
        {
            categories = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            },
            mesh = Path.Combine(GameLoader.MESH_PATH, "Manapump.obj"),
            icon = Path.Combine(GameLoader.ICON_PATH, "ManaPump.png"),
            onPlaceAudio = "Pandaros.Settlers.Metal",
            onRemoveAudio = "Pandaros.Settlers.MetalRemove",
            maxStackSize  = 300,
            destructionTime = 500,
            sideall = GameLoader.NAMESPACE + ".ManaPump",
            meshRotationEuler = new SerializableVector3()
            {
                y = 90
            },
            ConnectedBlock = new ConnectedBlock()
            {
                BlockType = "ManaPipe",
                Connections = new List<Models.BlockSide>()
                {
                    Models.BlockSide.Xn,
                    Models.BlockSide.Xp,
                    Models.BlockSide.Yn,
                    Models.BlockSide.Yp,
                    Models.BlockSide.Zn,
                    Models.BlockSide.Zp
                },
                AutoChange = false
            }
        };
    }
}
