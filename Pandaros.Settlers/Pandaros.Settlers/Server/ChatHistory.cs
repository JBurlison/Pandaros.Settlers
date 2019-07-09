using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatting;
using Pipliz;

namespace Pandaros.Settlers.Server
{
    [ModLoader.ModManager]
    public class ChatHistory : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (player != null)
            {

            }


            return false;
        }
    }
}
