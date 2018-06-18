using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public interface IPandaResearch
    {
        Dictionary<ushort, int> RequiredItems { get; }
        int NumberOfLevels { get; }
        string Name { get; }
        float BaseValue { get; }
        List<string> Dependancies { get; }
        int BaseIterationCount { get; }
        bool AddLevelToName { get; }       

        void ResearchComplete(object sender, ResearchCompleteEventArgs e);
    }
}
