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

        public static void SendThrottle(Players.Player  player, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white,
                                        params string[] args)
        {
            if (CanSendMesssage(player))
            {
                var messageBuilt = BuildMessage(string.Format(localizationHelper.LocalizeOrDefault(message, player), LocalizeArgs(player, localizationHelper, args)), player, localizationHelper, color);
                Chat.Send(player, messageBuilt);
                _nextSendTime[player] = Time.SecondsSinceStartDouble + 10;
            }
        }

        public static void SendThrottle(Players.Player player, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white,
                                        ChatStyle      style  = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            if (CanSendMesssage(player))
            {
                var messageBuilt = BuildMessage(message, player, localizationHelper, color, style);
                Chat.Send(player, messageBuilt, sender);
                _nextSendTime[player] = Time.SecondsSinceStartDouble + 10;
            }
        }

        public static void SendThrottle(Colony colony, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            colony.ForEachOwner(o => SendThrottle(o, localizationHelper, message, color, args));
        }

        public static void SendThrottle(Colony colony, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white,
                                        ChatStyle style = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ForEachOwner(o => SendThrottle(o, localizationHelper, message, color, style, sender));
        }

        public static void SendThrottle(ColonyState colony, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            colony.ColonyRef.ForEachOwner(o => SendThrottle(o, localizationHelper, message, color, args));
        }

        public static void SendThrottle(ColonyState colony, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white,
                                        ChatStyle style = ChatStyle.normal,
                                        EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ColonyRef.ForEachOwner(o => SendThrottle(o, localizationHelper, message, color, style, sender));
        }

        public static void Send(Players.Player player, localization.LocalizationHelper localizationHelper, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            var messageBuilt = BuildMessage(string.Format(localizationHelper.LocalizeOrDefault(message, player), LocalizeArgs(player, localizationHelper, args)), player, localizationHelper, color);
            Chat.Send(player, messageBuilt);
        }

        public static void Send(Players.Player player, localization.LocalizationHelper localizationHelper, string message, params string[] args)
        {
            var messageBuilt = BuildMessage(string.Format(localizationHelper.LocalizeOrDefault(message, player), LocalizeArgs(player, localizationHelper, args)), player, localizationHelper);
            Chat.Send(player, messageBuilt);
        }

        public static void Send(Players.Player player, localization.LocalizationHelper localizationHelper,
                                string message,
                                ChatColor      color = ChatColor.white,
                                ChatStyle      style = ChatStyle.normal, 
                                EChatSendOptions sender = EChatSendOptions.Default)
        {
            var messageBuilt = BuildMessage(message, player, localizationHelper, color, style);
            Chat.Send(player, messageBuilt, sender);
        }

        public static void Send(Colony colony, 
                                localization.LocalizationHelper localizationHelper, 
                                string message, ChatColor color = ChatColor.white,
                                params string[] args)
        {
            colony.ForEachOwner(o =>
            {
                var messageBuilt = BuildMessage(colony.Name + ": " + string.Format(localizationHelper.LocalizeOrDefault(message, o), LocalizeArgs(o, localizationHelper, args)), o, localizationHelper, color);
                Chat.Send(o, messageBuilt);
            });
        }

        public static void Send(Colony colony,
                                localization.LocalizationHelper localizationHelper,
                                string message, 
                                params string[] args)
        {
            colony.ForEachOwner(o =>
            {
                var messageBuilt = BuildMessage(colony.Name + ": " + string.Format(localizationHelper.LocalizeOrDefault(message, o), LocalizeArgs(o, localizationHelper, args)), o, localizationHelper);
                Chat.Send(o, messageBuilt);
            });
        }

        public static void Send(Colony colony,
                                localization.LocalizationHelper localizationHelper,
                                string message,
                                ChatColor color = ChatColor.white,
                                ChatStyle style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ForEachOwner(o =>
            {
                var messageBuilt = BuildMessage(colony.Name + ": " + localizationHelper.LocalizeOrDefault(message, o), o, localizationHelper, color, style);
                Chat.Send(o, messageBuilt, sender);
            });
        }

        public static void Send(ColonyState colony,
                                 localization.LocalizationHelper localizationHelper,
                                string message, 
                                ChatColor color = ChatColor.white,
                                params string[] args)
        {
            colony.ColonyRef.ForEachOwner(o =>
            {
                var messageBuilt = BuildMessage(colony.ColonyRef.Name + ": " + string.Format(localizationHelper.LocalizeOrDefault(message, o), LocalizeArgs(o, localizationHelper, args)), o, localizationHelper, color);
                Chat.Send(o, messageBuilt);
            });
        }

        public static void Send(ColonyState colony,
                                localization.LocalizationHelper localizationHelper,
                                string message,
                                ChatColor color = ChatColor.white,
                                ChatStyle style = ChatStyle.normal, EChatSendOptions sender = EChatSendOptions.Default)
        {
            colony.ColonyRef.ForEachOwner(p => { 
                var messageBuilt = BuildMessage(colony.ColonyRef.Name + ": " + localizationHelper.LocalizeOrDefault(message, p), p, localizationHelper, color, style);
                Chat.Send(p, messageBuilt, sender);
            });
        }

        public static void SendToAll(string    message,
                                     localization.LocalizationHelper localizationHelper,
                                     ChatColor      color  = ChatColor.white,
                                     ChatStyle style = ChatStyle.normal, 
                                     EChatSendOptions sender = EChatSendOptions.Default)
        {
            foreach (var p in Players.PlayerDatabase.Values)
                if (p.IsConnected())
                {
                    var messageBuilt = BuildMessage(message, p, localizationHelper, color, style);
                    Chat.Send(p, messageBuilt, sender);
                }
        }

        public static void SendToAll(string message,
                                     localization.LocalizationHelper localizationHelper,
                                     params string[] args)
        {
            foreach (var p in Players.PlayerDatabase.Values)
                if (p.IsConnected())
                {
                    var messageBuilt = BuildMessage(string.Format(localizationHelper.LocalizeOrDefault(message, p), LocalizeArgs(p, localizationHelper, args)), p, localizationHelper);
                    Chat.Send(p, messageBuilt);
                }
        }

        public static string[] LocalizeArgs(Players.Player p, localization.LocalizationHelper localizationHelper, params string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = localizationHelper.LocalizeOrDefault(args[i], p);
            }

            return args;
        }

        public static string BuildMessageNoLocal(string message,
                                        ChatColor color = ChatColor.white,
                                        ChatStyle style = ChatStyle.normal)
        {
            var colorPrefix = "<color=" + color + ">";
            var colorSuffix = "</color>";
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


        public static string BuildMessage(string    message, 
                                         Players.Player p, 
                                         localization.LocalizationHelper localization, 
                                         ChatColor color = ChatColor.white,
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

            return stylePrefix + colorPrefix + localization.LocalizeOrDefault(message, p)+ colorSuffix + styleSuffix;
        }
    }
}