using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandaros.Settlers.WorldGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.WorldGen.Tests
{
    [TestClass()]
    public class OreTests
    {
        [TestInitialize]
        public void Init()
        {
            GameLoader.OnAssemblyLoaded(@"C:\Program Files (x86)\Steam\steamapps\common\Colony Survival\gamedata\mods\Pandaros\Settlers\Pandaros.Settlers.dll");
        }

        [TestMethod()]
        public void AddOresTest()
        {
            Ore.AddOres();
        }
    }
}