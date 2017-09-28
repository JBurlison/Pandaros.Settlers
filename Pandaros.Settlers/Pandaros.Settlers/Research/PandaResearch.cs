using BlockTypes.Builtin;
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

        public PandaResearch(Dictionary<ushort, int> requiredItems, int level, string name, float baseValue)
        {
            _value = baseValue * _level;
            _level = level;
            _tmpValueKey = GetTempValueKey(name);

            key = _tmpValueKey + level;
            icon = GameLoader.ICON_FOLDER_PANDA + "\\" + name + level + ".png";

            iterationCount = 20 + (5 * level);

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (level > 1)
                    for (int i = 1; i <= level; i++)
                        if (i % 3 == 0)
                            val = val * 2;
                        else
                            val += kvp.Value * level;

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
            var researchDic = new Dictionary<ushort, int>();

            AddMaxSettlers(researchDic);
            AddMinSettlers(researchDic);
            AddReducedWaste(researchDic);
            AddSettlerChance(researchDic);
            AddTimeBetween(researchDic);
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
