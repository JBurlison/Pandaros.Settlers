using Pipliz.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class ReducedWaste
    {
        public static readonly string TEMP_VAL_KEY = GameLoader.NAMESPACE + ".ReducedWaste";
        
        [AutoLoadedResearchable]
        public class ReducedWaste1 : BaseResearchable
        {
            public ReducedWaste1()
            {
                key = TEMP_VAL_KEY + "1";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\ReducedWaste1.png";
                PandaLogger.Log("chance 1:" + icon);
                iterationCount = 20;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 10);
                AddIterationRequirement(ColonyItems.sciencebaglife, 20);
                AddIterationRequirement(ColonyItems.berry, 10);
                AddIterationRequirement(ColonyItems.linseedoil, 20);
                AddIterationRequirement(ColonyItems.bread, 5);
                AddIterationRequirement(ColonyItems.goldcoin, 250);
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 0.05f);
            }
        }

        [AutoLoadedResearchable]
        public class ReducedWaste2 : BaseResearchable
        {
            public ReducedWaste2()
            {
                key = TEMP_VAL_KEY + "2";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\ReducedWaste2.png";
                iterationCount = 25;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 20);
                AddIterationRequirement(ColonyItems.sciencebaglife, 40);
                AddIterationRequirement(ColonyItems.berry, 20);
                AddIterationRequirement(ColonyItems.linseedoil, 40);
                AddIterationRequirement(ColonyItems.bread, 5);
                AddIterationRequirement(ColonyItems.goldcoin, 500);
                AddDependency(TEMP_VAL_KEY + "1");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 0.1f);
            }
        }

        [AutoLoadedResearchable]
        public class ReducedWaste3 : BaseResearchable
        {
            public ReducedWaste3()
            {
                key = TEMP_VAL_KEY + "3";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\ReducedWaste3.png";
                iterationCount = 30;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 40);
                AddIterationRequirement(ColonyItems.sciencebaglife, 80);
                AddIterationRequirement(ColonyItems.berry, 40);
                AddIterationRequirement(ColonyItems.linseedoil, 80);
                AddIterationRequirement(ColonyItems.bread, 10);
                AddIterationRequirement(ColonyItems.goldcoin, 1000);
                AddDependency(TEMP_VAL_KEY + "2");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 0.15f);
            }
        }

        [AutoLoadedResearchable]
        public class ReducedWaste4 : BaseResearchable
        {
            public ReducedWaste4()
            {
                key = TEMP_VAL_KEY + "4";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\ReducedWaste4.png";
                iterationCount = 35;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 80);
                AddIterationRequirement(ColonyItems.sciencebaglife, 160);
                AddIterationRequirement(ColonyItems.berry, 80);
                AddIterationRequirement(ColonyItems.linseedoil, 160);
                AddIterationRequirement(ColonyItems.bread, 10);
                AddIterationRequirement(ColonyItems.goldcoin, 2000);
                AddDependency(TEMP_VAL_KEY + "3");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 0.2f);
            }
        }

        [AutoLoadedResearchable]
        public class ReducedWaste5 : BaseResearchable
        {
            public ReducedWaste5()
            {
                key = TEMP_VAL_KEY + "5";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\ReducedWaste5.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 160);
                AddIterationRequirement(ColonyItems.sciencebaglife, 340);
                AddIterationRequirement(ColonyItems.berry, 160);
                AddIterationRequirement(ColonyItems.linseedoil, 340);
                AddIterationRequirement(ColonyItems.bread, 15);
                AddIterationRequirement(ColonyItems.goldcoin, 4000);
                AddDependency(TEMP_VAL_KEY + "4");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 0.25f);
            }
        }
    }
}
