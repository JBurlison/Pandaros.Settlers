using ChatCommands;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Pandaros.Settlers
{
    public static class VersionChecker
    {
        private static bool _newVer = false;
        const string GIT_URL = "https://api.github.com/repos/JBurlison/Pandaros.Settlers/releases";
        const string NAME = "\"name\": \"";
        const string ASSETS = "\"assets\":";
        const string ZIP = "\"browser_download_url\": \"";
        const int HOUR = 3600000;

        static VersionChecker()
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                while(GameLoader.RUNNING)
                {
                    System.Threading.Thread.Sleep(HOUR);
                    WriteVersionsToConsole();
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static string GetReleases()
        {
            string releases = null;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    // Added user agent
                    webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.2.6) Gecko/20100625 Firefox/3.6.6 (.NET CLR 3.5.30729)";

                    using (Stream data = webClient.OpenRead(GIT_URL))
                    using (StreamReader reader = new StreamReader(data))
                        releases = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            return releases;
        }

        /// <summary>
        /// Certificate validation callback.
        /// </summary>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        public static Version GetGitVerion()
        {
            Version version = null;
            var releases = GetReleases();

            if (!string.IsNullOrEmpty(releases))
            {
                var iName = releases.IndexOf(NAME);
                var nameSub = releases.Substring(iName + NAME.Length);
                var iEndName = nameSub.IndexOf("\"");
                var verString = nameSub.Substring(0, iEndName);

                PandaLogger.Log(verString);
                version = new Version(verString);
            }

            return version;
        }

        public static void WriteVersionsToConsole()
        {
            var gitVer = GetGitVerion();
            var bkFolder = GameLoader.GAMEDATA_FOLDER + "Pandaros.bk";

            PandaLogger.Log(ChatColor.green, "Mod version: {0}.", GameLoader.MOD_VER.ToString());
            PandaLogger.Log(ChatColor.green, "Git version: {0}.", gitVer.ToString());

            var versionCompare = GameLoader.MOD_VER.CompareTo(gitVer);

            if (versionCompare < 0)
            {
                PandaLogger.Log(ChatColor.red, "Settlers! version is out of date. Downloading new version from: {0}", VersionChecker.GIT_URL);

                var releases = GetReleases();
                var iName = releases.IndexOf(ASSETS);
                var nameSub = releases.Substring(iName + ASSETS.Length);
                var zip = releases.IndexOf(ZIP);
                var zipSub = releases.Substring(zip + ZIP.Length);
                var iEndName = zipSub.IndexOf("\"");
                var verString = zipSub.Substring(0, iEndName);
                var newVer = GameLoader.MODS_FOLDER + $"/{gitVer}.zip";
                var oldVer = GameLoader.MODS_FOLDER + $"/{GameLoader.MOD_VER}.zip";

                PandaLogger.Log(verString);

                WebClient webClient = new WebClient();
                webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.2.6) Gecko/20100625 Firefox/3.6.6 (.NET CLR 3.5.30729)";
                webClient.DownloadFileCompleted += (s, e) =>
                {
                    if (!_newVer)
                    {
                        _newVer = true;

                        try
                        {
                            if (Directory.Exists(bkFolder))
                                Directory.Delete(bkFolder, true);

                            PandaLogger.Log(ChatColor.green, $"Settlers! update {gitVer} downloaded. Making a backup..");
                            Directory.Move(GameLoader.MODS_FOLDER + "/Pandaros", bkFolder);

                            if (File.Exists(oldVer))
                                File.Delete(oldVer);

                            PandaLogger.Log(ChatColor.green, $"Installing...");

                            try
                            {
                                using (ZipFile zf = ZipFile.Read(newVer))
                                    zf.ExtractAll(GameLoader.MODS_FOLDER);
                            }
                            catch (Exception ex)
                            {
                                if (Directory.Exists(bkFolder))
                                    Directory.Move(bkFolder, GameLoader.MODS_FOLDER + "/Pandaros");

                                PandaLogger.LogError(ex);
                            }

                            PandaLogger.Log(ChatColor.green, $"Settlers! update {gitVer} installed. Restart to update!");
                        }
                        catch (Exception ex)
                        {
                            if (Directory.Exists(bkFolder))
                                Directory.Move(bkFolder, GameLoader.MODS_FOLDER + "/Pandaros");

                            PandaLogger.LogError(ex);
                        }

                        if (File.Exists(newVer))
                            File.Delete(newVer);
                    }
                };


                    webClient.DownloadFileAsync(new Uri(verString), newVer);
            }
        }

    }

    public class VersionChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/settlers", StringComparison.OrdinalIgnoreCase) || chat.StartsWith("/settlers", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);

            var gitVer = VersionChecker.GetGitVerion();
            var versionCompare = GameLoader.MOD_VER.Major.CompareTo(gitVer.Major);

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Mod version: {0}.", ChatColor.green, GameLoader.MOD_VER.ToString());
                PandaChat.Send(player, "Settlers! Git version: {0}.", ChatColor.green, gitVer.ToString());

                if (versionCompare < 0)
                {
                    PandaChat.Send(player, "Settlers! Settlers version is out of date. Please update at: https://github.com/JBurlison/Pandaros.Settlers/releases", ChatColor.red);
                }
            }
            

            return true;
        }
    }
   
}
