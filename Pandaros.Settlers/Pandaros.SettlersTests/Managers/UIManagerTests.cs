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
            if(GameLoader.ModInfo.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
            {
                foreach (var jsonNode in jsonFilles.LoopArray())
                {
                    if (jsonNode.TryGetAs("fileType", out string jsonFileType) && jsonFileType == GameLoader.NAMESPACE + ".MenuFile" && jsonNode.TryGetAs("relativePath", out string menuFilePath))
                    {
                        var newMenu = JSON.Deserialize(GameLoader.MOD_FOLDER + "\\" + menuFilePath);

                        if (LoadedMenus == null)
                            LoadedMenus = newMenu;
                        else
                        {
                            UIManager.MergeJsons(LoadedMenus, newMenu);
                        }

                        PandaLogger.Log("Loaded Menu: {0}", menuFilePath);
                    }
                }
            }
            else
                PandaLogger.Log(ChatColor.yellow, "Missing json files node from modinfo.json. Unable to load UI files.");
        }
    }
}