using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pipliz;
using UnityEngine;

namespace Pandaros.Settlers
{
    /// <summary>
    /// Actions that apply to system files.
    /// </summary>
    public enum FileAction : int
    {
        /// <summary>
        /// A command to move a file.
        /// </summary>
        Move = 0,
        /// <summary>
        /// A command to delete a file.
        /// </summary>
        Delete = 1
    }

    /// <summary>
    /// Look for file in the format xxxx.99999.log
    /// The number is extracted from both filenames and this number is used for the comparison
    /// </summary>
    public class LogFileSorter : Comparer<string>
    {
        public override int Compare(string x, string y)
        {

            if (string.IsNullOrEmpty(x))
            {
                if (string.IsNullOrEmpty(y))
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(y))
                {
                    return 1;
                }
                else
                {
                    int xval = PandaLogger.GetNumFromFilename(x);
                    int yval = PandaLogger.GetNumFromFilename(y);
                    return ((new CaseInsensitiveComparer()).Compare(xval, yval));
                }
            }
        }
    }

    public static class PandaLogger
    {
        public static readonly string LOG_DIR;
        const string LOG_NAME = "Pandalog";
        const string ONE_DOT_LOG = ".1.log";
        const string DOT_STAR_DOT_LOG = ".*.log";
        const string DOT_LOG = ".log";
        static readonly char[] dot = new char[] { '.' };
        const int LOGGER_TRY = 1000; 
        static Thread _thread = new Thread(new ThreadStart(Log));
        static Queue<string> _logQueue = new Queue<string>();
        static AutoResetEvent _loggerSemaphore = new AutoResetEvent(false);
        static string _logFile;

        static PandaLogger()
        {
            LOG_DIR = GameLoader.GAME_ROOT + "Logs/" + GameLoader.NAMESPACE + "/";

            if (!Directory.Exists(LOG_DIR))
                Directory.CreateDirectory(LOG_DIR);

            _logFile = LOG_DIR + LOG_NAME + DOT_LOG;
            _thread.IsBackground = true;
            _thread.Start();
        }

        public static void Log(ChatColor color, string message, params object[] args)
        {
            if (args != null && args.Length != 0)
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), color), LogType.Log));
            else
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(message), color), LogType.Log));
        }

        public static void Log(string message, params object[] args)
        {
            if (args != null && args.Length != 0)
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args))), LogType.Log));
            else
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(message)), LogType.Log));
        }

        public static void Log(string message)
        {
            ServerLog.LogAsyncMessage(new LogMessage(GetFormattedMessage(message), LogType.Log));
        }

        public static void LogError(Exception e, string message)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(message), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        public static void LogError(Exception e, string message, params object[] args)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        public static void LogError(Exception e)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage("Exception", ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        private static string GetFormattedMessage(string message)
        {
            message = string.Format("[{0}]<Pandaros => Settlers> {1}", DateTime.Now, message);
            _logQueue.Enqueue(message);
            _loggerSemaphore.Set();
            return message;
        }

        private static void Log()
        {
            while (true)
            {
                _loggerSemaphore.WaitOne(2000);

                using (var sw = new StreamWriter(_logFile, true))
                    while (_logQueue.Count != 0)
                    {
                        var queuedMessage = _logQueue.Dequeue();

                        if (!string.IsNullOrEmpty(queuedMessage))
                        {
                            try
                            {

                                sw.WriteLine(queuedMessage);
                            }
                            finally
                            {
                                RotateLogs();
                            }
                        }
                    }
            }
        }

        private static void RotateLogs()
        {
            if (!File.Exists(_logFile)) return;

            FileInfo fix = new FileInfo(_logFile);
            long fixedSize = fix.Length / 1024L; 
            if (fixedSize >= 10240)
            {
                string oldFilename = LOG_DIR + LOG_NAME + ONE_DOT_LOG;
                if (File.Exists(oldFilename))
                { 
                    RotateOld(LOG_DIR);
                }
                int j = 0;
                while (!LoggerCheck(_logFile, oldFilename, FileAction.Move))
                {
                    j++;
                    if (j > LOGGER_TRY) break;
                }
            }
        }

        private static void RotateOld(string logFileDir)
        {
            string[] raw = Directory.GetFiles(logFileDir, LOG_NAME + DOT_STAR_DOT_LOG);
            ArrayList files = new ArrayList();
            files.AddRange(raw);

            Comparer<string> myComparer = new LogFileSorter();
            files.Sort(myComparer);
            files.Reverse();

            foreach (string f in files)
            {
                int logfnum = GetNumFromFilename(f);
                if (logfnum > 0)
                {
                    string newname = string.Format("{0}{1}.{2}.log", LOG_DIR, LOG_NAME, logfnum + 1);

                    if (logfnum >= 5)
                    {
                        int j = 0;
                        while (!LoggerCheck(f, string.Empty, FileAction.Delete))
                        {
                            j++;
                            if (j > LOGGER_TRY) break;
                        }
                    }
                    else
                    {
                        int j = 0;
                        while (!LoggerCheck(f, newname, FileAction.Move))
                        {
                            j++;
                            if (j > LOGGER_TRY) break;
                        }
                    }
                }
            }
        }

        private static bool LoggerCheck(string f, string oldFilename, FileAction action)
        {
            if (File.Exists(f))
            {
                if (action == FileAction.Move)
                {
                    try
                    {
                        if (File.Exists(oldFilename))
                            File.Delete(oldFilename);
                        File.Move(f, oldFilename);
                        return true;
                    }
                    catch { }

                }
                else if (action == FileAction.Delete)
                {
                    try
                    {
                        File.Delete(f);
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        /// <summary>
        /// Return the log number for a file in the format xxxx.9999.log
        /// If there is an error return 0
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static int GetNumFromFilename(string filename)
        {
            int filenum = 0;
            if (!string.IsNullOrEmpty(filename))
            {
                string[] xtokens = filename.Split(dot);
                int xtokencount = xtokens.GetLength(0);
                if (xtokencount > 2)
                {
                    int xval = 0;
                    if (Int32.TryParse(xtokens[xtokencount - 2], out xval))
                    {
                        filenum = xval;
                    }
                }
            }
            return filenum;
        }
    }
}