using Pipliz.APIProvider.Science;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class MinSettlers
    {
        public static readonly string TEMP_VAL_KEY = GameLoader.NAMESPACE + ".MinSettlers";
        
        [AutoLoadedResearchable]
        public class MinSettlers1 : BaseResearchable
        {
            public MinSettlers1()
            {
                key = TEMP_VAL_KEY + "1";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers1.png";
                PandaLogger.Log("chance 1:" + icon);
                iterationCount = 20;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 2);
                AddIterationRequirement(ColonyItems.sciencebaglife, 4);
                AddIterationRequirement(ColonyItems.bricks, 10);
                AddIterationRequirement(ColonyItems.coatedplanks, 5);
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
        public class MinSettlers2 : BaseResearchable
        {
            public MinSettlers2()
            {
                key = TEMP_VAL_KEY + "2";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers2.png";
                iterationCount = 25;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 4);
                AddIterationRequirement(ColonyItems.sciencebaglife, 8);
                AddIterationRequirement(ColonyItems.bricks, 20);
                AddIterationRequirement(ColonyItems.coatedplanks, 10);
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
        public class MinSettlers3 : BaseResearchable
        {
            public MinSettlers3()
            {
                key = TEMP_VAL_KEY + "3";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers3.png";
                iterationCount = 30;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 8);
                AddIterationRequirement(ColonyItems.sciencebaglife, 16);
                AddIterationRequirement(ColonyItems.bricks, 40);
                AddIterationRequirement(ColonyItems.coatedplanks, 20);
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
        public class MinSettlers4 : BaseResearchable
        {
            public MinSettlers4()
            {
                key = TEMP_VAL_KEY + "4";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers4.png";
                iterationCount = 35;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 16);
                AddIterationRequirement(ColonyItems.sciencebaglife, 24);
                AddIterationRequirement(ColonyItems.bricks, 80);
                AddIterationRequirement(ColonyItems.coatedplanks, 40);
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
        public class MinSettlers5 : BaseResearchable
        {
            public MinSettlers5()
            {
                key = TEMP_VAL_KEY + "5";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers5.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 32);
                AddIterationRequirement(ColonyItems.sciencebaglife, 48);
                AddIterationRequirement(ColonyItems.bricks, 160);
                AddIterationRequirement(ColonyItems.coatedplanks, 50);
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
        public class MinSettlers6 : BaseResearchable
        {
            public MinSettlers6()
            {
                key = TEMP_VAL_KEY + "6";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers6.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 48);
                AddIterationRequirement(ColonyItems.sciencebaglife, 96);
                AddIterationRequirement(ColonyItems.bricks, 320);
                AddIterationRequirement(ColonyItems.coatedplanks, 55);
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
        public class MinSettlers7 : BaseResearchable
        {
            public MinSettlers7()
            {
                key = TEMP_VAL_KEY + "7";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers7.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 96);
                AddIterationRequirement(ColonyItems.sciencebaglife, 192);
                AddIterationRequirement(ColonyItems.bricks, 640);
                AddIterationRequirement(ColonyItems.coatedplanks, 60);
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
        public class MinSettlers8 : BaseResearchable
        {
            public MinSettlers8()
            {
                key = TEMP_VAL_KEY + "8";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers8.png";
                iterationCount = 40;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 192);
                AddIterationRequirement(ColonyItems.sciencebaglife, 384);
                AddIterationRequirement(ColonyItems.bricks, 1280);
                AddIterationRequirement(ColonyItems.coatedplanks, 65);
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
        public class MinSettlers9 : BaseResearchable
        {
            public MinSettlers9()
            {
                key = TEMP_VAL_KEY + "9";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers9.png";
                iterationCount = 50;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 384);
                AddIterationRequirement(ColonyItems.sciencebaglife, 768);
                AddIterationRequirement(ColonyItems.bricks, 2560);
                AddIterationRequirement(ColonyItems.coatedplanks, 65);
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
        public class MinSettlers10 : BaseResearchable
        {
            public MinSettlers10()
            {
                key = TEMP_VAL_KEY + "10";
                icon = GameLoader.ICON_FOLDER_PANDA_REL + "\\MinSettlers10.png";
                iterationCount = 50;
                AddIterationRequirement(ColonyItems.sciencebagbasic, 500);
                AddIterationRequirement(ColonyItems.sciencebaglife, 1000);
                AddIterationRequirement(ColonyItems.bricks, 2560);
                AddIterationRequirement(ColonyItems.coatedplanks, 70);
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
