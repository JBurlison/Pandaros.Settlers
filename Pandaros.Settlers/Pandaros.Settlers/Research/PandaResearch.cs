using Pandaros.Settlers.Models;
using Science;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Research
{
    public class PandaResearch : AbstractResearchable
    {
        public string TmpValueKey { get; private set; } = string.Empty;
        public int Level { get; private set; } = 1;
        public float Value { get; private set; }
        public float BaseValue { get; private set; }
        public string LevelKey { get; private set; } = string.Empty;
        public override string RequiredScienceBiome { get; set; }
        public string Key { get; private set; }
        public string Icon { get; private set; }
        public int IterationCount { get; private set; }
        public List<string> Dependantcies { get; private set; } = new List<string>();
        public List<IResearchableCondition> Conditions { get; private set; } = new List<IResearchableCondition>();

        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;

        public PandaResearch(IPandaResearch pandaResearch, int currentLevel)
        {
            if (pandaResearch.Conditions != null && pandaResearch.Conditions.Count > 0)
                Conditions.AddRange(pandaResearch.Conditions);

            AddCraftingCondition(pandaResearch.RequiredItems.ToDictionary(k => k.Key.Id, v => v.Value), currentLevel);
            Initialize(currentLevel, pandaResearch.name, pandaResearch.BaseValue, pandaResearch.IconDirectory, pandaResearch.Dependancies, pandaResearch.BaseIterationCount, pandaResearch.AddLevelToName);
        }

        public PandaResearch(Dictionary<ItemId, int> requiredItems,
                           int currentLevel,
                           string name,
                           float baseValue,
                           string iconPath,
                           List<string> dependancies = null,
                           int baseIterationCount = 10,
                           bool addLevelToName = true) :
           this(requiredItems.ToDictionary(k => k.Key.Id, v => v.Value), currentLevel, name, baseValue, iconPath, dependancies, baseIterationCount, addLevelToName)
        {

        }

        public PandaResearch(Dictionary<ushort, int> requiredItems,
                         int currentLevel,
                         string name,
                         float baseValue,
                         string iconPath,
                         List<string> dependancies = null,
                         int baseIterationCount = 10,
                         bool addLevelToName = true)
        {
            AddCraftingCondition(requiredItems, currentLevel);
            Initialize(currentLevel, name, baseValue, iconPath, dependancies, baseIterationCount, addLevelToName);
        }

        public void AddCraftingCondition(Dictionary<ushort, int> requiredItems, int currentLevel)
        {
            List<InventoryItem> iterationItems = new List<InventoryItem>();

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Value;

                if (currentLevel > 1)
                    for (var i = 1; i <= currentLevel; i++)
                        if (i % 2 == 0)
                            val += kvp.Value * 2;
                        else
                            val += kvp.Value;

                iterationItems.Add(new InventoryItem(kvp.Key, val));
            }

            Conditions.Add(new ScientistCyclesCondition() { CycleCount = IterationCount, ItemsPerCycle = iterationItems });
        }

        public PandaResearch(List<IResearchableCondition> conditions,
                         int currentLevel,
                         string name,
                         float baseValue,
                         string iconPath,
                         List<string> dependancies = null,
                         int baseIterationCount = 10,
                         bool addLevelToName = true)
        {
            Conditions.AddRange(conditions);
            Initialize(currentLevel, name, baseValue, iconPath, dependancies, baseIterationCount, addLevelToName);
        }

        private void Initialize(int currentLevel, string name, float baseValue, string iconPath, List<string> dependancies, int baseIterationCount, bool addLevelToName)
        {
            BaseValue = baseValue;
            Value = baseValue * currentLevel;
            Level = currentLevel;
            TmpValueKey = GetResearchKey(name);
            LevelKey = GetLevelKey(name);

            Key = TmpValueKey + currentLevel;
            Icon = iconPath + name + currentLevel + ".png";

            if (!addLevelToName)
                Icon = iconPath + name + ".png";

            IterationCount = baseIterationCount + 2 * currentLevel;

            if (currentLevel != 1)
                Dependantcies.Add(TmpValueKey + (currentLevel - 1));

            if (dependancies != null)
                foreach (var dep in dependancies)
                    Dependantcies.Add(dep);

            PandaLogger.LogToFile($"PandaResearch Added: {name} Level {currentLevel}");
        }

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

        public override string GetKey()
        {
            return Key;
        }

        public override string GetIcon()
        {
            return Icon;
        }

        public override IList<string> GetDependencies()
        {
            return Dependantcies;
        }

        public override List<IResearchableCondition> GetConditions()
        {
            return Conditions;
        }

        public static string GetLevelKey(string researchName)
        {
            return GetResearchKey(researchName) + "_Level";
        }

        public static string GetResearchKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }
    }
}