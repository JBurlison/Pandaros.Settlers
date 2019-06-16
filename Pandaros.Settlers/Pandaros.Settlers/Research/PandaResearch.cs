using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Science;

namespace Pandaros.Settlers.Research
{
    public abstract class PandaResearch : IPandaResearch
    {
        public abstract string name { get; }

        public abstract string IconDirectory { get; }

        public virtual Dictionary<int, List<InventoryItem>> RequiredItems { get; }

        public virtual Dictionary<int, List<IResearchableCondition>> Conditions { get; }

        public virtual Dictionary<int, List<RecipeUnlock>> Unlocks { get; }

        public virtual Dictionary<int, List<string>> Dependancies { get; }

        public virtual int NumberOfLevels { get; } = 5;

        public virtual float BaseValue { get; } = 1;

        public virtual int BaseIterationCount { get; } = 10;

        public virtual bool AddLevelToName { get; } = true;

        public virtual void OnRegister()
        {
            
        }

        public virtual void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            
        }
    }
}
