using Chatting;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class BossesChatCommand : IChatCommand
    {
        private static string _Bosses = GameLoader.NAMESPACE + ".Bosses";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameLoader.NAMESPACE + "Bosses.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            if (player.ActiveColony != null)
            {
                menu.Items.Add(new NetworkUI.Items.DropDown("Settlers Bosses", _Bosses, new List<string>() { "Disabled", "Enabled" }));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                menu.LocalStorage.SetAs(_Bosses, Convert.ToInt32(ps.BossesEnabled));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameLoader.NAMESPACE + "Bosses.ChangedSetting")]
        public static void ChangedSetting(ValueTuple<Players.Player, JSONNode, string> data)
        {
            if (data.Item1.ActiveColony != null)

                switch (data.Item3)
                {
                    case "server_popup":
                        var ps = ColonyState.GetColonyState(data.Item1.ActiveColony);

                        if (ps != null && data.Item2.GetAsOrDefault(_Bosses, Convert.ToInt32(ps.BossesEnabled)) != Convert.ToInt32(ps.BossesEnabled))
                        {
                            if (!Configuration.GetorDefault("BossesCanBeDisabled", true))
                                PandaChat.Send(data.Item1, "The server administrator had disabled the changing of bosses.", ChatColor.red);
                            else
                                ps.BossesEnabled = data.Item2.GetAsOrDefault(_Bosses, Convert.ToInt32(ps.BossesEnabled)) != 0;

                            PandaChat.Send(data.Item1, "Settlers! Mod Bosses are now " + (ps.BossesEnabled ? "on" : "off"), ChatColor.green);
                        }

                        break;
                }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/bosses", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server)
                return true;


            if (player.ActiveColony == null)
                PandaChat.Send(player, "You must be near a colony to set its difficulty", ChatColor.red);

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);
            var state  = ColonyState.GetColonyState(player.ActiveColony);

            if (array.Count == 1)
            {
                PandaChat.Send(player, "Settlers! Bosses are {0}.", ChatColor.green,
                               state.BossesEnabled ? "on" : "off");

                return true;
            }

            if (array.Count == 2 && Configuration.GetorDefault("BossesCanBeDisabled", true))
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    state.BossesEnabled = true;
                    PandaChat.Send(player, "Settlers! Mod Bosses are now on.", ChatColor.green);
                }
                else
                {
                    state.BossesEnabled = false;
                    PandaChat.Send(player, "Settlers! Mod Bosses are now off.", ChatColor.green);
                }
            }

            NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
            if (!Configuration.GetorDefault("BossesCanBeDisabled", true))
                PandaChat.Send(player, "The server administrator had disabled the changing of bosses.",
                                ChatColor.red);
            

            return true;
        }
    }
}