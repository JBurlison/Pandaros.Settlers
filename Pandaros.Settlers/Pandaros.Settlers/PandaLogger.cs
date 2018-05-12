using System;
using Pipliz;
using UnityEngine;

namespace Pandaros.Settlers
{
    internal class PandaLogger
    {
        internal static void Log(ChatColor color, string message, params object[] args)
        {
            ServerLog.LogAsyncMessage(new
                                          LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), color),
                                                     LogType.Log));
        }

        internal static void Log(string message, params object[] args)
        {
            ServerLog.LogAsyncMessage(new LogMessage(GetFormattedMessage(string.Format(message, args)), LogType.Log));
        }

        internal static void Log(string message)
        {
            ServerLog.LogAsyncMessage(new LogMessage(GetFormattedMessage(message), LogType.Log));
        }

        internal static void LogError(Exception e, string message)
        {
            ServerLog.LogAsyncExceptionMessage(new
                                                   LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(message), ChatColor.red),
                                                                       e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e, string message, params object[] args)
        {
            ServerLog.LogAsyncExceptionMessage(new
                                                   LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), ChatColor.red),
                                                                       e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e)
        {
            ServerLog
               .LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage("Exception", ChatColor.red),
                                                                 e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        private static string GetFormattedMessage(string message)
        {
            return string.Format("[{0}]<Pandaros => Settlers> {1}", DateTime.Now, message);
        }
    }
}