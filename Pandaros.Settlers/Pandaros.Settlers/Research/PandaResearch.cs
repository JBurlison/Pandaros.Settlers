using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz.APIProvider.Science;
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
        public const string MaxSettlers = "MaxSettlers";
        public const string MinSettlers = "MinSettlers";
        public const string ReducedWaste = "ReducedWaste";
        public const string SettlerChance = "SettlerChance";
        public const string TimeBetween = "TimeBetween";
        public const string NumberSkilledLaborer = "NumberSkilledLaborer";
        public const string SkilledLaborer = "SkilledLaborer";
        public const string ArmorSmithing = "ArmorSmithing";
        public const string SwordSmithing = "SwordSmithing";
        public const string ColonistHealth = "ColonistHealth";

        public string TmpValueKey { get; private set; } = string.Empty;
        public int Level { get; private set; } = 1;
        public float Value { get; private set; } = 0;
        public float BaseValue { get; private set; } = 0;
        public string LevelKey { get; private set; } = string.Empty;

        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;

        public PandaResearch(Dictionary<ushort, int> requiredItems, int level, string name, float baseValue, List<string> dependancies = null, int baseIterationCount = 20, bool addLevelToName = true)
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

        public override void OnResearchComplete(ScienceManagerPlayer manager)
        {
            manager.Player.GetTempValues(true).Set(TmpValueKey, Value);
            manager.Player.GetTempValues(true).Set(LevelKey, Level);

            if (ResearchComplete != null)
                ResearchComplete(this, new ResearchCompleteEventArgs(this, manager));
        }

        public static string GetLevelKey(string researchName)
        {
            return GetResearchKey(researchName) + "_Level";
        }

        public static string GetResearchKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();

            AddMaxSettlers(researchDic);
            AddMinSettlers(researchDic);
            AddReducedWaste(researchDic);
            AddSettlerChance(researchDic);
            AddTimeBetween(researchDic);
            AddBanner(researchDic);
            AddArmorSmithing(researchDic);
            AddColonistHealth(researchDic);
            AddSwordSmithing(researchDic);
            //AddSkilledLaborer(researchDic);
            //AddNumberSkilledLaborer(researchDic);
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

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, Settlement, 1f, requirements, 30, false));

            for (int i = 2; i <= 20; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, Settlement, 1f, null, 30, false));
        }

        private static void AddSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 10);
            researchDic.Add(BuiltinBlocks.CopperTools, 20);
            researchDic.Add(BuiltinBlocks.IronBlock, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 30);

            var requirements = new List<string>()
            {
                GetResearchKey(SettlerChance) + "2",
                GetResearchKey(ReducedWaste) + "2",
                GetResearchKey(TimeBetween) + "1"
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SkilledLaborer, 0.02f, requirements));

            for (int i = 2; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SkilledLaborer, 0.02f));
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

            var requirements = new List<string>()
            {
                GetResearchKey(SkilledLaborer) + "1"
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, NumberSkilledLaborer, 1f, requirements));

            for (int i = 2; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, NumberSkilledLaborer, 1f));
        }

        private static void AddArmorSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CopperParts, 3);
            researchDic.Add(BuiltinBlocks.CopperNails, 5);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 5);
            RegisterArmorSmithng(researchDic, 1);

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

        private static void RegisterArmorSmithng(Dictionary<ushort, int> researchDic, int level)
        {
            var research = new PandaResearch(researchDic, level, ArmorSmithing, 1f);
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
            researchDic.Add(BuiltinBlocks.CopperParts, 3);
            researchDic.Add(BuiltinBlocks.CopperNails, 5);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 5);
            RegisterSwordmithng(researchDic, 1);

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

        private static void RegisterSwordmithng(Dictionary<ushort, int> researchDic, int level)
        {
            var research = new PandaResearch(researchDic, level, SwordSmithing, 1f);
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

        private static void AddMaxSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.PlasterBlock, 5);
            researchDic.Add(BuiltinBlocks.IronIngot, 5);
            researchDic.Add(BuiltinBlocks.Bed, 10);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                GetResearchKey(SettlerChance) + "1"
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MaxSettlers, 1f, requirements));

            for (int i = 2; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MaxSettlers, 1f));
        }

        private static void AddMinSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.Bricks, 5);
            researchDic.Add(BuiltinBlocks.CoatedPlanks, 5);
            researchDic.Add(BuiltinBlocks.Clothing, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                GetResearchKey(MaxSettlers) + "3"
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MinSettlers, 1f, requirements));

            for (int i = 2; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MinSettlers, 1f));
        }

        private static void AddReducedWaste(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 3);
            researchDic.Add(BuiltinBlocks.Berry, 2);
            researchDic.Add(BuiltinBlocks.Bread, 2);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, ReducedWaste, 0.05f));
        }

        private static void AddSettlerChance(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 6);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.Torch, 5);
            researchDic.Add(BuiltinBlocks.StoneBricks, 10);
            researchDic.Add(BuiltinBlocks.Bed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.BannerRadius2
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SettlerChance, 0.1f, requirements));

            for (int i = 2; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SettlerChance, 0.1f));
        }

        private static void AddColonistHealth(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 4);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.Linen, 5);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 10);

            for (int i = 1; i <= 5; i++)
            {
                var research = new PandaResearch(researchDic, i, ColonistHealth, 10f);
                research.ResearchComplete += Research_ResearchComplete1;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void Research_ResearchComplete1(object sender, ResearchCompleteEventArgs e)
        {
            var maxHp = e.Manager.Player.GetTempValues(true).GetOrDefault<float>(GameLoader.NAMESPACE + ".MAXCOLONISTHP", NPC.NPCBase.MaxHealth);

            NPC.NPCBase.MaxHealth = maxHp + (e.Research.Level * e.Research.BaseValue);
        }

        private static void AddTimeBetween(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 6);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.CarpetBlue, 5);
            researchDic.Add(BuiltinBlocks.Bed, 10);
            researchDic.Add(BuiltinBlocks.CarpetRed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, TimeBetween, 1f));
        }
    }
}
