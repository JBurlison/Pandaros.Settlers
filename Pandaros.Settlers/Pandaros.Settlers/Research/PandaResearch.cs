using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz.APIProvider.Jobs;
using Pipliz.APIProvider.Science;
using Pipliz.BlockNPCs.Implementations;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class ResearchCompleteEventArgs : EventArgs
    {
        public PandaResearch Research { get; private set; }

        public ScienceManagerPlayer Manager { get; private set; }

        public ResearchCompleteEventArgs(PandaResearch research, ScienceManagerPlayer player)
        {
            Research = research;
            Manager = player;
        }
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
        public const string Herbalists = "Herbalists";
        public const string ImprovedSling = "ImprovedSling";
        public const string ImprovedBow = "ImprovedBow";
        public const string ImprovedCrossbow = "ImprovedCrossbow";
        public const string ImprovedMatchlockgun = "ImprovedMatchlockgun";
        public const string ImprovedDurability = "ImprovedDurability";
        public const string ImprovedFuelCapacity = "ImprovedFuelCapacity";
        public const string IncreasedCapacity = "IncreasedCapacity";

        public string TmpValueKey { get; private set; } = string.Empty;
        public int Level { get; private set; } = 1;
        public float Value { get; private set; } = 0;
        public float BaseValue { get; private set; } = 0;
        public string LevelKey { get; private set; } = string.Empty;

        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;
        static Dictionary<string, float> _baseSpeed = new Dictionary<string, float>();

        public PandaResearch(Dictionary<ushort, int> requiredItems, int level, string name, float baseValue, List<string> dependancies = null, int baseIterationCount = 10, bool addLevelToName = true)
        {
            BaseValue = baseValue;
            Value = baseValue * level;
            Level = level;
            TmpValueKey = GetResearchKey(name);
            LevelKey = GetLevelKey(name);

            key = TmpValueKey + level;
            icon = GameLoader.ICON_FOLDER_PANDA + "\\" + name + level + ".png";

            if (!addLevelToName)
                icon = GameLoader.ICON_FOLDER_PANDA + "\\" + name + ".png";

            iterationCount = baseIterationCount + (2 * level);

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (level > 1)
                    for (int i = 1; i <= level; i++)
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
        }

        public override void OnResearchComplete(ScienceManagerPlayer manager, EResearchCompletionReason reason)
        {
            try
            {
                manager.Player.GetTempValues(true).Set(TmpValueKey, Value);
                manager.Player.GetTempValues(true).Set(LevelKey, Level);

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();

            AddBanner(researchDic);
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
            AddHerbResearch(researchDic);
        }

        private static void AddBanner(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.BannerRadius3
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, Settlement, 1f, requirements, 20, false));

            for (int i = 2; i <= 20; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, Settlement, 1f, null, 20, false));
        }

        private static void AddArmorSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperParts, 2);
            researchDic.Add(BuiltinBlocks.CopperNails, 3);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 4);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.BronzeAnvil
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

        private static void RegisterArmorSmithng(Dictionary<ushort, int> researchDic, int level, List<string> requirements = null)
        {
            var research = new PandaResearch(researchDic, level, ArmorSmithing, 1f, requirements);
            research.ResearchComplete += Research_ResearchComplete;
            ScienceManager.RegisterResearchable(research);
        }

        private static void Research_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            List<Items.Armor.ArmorMetadata> armor = new List<Items.Armor.ArmorMetadata>();

            switch (e.Research.Level)
            {
                case 1:
                    armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.MetalType.Copper));
                    break;
                case 2:
                    armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.MetalType.Bronze));
                    break;
                case 3:
                    armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.MetalType.Iron));
                    break;
                case 4:
                    armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.MetalType.Steel));
                    break;
            }

            foreach (var item in armor)
                RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(item.ItemType.name, true, Items.Armor.JOB_METALSMITH);
        }

        private static void AddSwordSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperParts, 2);
            researchDic.Add(BuiltinBlocks.CopperNails, 3);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 2);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.BronzeAnvil
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

        private static void RegisterSwordmithng(Dictionary<ushort, int> researchDic, int level, List<string> requirements = null)
        {
            var research = new PandaResearch(researchDic, level, SwordSmithing, 1f, requirements);
            research.ResearchComplete += SwordResearch_ResearchComplete;
            ScienceManager.RegisterResearchable(research);
        }

        private static void SwordResearch_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            List<Items.WeaponMetadata> sword = new List<Items.WeaponMetadata>();

            switch (e.Research.Level)
            {
                case 1:
                    sword.AddRange(Items.ItemFactory.WeaponLookup.Values.Where(a => a.Metal == Items.MetalType.Copper));
                    break;
                case 2:
                    sword.AddRange(Items.ItemFactory.WeaponLookup.Values.Where(a => a.Metal == Items.MetalType.Bronze));
                    break;
                case 3:
                    sword.AddRange(Items.ItemFactory.WeaponLookup.Values.Where(a => a.Metal == Items.MetalType.Iron));
                    break;
                case 4:
                    sword.AddRange(Items.ItemFactory.WeaponLookup.Values.Where(a => a.Metal == Items.MetalType.Steel));
                    break;
            }

            foreach (var item in sword)
                RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(item.ItemType.name, true, Items.Armor.JOB_METALSMITH);
        }

        private static void AddColonistHealth(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 2);
            researchDic.Add(BuiltinBlocks.Linen, 5);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 10);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagLife
            };

            var research = new PandaResearch(researchDic, 1, ColonistHealth, 10f, requirements);
            research.ResearchComplete += Research_ResearchComplete1;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ColonistHealth, 10f);
                research.ResearchComplete += Research_ResearchComplete1;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void Research_ResearchComplete1(object sender, ResearchCompleteEventArgs e)
        {
            var maxHp = e.Manager.Player.GetTempValues(true).GetOrDefault<float>(GameLoader.NAMESPACE + ".MAXCOLONISTHP", NPC.NPCBase.MaxHealth);

            NPC.NPCBase.MaxHealth = maxHp + (e.Research.Level * e.Research.BaseValue);
        }

        private static void AddReducedWaste(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 1);
            researchDic.Add(BuiltinBlocks.Berry, 2);
            researchDic.Add(BuiltinBlocks.Bread, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagLife
            };
            
            var research = new PandaResearch(researchDic, 1, ReducedWaste, 0.05f, requirements);
            research.ResearchComplete += ReducedWaste_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ReducedWaste, 0.05f, requirements);
                research.ResearchComplete += ReducedWaste_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ReducedWaste_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            Managers.SettlerManager.UpdateFoodUse(e.Manager.Player);
        }

        private static void AddKnightResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperTools, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.Linen, 2);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 10);

            var requirements = new List<string>()
            {
                GetResearchKey(SwordSmithing + "1") 
            };

            var research = new PandaResearch(researchDic, 1, Knights, 1f, requirements);
            research.ResearchComplete += Knights_ResearchComplete;
            ScienceManager.RegisterResearchable(research);
        }

        private static void Knights_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            Jobs.PatrolTool.GivePlayerPatrolTool(e.Manager.Player);
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Jobs.PatrolTool.PatrolFlag.name, true, Items.ItemFactory.JOB_CRAFTER);
        }

        private static void AddHerbResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 2);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagAdvanced
            };

            var research = new PandaResearch(researchDic, 1, Herbalists, 1f, requirements);
            research.ResearchComplete += Herbs_ResearchComplete;
            ScienceManager.RegisterResearchable(research);
        }

        private static void Herbs_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Jobs.HerbalistRegister.JOB_RECIPE, true, Items.ItemFactory.JOB_CRAFTER);
            RecipePlayer.UnlockOptionalRecipe(e.Manager.Player, Jobs.HerbalistRegister.JOB_RECIPE);
        }

        

        private static void AddImprovedSlings(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Sling, 1);
            researchDic.Add(BuiltinBlocks.SlingBullet, 5);

            for (int i = 1; i <= 5; i++)
            {
                var research = new PandaResearch(researchDic, i, ImprovedSling, .05f);
                research.ResearchComplete += ImprovedSlings_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedSlings_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (!_baseSpeed.ContainsKey(nameof(GuardSlingerJobDay)))
                _baseSpeed.Add(nameof(GuardSlingerJobDay), GuardSlingerJobDay.GetGuardSettings().cooldownShot);

            if (!_baseSpeed.ContainsKey(nameof(GuardSlingerJobNight)))
                _baseSpeed.Add(nameof(GuardSlingerJobNight), GuardSlingerJobNight.GetGuardSettings().cooldownShot);

            GuardSlingerJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardSlingerJobDay)] - (_baseSpeed[nameof(GuardSlingerJobDay)] * e.Research.Value);
            GuardSlingerJobNight.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardSlingerJobNight)] - (_baseSpeed[nameof(GuardSlingerJobNight)] * e.Research.Value);
        }

        private static void AddImprovedBows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bow, 1);
            researchDic.Add(BuiltinBlocks.BronzeArrow, 5);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.TailorShop
            };

            var research = new PandaResearch(researchDic, 1, ImprovedBow, .05f, requirements);
            research.ResearchComplete += ImprovedBows_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedBow, .05f);
                research.ResearchComplete += ImprovedBows_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedBows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (!_baseSpeed.ContainsKey(nameof(GuardBowJobDay)))
                _baseSpeed.Add(nameof(GuardBowJobDay), GuardBowJobDay.GetGuardSettings().cooldownShot);

            if (!_baseSpeed.ContainsKey(nameof(GuardBowJobNight)))
                _baseSpeed.Add(nameof(GuardBowJobNight), GuardBowJobNight.GetGuardSettings().cooldownShot);

            GuardBowJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardBowJobDay)] - (_baseSpeed[nameof(GuardBowJobDay)] * e.Research.Value);
            GuardBowJobNight.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardBowJobNight)] - (_baseSpeed[nameof(GuardBowJobNight)] * e.Research.Value);
        }

        private static void AddImprovedCrossbows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Crossbow, 1);
            researchDic.Add(BuiltinBlocks.CrossbowBolt, 5);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.CrossBowBolt
            };

            var research = new PandaResearch(researchDic, 1, ImprovedCrossbow, .05f, requirements);
            research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedCrossbow, .05f);
                research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedCrossbows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (!_baseSpeed.ContainsKey(nameof(GuardCrossbowJobDay)))
                _baseSpeed.Add(nameof(GuardCrossbowJobDay), GuardCrossbowJobDay.GetGuardSettings().cooldownShot);

            if (!_baseSpeed.ContainsKey(nameof(GuardCrossbowJobNight)))
                _baseSpeed.Add(nameof(GuardCrossbowJobNight), GuardCrossbowJobNight.GetGuardSettings().cooldownShot);

            GuardCrossbowJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardCrossbowJobDay)] - (_baseSpeed[nameof(GuardCrossbowJobDay)] * e.Research.Value);
            GuardCrossbowJobNight.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardCrossbowJobNight)] - (_baseSpeed[nameof(GuardCrossbowJobNight)] * e.Research.Value);
        }

        private static void AddImprovedMatchlockgun(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.MatchlockGun, 1);
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.GunpowderPouch, 2);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.MatchlockGun
            };

            var research = new PandaResearch(researchDic, 1, ImprovedMatchlockgun, .05f, requirements);
            research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedMatchlockgun, .05f);
                research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedMatchlockguns_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (!_baseSpeed.ContainsKey(nameof(GuardMatchlockJobDay)))
                _baseSpeed.Add(nameof(GuardMatchlockJobDay), GuardMatchlockJobDay.GetGuardSettings().cooldownShot);

            if (!_baseSpeed.ContainsKey(nameof(GuardMatchlockJobNight)))
                _baseSpeed.Add(nameof(GuardMatchlockJobNight), GuardMatchlockJobNight.GetGuardSettings().cooldownShot);

            GuardMatchlockJobDay.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardMatchlockJobDay)] - (_baseSpeed[nameof(GuardMatchlockJobDay)] * e.Research.Value);
            GuardMatchlockJobNight.CachedSettings.cooldownShot = _baseSpeed[nameof(GuardMatchlockJobNight)] - (_baseSpeed[nameof(GuardMatchlockJobNight)] * e.Research.Value);
        }

        private static void AddMachines(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronWrought, 1);
            researchDic.Add(BuiltinBlocks.CopperTools, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.Linen, 2);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.Bloomery
            };

            var research = new PandaResearch(researchDic, 1, Machines, 1f, requirements, 20, false);
            research.ResearchComplete += Machiness_ResearchComplete;
            ScienceManager.RegisterResearchable(research);
        }

        private static void Machiness_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Items.Machines.Miner.Item.name, true, Jobs.AdvancedCrafterRegister.JOB_NAME);
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Items.Machines.GateLever.Item.name, true, Jobs.AdvancedCrafterRegister.JOB_NAME);
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Items.Machines.GateLever.GateItem.name, true, Jobs.AdvancedCrafterRegister.JOB_NAME);
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Jobs.AdvancedCrafterRegister.JOB_RECIPE, true, Items.ItemFactory.JOB_CRAFTER);
            RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(Jobs.MachinistRegister.JOB_RECIPE, true, Items.ItemFactory.JOB_CRAFTER);
            RecipePlayer.UnlockOptionalRecipe(e.Manager.Player, Jobs.MachinistRegister.JOB_RECIPE);
            RecipePlayer.UnlockOptionalRecipe(e.Manager.Player, Jobs.AdvancedCrafterRegister.JOB_RECIPE);

            foreach (var item in Items.Machines.Turret.TurretSettings)
                RecipeStorage.GetPlayerStorage(e.Manager.Player).SetRecipeAvailability(item.Value.TurretItem.name, true, Jobs.AdvancedCrafterRegister.JOB_NAME);
        }

        private static void AddImprovedDuarability(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>()
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedDurability, .1f, requirements);
            research.ResearchComplete += ImprovedDuarability_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedDurability, .1f);
                research.ResearchComplete += ImprovedDuarability_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedDuarability_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            Items.Machines.MachineState.MAX_DURABILITY[e.Manager.Player] = Items.Machines.MachineState.DEFAULT_MAX_DURABILITY + (Items.Machines.MachineState.DEFAULT_MAX_DURABILITY * e.Research.Value);
        }

        private static void AddImprovedFuelCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>()
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedFuelCapacity, .1f, requirements);
            research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedFuelCapacity, .1f);
                research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void ImprovedFuelCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            Items.Machines.MachineState.MAX_FUEL[e.Manager.Player] = Items.Machines.MachineState.DEFAULT_MAX_FUEL + (Items.Machines.MachineState.DEFAULT_MAX_FUEL * e.Research.Value);
        }

        private static void AddIncreasedCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronBlock, 1);
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.SteelIngot, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            var requirements = new List<string>()
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, IncreasedCapacity, .2f, requirements);
            research.ResearchComplete += IncreasedCapacity_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, IncreasedCapacity, .2f);
                research.ResearchComplete += IncreasedCapacity_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void IncreasedCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            Items.Machines.MachineState.MAX_LOAD[e.Manager.Player] = Items.Machines.MachineState.DEFAULT_MAX_LOAD + (Items.Machines.MachineState.DEFAULT_MAX_LOAD * e.Research.Value);
        }
    }
}
