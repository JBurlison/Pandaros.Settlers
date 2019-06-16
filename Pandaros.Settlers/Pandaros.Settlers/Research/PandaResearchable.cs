using Pandaros.Settlers.Models;
using Science;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Research
{
    public class PandaResearchable : DefaultResearchable
    {
        public string TmpValueKey { get; private set; } = string.Empty;
        public int Level { get; private set; } = 1;
        public float Value { get; private set; }
        public float BaseValue { get; private set; }
        public string LevelKey { get; private set; } = string.Empty;
        public override string RequiredScienceBiome { get; set; }
        public int IterationCount { get; private set; }
        public event EventHandler<ResearchCompleteEventArgs> ResearchComplete;

        public PandaResearchable(IPandaResearch pandaResearch, int currentLevel)
        {
            Conditions = new List<IResearchableCondition>();
            RecipeUnlocks = new List<RecipeUnlock>();
            Dependencies = new List<string>();

            try
            {
                List<IResearchableCondition> researchableConditions;

                if (pandaResearch.Conditions != null && (pandaResearch.Conditions.TryGetValue(currentLevel, out researchableConditions) || pandaResearch.Conditions.TryGetValue(0, out researchableConditions)))
                    Conditions.AddRange(researchableConditions);

                List<string> dependancies = null;

                if (pandaResearch.Dependancies != null && !pandaResearch.Dependancies.TryGetValue(currentLevel, out dependancies))
                    pandaResearch.Dependancies.TryGetValue(0, out dependancies);

                Initialize(currentLevel, pandaResearch.name, pandaResearch.BaseValue, pandaResearch.IconDirectory, dependancies, pandaResearch.BaseIterationCount, pandaResearch.AddLevelToName);

                List<InventoryItem> inventoryItems;

                if (pandaResearch.RequiredItems != null && (pandaResearch.RequiredItems.TryGetValue(currentLevel, out inventoryItems) || pandaResearch.RequiredItems.TryGetValue(0, out inventoryItems)))
                    AddCraftingCondition(inventoryItems, currentLevel);

                List<RecipeUnlock> listUnlocks;
                if (pandaResearch.Unlocks != null && (pandaResearch.Unlocks.TryGetValue(currentLevel, out listUnlocks) || pandaResearch.Unlocks.TryGetValue(0, out listUnlocks)))
                    RecipeUnlocks.AddRange(listUnlocks);

                Register(currentLevel, pandaResearch.name);
            }
            catch (NullReferenceException nullRef)
            {
                PandaLogger.LogToFile(nullRef.StackTrace);
                PandaLogger.LogToFile(nullRef.Message);
                PandaLogger.LogError(nullRef);
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        public void AddCraftingCondition(List<InventoryItem> requiredItems, int currentLevel)
        {
            List<InventoryItem> iterationItems = new List<InventoryItem>();

            foreach (var kvp in requiredItems)
            {
                var val = kvp.Amount;

                if (currentLevel > 1)
                    for (var i = 1; i <= currentLevel; i++)
                        if (i % 2 == 0)
                            val += kvp.Amount * 2;
                        else
                            val += kvp.Amount;

                iterationItems.Add(new InventoryItem(kvp.Type, val));
            }

            Conditions.Add(new ScientistCyclesCondition() { CycleCount = IterationCount, ItemsPerCycle = iterationItems });
        }

        public override void OnResearchComplete(ColonyScienceState manager, EResearchCompletionReason reason)
        {
            base.OnResearchComplete(manager, reason);

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
            return researchName + "_Level";
        }

        private void Initialize(int currentLevel, string name, float baseValue, string iconPath, List<string> dependancies, int baseIterationCount, bool addLevelToName)
        {
            BaseValue = baseValue;
            Value = baseValue * currentLevel;
            Level = currentLevel;
            TmpValueKey = name;
            LevelKey = GetLevelKey(name);

            Key = TmpValueKey + currentLevel;
            Icon = iconPath + name + currentLevel + ".png";

            if (!addLevelToName)
                Icon = iconPath + name + ".png";

            IterationCount = baseIterationCount + 2 * currentLevel;

            if (currentLevel != 1)
                Dependencies.Add(TmpValueKey + (currentLevel - 1));

            if (dependancies != null)
                foreach (var dep in dependancies)
                    Dependencies.Add(dep);
        }

        private void Register(int currentLevel, string name)
        {
            ServerManager.ScienceManager.RegisterResearchable(this);
            PandaLogger.LogToFile($"PandaResearch Added: {name} Level {currentLevel}");
        }

    }
}