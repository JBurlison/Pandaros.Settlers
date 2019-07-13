using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatting;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Extender;
using Pipliz;

namespace Pandaros.Settlers.Server
{
    [ModLoader.ModManager]
    public class ChatHistory : IChatCommand, IOnTimedUpdate, IOnConstructInventoryManageColonyUI
    {
        public ChatHistory() { }

        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameLoader.NAMESPACE, "System");

        public double NextUpdateTimeMin => 30;

        public double NextUpdateTimeMax => 30;

        public double NextUpdateTime { get; set; }

        public void OnTimedUpdate()
        {
            var file = Path.Combine(GameLoader.SAVE_LOC, "ChatLog.log");

            if (File.Exists(file))
            {
                var lines = File.ReadAllLines(file).ToList();

                if (lines.Count > 10000)
                {
                    File.WriteAllLines(file, lines.Skip(lines.Count - 10000));
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Server.ChatHistory.OnPlayerPushedNetworkUIButton")]
        public static void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".ChatHistory")
            {
                PandaLogger.Log(data.ButtonIdentifier);

                NetworkMenu menu = new NetworkMenu();
                var file = Path.Combine(GameLoader.SAVE_LOC, "ChatLog.log");
                menu.ForceClosePopups = true;
                menu.Width = 1000;
                menu.Height = 700;
                menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("ChatHistory", data.Player));

                if (File.Exists(file))
                    foreach (var item in File.ReadAllLines(file))
                        menu.Items.Add(new Label(new LabelData(item, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.None)));
                
                NetworkMenuManager.SendServerPopup(data.Player, menu);
            }
        }

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu menu)
        {
            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".ChatHistory", new LabelData(_localizationHelper.GetLocalizationKey("ChatHistory"), UnityEngine.Color.black), 200));
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (player != null)
                File.AppendAllText(Path.Combine(GameLoader.SAVE_LOC, "ChatLog.log"), string.Format("[{0}] {1}: {2}", DateTime.Now, player.Name, chat) + Environment.NewLine);

            return false;
        }
    }
}
