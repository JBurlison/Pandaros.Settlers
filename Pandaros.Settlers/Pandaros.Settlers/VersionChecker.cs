using Chatting;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading;

namespace Pandaros.Settlers
{
    public static class VersionChecker
    {
        private const string GIT_URL = "https://api.github.com/repos/JBurlison/Pandaros.Settlers/releases";
        private const int HOUR = 3600000;
        public const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
        internal static bool NewVer;

        static VersionChecker()
        {
            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var t = new Thread(() =>
            {
                while (GameLoader.RUNNING)
                {
                    Thread.Sleep(HOUR);
                    WriteVersionsToConsole();
                }
            });

            t.IsBackground = true;
            t.Start();
        }

        public static string GetReleases()
        {
            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string releases = null;
            try
            {
                using (var webClient = new WebClient())
                {
                    // Added user agent
                    webClient.Headers["User-Agent"] =
                        "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

                    webClient.Headers["Content-Type"] = "text";

                    using (var data = webClient.OpenRead(GIT_URL))
                    using (var reader = new StreamReader(data))
                    {
                        releases = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.Log(ChatColor.yellow, "There was a error getting the server version to check for an update. Unable to check if there is an update to the mod.");
                PandaLogger.LogToFile(ex.Message);
            }

            return releases;
        }

        public static Version GetGitVerion()
        {
            Version version = null;
            var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(GetReleases());
            GithubRelease githubRelease = null;

            if (releases != null)
            {
                foreach (var release in  releases)
                {
                    if (release.prerelease)
                        continue;

                    githubRelease = release;
                    break;
                }

                PandaLogger.Log("Current Github Version: " + githubRelease.name);
                version = new Version(githubRelease.name);
            }

            return version;
        }

        public static void WriteVersionsToConsole()
        {
            try
            {
                var gitVer = GetGitVerion();
                var bkFolder = GameLoader.GAMEDATA_FOLDER + "Pandaros.bk";

                PandaLogger.Log(ChatColor.green, "Mod version: {0}.", GameLoader.MOD_VER.ToString());
                PandaLogger.Log(ChatColor.green, "Git version: {0}.", gitVer.ToString());

                var versionCompare = GameLoader.MOD_VER.CompareTo(gitVer);

                if (versionCompare < 0)
                {
                    PandaLogger.Log(ChatColor.red, "Settlers! version is out of date. Downloading new version from: {0}",
                                    GIT_URL);

                    var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(GetReleases());
                    GithubRelease githubRelease = null;

                    if (releases != null)
                    {
                        foreach (var release in releases)
                        {
                            if (release.prerelease)
                                continue;

                            githubRelease = release;
                            break;
                        }
                    }

                    var newVer = GameLoader.MODS_FOLDER + $"/{gitVer}.zip";
                    var oldVer = GameLoader.MODS_FOLDER + $"/{GameLoader.MOD_VER}.zip";
                    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;

                    var webClient = new WebClient();

                    webClient.Headers["User-Agent"] =
                        "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

                    webClient.Headers["Content-Type"] = "text";

                    webClient.DownloadFileCompleted += (s, e) =>
                    {
                        if (!NewVer)
                        {
                            NewVer = true;
                            var error = false;

                            try
                            {
                                if (Directory.Exists(bkFolder))
                                    Directory.Delete(bkFolder, true);

                                PandaLogger.Log(ChatColor.green,
                                                $"Settlers! update {gitVer} downloaded. Making a backup..");

                                Directory.Move(GameLoader.MODS_FOLDER + "/Pandaros", bkFolder);

                                if (File.Exists(oldVer))
                                    File.Delete(oldVer);

                                PandaLogger.Log(ChatColor.green, $"Installing...");

                                try
                                {
                                    var fastZip = new FastZip();
                                    fastZip.ExtractZip(newVer, GameLoader.MODS_FOLDER, null);
                                }
                                catch (Exception ex)
                                {
                                    error = true;

                                    if (Directory.Exists(bkFolder))
                                        Directory.Move(bkFolder, GameLoader.MODS_FOLDER + "/Pandaros");

                                    PandaLogger.LogError(ex);

                                    PandaLogger.Log(ChatColor.red,
                                                    $"There was an error updating to the latest version of Settlers!");
                                }

                                if (!error)
                                {
                                    PandaLogger.Log(ChatColor.green,
                                                    $"Settlers! update {gitVer} installed. Restart to update!");

                                    PandaChat.SendToAll($"Settlers! update {gitVer} installed. Restart server to update!",
                                                        ChatColor.maroon, ChatStyle.bolditalic);
                                }
                            }
                            catch (Exception)
                            {
                                if (Directory.Exists(bkFolder))
                                    Directory.Move(bkFolder, GameLoader.MODS_FOLDER + "/Pandaros");
                            }

                            if (File.Exists(newVer))
                                File.Delete(newVer);
                        }
                    };


                    webClient.DownloadFileAsync(new Uri(githubRelease.assets[0].browser_download_url), newVer);
                }
            }
            catch (Exception) { }
        }
    }

    public class VersionChatCommand : IChatCommand
    {

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/settlersversion", StringComparison.OrdinalIgnoreCase))
                return false;

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);
            var gitVer = VersionChecker.GetGitVerion();
            var versionCompare = GameLoader.MOD_VER.Major.CompareTo(gitVer.Major);

            PandaChat.Send(player, "Settlers! Mod version: {0}.", ChatColor.green, GameLoader.MOD_VER.ToString());
            PandaChat.Send(player, "Settlers! Git version: {0}.", ChatColor.green, gitVer.ToString());

            if (versionCompare < 0)
            {
                if (!VersionChecker.NewVer)
                {
                    PandaChat.Send(player, "Settlers! version is out of date. The mod will automatically update now.",
                                   ChatColor.red);

                    VersionChecker.WriteVersionsToConsole();
                }
                else
                {
                    PandaChat.Send(player, "Settlers! Has been updated. Restart the server/game to apply.",
                                   ChatColor.red);
                }
            }

            return true;
        }
    }



    public class GithubRelease
    {
        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public bool draft { get; set; }
        public Author author { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public Asset[] assets { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public string body { get; set; }
    }

    public class Author
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    public class Asset
    {
        public string url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string name { get; set; }
        public object label { get; set; }
        public Uploader uploader { get; set; }
        public string content_type { get; set; }
        public string state { get; set; }
        public int size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string browser_download_url { get; set; }
    }

    public class Uploader
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

}