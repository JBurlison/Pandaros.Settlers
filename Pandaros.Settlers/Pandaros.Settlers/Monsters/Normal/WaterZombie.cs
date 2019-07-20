﻿using AI;
using Monsters;
using NPC;
using Pandaros.API;
using Pandaros.API.Monsters;
using Pipliz.JSON;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters.Normal
{
    [ModLoader.ModManager]
    public class WaterZombie : Zombie, IPandaZombie
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.WaterZombie";
        private static NPCTypeMonsterSettings _mts;
        private double _cooldown = 2;
        private float _totalHealth = 600;

        public WaterZombie() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public WaterZombie(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new WaterZombie(path, p);
        }


        public string name => "WaterZombie";
        public override float TotalHealth => _totalHealth;
        public int MinColonists => 150;
        public bool KilledBefore
        {
            get => false;
            set => killedBefore = value;
        }

        public float ZombieHPBonus => 0;
        public float MissChance => 0.1f;
        public string MosterType => "PandaZombie";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Water, 50f}
        };

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
             {DamageType.Water, 0.10f},
             {DamageType.Physical, 0.15f}
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Monsters.Normal.WaterZombie.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Water Zombie")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "Water_monster.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "Juggernaut_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "Juggernaut_emissive.png")
                    .SetAs("initialHealth", 600)
                    .SetAs("movementSpeed", 1f)
                    .SetAs("punchCooldownMS", 2000)
                    .SetAs("punchDamage", 50);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}