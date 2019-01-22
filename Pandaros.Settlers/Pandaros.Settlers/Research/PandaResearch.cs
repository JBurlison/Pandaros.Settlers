using BlockTypes;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Healing;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Items.Weapons;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Managers;
using Science;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Research
{
    public class ResearchCompleteEventArgs : EventArgs
    {
        public ResearchCompleteEventArgs(PandaResearch research, ColonyScienceState player)
        {
            Research = research;
            Manager  = player;
        }

        public PandaResearch Research { get; }

        public ColonyScienceState Manager { get; }
    }

    [ModLoader.ModManager]
    public class PandaResearch : BaseResearchable
    {
        public const string Settlement = "Settlement";
        public const string Machines = "Machines";
        public const string ReducedWaste = "ReducedWaste";
        public const string ArmorSmithing = "ArmorSmithing";
        public const string SwordSmithing = "SwordSmithing";
        public const string ColonistHealth = "ColonistHealth";
        public const string Knights = "Knights";
        public const string Apothecary = "Apothecaries";
        public const string ImprovedSling = "ImprovedSling";
        public const string ImprovedBow = "ImprovedBow";
        public const string ImprovedCrossbow = "ImprovedCrossbow";
        public const string ImprovedMatchlockgun = "ImprovedMatchlockgun";
        public const string ImprovedDurability = "ImprovedDurability";
        public const string ImprovedFuelCapacity = "ImprovedFuelCapacity";
        public const string IncreasedCapacity = "IncreasedCapacity";
        public const string AdvancedApothecary = "AdvancedApothecary";
        public const string Mana = "Mana";
        public const string Elementium = "Elementium";
        public const string BuildersWand = "BuildersWand";
        public const string BetterBuildersWand = "BetterBuildersWand";
        public const string Teleporters = "Teleportation";
        public const string MaxSettlers = "MaxSettlers";
        public const string MinSettlers = "MinSettlers";
        public const string SettlerChance = "SettlerChance";
        public const string NumberSkilledLaborer = "NumberSkilledLaborer";
        public const string SkilledLaborer = "SkilledLaborer";
        private static readonly Dictionary<string, float> _baseSpeed = new Dictionary<string, float>();

        public PandaResearch(Dictionary<ushort, int> requiredItems, 
                             int level, 
                             string name,
                             float baseValue,
                             List<string> dependancies = null, 
                             int baseIterationCount = 10,
                             bool addLevelToName = true)
        {
            BaseValue   = baseValue;
            Value       = baseValue * level;
            Level       = level;
            TmpValueKey = GetResearchKey(name);
            LevelKey    = GetLevelKey(name);

            Key = TmpValueKey + level;
            Icon = GameLoader.ICON_PATH + name + level + ".png";

            if (!addLevelToName)
                Icon = GameLoader.ICON_PATH + name + ".png";

            IterationCount = baseIterationCount + 2 * level;

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (level > 1)
                    for (var i = 1; i <= level; i++)
                        if (i % 2 == 0)
                            val += kvp.Value * 2;
                        else
                            val += kvp.Value;

                AddIterationRequirement(kvp.Key, val);
            }

            if (level != 1)
                AddDependency(TmpValueKey + (level - 1));

            if (dependancies != null)
                foreach (var dep in dependancies)
                    AddDependency(dep);

            PandaLogger.LogToFile($"PandaResearch Added: {name} Level {level}");
        }

        public string TmpValueKey { get; } = string.Empty;
        public int Level { get; } = 1;
        public float Value { get; }
        public float BaseValue { get; }
        public string LevelKey { get; } = string.Empty;

        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;

        public override void OnResearchComplete(ColonyScienceState manager, EResearchCompletionReason reason)
        {
            try
            {
                foreach (var p in manager.Colony.Owners)
                {
                    p.GetTempValues(true).Set(TmpValueKey, Value);
                    p.GetTempValues(true).Set(LevelKey, Level);
                }

                manager.Colony.TemporaryData.SetAs(TmpValueKey, Value);
                manager.Colony.TemporaryData.SetAs(LevelKey, Level);

                if (ResearchComplete != null)
                    ResearchComplete(this, new ResearchCompleteEventArgs(this, manager));
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex, $"Error OnResearchComplete for {TmpValueKey} Level: {Level}.");
            }
        }

        public static string GetLevelKey(string researchName)
        {
            return GetResearchKey(researchName) + "_Level";
        }

        public static string GetResearchKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }

        /// <summary>
        ///     This is reqquired to make sure jobs get registered before research
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, "BLOCKNPCS_WORKAROUND")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        static void Dummy()
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();
            PandaLogger.Log("Registering Panda Research.");

            AddMaxSettlers(researchDic);
            AddMinSettlers(researchDic);
            AddSettlerChance(researchDic);
            AddSkilledLaborer(researchDic);
            AddNumberSkilledLaborer(researchDic);
            AddReducedWaste(researchDic);
            AddArmorSmithing(researchDic);
            AddColonistHealth(researchDic);
            AddSwordSmithing(researchDic);
            AddKnightResearch(researchDic);
            AddImprovedSlings(researchDic);
            AddImprovedBows(researchDic);
            AddImprovedCrossbows(researchDic);
            AddImprovedMatchlockgun(researchDic);
            AddMachines(researchDic);
            AddImprovedDuarability(researchDic);
            AddImprovedFuelCapacity(researchDic);
            AddIncreasedCapacity(researchDic);
            AddApocthResearch(researchDic);
            AddAdvanceApocthResearch(researchDic);
            AddManaResearch(researchDic);
            AddElementiumResearch(researchDic);
            AddTeleporters(researchDic);
            AddBuildersWandResearch(researchDic);
            AddBetterBuildersWandResearch(researchDic);
            
            PandaLogger.Log("Panda Research Registering Complete!");
        }

        private static void AddReducedWaste(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 1);
            researchDic.Add(BuiltinBlocks.Berry, 2);
            researchDic.Add(BuiltinBlocks.Bread, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.SCIENCEBAGLIFE
            };

            var research = new PandaResearch(researchDic, 1, ReducedWaste, 0.001f, requirements);
            research.ResearchComplete += ReducedWaste_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ReducedWaste, 0.001f, requirements);
                research.ResearchComplete += ReducedWaste_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ReducedWaste_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.UpdateFoodUse(ColonyState.GetColonyState(e.Manager.Colony));
        }

        private static void AddArmorSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperParts, 2);
            researchDic.Add(BuiltinBlocks.CopperNails, 3);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BRONZEANVIL
            };

            RegisterArmorSmithng(researchDic, 1, requirements);

            researchDic.Remove(BuiltinBlocks.CopperParts);
            researchDic.Remove(BuiltinBlocks.CopperNails);
            researchDic.Add(BuiltinBlocks.BronzePlate, 3);
            researchDic.Add(BuiltinBlocks.BronzeAxe, 1);
            RegisterArmorSmithng(researchDic, 2);

            researchDic.Add(BuiltinBlocks.IronRivet, 3);
            researchDic.Add(BuiltinBlocks.IronSword, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 1);
            researchDic.Remove(BuiltinBlocks.BronzePlate);
            researchDic.Remove(BuiltinBlocks.BronzeAxe);
            RegisterArmorSmithng(researchDic, 3);

            researchDic.Add(BuiltinBlocks.SteelParts, 3);
            researchDic.Add(BuiltinBlocks.SteelIngot, 1);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);
            researchDic.Remove(BuiltinBlocks.IronRivet);
            RegisterArmorSmithng(researchDic, 4);
        }

        private static void RegisterArmorSmithng(Dictionary<ushort, int> researchDic, int level,
                                                 List<string>            requirements = null)
        {
            var research = new PandaResearch(researchDic, level, ArmorSmithing, 1f, requirements);
            research.ResearchComplete += Research_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Research_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            var armor = new List<IArmor>();

            switch (e.Research.Level)
            {
                case 1:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Copper")));
                    break;
                case 2:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Bronze")));
                    break;
                case 3:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Iron")));
                    break;
                case 4:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Steel")));
                    break;
            }

            foreach (var item in armor.Where(a => a.IsMagical == false))
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(item.name), true);
        }

        private static void AddSwordSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperParts, 2);
            researchDic.Add(BuiltinBlocks.CopperNails, 3);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BRONZEANVIL
            };

            RegisterSwordmithng(researchDic, 1, requirements);

            researchDic.Remove(BuiltinBlocks.CopperParts);
            researchDic.Remove(BuiltinBlocks.CopperNails);
            researchDic.Add(BuiltinBlocks.BronzePlate, 3);
            researchDic.Add(BuiltinBlocks.BronzeAxe, 1);
            RegisterSwordmithng(researchDic, 2);

            researchDic.Add(BuiltinBlocks.IronRivet, 3);
            researchDic.Add(BuiltinBlocks.IronSword, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 1);
            researchDic.Remove(BuiltinBlocks.BronzePlate);
            researchDic.Remove(BuiltinBlocks.BronzeAxe);
            RegisterSwordmithng(researchDic, 3);

            researchDic.Add(BuiltinBlocks.SteelParts, 3);
            researchDic.Add(BuiltinBlocks.SteelIngot, 1);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);
            researchDic.Remove(BuiltinBlocks.IronRivet);
            RegisterSwordmithng(researchDic, 4);
        }

        private static void RegisterSwordmithng(Dictionary<ushort, int> researchDic, int level,
                                                List<string>            requirements = null)
        {
            var research = new PandaResearch(researchDic, level, SwordSmithing, 1f, requirements);
            research.ResearchComplete += SwordResearch_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void SwordResearch_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            var sword = new List<IWeapon>();

            switch (e.Research.Level)
            {
                case 1:
                    sword.AddRange(WeaponFactory.WeaponLookup.Values.Where(a => a.name == "Copper Sword"));
                    break;
                case 2:
                    sword.AddRange(WeaponFactory.WeaponLookup.Values.Where(a => a.name == "Bronze Sword"));
                    break;
                case 3:
                    sword.AddRange(WeaponFactory.WeaponLookup.Values.Where(a => a.name == "Iron Sword"));
                    break;
                case 4:
                    sword.AddRange(WeaponFactory.WeaponLookup.Values.Where(a => a.name == "Steel Sword"));
                    break;
            }

            foreach (var item in sword)
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(item.name), true);
        }

        private static void AddColonistHealth(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 2);
            researchDic.Add(BuiltinBlocks.Linen, 5);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 10);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.SCIENCEBAGLIFE
            };

            var research = new PandaResearch(researchDic, 1, ColonistHealth, 10f, requirements);
            research.ResearchComplete += Research_ResearchComplete1;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ColonistHealth, 10f);
                research.ResearchComplete += Research_ResearchComplete1;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void Research_ResearchComplete1(object sender, ResearchCompleteEventArgs e)
        {
            var maxHp = e.Manager.Colony.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MAXCOLONISTHP", e.Manager.Colony.NPCHealthMax);
            e.Manager.Colony.NPCHealthMax = maxHp + e.Research.Level * e.Research.BaseValue;

            foreach (var follower in e.Manager.Colony.Followers)
                follower.health = e.Manager.Colony.NPCHealthMax;
        }

        private static void AddKnightResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperTools, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.Linen, 2);

            var requirements = new List<string>
            {
                GetResearchKey(SwordSmithing + "1"),
                ColonyBuiltIn.Research.SCIENCEBAGBASIC
            };

            var research = new PandaResearch(researchDic, 1, Knights, 1f, requirements);
            research.ResearchComplete += Knights_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Knights_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.ForEachOwner(o => PatrolTool.GivePlayerPatrolTool(o));
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(PatrolTool.PatrolFlag.name), true);
        }

        private static void AddApocthResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.HERBFARMING,
                ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                ColonyBuiltIn.Research.OLIVEFARMER
            };

            var research = new PandaResearch(researchDic, 1, Apothecary, 1f, requirements);
            research.ResearchComplete += Apocth_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Apocth_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ApothecaryRegister.JOB_RECIPE), true);
        }

        private static void AddAdvanceApocthResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 2);

            var requirements = new List<string>
            {
                GetResearchKey(Apothecary + "1")
            };

            var research = new PandaResearch(researchDic, 1, AdvancedApothecary, 1f, requirements);
            research.ResearchComplete += AdvanceApocth_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void AdvanceApocth_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Anitbiotic.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(TreatedBandage.Item.name), true);
        }

        private static void AddManaResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Alkanet, 10);
            researchDic.Add(BuiltinBlocks.Wolfsbane, 10);
            researchDic.Add(BuiltinBlocks.Hollyhock, 10);
            researchDic.Add(BuiltinBlocks.Gypsum, 10);
            researchDic.Add(BuiltinBlocks.Crystal, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Apothecary + "1")
            };

            var research = new PandaResearch(researchDic, 1, Mana, 1f, requirements, 50);
            research.ResearchComplete += Mana_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Mana_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Mana.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Aether.Item.name), true);
        }

        private static void AddElementiumResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 1);
            researchDic.Add(BuiltinBlocks.Copper, 20);
            researchDic.Add(BuiltinBlocks.IronOre, 20);
            researchDic.Add(BuiltinBlocks.Tin, 20);
            researchDic.Add(BuiltinBlocks.GoldOre, 20);
            researchDic.Add(BuiltinBlocks.GalenaSilver, 20);
            researchDic.Add(BuiltinBlocks.GalenaLead, 20);

            var requirements = new List<string>
            {
                GetResearchKey(Mana + "1")
            };

            var research = new PandaResearch(researchDic, 1, Elementium, 1f, requirements, 50);
            research.ResearchComplete += Elementium_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Elementium_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Elementium.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(EarthStone.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(FireStone.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(WaterStone.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(AirStone.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ElementalTurrets.AIRTURRET_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ElementalTurrets.FIRETURRET_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ElementalTurrets.EARTHTURRET_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ElementalTurrets.WATERTURRET_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(ElementalTurrets.VOIDTURRET_NAMESPACE), true);
        }

        private static void AddBuildersWandResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 2);
            researchDic.Add(BuiltinBlocks.SteelIngot, 10);
            researchDic.Add(BuiltinBlocks.GoldIngot, 10);
            researchDic.Add(BuiltinBlocks.SilverIngot, 10);
            researchDic.Add(Items.Elementium.Item.ItemIndex, 1);

            var requirements = new List<string>
            {
                GetResearchKey(Elementium + "1")
            };

            var research = new PandaResearch(researchDic, 1, BuildersWand, 1f, requirements, 50);
            research.ResearchComplete += BuildersWand_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void BuildersWand_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.BuildersWand.Item.name), true);
        }

        private static void AddBetterBuildersWandResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 2);
            researchDic.Add(BuiltinBlocks.SteelIngot, 10);
            researchDic.Add(BuiltinBlocks.GoldIngot, 10);
            researchDic.Add(BuiltinBlocks.SilverIngot, 10);
            researchDic.Add(BuiltinBlocks.Planks, 10);

            var requirements = new List<string>
            {
                GetResearchKey(BuildersWand + "1")
            };

            var research = new PandaResearch(researchDic, 1, BetterBuildersWand, 250f, requirements, 50);
            research.ResearchComplete += BetterBuildersWand_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research =
                    new PandaResearch(researchDic, i, BetterBuildersWand, 250f, requirements, 50);

                research.ResearchComplete += BetterBuildersWand_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void BetterBuildersWand_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            // TODO: make global
            //var ps = PlayerState.GetPlayerState(e.Manager.Player);
            //ps.BuildersWandMaxCharge =  (int) e.Research.Value;
            //ps.BuildersWandCharge    += ps.BuildersWandMaxCharge;
        }

        private static void AddTeleporters(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Items.Mana.Item.ItemIndex, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 10);
            researchDic.Add(BuiltinBlocks.StoneBricks, 20);
            researchDic.Add(BuiltinBlocks.Crystal, 20);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>
            {
                GetResearchKey(Mana + "1"),
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, Teleporters, 1f, requirements, 100, false);
            research.ResearchComplete += Teleporters_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Teleporters_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(TeleportPad.Item.name), true);
        }

        private static void AddImprovedSlings(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Sling, 1);
            researchDic.Add(BuiltinBlocks.SlingBullet, 5);

            for (var i = 1; i <= 5; i++)
            {
                var research = new PandaResearch(researchDic, i, ImprovedSling, .05f);
                research.ResearchComplete += ImprovedSlings_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedSlings_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            // TODO 
            //if (!_baseSpeed.ContainsKey(nameof(GuardSlingerJobDay)))
            //    _baseSpeed.Add(nameof(GuardSlingerJobDay), GuardSlingerJobDay.GetGuardSettings().cooldownShot);

            //if (!_baseSpeed.ContainsKey(nameof(GuardSlingerJobNight)))
            //    _baseSpeed.Add(nameof(GuardSlingerJobNight), GuardSlingerJobNight.GetGuardSettings().cooldownShot);

            //GuardSlingerJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardSlingerJobDay)] -
            //                                                 _baseSpeed[nameof(GuardSlingerJobDay)] * e.Research.Value;

            //GuardSlingerJobNight.CachedSettings.cooldownShot =
            //    _baseSpeed[nameof(GuardSlingerJobNight)] - _baseSpeed[nameof(GuardSlingerJobNight)] * e.Research.Value;

            //foreach (BlockJobManager<GuardJobInstance> w in ServerManager.BlockEntityCallbacks.AutoLoadedInstances.Where(t => t as BlockJobManager<GuardJobInstance> != null))
        }

        private static void AddImprovedBows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bow, 1);
            researchDic.Add(BuiltinBlocks.BronzeArrow, 5);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.ARCHERY
            };

            var research = new PandaResearch(researchDic, 1, ImprovedBow, .05f, requirements);
            research.ResearchComplete += ImprovedBows_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ImprovedBow, .05f);
                research.ResearchComplete += ImprovedBows_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedBows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            //if (!_baseSpeed.ContainsKey(nameof(GuardBowJobDay)))
            //    _baseSpeed.Add(nameof(GuardBowJobDay), GuardBowJobDay.GetGuardSettings().cooldownShot);

            //if (!_baseSpeed.ContainsKey(nameof(GuardBowJobNight)))
            //    _baseSpeed.Add(nameof(GuardBowJobNight), GuardBowJobNight.GetGuardSettings().cooldownShot);

            //GuardBowJobDay.CachedSettings.cooldownShot =
            //    _baseSpeed[nameof(GuardBowJobDay)] - _baseSpeed[nameof(GuardBowJobDay)] * e.Research.Value;

            //GuardBowJobNight.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardBowJobNight)] -
            //                                               _baseSpeed[nameof(GuardBowJobNight)] * e.Research.Value;
        }

        private static void AddImprovedCrossbows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Crossbow, 1);
            researchDic.Add(BuiltinBlocks.CrossbowBolt, 5);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.CROSSBOW
            };

            var research = new PandaResearch(researchDic, 1, ImprovedCrossbow, .05f, requirements);
            research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ImprovedCrossbow, .05f);
                research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedCrossbows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            //if (!_baseSpeed.ContainsKey(nameof(GuardCrossbowJobDay)))
            //    _baseSpeed.Add(nameof(GuardCrossbowJobDay), GuardCrossbowJobDay.GetGuardSettings().cooldownShot);

            //if (!_baseSpeed.ContainsKey(nameof(GuardCrossbowJobNight)))
            //    _baseSpeed.Add(nameof(GuardCrossbowJobNight), GuardCrossbowJobNight.GetGuardSettings().cooldownShot);

            //GuardCrossbowJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardCrossbowJobDay)] -
            //                                                  _baseSpeed[nameof(GuardCrossbowJobDay)] *
            //                                                  e.Research.Value;

            //GuardCrossbowJobNight.CachedSettings.cooldownShot =
            //    _baseSpeed[nameof(GuardCrossbowJobNight)] -
            //    _baseSpeed[nameof(GuardCrossbowJobNight)] * e.Research.Value;
        }

        private static void AddImprovedMatchlockgun(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.MatchlockGun, 1);
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.GunpowderPouch, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.MATCHLOCKGUN
            };

            var research = new PandaResearch(researchDic, 1, ImprovedMatchlockgun, .05f, requirements);
            research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ImprovedMatchlockgun, .05f);
                research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedMatchlockguns_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            //if (!_baseSpeed.ContainsKey(nameof(GuardMatchlockJobDay)))
            //    _baseSpeed.Add(nameof(GuardMatchlockJobDay), GuardMatchlockJobDay.GetGuardSettings().cooldownShot);

            //if (!_baseSpeed.ContainsKey(nameof(GuardMatchlockJobNight)))
            //    _baseSpeed.Add(nameof(GuardMatchlockJobNight), GuardMatchlockJobNight.GetGuardSettings().cooldownShot);

            //GuardMatchlockJobDay.CachedSettings.cooldownShot =
            //    _baseSpeed[nameof(GuardMatchlockJobDay)] - _baseSpeed[nameof(GuardMatchlockJobDay)] * e.Research.Value;

            //GuardMatchlockJobNight.CachedSettings.cooldownShot =
            //    _baseSpeed[nameof(GuardMatchlockJobNight)] -
            //    _baseSpeed[nameof(GuardMatchlockJobNight)] * e.Research.Value;
        }

        private static void AddMachines(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronWrought, 1);
            researchDic.Add(BuiltinBlocks.CopperTools, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.Linen, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BLOOMERY
            };

            var research = new PandaResearch(researchDic, 1, Machines, 1f, requirements, 20, false);
            research.ResearchComplete += Machiness_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);
        }

        private static void Machiness_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Miner.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(GateLever.Item.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(GateLever.GateItem.name), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Turret.BRONZEARROW_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Turret.STONE_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Turret.CROSSBOW_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Turret.MATCHLOCK_NAMESPACE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(AdvancedCrafterRegister.JOB_RECIPE), true);
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(MachinistDay.JOB_RECIPE), true);
        }

        private static void AddImprovedDuarability(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedDurability, .1f, requirements);
            research.ResearchComplete += ImprovedDuarability_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ImprovedDurability, .1f);
                research.ResearchComplete += ImprovedDuarability_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedDuarability_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.REPAIR, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddImprovedFuelCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedFuelCapacity, .1f, requirements);
            research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, ImprovedFuelCapacity, .1f);
                research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedFuelCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.REFUEL, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddIncreasedCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, IncreasedCapacity, .2f, requirements);
            research.ResearchComplete += IncreasedCapacity_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, IncreasedCapacity, .2f);
                research.ResearchComplete += IncreasedCapacity_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void IncreasedCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.RELOAD, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddMaxSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 1);
            researchDic.Add(BuiltinBlocks.PlasterBlock, 5);
            researchDic.Add(BuiltinBlocks.IronIngot, 5);
            researchDic.Add(BuiltinBlocks.Bed, 10);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>
            {
                GetResearchKey(SettlerChance) + "1"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MaxSettlers, 1f, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MaxSettlers, 1f));
        }

        private static void AddMinSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 1);
            researchDic.Add(BuiltinBlocks.Bricks, 5);
            researchDic.Add(BuiltinBlocks.CoatedPlanks, 5);
            researchDic.Add(BuiltinBlocks.Bed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>
            {
                GetResearchKey(MaxSettlers) + "3"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MinSettlers, 1f, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MinSettlers, 1f));
        }

        private static void AddSettlerChance(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 2);
            researchDic.Add(BuiltinBlocks.Torch, 5);
            researchDic.Add(BuiltinBlocks.StoneBricks, 10);
            researchDic.Add(BuiltinBlocks.Bed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BANNERRADIUS2
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SettlerChance, 0.1f, requirements));

            for (var i = 2; i <= 5; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SettlerChance, 0.1f));
        }

        private static void AddSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 10);
            researchDic.Add(BuiltinBlocks.CopperTools, 20);
            researchDic.Add(BuiltinBlocks.IronBlock, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 30);

            var requirements = new List<string>
            {
                GetResearchKey(SettlerChance) + "2",
                GetResearchKey(ReducedWaste) + "2"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SkilledLaborer, 0.02f, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SkilledLaborer, 0.02f));
        }

        private static void AddNumberSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 10);
            researchDic.Add(BuiltinBlocks.CopperParts, 20);
            researchDic.Add(BuiltinBlocks.CopperNails, 30);
            researchDic.Add(BuiltinBlocks.Tin, 10);
            researchDic.Add(BuiltinBlocks.IronRivet, 20);
            researchDic.Add(BuiltinBlocks.GoldCoin, 30);

            var requirements = new List<string>
            {
                GetResearchKey(SkilledLaborer) + "1"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, NumberSkilledLaborer, 1f,
                                                                  requirements));

            for (var i = 2; i <= 5; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, NumberSkilledLaborer, 1f));
        }
    }
}