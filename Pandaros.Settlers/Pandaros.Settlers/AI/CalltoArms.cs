using BlockTypes.Builtin;
using ChatCommands;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Pipliz.BlockNPCs.Implementations;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPC;
using Pipliz.Collections;

namespace Pandaros.Settlers.AI
{
    [ModLoader.ModManager]
    public class CalltoArmsJob : NPC.Job
    {
        const int CALL_RAD = 100;

        static string COOLDOWN_KEY = GameLoader.NAMESPACE + ".CallToArmsCooldown";
        static Dictionary<InventoryItem, bool> _hadAmmo = new Dictionary<InventoryItem, bool>();
        static List<GuardBaseJob.GuardSettings> _weapons = new List<GuardBaseJob.GuardSettings>();
       
        static NPCTypeStandardSettings _callToArmsNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".CalledToArms",
            printName = "Called to Arms",
            maskColor0 = UnityEngine.Color.red,
            maskColor1 = UnityEngine.Color.magenta
        };
        public static NPCType CallToArmsNPCType;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, GameLoader.NAMESPACE + ".CalltoArms.Init")]
        public static void Init()
        {
            CallToArmsNPCType = new NPCType(_callToArmsNPCSettings.Type);
            NPCType.NPCTypes.Add(CallToArmsNPCType, _callToArmsNPCSettings);
        }

        GuardBaseJob.GuardSettings _weapon;
        BoxedDictionary _tmpVals;
        Colony _colony;
        PlayerState _playerState;
        Stockpile _stock;
        IMonster _target;

        public override bool ToSleep => false;

        public override NPCType NPCType => CallToArmsNPCType;

        public override void OnAssignedNPC(NPCBase npc)
        {
            _tmpVals = npc.GetTempValues(true);
            _colony = npc.Colony;
            _playerState = SettlerManager.GetPlayerState(_colony.Owner, _colony);
            _stock = Stockpile.GetStockPile(_colony.Owner);
            base.OnAssignedNPC(npc);
        }

        public override Vector3Int GetJobLocation()
        {
            if (_playerState.CallToArmsEnabled && _weapon != null)
            {
                var currentPos = new Vector3Int(usedNPC.Position);
                _target = MonsterTracker.Find(currentPos, _weapon.range);

                if (_target != null)
                {
                    PandaLogger.Log("Within Range");
                    return currentPos;
                }
                else
                {
                    _target = MonsterTracker.Find(currentPos, CALL_RAD);

                    if (_target != null)
                    {
                        PandaLogger.Log("Found New Target");
                        var ranged = _weapon.range - 5;
                        position = new Vector3Int(_target.Position).Add(ranged, ranged, 0);
                        return new Vector3Int(_target.Position).Add(ranged, ranged, 0);
                    }
                    else
                        _tmpVals.Set(COOLDOWN_KEY, _weapon.cooldownSearchingTarget);
                }
            }

            return base.GetJobLocation();
        }

        public static GuardBaseJob.GuardSettings GetWeapon(NPC.NPCBase npc)
        {
            GuardBaseJob.GuardSettings weapon = null;

            foreach (var w in _weapons)
                if (npc.Inventory.Contains(w.recruitmentItem))
                {
                    weapon = w;
                    break;
                }

            return weapon;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (CheckTime())
            {
                var currentposition = new Vector3Int(usedNPC.Position);
                _hadAmmo.Clear();

                if (_target == null || !_target.IsValid || !General.Physics.Physics.CanSee(usedNPC.Position, _target.Position))
                    _target = MonsterTracker.Find(currentposition, _weapon.range); 

                if (_target != null && General.Physics.Physics.CanSee(usedNPC.Position, _target.Position))
                {
                    PandaLogger.Log("Monster found.");
                    foreach (var projectile in _weapon.shootItem)
                    {
                        _hadAmmo[projectile] = false;

                        if (usedNPC.Inventory.Contains(projectile))
                        {
                            _hadAmmo[projectile] = true;
                            continue;
                        }

                        if (_stock.Contains(projectile))
                            _hadAmmo[projectile] = true;
                    }

                    if (!_hadAmmo.Any(a => !a.Value))
                    {
                        state.SetIndicator(NPCIndicatorType.Crafted, _weapon.cooldownShot, _weapon.shootItem[0].Type);
                        PandaLogger.Log("we have ammo.");
                        foreach (var ammo in _hadAmmo)
                        {
                            if (usedNPC.Inventory.Contains(ammo.Key))
                            {
                                usedNPC.Inventory.Remove(ammo.Key);
                                continue;
                            }

                            if (_stock.Contains(ammo.Key))
                                _stock.Remove(ammo.Key);
                        }

                        usedNPC.LookAt(_target.Position);

                        if (_weapon.OnShootAudio != null)
                            ServerManager.SendAudio(position.Vector, _weapon.OnShootAudio);

                        if (_weapon.OnHitAudio != null)
                            ServerManager.SendAudio(_target.PositionToAimFor, _weapon.OnHitAudio);

                        _target.OnHit(_weapon.shootDamage);
                        _tmpVals.Set(COOLDOWN_KEY, _weapon.cooldownShot);
                    }
                    else
                    {
                        state.SetIndicator(NPCIndicatorType.MissingItem, _weapon.cooldownMissingItem, _weapon.shootItem[0].Type);
                        _tmpVals.Set(COOLDOWN_KEY, _weapon.cooldownMissingItem);
                    }
                }
                else
                    _target = null;
            }
        }

        public override void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            if (_weapon != null)
                return;

            if (GuardBowJobDay.CachedSettings != null && !_weapons.Contains(GuardBowJobDay.CachedSettings))
                _weapons.Add(GuardBowJobDay.CachedSettings);

            if (GuardCrossbowJobDay.CachedSettings != null && !_weapons.Contains(GuardCrossbowJobDay.CachedSettings))
                _weapons.Add(GuardCrossbowJobDay.CachedSettings);

            if (GuardMatchlockJobDay.CachedSettings != null && !_weapons.Contains(GuardMatchlockJobDay.CachedSettings))
                _weapons.Add(GuardMatchlockJobDay.CachedSettings);

            if (GuardSlingerJobDay.CachedSettings != null && !_weapons.Contains(GuardSlingerJobDay.CachedSettings))
                _weapons.Add(GuardSlingerJobDay.CachedSettings);

            _weapons = _weapons.OrderBy(w => w.shootDamage).ToList();

            if (_playerState.CallToArmsEnabled && _weapons.Count != 0)
            {
                _weapon = GetWeapon(usedNPC);

                if (_weapon == null)
                {
                    foreach (var w in _weapons)
                    {
                        if (_stock.Contains(w.recruitmentItem))
                        {
                            _stock.Remove(w.recruitmentItem);
                            usedNPC.Inventory.Add(w.recruitmentItem);
                            _weapon = w;
                            break;
                        }
                    }
                }
            }
        }

        public override float TimeBetweenJobs => _weapon == null ? base.TimeBetweenJobs : _tmpVals.GetOrDefault(COOLDOWN_KEY, _weapon.cooldownSearchingTarget);

        public override bool NeedsItems => _weapon == null;

        public override Vector3Int KeyLocation => position;

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (_weapon == null)
                return NPCBase.NPCGoal.Stockpile;

            return NPCBase.NPCGoal.Job;
        }
    }

    [ModLoader.ModManager]
    public class CalltoArms : IChatCommand
    {
        static Dictionary<NPC.NPCBase, NPC.IJob> _Jobs = new Dictionary<NPC.NPCBase, NPC.IJob>();

        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/arms", StringComparison.OrdinalIgnoreCase) ||
                   chat.StartsWith("/cta", StringComparison.OrdinalIgnoreCase) ||
                   chat.StartsWith("/call", StringComparison.OrdinalIgnoreCase);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".CallToArms.OnPlayerDisconnected")]
        public void OnPlayerDisconnected(Players.Player p)
        {
            Colony c = Colony.Get(p);
            PlayerState state = SettlerManager.GetPlayerState(p);

            state.CallToArmsEnabled = false;

            foreach (var follower in c.Followers)
                follower.ClearJob();
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);
            Colony colony = Colony.Get(player);
            PlayerState state = SettlerManager.GetPlayerState(player, colony);
            state.CallToArmsEnabled = !state.CallToArmsEnabled;

            if (state.CallToArmsEnabled)
            {
                PandaChat.Send(player, "Call to arms activated!", ChatColor.red, ChatStyle.bold);

                foreach (var follower in colony.Followers)
                {
                    var job = follower.Job;
                    _Jobs[follower] = job;
                    follower.ClearJob();
                    follower.TakeJob(new CalltoArmsJob());
                }
            }
            else
            {
                PandaChat.Send(player, "Call to arms deactivated.", ChatColor.green, ChatStyle.bold);

                foreach (var follower in colony.Followers)
                    if (_Jobs.ContainsKey(follower))
                        follower.TakeJob(_Jobs[follower]);
                    else
                        follower.ClearJob();

                _Jobs.Clear();
            }

            return true;
        }
    }
}
