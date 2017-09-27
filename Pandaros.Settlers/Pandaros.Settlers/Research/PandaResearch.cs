using Pipliz.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class PandaResearch : BaseResearchable
    {
        public static readonly string MaxSettlers = "MaxSettlers";
        public static readonly string MinSettlers = "MinSettlers";
        public static readonly string ReducedWaste = "ReducedWaste";
        public static readonly string SettlerChance = "SettlerChance";
        public static readonly string TimeBetween = "TimeBetween";

        private string _tmpValueKey = string.Empty;
        private int _level = 1;
        private float _value = 0;

        public PandaResearch(Dictionary<string, int> requiredItems, int level, string name, float baseValue)
        {
            _value = baseValue * _level;
            _level = level;
            _tmpValueKey = GetTempValueKey(name);

            key = _tmpValueKey + level;
            icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\" + name + level + ".png";

            iterationCount = 20 + (5 * level);

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (level > 1)
                    for (int i = 1; i <= level; i++)
                        val = val * 2;

                val = (int)System.Math.Ceiling(val / (double)level);

                AddIterationRequirement(kvp.Key, val);
            }

            if (level != 1)
                AddDependency(_tmpValueKey + (level - 1));
        }

        public override void OnResearchComplete(ScienceManagerPlayer manager)
        {
            manager.Player.SetTemporaryValue(_tmpValueKey, _value);
        }

        public static string GetTempValueKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }

        public static void Register()
        {
            var researchDic = new Dictionary<string, int>();

            AddMaxSettlers(researchDic);
            AddMinSettlers(researchDic);
            AddReducedWaste(researchDic);
            AddSettlerChance(researchDic);
            AddTimeBetween(researchDic);
        }

        private static void AddMaxSettlers(Dictionary<string, int> researchDic)
        {
            researchDic.Add(ColonyItems.sciencebagbasic, 2);
            researchDic.Add(ColonyItems.sciencebaglife, 4);
            researchDic.Add(ColonyItems.plasterblock, 5);
            researchDic.Add(ColonyItems.ironingot, 5);
            researchDic.Add(ColonyItems.bread, 5);
            researchDic.Add(ColonyItems.goldcoin, 20);

            for (int i = 1; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MaxSettlers, 1f));
        }

        private static void AddMinSettlers(Dictionary<string, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyItems.sciencebagbasic, 2);
            researchDic.Add(ColonyItems.sciencebaglife, 4);
            researchDic.Add(ColonyItems.bricks, 5);
            researchDic.Add(ColonyItems.coatedplanks, 5);
            researchDic.Add(ColonyItems.bread, 5);
            researchDic.Add(ColonyItems.goldcoin, 20);

            for (int i = 1; i <= 10; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MinSettlers, 1f));
        }

        private static void AddReducedWaste(Dictionary<string, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyItems.sciencebagbasic, 6);
            researchDic.Add(ColonyItems.sciencebaglife, 10);
            researchDic.Add(ColonyItems.berry, 20);
            researchDic.Add(ColonyItems.linseedoil, 10);
            researchDic.Add(ColonyItems.bread, 5);
            researchDic.Add(ColonyItems.goldcoin, 100);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, ReducedWaste, 0.5f));
        }

        private static void AddSettlerChance(Dictionary<string, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyItems.sciencebagbasic, 6);
            researchDic.Add(ColonyItems.sciencebaglife, 10);
            researchDic.Add(ColonyItems.torch, 5);
            researchDic.Add(ColonyItems.stonebricks, 10);
            researchDic.Add(ColonyItems.bread, 5);
            researchDic.Add(ColonyItems.goldcoin, 100);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SettlerChance, 0.1f));
        }

        private static void AddTimeBetween(Dictionary<string, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyItems.sciencebagbasic, 6);
            researchDic.Add(ColonyItems.sciencebaglife, 10);
            researchDic.Add(ColonyItems.carpetblue, 5);
            researchDic.Add(ColonyItems.bed, 10);
            researchDic.Add(ColonyItems.carpetred, 5);
            researchDic.Add(ColonyItems.goldcoin, 100);

            for (int i = 1; i <= 5; i++)
                ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, TimeBetween, 1f));
        }
    }
}
