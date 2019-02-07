using Jobs;
using Monsters;
using NPC;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public static class ColonyArchive
    {
        public static int _idNext = 1;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedEarly, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedEarly")]
        public static void OnPlayerConnectedEarly(Players.Player p)
        {
            if (p.IsConnected && !Configuration.OfflineColonies)
            {
                foreach (Colony c in p.Colonies)
                {
                    if (c.Owners.Count(own => own.IsConnected) == 1)
                    {
                        var file = $"{GameLoader.GAMEDATA_FOLDER}/savegames/{ServerManager.WorldName}/NPCArchive/{c.ColonyID}.json";

                        if (File.Exists(file) && JSON.Deserialize(file, out var followersNode, false))
                        {
                            File.Delete(file);
                            PandaLogger.Log(ChatColor.cyan, $"Player {p.ID.steamID} is reconnected. Restoring Colony.");

                            foreach (var node in followersNode.LoopArray())
                                try
                                {
                                    node.SetAs("id", GetAIID());

                                    var npc = new NPCBase(c, node);
                                    c.RegisterNPC(npc);
                                    NPCTracker.Add(npc);
                                    ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCLoaded, npc, node);

                                    foreach (var job in new List<IJob>(c.JobFinder.JobsData.OpenJobs))
                                        if (node.TryGetAs("JobPoS", out JSONNode pos) && job.GetJobLocation() == (Vector3Int)pos)
                                        {
                                            if (job.IsValid && job.NeedsNPC)
                                            {
                                                npc.TakeJob(job);
                                                c.JobFinder.Remove(job);
                                            }

                                            break;
                                        }
                                }
                                catch (Exception ex)
                                {
                                    PandaLogger.LogError(ex);
                                }

                            JSON.Serialize(file, new JSONNode(NodeType.Array));
                            c.JobFinder.Update();
                            c.SendCommonData();
                        }
                    }
                }
            }
        }

        private static int GetAIID()
        {
            while (true)
            {
                if (_idNext == 1000000000) _idNext = 1;

                if (!NPCTracker.Contains(_idNext)) break;
                _idNext++;
            }

            return _idNext++;
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            if (p == null || p.Colonies == null || p.Colonies.Length == 0)
                return;

            foreach (Colony c in p.Colonies)
                SaveOffline(c);
        }

        public static void SaveOffline(Colony colony)
        {
            if (colony.OwnerIsOnline())
                return;

            try
            {
                var folder = $"{GameLoader.GAMEDATA_FOLDER}/savegames/{ServerManager.WorldName}/NPCArchive/";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var file = $"{folder}{colony.ColonyID}.json";

                if (!Configuration.OfflineColonies)
                {
                    if (!JSON.Deserialize(file, out var followers, false))
                        followers = new JSONNode(NodeType.Array);

                    followers.ClearChildren();

                    PandaLogger.Log(ChatColor.cyan, $"All players from {colony.ColonyID} have disconnected. Clearing colony until reconnect.");

                    var copyOfFollowers = new List<NPCBase>();

                    foreach (var follower in colony.Followers)
                    {
                        JSONNode jobloc = null;

                        if (follower.IsValid)
                        {
                            var job = follower.Job;

                            if (job != null && job.GetJobLocation() != Vector3Int.invalidPos)
                            {
                                jobloc = (JSONNode)job.GetJobLocation();
                                job.SetNPC(null);
                                follower.ClearJob();
                            }
                        }

                        if (follower.TryGetJSON(out var node))
                        {
                            if (jobloc != null)
                                node.SetAs("JobPoS", jobloc);

                            ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCSaved, follower, node);
                            followers.AddToArray(node);
                            copyOfFollowers.Add(follower);
                        }
                    }

                    JSON.Serialize(file, followers);

                    foreach (var deadMan in copyOfFollowers)
                        deadMan.OnDeath();

                    colony.ForEachOwner(o => MonsterTracker.KillAllZombies(o));
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }
    }
}
