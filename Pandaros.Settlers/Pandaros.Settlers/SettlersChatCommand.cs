﻿using Chatting;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public class SettlersChatCommand : IChatCommand
    {
        private static string _Setters = GameLoader.NAMESPACE + ".Settlers";
        private static LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Settlers");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameLoader.NAMESPACE + "Settlers.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            if (player.ActiveColony != null)
            {
                menu.Items.Add(new NetworkUI.Items.DropDown("Random Settlers", _Setters, new List<string>() { "Prompt", "Always Accept", "Disabled" }));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                menu.LocalStorage.SetAs(_Setters, Convert.ToInt32(ps.SettlersEnabled));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameLoader.NAMESPACE + "Settlers.ChangedSetting")]
        public static void ChangedSetting(ValueTuple<Players.Player, JSONNode, string> data)
        {
            if (data.Item1.ActiveColony != null)
                switch (data.Item3)
                {
                    case "server_popup":
                        var cs = ColonyState.GetColonyState(data.Item1.ActiveColony);
                        var maxToggleTimes = SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4);

                        if (cs != null)
                        {
                            if (!SettlersConfiguration.GetorDefault("SettlersEnabled", true))
                                PandaChat.Send(data.Item1, _localizationHelper, "AdminDisabledSettlers", ChatColor.red);
                            else if (!HasToggeledMaxTimes(maxToggleTimes, cs, data.Item1))
                            {
                                var def = (int)cs.SettlersEnabled;
                                var enabled = data.Item2.GetAsOrDefault(_Setters, def);

                                if (def != enabled)
                                {
                                    TurnSettlersOn(data.Item1, cs, maxToggleTimes, (SettlersState)enabled);
                                    PandaChat.Send(data.Item1, _localizationHelper, "SettlersState", ChatColor.green, cs.SettlersEnabled.ToString());
                                }
                            }
                        }

                        break;
                }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/settlers", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server || player.ActiveColony == null)
                return true;

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);
            var state          = ColonyState.GetColonyState(player.ActiveColony);
            var maxToggleTimes = SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4);

            if (maxToggleTimes == 0 && !SettlersConfiguration.GetorDefault("SettlersEnabled", true))
            {
                PandaChat.Send(player, _localizationHelper, "AdminDisabledSettlers.", ChatColor.red);

                return true;
            }

            if (HasToggeledMaxTimes(maxToggleTimes, state, player))
                return true;

            if (array.Count == 1)
            {
                PandaChat.Send(player,
                               _localizationHelper,
                               "SettlersToggleTime",
                               ChatColor.green, 
                               state.SettlersEnabled.ToString(),
                               state.SettlersToggledTimes.ToString(),
                               maxToggleTimes.ToString());
                return true;
            }

            if (array.Count == 2 && state.SettlersToggledTimes <= maxToggleTimes && Enum.TryParse<SettlersState>(array[1].ToLower().Trim(), out var settlersState))
            {
                TurnSettlersOn(player, state, maxToggleTimes, settlersState);
            }

            return true;
        }

        private static bool HasToggeledMaxTimes(int maxToggleTimes, ColonyState state, Players.Player player)
        {
            if (state.SettlersToggledTimes >= maxToggleTimes)
            {
                PandaChat.Send(player, _localizationHelper, "AbuseWarning", ChatColor.red, maxToggleTimes.ToString());

                return true;
            }

            return false;
        }

        private static void TurnSettlersOn(Players.Player player, ColonyState state, int maxToggleTimes, SettlersState enabled)
        {
            if (state.SettlersEnabled == SettlersState.Disabled && enabled != SettlersState.Disabled)
                state.SettlersToggledTimes++;

            state.SettlersEnabled = enabled;

            PandaChat.Send(player,
                            _localizationHelper,
                            "SettlersToggleTime",
                            ChatColor.green,
                            state.SettlersEnabled.ToString(),
                            state.SettlersToggledTimes.ToString(),
                            maxToggleTimes.ToString());

            NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
        }
    }
}