using System.Collections.Generic;
using System.Linq;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.Mods.APIProvider.Jobs;
using Pipliz.Mods.BaseGame.BlockNPCs;
using Server.Monsters;
using Shared;

namespace Pandaros.Settlers.Items
{
    public enum WeaponType
    {
        Sword
    }

    public enum MetalType
    {
        Copper,
        Bronze,
        Iron,
        Steel
    }

    [ModLoader.ModManagerAttribute]
    public static class ItemFactory
    {
        public const string JOB_BAKER = "pipliz.baker";
        public const string JOB_CRAFTER = "pipliz.crafter";
        public static List<GuardBaseJob.GuardSettings> WeaponGuardSettings = new List<GuardBaseJob.GuardSettings>();

        public static Dictionary<ushort, WeaponMetadata> WeaponLookup { get; } =
            new Dictionary<ushort, WeaponMetadata>();

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
                    shootDamage             = weap.Value.Damage,
                    OnShootAudio            = "sling",
                    OnHitAudio              = "fleshHit"
                });

            WeaponGuardSettings = WeaponGuardSettings.OrderBy(w => w.shootDamage).Reverse().ToList();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked,
            GameLoader.NAMESPACE + ".Items.WeaponAttack")]
        [ModLoader.ModCallbackProvidesForAttribute("pipliz.server.players.hitnpc")]
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

                if (ZombieID.IsZombieID(rayCastHit.hitNPCID))
                {
                    if (MonsterTracker.TryGetMonsterByID(rayCastHit.hitNPCID, out var monster))
                    {
                        monster.OnHit(WeaponLookup[click.typeSelected].Damage);
                        state.Weapon.Durability--;
                        ServerManager.SendAudio(monster.PositionToAimFor, "punch");
                    }
                }
                else if (NPCTracker.TryGetNPC(rayCastHit.hitNPCID, out var nPCBase))
                {
                    nPCBase.OnHit(WeaponLookup[click.typeSelected].Damage);
                    state.Weapon.Durability--;
                    ServerManager.SendAudio(nPCBase.Position.Vector, "punch");
                }

                if (state.Weapon.Durability <= 0)
                {
                    state.Weapon = new SettlerInventory.ArmorState();
                    player.TakeItemFromInventory(click.typeSelected);

                    PandaChat.Send(player,
                                   $"Your {WeaponLookup[click.typeSelected].Metal} {WeaponLookup[click.typeSelected].WeaponType} has broke!",
                                   ChatColor.orange);
                }
            }
        }
    }
}