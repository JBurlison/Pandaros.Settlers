using Pipliz.JSON;
using Pipliz.Mods.BaseGame.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class ArchitectLoader : IConstructionLoader
    {
        public const string NAME = GameLoader.NAMESPACE + ".Architect";
        public string JobName => NAME;

        public void ApplyTypes(ConstructionArea area, JSONNode node)
        {
            
        }

        public void SaveTypes(ConstructionArea area, JSONNode node)
        {
            
        }
    }
}
