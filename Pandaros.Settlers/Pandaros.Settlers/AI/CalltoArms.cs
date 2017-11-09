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
using Pandaros.Settlers.Managers;

namespace Pandaros.Settlers.AI
{
    [ModLoader.ModManager]
    public class CalltoArmsJob : NPC.Job
    {
        const int CALL_RAD = 100;

        static string COOLDOWN_KEY = GameLoader.NAMESPACE + ".CallToArmsCooldown";
        static Dictionary<InventoryItem, bool> _hadAmmo = new Dictionary<InventoryItem, bool>();

        static NPCTypeStandardSettings _callToArmsNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".CalledToArms",
            printName = "Called to Arms",
            maskColor0 = UnityEngine.Color.red,
            maskColor1 = UnityEngine.Color.magenta
        };
        public static NPCType CallToArmsNPCType;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".CalltoArms.Init"), 
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            NPCType.AddSettings(_callToArmsNPCSettings);
            CallToArmsNPCType = NPCType.GetByKeyNameOrDefault(_callToArmsNPCSettings.keyName);
        }

        GuardBaseJob.GuardSettings _weapon;
        BoxedDictionary _tmpVals;
        Colony _colony;
        PlayerState _playerState;
        Stockpile _stock;
        IMonster _target;
        int _waitingFor;

        public override bool ToSleep => false;

        public override NPCType NPCType => CallToArmsNPCType;

        public override void OnAssignedNPC(NPCBase npc)
        {
            owner = npc.Colony.Owner;
            _tmpVals = npc.GetTempValues(true);
            _colony = npc.Colony;
            _playerState = PlayerState.GetPlayerState(_colony.Owner, _colony);
            _stock = Stockpile.GetStockPile(_colony.Owner);
            base.OnAssignedNPC(npc);
        }

        public override Vector3Int GetJobLocation()
        {
            var currentPos = usedNPC.Position;

            if (_playerState.CallToArmsEnabled && _weapon != null)
            {
                _target = MonsterTracker.Find(currentPos, _weapon.range, _weapon.shootDamage);

                if (_target != null)
                {
                    return currentPos;
                }
                else
                {
                    _target = MonsterTracker.Find(currentPos, CALL_RAD, _weapon.shootDamage);

                    if (_target != null)
                    {
                        var ranged = _weapon.range - 5;
                        position = new Vector3Int(_target.Position).Add(ranged, 0, ranged);
                        position = Server.AI.AIManager.ClosestPosition(position, currentPos);

                        if (!Server.AI.AIManager.CanStandAt(position))
                        {
                            _tmpVals.Set(COOLDOWN_KEY, _weapon.cooldownMissingItem);
                            _waitingFor++;
                        }
                        else
                            return position;
                    }
                    else
                    {
                        _tmpVals.Set(COOLDOWN_KEY, _weapon.cooldownMissingItem);
                        _waitingFor++;
                    }
                }
            }

            if (_waitingFor > 10)
            {
                var banner = BannerManager.GetClosestBanner(usedNPC.Colony.Owner, currentPos);

                if (banner != null)
                    return banner.KeyLocation;
            }

            return currentPos;
        }

        public static GuardBaseJob.GuardSettings GetWeapon(NPC.NPCBase npc)
        {
            GuardBaseJob.GuardSettings weapon = null;
            var inv = SettlerInventory.GetSettlerInventory(npc);

            foreach (var w in Items.ItemFactory.WeaponGuardSettings)
                if (npc.Inventory.Contains(w.recruitmentItem) || inv.Weapon.Id == w.recruitmentItem.Type)
                {
                    weapon = w;
                    break;
                }

            return weapon;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            try
            {
                var currentposition =usedNPC.Position;
                _hadAmmo.Clear();

                if (_target == null || !_target.IsValid || !General.Physics.Physics.CanSee(usedNPC.Position.Vector, _target.Position))
                    _target = MonsterTracker.Find(currentposition, _weapon.range, _weapon.shootDamage);

                if (_target != null && General.Physics.Physics.CanSee(usedNPC.Position.Vector, _target.Position))
                {
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
                        foreach (var ammo in _hadAmmo)
                        {
                            if (usedNPC.Inventory.Contains(ammo.Key))
                            {
                                usedNPC.Inventory.TryRemove(ammo.Key);
                                continue;
                            }

                            if (_stock.Contains(ammo.Key))
                                _stock.TryRemove(ammo.Key);
                        }

                        usedNPC.LookAt(_target.Position);

                        if (_weapon.OnShootAudio != null)
                            ServerManager.SendAudio(position.Vector, _weapon.OnShootAudio);

                        if (_weapon.OnHitAudio != null)
                            ServerManager.SendAudio(_target.PositionToAimFor, _weapon.OnHitAudio);

                        if (_weapon.shootItem.Count > 0)
                        {
                            foreach (var proj in _weapon.shootItem)
                            {
                                string projName = ItemTypes.IndexLookup.GetName(proj.Type);

                                if (AnimationManager.AnimatedObjects.ContainsKey(projName))
                                {
                                    AnimationManager.AnimatedObjects[projName].SendMoveToInterpolatedOnce(position.Vector, _target.PositionToAimFor);
                                    break;
                                }
                            }
                        }

                        _target.OnHit(_weapon.shootDamage);
                        state.SetCooldown(_weapon.cooldownShot);
                        _waitingFor = 0;
                    }
                    else
                    {
                        state.SetIndicator(NPCIndicatorType.MissingItem, _weapon.cooldownMissingItem, _weapon.shootItem[0].Type);
                        state.SetCooldown(_weapon.cooldownMissingItem);
                    }
                }
                else
                {
                    state.SetIndicator(NPCIndicatorType.MissingItem, _weapon.cooldownSearchingTarget, GameLoader.MissingMonster_Icon);
                    state.SetCooldown(_weapon.cooldownMissingItem);
                    _target = null;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                PandaLogger.LogError(ex);
#endif
                state.SetIndicator(NPCIndicatorType.MissingItem, _weapon.cooldownSearchingTarget, GameLoader.MissingMonster_Icon);
                state.SetCooldown(_weapon.cooldownMissingItem);
                _target = null;
            }

        }

        public override void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            if (_weapon != null)
                return;

            if (_playerState.CallToArmsEnabled && Items.ItemFactory.WeaponGuardSettings.Count != 0)
            {
                _weapon = GetWeapon(usedNPC);

                if (_weapon == null)
                {
                    foreach (var w in Items.ItemFactory.WeaponGuardSettings)
                    {
                        if (_stock.Contains(w.recruitmentItem))
                        {
                            _stock.TryRemove(w.recruitmentItem);
                            usedNPC.Inventory.Add(w.recruitmentItem);
                            _weapon = w;
                            break;
                        }
                    }
                }
            }
        }

        public override bool NeedsItems => _weapon == null;

        public override Vector3Int KeyLocation => position;

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (_weapon == null)
                return NPCBase.NPCGoal.Stockpile;

            return NPCBase.NPCGoal.Job;
        }

        public override void OnRemove()
        {
            isValid = false;
            if (usedNPC != null)
            {
                usedNPC.ClearJob();
                usedNPC = null;
            }
        }

        public override void OnRemovedNPC()
        {
            usedNPC = null;
        }

        new public void InitializeJob(Players.Player owner, Vector3Int position, int desiredNPCID)
        {
            this.position = position;
            this.owner = owner;
            if (desiredNPCID != 0 && NPCTracker.TryGetNPC(desiredNPCID, out this.usedNPC))
            {
                this.usedNPC.TakeJob(this);
            }
            else
            {
                desiredNPCID = 0;
            }
        }
    }

    [ModLoader.ModManager]
    public class CalltoArms : IChatCommand
    {
        Dictionary<NPC.NPCBase, NPC.IJob> _Jobs = new Dictionary<NPC.NPCBase, NPC.IJob>();
        List<CalltoArmsJob> _callToArmsJobs = new List<CalltoArmsJob>();

        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/arms", StringComparison.OrdinalIgnoreCase) ||
                   chat.StartsWith("/cta", StringComparison.OrdinalIgnoreCase) ||
                   chat.StartsWith("/call", StringComparison.OrdinalIgnoreCase);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".CallToArms.OnPlayerDisconnected")]
        public void OnPlayerDisconnected(Players.Player p)
        {
            PlayerState state = PlayerState.GetPlayerState(p);

            if (state.CallToArmsEnabled)
                TryDoCommand(p, "");
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);
            Colony colony = Colony.Get(player);
            PlayerState state = PlayerState.GetPlayerState(player, colony);
            state.CallToArmsEnabled = !state.CallToArmsEnabled;

            if (state.CallToArmsEnabled)
            {
                PandaChat.Send(player, "Call to arms activated!", ChatColor.red, ChatStyle.bold);

                foreach (var follower in colony.Followers)
                {
                    try
                    {
                        var job = follower.Job;

                        if (job != null)
                        {
                            if (job.GetType() != typeof(CalltoArmsJob))
                                _Jobs[follower] = job;

                            job.OnRemovedNPC();
                            follower.ClearJob();
                        }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }

                    var armsJob = new CalltoArmsJob();
                    _callToArmsJobs.Add(armsJob);
                    armsJob.OnAssignedNPC(follower);
                    follower.TakeJob(armsJob);
                }
            }
            else
            {
                PandaChat.Send(player, "Call to arms deactivated.", ChatColor.green, ChatStyle.bold);
                List<NPCBase> assignedWorkers = new List<NPCBase>();

                foreach (var follower in colony.Followers)
                {
                    var job = follower.Job;

                    if (job != null && job.GetType() == typeof(CalltoArmsJob))
                    {
                        follower.ClearJob();
                        job.OnRemovedNPC();
                    }

                    if (_Jobs.ContainsKey(follower) && _Jobs[follower].NeedsNPC)
                    {
                        assignedWorkers.Add(follower);
                        follower.TakeJob(_Jobs[follower]);
                        _Jobs[follower].OnAssignedNPC(follower);
                    }
                }

                _Jobs.Clear();
            }

            foreach (var armsJob in _callToArmsJobs)
                JobTracker.Remove(player, armsJob.KeyLocation);

            _callToArmsJobs.Clear();
            JobTracker.Update();
            Colony.SendColonistCount(player);

            return true;
        }
    }
}
