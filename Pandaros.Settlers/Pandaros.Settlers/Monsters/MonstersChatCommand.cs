using System;
using System.Collections.Generic;
using ChatCommands;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.JSON;
using Server.Monsters;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class MonstersChatCommand : IChatCommand
    {
        private static string _Monsters = GameLoader.NAMESPACE + ".Monsters";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameLoader.NAMESPACE + "Monsters.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            menu.Items.Add(new NetworkUI.Items.DropDown("Settlers Monsters", _Monsters, new List<string>() { "Disabled", "Enabled" }));
            var ps = PlayerState.GetPlayerState(player);

            if (ps != null)
                menu.LocalStorage.SetAs(_Monsters, Convert.ToInt32(ps.MonstersEnabled));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameLoader.NAMESPACE + "Monsters.ChangedSetting")]
        public static void ChangedSetting(TupleStruct<Players.Player, JSONNode, string> data)
        {
            switch (data.item3)
            {
                case "world_settings":
                    var ps = PlayerState.GetPlayerState(data.item1);

                    if (ps != null && data.item2.GetAsOrDefault(_Monsters, Convert.ToInt32(ps.MonstersEnabled)) != Convert.ToInt32(ps.MonstersEnabled))
                    {
                        if (!Configuration.GetorDefault("MonstersCanBeDisabled", true))
                            PandaChat.Send(data.item1, "The server administrator had disabled the changing of Monsters.", ChatColor.red);
                        else
                            ps.MonstersEnabled = data.item2.GetAsOrDefault(_Monsters, Convert.ToInt32(ps.MonstersEnabled)) != 0;

                        PandaChat.Send(data.item1, "Settlers! Mod Monsters are now " + (ps.MonstersEnabled ? "on" : "off"), ChatColor.green);

                        if (!ps.MonstersEnabled)
                            MonsterTracker.KillAllZombies(data.item1);
                    }

                    break;
            }
        }

        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/monsters", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            var array  = CommandManager.SplitCommand(chat);
            var colony = Colony.Get(player);
            var state  = PlayerState.GetPlayerState(player);

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Monsters are {0}.", ChatColor.green,
                               state.MonstersEnabled ? "on" : "off");

                return true;
            }

            if (array.Length == 2 && Configuration.GetorDefault("MonstersCanBeDisabled", true))
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    state.MonstersEnabled = true;
                    PandaChat.Send(player, "Settlers! Mod Monsters are now on.", ChatColor.green);
                }
                else
                {
                    state.MonstersEnabled = false;
                    MonsterTracker.KillAllZombies(player);
                    PandaChat.Send(player, "Settlers! Mod Monsters are now off.", ChatColor.green);
                }
            }

            NetworkUI.NetworkMenuManager.SendWorldSettingsUI(player);
            if (!Configuration.GetorDefault("MonstersCanBeDisabled", true))
                PandaChat.Send(player, "The server administrator had disabled the changing of Monsters.",
                               ChatColor.red);

            return true;
        }
    }
}