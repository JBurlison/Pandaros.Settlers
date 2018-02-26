using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class PutridCorpse : Zombie, IPandaBoss
    {
        int _nextBossUpdateTime = int.MinValue;
        private float _totalHealth = 10000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse";
        static NPCTypeMonsterSettings _mts;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "PutridCorpse")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/PutridCorpse.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Hoarder.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/Hoarder.png")
                .SetAs("initialHealth", 10000)
                .SetAs("movementSpeed", .75f)
                .SetAs("punchCooldownMS", 3000)
                .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new PutridCorpse(path, p);
        }

        public string AnnouncementText => "Hehehe Smell that?!?!?! Come a little closer...";
        public string DeathText => "ffffffaaarrt....";

        public string Name => "Putrid Corpse";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 20;
        public Dictionary<ushort, int> KillRewards => REWARDS;

        public PutridCorpse(Path path, Players.Player originalGoal) :
            base (NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            Colony c = Colony.Get(originalGoal);
            var ps = PlayerState.GetPlayerState(originalGoal);
            var hp = c.FollowerCount * ps.Difficulty.BossHPPerColonist;

            if (hp < _totalHealth)
                _totalHealth = hp;

            health = _totalHealth;
        }

        public override bool Update()
        {
            if (_nextBossUpdateTime < Pipliz.Time.SecondsSinceStartInt)
            {
                Colony c = Colony.Get(originalGoal);

                foreach (var follower in c.Followers)
                {
                    float dis = Vector3.Distance(Position, follower.Position.Vector);

                    if (dis <= 20)
                        follower.OnHit(10);
                }

                if (Vector3.Distance(Position, originalGoal.Position) <= 20)
                    Players.TakeHit(originalGoal, 10, true);

                _nextBossUpdateTime = Pipliz.Time.SecondsSinceStartInt + 5;
            }


            killedBefore = false;
            return base.Update();
        }
    }
}
