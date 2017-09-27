using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    internal class PandaLogger
    {
        internal static void Log(ChatColor color, string message, params object[] args)
        {
            ServerLog.LogAsyncMessage(new Pipliz.LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), color), UnityEngine.LogType.Log));
        }

        internal static void Log(string message, params object[] args)
        {
            ServerLog.LogAsyncMessage(new Pipliz.LogMessage(GetFormattedMessage(string.Format(message, args)), UnityEngine.LogType.Log));
        }

        internal static void Log(string message)
        {
            ServerLog.LogAsyncMessage(new Pipliz.LogMessage(GetFormattedMessage(message), UnityEngine.LogType.Log));
        }

        internal static void LogError(Exception e, string message)
        {
            ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(message), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e, string message, params object[] args)
        {
            ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(message), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        internal static void LogError(Exception e)
        {
            ServerLog.LogAsyncExceptionMessage(new Pipliz.LogExceptionMessage(PandaChat.BuildMessage("Exception", ChatColor.red), e));

            if(e.InnerException != null)
                LogError(e.InnerException);
        }

        private static string GetFormattedMessage(string message)
        {
            return string.Format("[{0}]<Pandaros => Settlers> {1}", DateTime.Now, message);
        }
    }
}
