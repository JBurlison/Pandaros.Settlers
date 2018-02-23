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

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class ZombieKing : Zombie, IPandaBoss
    {
        private float _totalHealth = 10000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing";
        static NPCTypeMonsterSettings _mts;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "ZombieKing")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/ZombieKing.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/ZombieQueen.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/ZombieQueen.png")
                .SetAs("initialHealth", 10000)
                .SetAs("movementSpeed", 1.5f)
                .SetAs("punchCooldownMS", 1000)
                .SetAs("punchDamage", 80);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new ZombieKing(path, p);
        }

        public string AnnouncementText => "YOU WILL DO MY BIDDING!";
        public string DeathText => "UGH Help me you useless bags of meat......";

        public string Name => "ZombieKing";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1.10f;
        
        public Dictionary<ushort, int> KillRewards => REWARDS;
        public float ZombieHPBonus => 200;

        public ZombieKing(Path path, Players.Player originalGoal) :
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
            killedBefore = false;
            return base.Update();
        }
    }
}
