using Pipliz.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class TimeBetween
    {
        public static readonly string TEMP_VAL_KEY = GameLoader.NAMESPACE + ".TimeBetween";
        
        [AutoLoadedResearchable]
        public class TimeBetween1 : BaseResearchable
        {
            public TimeBetween1()
            {
                key = TEMP_VAL_KEY + "1";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\TimeBetween1.png";
                PandaLogger.Log("chance 1:" + icon);
                iterationCount = 20;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 10);
                AddIterationRequirement(ColonyItems.sciencebaglife, 20);
                AddIterationRequirement(ColonyItems.carpetblue, 10);
                AddIterationRequirement(ColonyItems.bed, 20);
                AddIterationRequirement(ColonyItems.carpetred, 5);
                AddIterationRequirement(ColonyItems.goldcoin, 250);
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 1);
            }
        }

        [AutoLoadedResearchable]
        public class TimeBetween2 : BaseResearchable
        {
            public TimeBetween2()
            {
                key = TEMP_VAL_KEY + "2";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\TimeBetween2.png";
                iterationCount = 25;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 20);
                AddIterationRequirement(ColonyItems.sciencebaglife, 40);
                AddIterationRequirement(ColonyItems.carpetblue, 20);
                AddIterationRequirement(ColonyItems.bed, 40);
                AddIterationRequirement(ColonyItems.carpetred, 5);
                AddIterationRequirement(ColonyItems.goldcoin, 500);
                AddDependency(TEMP_VAL_KEY + "1");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 2);
            }
        }

        [AutoLoadedResearchable]
        public class TimeBetween3 : BaseResearchable
        {
            public TimeBetween3()
            {
                key = TEMP_VAL_KEY + "3";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\TimeBetween3.png";
                iterationCount = 30;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 40);
                AddIterationRequirement(ColonyItems.sciencebaglife, 80);
                AddIterationRequirement(ColonyItems.carpetblue, 40);
                AddIterationRequirement(ColonyItems.bed, 80);
                AddIterationRequirement(ColonyItems.carpetred, 10);
                AddIterationRequirement(ColonyItems.goldcoin, 1000);
                AddDependency(TEMP_VAL_KEY + "2");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 3);
            }
        }

        [AutoLoadedResearchable]
        public class TimeBetween4 : BaseResearchable
        {
            public TimeBetween4()
            {
                key = TEMP_VAL_KEY + "4";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\TimeBetween4.png";
                iterationCount = 35;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 80);
                AddIterationRequirement(ColonyItems.sciencebaglife, 160);
                AddIterationRequirement(ColonyItems.carpetblue, 80);
                AddIterationRequirement(ColonyItems.bed, 160);
                AddIterationRequirement(ColonyItems.carpetred, 10);
                AddIterationRequirement(ColonyItems.goldcoin, 2000);
                AddDependency(TEMP_VAL_KEY + "3");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 4);
            }
        }

        [AutoLoadedResearchable]
        public class TimeBetween5 : BaseResearchable
        {
            public TimeBetween5()
            {
                key = TEMP_VAL_KEY + "5";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\TimeBetween5.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 160);
                AddIterationRequirement(ColonyItems.sciencebaglife, 340);
                AddIterationRequirement(ColonyItems.carpetblue, 160);
                AddIterationRequirement(ColonyItems.straw, 340);
                AddIterationRequirement(ColonyItems.carpetred, 15);
                AddIterationRequirement(ColonyItems.goldcoin, 4000);
                AddDependency(TEMP_VAL_KEY + "4");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 5);
            }
        }
    }
}
