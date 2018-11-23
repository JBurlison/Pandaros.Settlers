using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandaros.Settlers.Buildings.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT.Tests
{
    [TestClass()]
    public class SchematicReaderTests
    {
        [TestInitialize]
        public void Init()
        {
            GameLoader.OnAssemblyLoaded(@"C:\Program Files (x86)\Steam\steamapps\common\Colony Survival\gamedata\mods\Pandaros\Settlers\Pandaros.Settlers.dll");
        }

        [TestMethod()]
        public void TryGetSchematicTest()
        {
            
            SchematicReader.TryGetSchematicMetadata("castle-froidinshire.schematic", 1, out var schematic);

            var s = schematic.Blocks;
        }
    }
}