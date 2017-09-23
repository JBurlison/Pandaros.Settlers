using General.Networking;
using Pipliz;
using Pipliz.Chatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static void Send(Players.Player ply, string message, ChatColor color = ChatColor.white, params string[] args)
        {
            string messageBuilt = BuildMessage(string.Format(message, args), color);
            Pipliz.Chatting.Chat.Send(ply, messageBuilt);
        }

        public static void Send(Players.Player ply, string message, ChatColor color = ChatColor.white, ChatStyle style = ChatStyle.normal, Pipliz.Chatting.ChatSenderType sender = Pipliz.Chatting.ChatSenderType.Server)
        {
            string messageBuilt = BuildMessage(message, color, style);
            Pipliz.Chatting.Chat.Send(ply, messageBuilt, sender);
        }

        public static void SendToAll(string message, ChatColor color = ChatColor.white, ChatStyle style = ChatStyle.normal, Pipliz.Chatting.ChatSenderType sender = Pipliz.Chatting.ChatSenderType.Server)
        {
            string messageBuilt = BuildMessage(message, color, style);
            Pipliz.Chatting.Chat.SendToAll(messageBuilt, sender);
        }

        public static void SendToAllBut(Players.Player ply, string message, ChatColor color = ChatColor.white, ChatStyle style = ChatStyle.normal, Pipliz.Chatting.ChatSenderType sender = Pipliz.Chatting.ChatSenderType.Server)
        {
            string messageBuilt = BuildMessage(message, color, style);
            Pipliz.Chatting.Chat.SendToAllBut(ply, messageBuilt, sender);
        }


        public static string BuildMessage(string message, ChatColor color = ChatColor.white, ChatStyle style = ChatStyle.normal)
        {
            string colorPrefix = "<color=" + color.ToString() + ">";
            string colorSuffix = "</color>";
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
