using NetworkUI;
using NetworkUI.Items;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.localization;

namespace Pandaros.Settlers.Help
{
    [ModLoader.ModManager]
    public class TutorialManager : API.Extender.IOnTimedUpdate
    {
        static LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Tutorial");

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
