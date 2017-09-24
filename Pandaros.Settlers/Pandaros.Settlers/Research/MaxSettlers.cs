using Pipliz.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class MaxSettlers
    {
        public static readonly string TEMP_VAL_KEY = GameLoader.NAMESPACE + ".MaxSettlers";
        
        [AutoLoadedResearchable]
        public class MaxSettlers1 : BaseResearchable
        {
            public MaxSettlers1()
            {
                key = TEMP_VAL_KEY + "1";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers1.png";
                PandaLogger.Log("chance 1:" + icon);
                iterationCount = 20;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 2);
                AddIterationRequirement(ColonyItems.sciencebaglife, 4);
                AddIterationRequirement(ColonyItems.plasterblock, 10);
                AddIterationRequirement(ColonyItems.ironingot, 5);
                AddIterationRequirement(ColonyItems.bread, 5);
                AddIterationRequirement(ColonyItems.goldcoin, 200);
                AddDependency(SettlerChance.TEMP_VAL_KEY + "3");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 1);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers2 : BaseResearchable
        {
            public MaxSettlers2()
            {
                key = TEMP_VAL_KEY + "2";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers2.png";
                iterationCount = 25;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 4);
                AddIterationRequirement(ColonyItems.sciencebaglife, 8);
                AddIterationRequirement(ColonyItems.plasterblock, 20);
                AddIterationRequirement(ColonyItems.ironingot, 10);
                AddIterationRequirement(ColonyItems.bread, 10);
                AddIterationRequirement(ColonyItems.goldcoin, 400);
                AddDependency(TEMP_VAL_KEY + "1");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 2);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers3 : BaseResearchable
        {
            public MaxSettlers3()
            {
                key = TEMP_VAL_KEY + "3";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers3.png";
                iterationCount = 30;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 8);
                AddIterationRequirement(ColonyItems.sciencebaglife, 16);
                AddIterationRequirement(ColonyItems.plasterblock, 40);
                AddIterationRequirement(ColonyItems.ironingot, 20);
                AddIterationRequirement(ColonyItems.bread, 20);
                AddIterationRequirement(ColonyItems.goldcoin, 600);
                AddDependency(TEMP_VAL_KEY + "2");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 3);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers4 : BaseResearchable
        {
            public MaxSettlers4()
            {
                key = TEMP_VAL_KEY + "4";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers4.png";
                iterationCount = 35;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 16);
                AddIterationRequirement(ColonyItems.sciencebaglife, 24);
                AddIterationRequirement(ColonyItems.plasterblock, 80);
                AddIterationRequirement(ColonyItems.ironingot, 40);
                AddIterationRequirement(ColonyItems.bread, 30);
                AddIterationRequirement(ColonyItems.goldcoin, 800);
                AddDependency(TEMP_VAL_KEY + "3");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 4);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers5 : BaseResearchable
        {
            public MaxSettlers5()
            {
                key = TEMP_VAL_KEY + "5";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers5.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 32);
                AddIterationRequirement(ColonyItems.sciencebaglife, 48);
                AddIterationRequirement(ColonyItems.plasterblock, 160);
                AddIterationRequirement(ColonyItems.ironingot, 50);
                AddIterationRequirement(ColonyItems.bread, 35);
                AddIterationRequirement(ColonyItems.goldcoin, 1000);
                AddDependency(TEMP_VAL_KEY + "4");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 5);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers6 : BaseResearchable
        {
            public MaxSettlers6()
            {
                key = TEMP_VAL_KEY + "6";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers6.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 48);
                AddIterationRequirement(ColonyItems.sciencebaglife, 96);
                AddIterationRequirement(ColonyItems.plasterblock, 320);
                AddIterationRequirement(ColonyItems.ironingot, 55);
                AddIterationRequirement(ColonyItems.bread, 45);
                AddIterationRequirement(ColonyItems.goldcoin, 1200);
                AddDependency(TEMP_VAL_KEY + "5");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 6);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers7 : BaseResearchable
        {
            public MaxSettlers7()
            {
                key = TEMP_VAL_KEY + "7";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers7.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 96);
                AddIterationRequirement(ColonyItems.sciencebaglife, 192);
                AddIterationRequirement(ColonyItems.plasterblock, 640);
                AddIterationRequirement(ColonyItems.ironingot, 60);
                AddIterationRequirement(ColonyItems.bread, 50);
                AddIterationRequirement(ColonyItems.goldcoin, 1400);
                AddDependency(TEMP_VAL_KEY + "6");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 7);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers8 : BaseResearchable
        {
            public MaxSettlers8()
            {
                key = TEMP_VAL_KEY + "8";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers8.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 192);
                AddIterationRequirement(ColonyItems.sciencebaglife, 384);
                AddIterationRequirement(ColonyItems.plasterblock, 1280);
                AddIterationRequirement(ColonyItems.ironingot, 65);
                AddIterationRequirement(ColonyItems.bread, 50);
                AddIterationRequirement(ColonyItems.goldcoin, 1600);
                AddDependency(TEMP_VAL_KEY + "7");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 8);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers9 : BaseResearchable
        {
            public MaxSettlers9()
            {
                key = TEMP_VAL_KEY + "9";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers9.png";
                iterationCount = 50;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 384);
                AddIterationRequirement(ColonyItems.sciencebaglife, 768);
                AddIterationRequirement(ColonyItems.plasterblock, 2560);
                AddIterationRequirement(ColonyItems.ironingot, 65);
                AddIterationRequirement(ColonyItems.bread, 50);
                AddIterationRequirement(ColonyItems.goldcoin, 1800);
                AddDependency(TEMP_VAL_KEY + "8");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 9);
            }
        }

        [AutoLoadedResearchable]
        public class MaxSettlers10 : BaseResearchable
        {
            public MaxSettlers10()
            {
                key = TEMP_VAL_KEY + "10";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MaxSettlers10.png";
                iterationCount = 50;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 500);
                AddIterationRequirement(ColonyItems.sciencebaglife, 1000);
                AddIterationRequirement(ColonyItems.plasterblock, 2560);
                AddIterationRequirement(ColonyItems.ironingot, 70);
                AddIterationRequirement(ColonyItems.bread, 60);
                AddIterationRequirement(ColonyItems.goldcoin, 2000);
                AddDependency(TEMP_VAL_KEY + "9");
            }

            public override void OnResearchComplete(ScienceManagerPlayer manager)
            {
                manager.Player.SetTemporaryValue(TEMP_VAL_KEY, 10);
            }
        }

    }
}
