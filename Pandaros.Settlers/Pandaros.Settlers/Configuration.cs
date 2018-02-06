using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class Configuration
    {
        public static GameDifficulty MinDifficulty { get; private set; } = GameDifficulty.Normal;
        public static GameDifficulty DefaultDifficulty { get; private set; } = GameDifficulty.Medium;
        public static bool DifficutlyCanBeChanged { get; private set; } = true;
        public static bool OfflineColonies { get; set; } = true;


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameLoader.NAMESPACE + ".Configuration.OnAssemblyLoaded"),
            ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".OnAssemblyLoaded")]
        public static void AfterStartup(string path)
        {
            var fileName = GameLoader.MOD_FOLDER + "/Configuration.json";

            if (File.Exists(fileName) && JSON.Deserialize(fileName, out var config))
            {
                if (config.TryGetAs(nameof(MinDifficulty), out string diffStr))
                    if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                        MinDifficulty = GameDifficulty.GameDifficulties[diffStr];

                if (config.TryGetAs(nameof(DefaultDifficulty), out string defdiffStr))
                    if (GameDifficulty.GameDifficulties.ContainsKey(defdiffStr))
                        DefaultDifficulty = GameDifficulty.GameDifficulties[defdiffStr];

                if (config.TryGetAs(nameof(DifficutlyCanBeChanged), out bool diffChanged))
                    DifficutlyCanBeChanged = diffChanged;

                if (config.TryGetAs(nameof(OfflineColonies), out bool offlineColonies))
                    OfflineColonies = offlineColonies;

                if (config.TryGetAs("GameDifficulties", out JSONNode diffs))
                {
                    foreach (var diff in diffs.LoopArray())
                    {
                        var newDiff = new GameDifficulty(diff);
                        GameDifficulty.GameDifficulties[newDiff.Name] = newDiff; 
                    }
                }
            }
            else
            {
                JSONNode newConfig = new JSONNode();
                newConfig.SetAs(nameof(MinDifficulty), MinDifficulty.Name);
                newConfig.SetAs(nameof(DifficutlyCanBeChanged), DifficutlyCanBeChanged);
                newConfig.SetAs(nameof(OfflineColonies), OfflineColonies);
                newConfig.SetAs(nameof(DefaultDifficulty), DefaultDifficulty);

                JSONNode diffs = new JSONNode(NodeType.Array);

                foreach (var diff in GameDifficulty.GameDifficulties.Values)
                    diffs.AddToArray(diff.ToJson());

                newConfig.SetAs("GameDifficulties", diffs);

                using (StreamWriter writer = File.CreateText(fileName))
                    newConfig.Serialize(writer, 1, 1);
            }
        }
    }
}
