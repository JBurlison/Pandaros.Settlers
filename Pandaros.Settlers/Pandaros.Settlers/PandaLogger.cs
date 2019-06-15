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
                    int xval = CSConsoleAndFileLogger.GetNumFromFilename(x);
                    int yval = CSConsoleAndFileLogger.GetNumFromFilename(y);
                    return ((new CaseInsensitiveComparer()).Compare(xval, yval));
                }
            }
        }
    }

    public static class PandaLogger
    {
        private static CSConsoleAndFileLogger _logger = new CSConsoleAndFileLogger(GameLoader.NAMESPACE, "PandaLog", "<Panaros => Settlers>");

        public static void LogToFile(string message, params object[] args)
        {
            _logger.LogToFile(message, args);
        }

        public static void Log(ChatColor color, string message, params object[] args)
        {
            _logger.Log(color, message, args);
        }

        public static void Log(string message, params object[] args)
        {
            _logger.Log(message, args);
        }

        public static void Log(string message)
        {
            _logger.Log(message);
        }

        public static void LogError(Exception e, string message)
        {
            _logger.LogError(e, message);
        }

        public static void LogError(Exception e, string message, params object[] args)
        {
            _logger.LogError(e, message, args);
        }

        public static void LogError(Exception e)
        {
            _logger.LogError(e);
        }
    }

    public class CSConsoleAndFileLogger
    {
        public readonly string LOG_DIR;
        string LOG_NAME;
        const string ONE_DOT_LOG = ".1.log";
        const string DOT_STAR_DOT_LOG = ".*.log";
        const string DOT_LOG = ".log";
        static readonly char[] dot = new char[] { '.' };
        const int LOGGER_TRY = 1000;
        Thread _thread;
        Queue<string> _logQueue = new Queue<string>();
        AutoResetEvent _loggerSemaphore = new AutoResetEvent(false);
        string _logFile;
        string _consolePrefix;

        public CSConsoleAndFileLogger(string namespaceStr, string logName, string consolePrefix)
        {
            LOG_DIR = GameLoader.GAME_ROOT + "Logs/" + namespaceStr + "/";
            _consolePrefix = consolePrefix;

            if (!Directory.Exists(LOG_DIR))
                Directory.CreateDirectory(LOG_DIR);

            LOG_NAME = logName + "." + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            _logFile = LOG_DIR + LOG_NAME + DOT_LOG;
            ServerLog.LogAsyncMessage(new LogMessage("Settlers Log file set to: " + _logFile, LogType.Log));
            _thread = new Thread(new ThreadStart(Log));
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void LogToFile(string message, params object[] args)
        {
            if (args != null && args.Length != 0)
                GetFormattedMessage(string.Format(message, args));
            else
                GetFormattedMessage(message);
        }

        public void Log(ChatColor color, string message, params object[] args)
        {
            if (args != null && args.Length != 0)
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), color), LogType.Log));
            else
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(message), color), LogType.Log));
        }

        public void Log(string message, params object[] args)
        {
            if (args != null && args.Length != 0)
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args))), LogType.Log));
            else
                ServerLog.LogAsyncMessage(new LogMessage(PandaChat.BuildMessage(GetFormattedMessage(message)), LogType.Log));
        }

        public void Log(string message)
        {
            ServerLog.LogAsyncMessage(new LogMessage(GetFormattedMessage(message), LogType.Log));
        }

        public void LogError(Exception e, string message)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(message), ChatColor.red), e));

            LogError(e);

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        public void LogError(Exception e, string message, params object[] args)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage(GetFormattedMessage(string.Format(message, args)), ChatColor.red), e));

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        public void LogError(Exception e)
        {
            ServerLog.LogAsyncExceptionMessage(new LogExceptionMessage(PandaChat.BuildMessage("Exception", ChatColor.red), e));

            lock (_logQueue)
            {
                _logQueue.Enqueue(e.Message);
                _logQueue.Enqueue(e.StackTrace);
            }
            _loggerSemaphore.Set();

            if (e.InnerException != null)
                LogError(e.InnerException);
        }

        private string GetFormattedMessage(string message)
        {
            message = string.Format("[{0}]{1} {2}", DateTime.Now, _consolePrefix, message);

            lock(_logQueue)
                _logQueue.Enqueue(message);
            _loggerSemaphore.Set();
            return message;
        }

        private void Log()
        {
            while (true)
            {
                _loggerSemaphore.WaitOne(2000);

                using (var sw = new StreamWriter(_logFile, true))
                    while (_logQueue.Count != 0)
                    {
                        var queuedMessage = string.Empty;

                        lock (_logQueue)
                            queuedMessage = _logQueue.Dequeue();

                        if (!string.IsNullOrEmpty(queuedMessage))
                        {
                            try
                            {

                                sw.WriteLine(queuedMessage);
                            }
                            finally
                            {

                            }
                        }
                    }

                RotateLogs();
            }
        }

        private void RotateLogs()
        {
            if (!File.Exists(_logFile)) return;

            FileInfo fix = new FileInfo(_logFile);
            long fixedSize = fix.Length / 1024L; 
            if (fixedSize >= 1024)
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

        private void RotateOld(string logFileDir)
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

        private bool LoggerCheck(string f, string oldFilename, FileAction action)
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