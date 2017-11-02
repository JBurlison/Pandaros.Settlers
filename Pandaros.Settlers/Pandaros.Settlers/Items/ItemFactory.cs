using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.APIProvider.Jobs;
using Pipliz.BlockNPCs.Implementations;
using Server.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    [ModLoader.ModManager]
    public static class ItemFactory
    {
        public static Dictionary<ushort, WeaponMetadata> WeaponLookup { get; private set; } = new Dictionary<ushort, WeaponMetadata>();
        public static List<GuardBaseJob.GuardSettings> WeaponGuardSettings = new List<GuardBaseJob.GuardSettings>();

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
                WeaponGuardSettings.Add(new GuardBaseJob.GuardSettings()
                {
                    cooldownMissingItem = 1.5f,
                    cooldownSearchingTarget = 0.5f,
                    cooldownShot = 3f,
                    range = 1,
                    recruitmentItem = new InventoryItem(weap.Key, 1),
                    shootItem = new List<InventoryItem>(),
                    shootDamage = weap.Value.Damage,
                    OnShootAudio = "sling",
                    OnHitAudio = "fleshHit"
                });

            WeaponGuardSettings = WeaponGuardSettings.OrderBy(w => w.shootDamage).Reverse().ToList();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Items.GetWeapon")]
        public static void GetWeapon()
        {
            //for colonists.
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.WeaponAttack"), ModLoader.ModCallbackProvidesFor("pipliz.server.players.hitnpc")]
        public static void WeaponAttack(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if (boxedData.item1.IsConsumed)
                return;

            var click = boxedData.item1;
            Shared.VoxelRayCastHit rayCastHit = click.rayCastHit;
            var state = PlayerState.GetPlayerState(player);

            if (ItemFactory.WeaponLookup.ContainsKey(click.typeSelected) &&
                click.rayCastHit.rayHitType == Shared.RayHitType.NPC &&
                click.clickType == Shared.PlayerClickedData.ClickType.Left)
            {
                long millisecondsSinceStart = Pipliz.Time.MillisecondsSinceStart;

                if (state.Weapon.IsEmpty() || state.Weapon.Id != click.typeSelected)
                    state.Weapon = new SettlerInventory.ArmorState()
                    {
                        Id = click.typeSelected,
                        Durability = WeaponLookup[click.typeSelected].Durability
                    };

                if (Players.LastPunches.TryGetValue(player, out long num) && millisecondsSinceStart - num < Players.PlayerPunchCooldownMS)
                {
                    return;
                }

                Players.LastPunches[player] = millisecondsSinceStart;
                boxedData.item1.consumedType = Shared.PlayerClickedData.ConsumedType.UsedByMod;

                if (ZombieID.IsZombieID(rayCastHit.hitNPCID))
                {
                    if (MonsterTracker.TryGetMonsterByID(rayCastHit.hitNPCID, out IMonster monster))
                    {
                        monster.OnHit(ItemFactory.WeaponLookup[click.typeSelected].Damage);
                        state.Weapon.Durability--;
                        ServerManager.SendAudio(monster.PositionToAimFor, "punch");
                    }
                }
                else if (NPCTracker.TryGetNPC(rayCastHit.hitNPCID, out NPCBase nPCBase))
                {
                    nPCBase.OnHit(WeaponLookup[click.typeSelected].Damage);
                    state.Weapon.Durability--;
                    ServerManager.SendAudio(nPCBase.Position, "punch");
                }

                if (state.Weapon.Durability <= 0)
                {
                    state.Weapon = new SettlerInventory.ArmorState();
                    var inv = Inventory.GetInventory(player).GetInventory();
                    var item = inv.FirstOrDefault(i => i.Type == click.typeSelected);

                    if (item != null)
                        inv.Remove(item);
                    
                    PandaChat.Send(player, $"Your {WeaponLookup[click.typeSelected].Metal} {WeaponLookup[click.typeSelected].WeaponType} has broke!", ChatColor.orange);
                }
            }
        }
    }
}
