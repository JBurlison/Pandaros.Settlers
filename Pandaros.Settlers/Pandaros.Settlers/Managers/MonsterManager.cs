using Pandaros.Settlers.Entities;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class MonsterManager
    {
        static double _nextUpdateTime;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (_nextUpdateTime < Pipliz.Time.SecondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in GetAllMonsters())
                {
                    if (m == null || (Vector3.Distance(monster.Value.Position, m.Position) > 10 && Pipliz.Random.NextBool()))
                    {
                        m = monster.Value;
                        ServerManager.SendAudio(monster.Value.Position, GameLoader.NAMESPACE + "ZombieAudio");
                    }
                }

                _nextUpdateTime = Pipliz.Time.SecondsSinceStartDouble + 5;
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Managers.MonsterManager.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "ZombieAudio", new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie1.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie2.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie3.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie4.ogg",
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, Pipliz.Box<float> box)
        {
            if (box.item1 > 0)
            {
                var state = PlayerState.GetPlayerState(player);
                box.Set(box.item1 + state.Difficulty.MonsterDamage);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPC.NPCBase npc, Pipliz.Box<float> box)
        {
            var state = PlayerState.GetPlayerState(npc.Colony.Owner);
            box.Set(box.item1 + state.Difficulty.MonsterDamage);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, Pipliz.Box<float> box)
        {
            var ps = PlayerState.GetPlayerState(monster.OriginalGoal);
            box.Set(box.item1 - (box.item1 * ps.Difficulty.MonsterDamageReduction));
            ServerManager.SendAudio(monster.Position, GameLoader.NAMESPACE + "ZombieAudio");
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}
