using AI;
using Chatting;
using Jobs;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Weapons;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.Collections;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.AI
{
    [ModLoader.ModManager]
    public class CalltoArmsJob : IJob
    {
        private const int CALL_RAD = 500;

        private static readonly string COOLDOWN_KEY = GameLoader.NAMESPACE + ".CallToArmsCooldown";
        private static readonly Dictionary<InventoryItem, bool> _hadAmmo = new Dictionary<InventoryItem, bool>();

        private static readonly NPCTypeStandardSettings _callToArmsNPCSettings = new NPCTypeStandardSettings
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".CalledToArms",
            printName = "Called to Arms",
            maskColor0 = UnityEngine.Color.red,
            maskColor1 = UnityEngine.Color.magenta
        };

        public static NPCType CallToArmsNPCType;
        private Colony _colony;
        private ColonyState _colonyState;
        private Stockpile _stock;
        private IMonster _target;
        private JSONNode _tmpVals;
        private int _waitingFor;

        private GuardJobSettings _weapon;

        public Colony Owner { get; set; }

        public bool NeedsNPC { get; set; }

        public InventoryItem RecruitmentItem { get; set; }

        public NPCBase NPC { get; set; }

        public bool IsValid { get; set; }

        public float NPCShopGameHourMinimum { get; set; }

        public float NPCShopGameHourMaximum { get; set; }

        public NPCType NPCType => CallToArmsNPCType;

        public Vector3Int Position { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".CalltoArms.Init")]
        public static void Init()
        {
            NPCType.AddSettings(_callToArmsNPCSettings);
            CallToArmsNPCType = NPCType.GetByKeyNameOrDefault(_callToArmsNPCSettings.keyName);
        }

        public void OnAssignedNPC(NPCBase npc)
        {
            Owner = npc.Colony;
            NPC = npc;
            NeedsNPC = false;
            IsValid = true;
            _tmpVals = npc.CustomData;
            _colony = npc.Colony;
            _colonyState = ColonyState.GetColonyState(_colony);
            _stock = npc.Colony.Stockpile;
        }

        public Vector3Int GetJobLocation()
        {
            var currentPos = NPC.Position;

            if (_colonyState.CallToArmsEnabled && _weapon != null)
            {
                _target = MonsterTracker.Find(currentPos, _weapon.Range, _weapon.Damage);

                if (_target != null)
                {
                    return currentPos;
                }

                _target = MonsterTracker.Find(currentPos, CALL_RAD, _weapon.Damage);

                if (_target != null)
                {
                    var ranged = _weapon.Range - 5;

                    if (ranged < 0)
                        ranged = 1;

                    Position = new Vector3Int(_target.Position).Add(ranged, 0, ranged);
                    Position = AIManager.ClosestPosition(Position, currentPos);

                    if (!AIManager.CanStandAt(Position))
                    {
                        _tmpVals.SetAs(COOLDOWN_KEY, _weapon.CooldownMissingItem);
                        _waitingFor++;
                    }
                    else
                    {
                        return Position;
                    }
                }
                else
                {
                    _tmpVals.SetAs(COOLDOWN_KEY, _weapon.CooldownMissingItem);
                    _waitingFor++;
                }
            }

            if (_waitingFor > 10)
            {
                var banner = NPC.Colony.GetClosestBanner(currentPos);

                if (banner != null)
                    return banner.Position;
            }

            return currentPos;
        }

        public static GuardJobSettings GetWeapon(NPCBase npc)
        {
            GuardJobSettings weapon = null;
            var inv = SettlerInventory.GetSettlerInventory(npc);

            foreach (BlockJobManager<GuardJobInstance> w in ServerManager.BlockEntityCallbacks.AutoLoadedInstances.Where(t => t as BlockJobManager<GuardJobInstance> != null))
                if (npc.Inventory.Contains(w.Settings.RecruitmentItem) || inv.Weapon.Id == w.Settings.RecruitmentItem.Type)
                {
                    weapon = w.Settings as GuardJobSettings;
                    break;
                }

            return weapon;
        }

        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            try
            {
                var currentposition = NPC.Position;
                _hadAmmo.Clear();

                if (_target == null || !_target.IsValid || !VoxelPhysics.CanSee(NPC.Position.Vector, _target.Position))
                    _target = MonsterTracker.Find(currentposition, _weapon.Range, _weapon.Damage);

                if (_target != null && VoxelPhysics.CanSee(NPC.Position.Vector, _target.Position))
                {
                    foreach (var projectile in _weapon.ShootItem)
                    {
                        _hadAmmo[projectile] = false;

                        if (NPC.Inventory.Contains(projectile))
                        {
                            _hadAmmo[projectile] = true;
                            continue;
                        }

                        if (_stock.Contains(projectile))
                            _hadAmmo[projectile] = true;
                    }

                    if (!_hadAmmo.Any(a => !a.Value))
                    {
                        state.SetIndicator(new IndicatorState(_weapon.CooldownShot, _weapon.ShootItem[0].Type));

                        foreach (var ammo in _hadAmmo)
                        {
                            if (NPC.Inventory.Contains(ammo.Key))
                            {
                                NPC.Inventory.TryRemove(ammo.Key);
                                continue;
                            }

                            if (_stock.Contains(ammo.Key))
                                _stock.TryRemove(ammo.Key);
                        }

                        NPC.LookAt(_target.Position);

                        if (_weapon.OnShootAudio != null)
                            ServerManager.SendAudio(Position.Vector, _weapon.OnShootAudio);

                        if (_weapon.OnHitAudio != null)
                            ServerManager.SendAudio(_target.PositionToAimFor, _weapon.OnHitAudio);

                        if (_weapon.ShootItem.Count > 0)
                            foreach (var proj in _weapon.ShootItem)
                            {
                                var projName = ItemTypes.IndexLookup.GetName(proj.Type);

                                if (AnimationManager.AnimatedObjects.ContainsKey(projName))
                                {
                                    AnimationManager.AnimatedObjects[projName].SendMoveToInterpolatedOnce(Position.Vector, _target.PositionToAimFor);

                                    break;
                                }
                            }

                        ServerManager.SendParticleTrail(currentposition.Vector, _target.PositionToAimFor, 2);
                        _target.OnHit(_weapon.Damage);
                        state.SetCooldown(_weapon.CooldownShot);
                        _waitingFor = 0;
                    }
                    else
                    {
                        state.SetIndicator(new IndicatorState(_weapon.CooldownMissingItem, _weapon.ShootItem[0].Type, true));
                        state.SetCooldown(_weapon.CooldownMissingItem);
                    }
                }
                else
                {
                    state.SetIndicator(new IndicatorState(_weapon.CooldownSearchingTarget, GameLoader.MissingMonster_Icon, true));
                    state.SetCooldown(_weapon.CooldownMissingItem);
                    _target = null;
                }
            }
            catch (Exception)
            {
                state.SetIndicator(new IndicatorState(_weapon.CooldownSearchingTarget, GameLoader.MissingMonster_Icon, true));
                state.SetCooldown(_weapon.CooldownMissingItem);
                _target = null;
            }
        }

        public void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            if (_weapon != null)
                return;

            if (_colonyState.CallToArmsEnabled)
            {
                _weapon = GetWeapon(NPC);

                if (_weapon == null)
                    foreach (BlockJobManager<GuardJobInstance> w in ServerManager.BlockEntityCallbacks.AutoLoadedInstances.Where(t => t as BlockJobManager<GuardJobInstance> != null))
                        if (_stock.Contains(w.Settings.RecruitmentItem))
                        {
                            _stock.TryRemove(w.Settings.RecruitmentItem);
                            NPC.Inventory.Add(w.Settings.RecruitmentItem);
                            _weapon = w.Settings as GuardJobSettings;
                            break;
                        }
            }
        }

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (_weapon == null)
                return NPCBase.NPCGoal.Stockpile;

            return NPCBase.NPCGoal.Job;
        }

        public void SetNPC(NPCBase npc)
        {
            throw new NotImplementedException();
        }
    }

    [ModLoader.ModManager]
    public class CalltoArms : IChatCommand
    {
        private readonly List<CalltoArmsJob> _callToArmsJobs = new List<CalltoArmsJob>();
        private readonly Dictionary<NPCBase, IJob> _Jobs = new Dictionary<NPCBase, IJob>();

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.StartsWith("/arms", StringComparison.OrdinalIgnoreCase) &&
                !chat.StartsWith("/cta", StringComparison.OrdinalIgnoreCase) &&
                !chat.StartsWith("/call", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server || player.ActiveColony == null)
                return true;

            var array = CommandManager.SplitCommand(chat);
            var colony = player.ActiveColony;
            ProcesssCallToArms(player, colony);

            return true;
        }

        private void ProcesssCallToArms(Players.Player player, Colony colony)
        {
            var state = ColonyState.GetColonyState(colony);
            state.CallToArmsEnabled = !state.CallToArmsEnabled;

            if (state.CallToArmsEnabled)
            {
                PandaChat.Send(player, "Call to arms activated!", ChatColor.red, ChatStyle.bold);

                foreach (var follower in colony.Followers)
                {
                    var job = follower.Job;

                    if (!CanCallToArms(job))
                        continue;

                    try
                    {
                        if (job != null)
                        {
                            if (job.GetType() != typeof(CalltoArmsJob))
                                _Jobs[follower] = job;
                        }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }

                    var armsJob = new CalltoArmsJob();
                    _callToArmsJobs.Add(armsJob);
                    follower.ClearJob();
                    follower.TakeJob(armsJob);
                }
            }
            else
            {
                PandaChat.Send(player, "Call to arms deactivated.", ChatColor.green, ChatStyle.bold);
                var assignedWorkers = new List<NPCBase>();

                foreach (var follower in colony.Followers)
                {
                    var job = follower.Job;

                    if (job != null && job.GetType() == typeof(CalltoArmsJob))
                    {
                        follower.ClearJob();
                        follower.Colony.JobFinder.JobsData.OpenJobs.Remove(job);
                    }

                    if (_Jobs.ContainsKey(follower) && _Jobs[follower].NeedsNPC)
                    {
                        assignedWorkers.Add(follower);
                        follower.TakeJob(_Jobs[follower]);
                        follower.Colony.JobFinder.Remove(_Jobs[follower]);
                    }
                }

                _Jobs.Clear();
            }

            foreach (var armsJob in _callToArmsJobs)
                armsJob.Owner.JobFinder.Remove(armsJob);

            _callToArmsJobs.Clear();
            colony.SendUpdate();
            colony.SendColonistCount();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected,
            GameLoader.NAMESPACE + ".CallToArms.OnPlayerDisconnected")]
        public void OnPlayerDisconnected(Players.Player p)
        {

            foreach (var colony in p.Colonies)
            {
                var state = ColonyState.GetColonyState(colony);

                if (state.CallToArmsEnabled)
                    ProcesssCallToArms(p, colony);
            }
        }

        public static bool CanCallToArms(IJob job)
        {
            return !(job is GuardJobInstance) &&
                   !(job is Knight) &&
                   !(job is RoamingJob);
        }
    }
}