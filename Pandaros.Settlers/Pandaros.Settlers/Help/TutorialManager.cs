using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Pipliz;
using Pipliz.JSON;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Help;
using Pandaros.Settlers.Items;
using Shared;
using Pandaros.Settlers.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Pandaros.Settlers.Entities;

namespace Pandaros.Settlers.Help
{
    [ModLoader.ModManager]
    public class TutorialManager : Extender.IOnTimedUpdate
    {
        static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameLoader.NAMESPACE, "Tutorial");

        public double NextUpdateTimeMin => 5;

        public double NextUpdateTimeMax => 10;

        public double NextUpdateTime { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Help.TutorialManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            var ps = PlayerState.GetPlayerState(p);

            if (!TutorialRun(ps, "FirstRunTutorial"))
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("TipsHeader", p));
                menu.Width = 800;
                menu.Height = 600;
                menu.ForceClosePopups = true;

                menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Intro"), UnityEngine.Color.black)));

                SetTutorialRun(ps, "FirstRunTutorial");
                NetworkMenuManager.SendServerPopup(p, menu);
            }
        }

        public static bool TutorialRun(PlayerState playerState, string tutorialName)
        {
            return playerState.Tutorials.TryGetValue(tutorialName, out bool tutorialRun) && tutorialRun;
        }

        public static void SetTutorialRun(PlayerState playerState, string tutorialName, bool run = true)
        {
            playerState.Tutorials[tutorialName] = run;
        }

        public void OnTimedUpdate()
        {
            if (ServerManager.ColonyTracker != null && ServerManager.ColonyTracker.ColoniesByID != null)
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.ValsRaw)
                {
                    if (colony == null || colony.FollowerCount < 125)
                        return;

                    var cs = ColonyState.GetColonyState(colony);

                    if (cs.BossesEnabled && colony.Owners != null &&  colony.Owners.Length > 0)
                    {
                        foreach (var p in colony.Owners)
                        {
                            if (!p.IsConnected())
                                continue;

                            var ps = PlayerState.GetPlayerState(p);

                            if (!TutorialRun(ps, "CloseToBosses"))
                            {
                                NetworkMenu menu = new NetworkMenu();
                                menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("CloseToBossesHeader", p));
                                menu.Width = 800;
                                menu.Height = 600;
                                menu.ForceClosePopups = true;

                                menu.Items.Add(new Label(new LabelData(string.Format(_localizationHelper.GetLocalizationKey("CloseToBosses"), colony.Name), UnityEngine.Color.black)));

                                SetTutorialRun(ps, "CloseToBosses");
                                NetworkMenuManager.SendServerPopup(p, menu);

                            }
                        }
                    }
                }
        }
    }
}
