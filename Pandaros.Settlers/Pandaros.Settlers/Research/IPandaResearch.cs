using Pandaros.Settlers.Models;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    public interface IPandaResearch : INameable
    {
        Dictionary<ItemId, int> RequiredItems { get; }
        int NumberOfLevels { get; }
        float BaseValue { get; }
        List<string> Dependancies { get; }
        int BaseIterationCount { get; }
        bool AddLevelToName { get; }       
        void ResearchComplete(object sender, ResearchCompleteEventArgs e);

        void OnRegister();
    }
}
