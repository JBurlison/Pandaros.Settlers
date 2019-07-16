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
        public float WatchArea => 21;
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

                                                state.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair", .001f);
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

    public class ManaPumpGenerate : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Manapump.obj");
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "ManaPump.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override string rotatablexn { get; set; } = GameLoader.NAMESPACE + ".ManaPumpXn";
        public override string rotatablexp { get; set; } = GameLoader.NAMESPACE + ".ManaPumpXp";
        public override string rotatablezn { get; set; } = GameLoader.NAMESPACE + ".ManaPumpZn";
        public override string rotatablezp { get; set; } = GameLoader.NAMESPACE + ".ManaPumpZp";
        public override bool? isRotatable { get; set; } = true;
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
                {
                    BlockSide.Xn,
                    BlockSide.Xp,
                    BlockSide.Zn,
                    BlockSide.Zp,
                    BlockSide.Yn,
                    BlockSide.Yp
                }
        };
    }

    public class ManaPumpGenerateXp : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPumpXp";
        public override string parentType { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override SerializableVector3 meshRotationEuler { get; set; } = new SerializableVector3()
        {
            y = 90
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
                {
                    BlockSide.Xn,
                    BlockSide.Xp,
                    BlockSide.Zn,
                    BlockSide.Zp,
                    BlockSide.Yn,
                    BlockSide.Yp
                }
        };
    }

    public class ManaPumpGenerateZn : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPumpZn";
        public override string parentType { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override SerializableVector3 meshRotationEuler { get; set; } = new SerializableVector3()
        {
            y = 180
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
                {
                    BlockSide.Xn,
                    BlockSide.Xp,
                    BlockSide.Zn,
                    BlockSide.Zp,
                    BlockSide.Yn,
                    BlockSide.Yp
                }
        };
    }

    public class ManaPumpGenerateXn : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPumpXn";
        public override string parentType { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override SerializableVector3 meshRotationEuler { get; set; } = new SerializableVector3()
        {
            y = 270
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
                {
                    BlockSide.Xn,
                    BlockSide.Xp,
                    BlockSide.Zn,
                    BlockSide.Zp,
                    BlockSide.Yn,
                    BlockSide.Yp
                }
        };
    }

    public class ManaPumpGenerateZp : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPumpZp";
        public override string parentType { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override SerializableVector3 meshRotationEuler { get; set; } = new SerializableVector3()
        {
            y = 0
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
                {
                    BlockSide.Xn,
                    BlockSide.Xp,
                    BlockSide.Zn,
                    BlockSide.Zp,
                    BlockSide.Yn,
                    BlockSide.Yp
                }
        };
    }
}
