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

        private string _tmpValueKey = string.Empty;
        private int _level = 1;
        private float _value = 0;

        public PandaResearch(Dictionary<ushort, int> requiredItems, int level, string name, float baseValue, List<string> dependancies = null, int baseIterationCount = 20, bool addLevelToName = true)
        {
            _value = baseValue * _level;
            _level = level;
            _tmpValueKey = GetResearchKey(name);

            key = _tmpValueKey + level;
            icon = GameLoader.ICON_FOLDER_PANDA + "\\" + name + level + ".png";

            if (!addLevelToName)
                icon = GameLoader.ICON_FOLDER_PANDA + "\\" + name + ".png";

            iterationCount = baseIterationCount + (2 * level);

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (level > 1)
                    for (int i = 1; i <= level; i++)
                        val += kvp.Value * level;

                AddIterationRequirement(kvp.Key, val);
            }

            if (level != 1)
                AddDependency(_tmpValueKey + (level - 1));

            if (dependancies != null)
                foreach (var dep in dependancies)
                    AddDependency(dep);
        }

        public override void OnResearchComplete(ScienceManagerPlayer manager)
        {
            manager.Player.GetTempValues(true).Set(_tmpValueKey, _value);

            if (_tmpValueKey.Contains(ArmorSmithing))
            {
                List<Items.Armor.ArmorMetadata> armor = new List<Items.Armor.ArmorMetadata>();

                switch (_level)
                {
                    case 1:
                        armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.Armor.MetalType.Copper));
                        break;
                    case 2:
                        armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.Armor.MetalType.Bronze));
                        break;
                    case 3:
                        armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.Armor.MetalType.Iron));
                        break;
                    case 4:
                        armor.AddRange(Items.Armor.ArmorLookup.Values.Where(a => a.Metal == Items.Armor.MetalType.Steel));
                        break;
                }

                foreach (var item in armor)
                    RecipeStorage.GetPlayerStorage(manager.Player).SetRecipeAvailability(item.ItemType.name, true, Items.Armor.JOB_METALSMITH);
            }
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
            //AddSkilledLaborer(researchDic);
            //AddNumberSkilledLaborer(researchDic);
        }

        private static void AddBanner(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 10);
            researchDic.Add(BuiltinBlocks.GoldCoin, 40);

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
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 40);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 40);
            researchDic.Add(BuiltinBlocks.CopperTools, 50);
            researchDic.Add(BuiltinBlocks.IronBlock, 50);
            researchDic.Add(BuiltinBlocks.GoldCoin, 30);

            var requirements = new List<string>()
            {
                GetResearchKey(SettlerChance) + "2",
                GetResearchKey(ReducedWaste) + "2",
                GetResearchKey(TimeBetween) + "1"
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SkilledLaborer, 0.1f, requirements));

            for (int i = 2; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SkilledLaborer, 0.1f));
        }

        private static void AddNumberSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 40);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 40);
            researchDic.Add(BuiltinBlocks.CopperParts, 50);
            researchDic.Add(BuiltinBlocks.CopperNails, 50);
            researchDic.Add(BuiltinBlocks.Tin, 50);
            researchDic.Add(BuiltinBlocks.IronRivet, 50);
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
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 5);
            researchDic.Add(BuiltinBlocks.CopperParts, 3);
            researchDic.Add(BuiltinBlocks.CopperNails, 5);

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, ArmorSmithing, 1f));

            researchDic.Remove(BuiltinBlocks.CopperParts);
            researchDic.Remove(BuiltinBlocks.CopperNails);
            researchDic.Add(BuiltinBlocks.BronzePlate, 3);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 5);
            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 2, ArmorSmithing, 1f));

            researchDic.Add(BuiltinBlocks.IronRivet, 3);
            researchDic.Add(BuiltinBlocks.IronSword, 1);
            researchDic.Remove(BuiltinBlocks.BronzePlate);
            researchDic.Remove(BuiltinBlocks.BronzeCoin);
            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 3, ArmorSmithing, 1f));

            researchDic.Add(BuiltinBlocks.SteelParts, 3);
            researchDic.Add(BuiltinBlocks.SteelIngot, 1);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);
            researchDic.Remove(BuiltinBlocks.IronRivet);
            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 4, ArmorSmithing, 1f));
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
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 6);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.Berry, 20);
            researchDic.Add(BuiltinBlocks.LinseedOil, 10);
            researchDic.Add(BuiltinBlocks.Bread, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 100);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, ReducedWaste, 0.5f));
        }

        private static void AddSettlerChance(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 6);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.Torch, 5);
            researchDic.Add(BuiltinBlocks.StoneBricks, 10);
            researchDic.Add(BuiltinBlocks.Bed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 100);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.BannerRadius2
            };

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SettlerChance, 0.1f, requirements));

            for (int i = 2; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SettlerChance, 0.1f));
        }

        private static void AddTimeBetween(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 6);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 10);
            researchDic.Add(BuiltinBlocks.CarpetBlue, 5);
            researchDic.Add(BuiltinBlocks.Bed, 10);
            researchDic.Add(BuiltinBlocks.CarpetRed, 5);
            researchDic.Add(BuiltinBlocks.GoldCoin, 100);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, TimeBetween, 1f));
        }
    }
}
