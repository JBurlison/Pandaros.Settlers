using System.Collections.Generic;
using Chatting;
using Pandaros.Settlers.Entities;
using Pipliz;

namespace Pandaros.Settlers
{
    public enum ChatColor
    {
        black,
        blue,
        brown,
        cyan,
        darkblue,
        green,
        grey,
        lightblue,
        lime,
        magenta,
        maroon,
        navy,
        olive,
        orange,
        purple,
        red,
        silver,
        teal,
        white,
        yellow
    }

    public enum ChatStyle
    {
        normal,
        bold,
        italic,
        bolditalic
    }

    public static class PandaChat
    {
        private static readonly Dictionary<Players.Player, double> _nextSendTime =
            new Dictionary<Players.Player, double>();

        public static bool CanSendMesssage(Players.Player p)
        {
            if (!_nextSendTime.ContainsKey(p))
                _nextSendTime.Add(p, 0);

            return Time.SecondsSinceStartDouble > _nextSendTime[p];
        }

        public static void SendThrottle(Players.Player  player, string message, ChatColor color = ChatColor.white,
                                        params string[] args)
        {
            if (CanSendMesssage(player))
            {
                var messageBuilt = BuildMessage(string.Format(message, args), color);
                Chat.Send(player, messageBuilt);
                _nextSendTime[player] = Time.SecondsSinceStartDouble + 10;
            }
        }

        public static void SendThrottle(Players.Player player, string message, ChatColor color = ChatColor.white,
                                        ChatStyle      style  = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            if (CanSendMesssage(player))
            {
                var messageBuilt = BuildMessage(message, color, style);
                Chat.Send(player, messageBuilt, sender);
                _nextSendTime[player] = Time.SecondsSinceStartDouble + 10;
            }
        }

        public static void SendThrottle(Colony colony, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            colony.ForEachOwner(o => SendThrottle(o, message, color, args));
        }

        public static void SendThrottle(Colony colony, string message, ChatColor color = ChatColor.white,
                                        ChatStyle style = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ForEachOwner(o => SendThrottle(o, message, color, style, sender));
        }

        public static void SendThrottle(ColonyState colony, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            colony.ColonyRef.ForEachOwner(o => SendThrottle(o, message, color, args));
        }

        public static void SendThrottle(ColonyState colony, string message, ChatColor color = ChatColor.white,
                                        ChatStyle style = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ColonyRef.ForEachOwner(o => SendThrottle(o, message, color, style, sender));
        }

        public static void Send(Players.Player  player, string message, ChatColor color = ChatColor.white,
                                params string[] args)
        {
            var messageBuilt = BuildMessage(string.Format(message, args), color);
            Chat.Send(player, messageBuilt);
        }

        public static void Send(Players.Player player, string message,
                                ChatColor      color = ChatColor.white,
                                ChatStyle      style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, color, style);
            Chat.Send(player, messageBuilt, sender);
        }
        public static void Send(Colony colony, string message, ChatColor color = ChatColor.white,
                                params string[] args)
        {
            var messageBuilt = BuildMessage(string.Format(message, args), color);
            colony.ForEachOwner(o => Chat.Send(o, messageBuilt));
        }

        public static void Send(Colony colony, string message,
                                ChatColor color = ChatColor.white,
                                ChatStyle style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, color, style);
            colony.ForEachOwner(o => Chat.Send(o, messageBuilt, sender));
        }

        public static void Send(ColonyState colony, string message, ChatColor color = ChatColor.white,
                                params string[] args)
        {
            var messageBuilt = BuildMessage(string.Format(message, args), color);
            colony.ColonyRef.ForEachOwner(o => Chat.Send(o, messageBuilt));
        }

        public static void Send(ColonyState colony, string message,
                                ChatColor color = ChatColor.white,
                                ChatStyle style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, color, style);
            colony.ColonyRef.ForEachOwner(o => Chat.Send(o, messageBuilt, sender));
        }

        public static void SendToAll(string    message,                  ChatColor      color  = ChatColor.white,
                                     ChatStyle style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, color, style);
            Chat.SendToConnected(messageBuilt, sender);
        }

        public static void SendToAllBut(Players.Player player, string message, ChatColor color = ChatColor.white,
                                        ChatStyle      style  = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, color, style);
            Chat.SendToConnectedBut(player, messageBuilt, sender);
        }


        public static string BuildMessage(string    message, ChatColor color = ChatColor.white,
                                          ChatStyle style = ChatStyle.normal)
        {
            var    colorPrefix = "<color=" + color + ">";
            var    colorSuffix = "</color>";
            string stylePrefix, styleSuffix;

            switch (style)
            {
                case ChatStyle.bold:
                    stylePrefix = "<b>";
                    styleSuffix = "</b>";
                    break;
                case ChatStyle.bolditalic:
                    stylePrefix = "<b><i>";
                    styleSuffix = "</i></b>";
                    break;
                case ChatStyle.italic:
                    stylePrefix = "<i>";
                    styleSuffix = "</i>";
                    break;
                default:
                    stylePrefix = "";
                    styleSuffix = "";
                    break;
            }

            return stylePrefix + colorPrefix + message + colorSuffix + styleSuffix;
        }
    }
}