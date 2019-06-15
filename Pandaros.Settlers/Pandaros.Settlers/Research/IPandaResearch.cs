using Pandaros.Settlers.Models;
using Science;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    public interface IPandaResearch : INameable
    {
        string IconDirectory { get; }
        Dictionary<ItemId, int> RequiredItems { get; }
        List<IResearchableCondition> Conditions { get; }
        int NumberOfLevels { get; }
        float BaseValue { get; }
        List<string> Dependancies { get; }
        int BaseIterationCount { get; }
        bool AddLevelToName { get; }       
        void ResearchComplete(object sender, ResearchCompleteEventArgs e);

        void OnRegister();
    }
}
