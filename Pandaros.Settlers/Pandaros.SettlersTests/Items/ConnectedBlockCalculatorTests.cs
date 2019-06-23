using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.ConnectedBlocks;
using Pandaros.Settlers.Models;

namespace Pandaros.SettlersTests.Items
{
    [TestClass]
    public class ConnectedBlockCalculatorTests
    {
        [TestMethod]
        public void CalculateInitialize()
        {
            ConnectedBlockCalculator.CalculationTypes.Add("Fence", new FenceCalculationType());
            ConnectedBlockCalculator.CalculationTypes.Add("Pipe", new PipeCalculationType());
            ConnectedBlockCalculator.CalculationTypes.Add("Track", new TrackCalculationType());

            ConnectedBlockCalculator.Initialize(new List<ModLoader.ModDescription>());
        }
    }
}
