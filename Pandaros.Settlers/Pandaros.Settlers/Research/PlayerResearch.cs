using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Research
{
    public class MaxMagicItems //: IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => throw new NotImplementedException();

        public int NumberOfLevels => throw new NotImplementedException();

        public float BaseValue => throw new NotImplementedException();

        public List<string> Dependancies => throw new NotImplementedException();

        public int BaseIterationCount => throw new NotImplementedException();

        public bool AddLevelToName => throw new NotImplementedException();

        public string Name => GameLoader.NAMESPACE + ".MaxMagicItems";

        public void OnRegister()
        {
            throw new NotImplementedException();
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            
        }
    }
}
