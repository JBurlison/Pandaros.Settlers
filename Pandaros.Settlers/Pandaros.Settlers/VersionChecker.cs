using ChatCommands;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Pandaros.Settlers
{
    public static class VersionChecker
    {
        internal static bool NewVer = false;
        const string GIT_URL = "http://download.settlersmod.com/";
        const string NAME = "\"name\": \"";
        const string ASSETS = "\"assets\":";
        const string ZIP = "\"browser_download_url\": \"";
        const int HOUR = 3600000;
        public const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
        
        static VersionChecker()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            ServicePointManager.SecurityProtocol = Tls12;

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
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            ServicePointManager.SecurityProtocol = Tls12;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    // Added user agent
                    webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
                    webClient.Headers["Content-Type"] = "text";
                    webClient.Headers[HttpRequestHeader.Authorization] = "Token 7b3d1cd9d45be2182e334f7a2fa73ff4c10868cc";
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
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;

                WebClient webClient = new WebClient();
                webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
                webClient.Headers["Content-Type"] = "text";
                webClient.Headers[HttpRequestHeader.Authorization] = "Token 7b3d1cd9d45be2182e334f7a2fa73ff4c10868cc";
                webClient.DownloadFileCompleted += (s, e) =>
                {
                    if (!NewVer)
                    {
                        NewVer = true;
                        bool error = false;

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
                                FastZip fastZip = new FastZip();
                                fastZip.ExtractZip(newVer, GameLoader.MODS_FOLDER, null);
                            }
                            catch (Exception ex)
                            {
                                error = true;

                                if (Directory.Exists(bkFolder))
                                    Directory.Move(bkFolder, GameLoader.MODS_FOLDER + "/Pandaros");

                                PandaLogger.LogError(ex);
                                PandaLogger.Log(ChatColor.red, $"There was an error updating to the latest version of Settlers!");
                            }

                            if (!error)
                            {
                                PandaLogger.Log(ChatColor.green, $"Settlers! update {gitVer} installed. Restart to update!");
                                PandaChat.SendToAll($"Settlers! update {gitVer} installed. Restart server to update!", ChatColor.maroon, ChatStyle.bolditalic);
                            }

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
            return chat.StartsWith("/settlersversion", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            string[] array = CommandManager.SplitCommand(chat);

            var gitVer = VersionChecker.GetGitVerion();
            var versionCompare = GameLoader.MOD_VER.Major.CompareTo(gitVer.Major);

            PandaChat.Send(player, "Settlers! Mod version: {0}.", ChatColor.green, GameLoader.MOD_VER.ToString());
            PandaChat.Send(player, "Settlers! Git version: {0}.", ChatColor.green, gitVer.ToString());

            if (versionCompare < 0)
            {
                if (!VersionChecker.NewVer)
                {
                    PandaChat.Send(player, "Settlers! version is out of date. The mod will automatically update now.", ChatColor.red);
                    VersionChecker.WriteVersionsToConsole();
                }
                else
                    PandaChat.Send(player, "Settlers! Has been updated. Restart the server/game to apply.", ChatColor.red);
            }
 
            return true;
        }
    }
   
}
