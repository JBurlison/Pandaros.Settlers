using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandaros.Settlers.Managers;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Managers.Tests
{
    [TestClass()]
    public class UIManagerTests
    {
        public static JSONNode LoadedMenus { get; private set; }

        [TestInitialize]
        public void Init()
        {
            GameLoader.OnAssemblyLoaded(@"C:\Program Files (x86)\Steam\steamapps\common\Colony Survival\gamedata\mods\Pandaros\Settlers\Pandaros.Settlers.dll");
        }

        [TestMethod()]
        public void MergeJsonsTest()
        {
            UIManager.OnAssemblyLoaded(@"C:\Program Files (x86)\Steam\steamapps\common\Colony Survival\gamedata\mods\Pandaros\Settlers\Pandaros.Settlers.dll");

        }
    }
}