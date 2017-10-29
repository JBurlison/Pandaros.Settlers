using ChatCommands;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{
    public class PandaResearchCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/science", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            var tempVals = player.GetTempValues(true);

            PandaChat.Send(player, "Science {0} is at {1}", ChatColor.white, PandaResearch.ArmorSmithing, tempVals.GetOrDefault(PandaResearch.GetLevelKey(PandaResearch.ArmorSmithing), 0).ToString());
            PandaChat.Send(player, "Science {0} is at {1}", ChatColor.white, PandaResearch.ColonistHealth, tempVals.GetOrDefault(PandaResearch.GetLevelKey(PandaResearch.ColonistHealth), 0).ToString());
            PandaChat.Send(player, "Science {0} is at {1}", ChatColor.white, PandaResearch.ReducedWaste, tempVals.GetOrDefault(PandaResearch.GetLevelKey(PandaResearch.ReducedWaste), 0).ToString());

            return true;
        }
    }
}
