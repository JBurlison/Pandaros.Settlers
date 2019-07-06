using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Models;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Weapons
{
    [ModLoader.ModManager]
    public static class WeaponFactory
    {
        public static Dictionary<ushort, IWeapon> WeaponLookup { get; } =  new Dictionary<ushort, IWeapon>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Items.Weapons.WeaponFactory.CheckIfMonstersNearby")]
        public static void CheckIfMonstersNearby()
        {
            if (ServerManager.ColonyTracker != null)
            {
                var punchDamage = SettlersConfiguration.GetorDefault("ColonistPunchDamage", 30);

                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                {
                    if (colony.DifficultySetting.ShouldSpawnZombies(colony))
                    {
                        foreach (var npc in colony.Followers)
                        {
                            var inv = Entities.SettlerInventory.GetSettlerInventory(npc);

                            if (inv.Weapon != null && !inv.Weapon.IsEmpty())
                            {
                                var target = MonsterTracker.Find(npc.Position, 2, punchDamage);

                                if (target != null && target.IsValid)
                                {
                                    npc.LookAt(target.Position);
                                    AudioManager.SendAudio(target.PositionToAimFor, "punch");

                                    if (inv.Weapon != null && !inv.Weapon.IsEmpty())
                                        target.OnHit(WeaponFactory.WeaponLookup[inv.Weapon.Id].Damage.TotalDamage());
                                    else
                                        target.OnHit(punchDamage);
                                }
                            }
                        }
                    }
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Weapons.WeaponFactory.WeaponAttack")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.players.hitnpc")]
        public static void WeaponAttack(Players.Player player, PlayerClickedData playerClickData)
        {
            if (playerClickData.IsConsumed)
                return;

            var click      = playerClickData;
            var state      = PlayerState.GetPlayerState(player);

            if (WeaponLookup.ContainsKey(click.TypeSelected) &&
                click.HitType == PlayerClickedData.EHitType.NPC &&
                click.ClickType == PlayerClickedData.EClickType.Left)
            {
                var millisecondsSinceStart = Time.MillisecondsSinceStart;

                if (state.Weapon.IsEmpty() || state.Weapon.Id != click.TypeSelected)
                    state.Weapon = new ItemState
                    {
                        Id         = click.TypeSelected,
                        Durability = WeaponLookup[click.TypeSelected].WepDurability
                    };

                if (Players.LastPunches.TryGetValue(player, out var num) &&
                    millisecondsSinceStart - num < Players.PlayerPunchCooldownMS) return;

                Players.LastPunches[player]  = millisecondsSinceStart;
                playerClickData.ConsumedType = PlayerClickedData.EConsumedType.UsedByMod;
                var rayCastHit = click.GetNPCHit();

                if (ZombieID.IsZombieID(rayCastHit.NPCID))
                {
                    if (MonsterTracker.TryGetMonsterByID(rayCastHit.NPCID, out var monster))
                    {
                        var dmg = WeaponLookup[click.TypeSelected].Damage.TotalDamage();
                        state.IncrimentStat("Damage Delt", dmg);
                        monster.OnHit(dmg);
                        state.Weapon.Durability--;
                        AudioManager.SendAudio(monster.PositionToAimFor, "punch");
                    }
                }
                else if (NPCTracker.TryGetNPC(rayCastHit.NPCID, out var nPCBase))
                {
                    var dmg = WeaponLookup[click.TypeSelected].Damage.TotalDamage();
                    state.IncrimentStat("Damage Delt", dmg);
                    nPCBase.OnHit(dmg, player, ModLoader.OnHitData.EHitSourceType.PlayerClick);
                    state.Weapon.Durability--;
                    AudioManager.SendAudio(nPCBase.Position.Vector, "punch");
                }

                if (state.Weapon.Durability <= 0)
                {
                    state.Weapon = new ItemState();
                    player.TakeItemFromInventory(click.TypeSelected);

                    PandaChat.Send(player,
                                   $"Your {WeaponLookup[click.TypeSelected].name} has broke!",
                                   ChatColor.orange);
                }
            }
        }

        public static bool GetBestWeapon(NPC.NPCBase npc, int limit)
        {
            var hasItem = false;

            try
            {
                if (npc != null)
                {
                    var  inv = SettlerInventory.GetSettlerInventory(npc);
                    var stock = npc.Colony.Stockpile;

                    hasItem = !inv.Weapon.IsEmpty();
                    IWeapon bestWeapon = null;

                    if (hasItem)
                        bestWeapon = WeaponFactory.WeaponLookup[inv.Weapon.Id];

                    foreach (var wep in WeaponFactory.WeaponLookup.Values.Where(w => w as IPlayerMagicItem == null && w is WeaponMetadata weaponMetadata && weaponMetadata.ItemType != null).Cast<WeaponMetadata>())
                        if (stock.Contains(wep.ItemType.ItemIndex) && bestWeapon == null ||
                            stock.Contains(wep.ItemType.ItemIndex) && bestWeapon != null &&
                            bestWeapon.Damage.TotalDamage() < wep.Damage.TotalDamage() &&
                            stock.AmountContained(ItemId.GetItemId(bestWeapon.name)) > limit)
                            bestWeapon = wep;

                    if (bestWeapon != null)
                    {
                        var wepId = ItemId.GetItemId(bestWeapon.name);
                        if (hasItem && inv.Weapon.Id != wepId || !hasItem)
                        {
                            hasItem = true;
                            stock.TryRemove(wepId);

                            if (!inv.Weapon.IsEmpty())
                                stock.Add(inv.Weapon.Id);

                            inv.Weapon = new ItemState
                            {
                                Id = wepId,
                                Durability = bestWeapon.WepDurability
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            return hasItem;
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
                    tmpDmg = tmpDmg - (tmpDmg * flatResist);

                damage += tmpDmg;
            }

            return damage;
        }
    }
}