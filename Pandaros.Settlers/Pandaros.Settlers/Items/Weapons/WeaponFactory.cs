using System.Collections.Generic;
using System.Linq;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.Mods.APIProvider.Jobs;
using Pipliz.Mods.BaseGame.BlockNPCs;
using Server.Monsters;
using Shared;

namespace Pandaros.Settlers.Items.Weapons
{
    [ModLoader.ModManager]
    public static class WeaponFactory
    {
        public static List<GuardBaseJob.GuardSettings> WeaponGuardSettings = new List<GuardBaseJob.GuardSettings>();

        public static Dictionary<ushort, IWeapon> WeaponLookup { get; } =  new Dictionary<ushort, IWeapon>();

        public static void RefreshGuardSettings()
        {
            if (!WeaponGuardSettings.Contains(GuardBowJobDay.GetGuardSettings()))
                WeaponGuardSettings.Add(GuardBowJobDay.GetGuardSettings());

            if (!WeaponGuardSettings.Contains(GuardCrossbowJobDay.GetGuardSettings()))
                WeaponGuardSettings.Add(GuardCrossbowJobDay.GetGuardSettings());

            if (!WeaponGuardSettings.Contains(GuardMatchlockJobDay.GetGuardSettings()))
                WeaponGuardSettings.Add(GuardMatchlockJobDay.GetGuardSettings());

            if (!WeaponGuardSettings.Contains(GuardSlingerJobDay.GetGuardSettings()))
                WeaponGuardSettings.Add(GuardSlingerJobDay.GetGuardSettings());

            foreach (var weap in WeaponLookup)
                WeaponGuardSettings.Add(new GuardBaseJob.GuardSettings
                {
                    cooldownMissingItem     = 1.5f,
                    cooldownSearchingTarget = 0.5f,
                    cooldownShot            = 3f,
                    range                   = 1,
                    recruitmentItem         = new InventoryItem(weap.Key, 1),
                    shootItem               = new List<InventoryItem>(),
                    shootDamage             = weap.Value.Damage[DamageType.Physical],
                    OnShootAudio            = "sling",
                    OnHitAudio              = "fleshHit"
                });

            WeaponGuardSettings = WeaponGuardSettings.OrderBy(w => w.shootDamage).Reverse().ToList();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Weapons.WeaponFactory.WeaponAttack")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.players.hitnpc")]
        public static void WeaponAttack(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (boxedData.item1.IsConsumed)
                return;

            var click      = boxedData.item1;
            var rayCastHit = click.rayCastHit;
            var state      = PlayerState.GetPlayerState(player);

            if (WeaponLookup.ContainsKey(click.typeSelected) &&
                click.rayCastHit.rayHitType == RayHitType.NPC &&
                click.clickType == PlayerClickedData.ClickType.Left)
            {
                var millisecondsSinceStart = Time.MillisecondsSinceStart;

                if (state.Weapon.IsEmpty() || state.Weapon.Id != click.typeSelected)
                    state.Weapon = new SettlerInventory.ArmorState
                    {
                        Id         = click.typeSelected,
                        Durability = WeaponLookup[click.typeSelected].Durability
                    };

                if (Players.LastPunches.TryGetValue(player, out var num) &&
                    millisecondsSinceStart - num < Players.PlayerPunchCooldownMS) return;

                Players.LastPunches[player]  = millisecondsSinceStart;
                boxedData.item1.consumedType = PlayerClickedData.ConsumedType.UsedByMod;

                // TODO: fix the damage.
                if (ZombieID.IsZombieID(rayCastHit.hitNPCID))
                {
                    if (MonsterTracker.TryGetMonsterByID(rayCastHit.hitNPCID, out var monster))
                    {
                        monster.OnHit(WeaponLookup[click.typeSelected].Damage[DamageType.Physical]);
                        state.Weapon.Durability--;
                        ServerManager.SendAudio(monster.PositionToAimFor, "punch");
                    }
                }
                else if (NPCTracker.TryGetNPC(rayCastHit.hitNPCID, out var nPCBase))
                {
                    nPCBase.OnHit(WeaponLookup[click.typeSelected].Damage[DamageType.Physical]);
                    state.Weapon.Durability--;
                    ServerManager.SendAudio(nPCBase.Position.Vector, "punch");
                }

                if (state.Weapon.Durability <= 0)
                {
                    state.Weapon = new SettlerInventory.ArmorState();
                    player.TakeItemFromInventory(click.typeSelected);

                    PandaChat.Send(player,
                                   $"Your {WeaponLookup[click.typeSelected].Name} has broke!",
                                   ChatColor.orange);
                }
            }
        }

        public static IPandaDamage GetWeapon(ModLoader.OnHitData box)
        {
            var weap = box.HitSourceObject as IPandaDamage;

            if (weap == null && box.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = box.HitSourceObject as NPCBase;

                if (npc != null)
                {
                    var inv = SettlerInventory.GetSettlerInventory(npc);

                    if (!inv.Weapon.IsEmpty() && WeaponLookup.ContainsKey(inv.Weapon.Id))
                        weap = WeaponLookup[inv.Weapon.Id];
                }
            }

            if (weap == null &&
                (box.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile ||
                box.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick))
            {
                var p = box.HitSourceObject as Players.Player;

                if (p != null)
                {
                    var ps = PlayerState.GetPlayerState(p);

                    if (!ps.Weapon.IsEmpty() && WeaponLookup.ContainsKey(ps.Weapon.Id))
                        weap = WeaponLookup[ps.Weapon.Id];
                }
            }

            return weap;
        }

        public static float CalcDamage(IPandaArmor pandaArmor, IPandaDamage pamdaDamage)
        {
            var damage = 0f;

            foreach (var dt in pamdaDamage.Damage)
            {
                var tmpDmg = dt.Key.CalcDamage(pandaArmor.ElementalArmor, dt.Value);

                if (pandaArmor.AdditionalResistance.TryGetValue(dt.Key, out var flatResist))
                    tmpDmg = tmpDmg - tmpDmg * flatResist;

                damage += tmpDmg;
            }

            return damage;
        }
    }
}