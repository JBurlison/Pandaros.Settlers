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
        public static readonly string Settlement = "Settlement";
        public static readonly string MaxSettlers = "MaxSettlers";
        public static readonly string MinSettlers = "MinSettlers";
        public static readonly string ReducedWaste = "ReducedWaste";
        public static readonly string SettlerChance = "SettlerChance";
        public static readonly string TimeBetween = "TimeBetween";
        public static readonly string NumberSkilledLaborer = "NumberSkilledLaborer";
        public static readonly string SkilledLaborer = "SkilledLaborer";

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
            var state = PlayerState.GetPlayerState(manager.Player);
            manager.Player.GetTempValues(true).Set(_tmpValueKey, _value);
            PandaLogger.Log($"Research Complete: {_tmpValueKey} - {_value}");
           
            if (_tmpValueKey.Equals(GetResearchKey(Settlement)))
                BannerManager.EvaluateBanners();
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
            AddSkilledLaborer(researchDic);
            AddNumberSkilledLaborer(researchDic);
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

            for (int i = 1; i <= 20; i++)
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

            ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SkilledLaborer, 0.02f, requirements));

            for (int i = 2; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SkilledLaborer, 0.02f));
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

        private static void AddMaxSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 2);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 4);
            researchDic.Add(BuiltinBlocks.PlasterBlock, 5);
            researchDic.Add(BuiltinBlocks.IronIngot, 5);
            researchDic.Add(BuiltinBlocks.Bed, 10);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);
            
            for (int i = 1; i <= 10; i++)
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

            for (int i = 1; i <= 10; i++)
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

            for (int i = 1; i <= 5; i++)
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
