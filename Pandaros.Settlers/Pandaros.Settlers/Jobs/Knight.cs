using System;
using System.Collections.Generic;
using System.Linq;
using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Weapons;
using Pipliz;
using Pipliz.Collections;
using Pipliz.Mods.APIProvider.Jobs;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using Shared;
using UnityEngine;
using Physics = General.Physics.Physics;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public class Knight : IJob, INPCTypeDefiner
    {
        private const float COOLDOWN = 2f;
        public static NPCType KnightNPCType;

        private static readonly NPCTypeStandardSettings _knightNPCSettings = new NPCTypeStandardSettings
        {
            type       = NPCTypeID.GetNextID(),
            keyName    = GameLoader.NAMESPACE + ".Knight",
            printName  = "Knight",
            maskColor0 = Color.blue,
            maskColor1 = Color.black
        };

        private Colony _colony;
        private int _currentPatrolPos;
        private bool _forward = true;
        private SettlerInventory _inv;
        private PlayerState _playerState;
        private Stockpile _stock;
        private IMonster _target;
        private int _timeAtPatrol;
        private double _timeJob;
        private BoxedDictionary _tmpVals;

        private int _waitingFor;

        public Knight(List<Vector3Int> potrolPoints, Players.Player owner)
        {
            if (!Knights.ContainsKey(owner))
                Knights.Add(owner, new List<Knight>());

            Knights[owner].Add(this);
            PatrolPoints = potrolPoints;
            KeyLocation  = PatrolPoints[0];
            Owner        = owner;
        }

        public static Dictionary<Players.Player, List<Knight>> Knights { get; } = new Dictionary<Players.Player, List<Knight>>();

        public PatrolType PatrolType { get; set; }

        public List<Vector3Int> PatrolPoints { get; }
        public NPCBase UsedNPC { get; private set; }
        public bool IsValid { get; private set; } = true;
        public Players.Player Owner { get; }
        public Vector3Int KeyLocation { get; private set; }
        public bool NeedsNPC => UsedNPC == null || !UsedNPC.IsValid;
        public NPCType NPCType => KnightNPCType;

        public InventoryItem RecruitementItem => InventoryItem.Empty;

        public NPCBase NPC
        {
            get => UsedNPC;
            set => UsedNPC = value;
        }

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            var inv = SettlerInventory.GetSettlerInventory(UsedNPC);

            if (!inv.Weapon.IsEmpty())
                return NPCBase.NPCGoal.Job;

            return NPCBase.NPCGoal.Stockpile;
        }

        public Vector3Int GetJobLocation()
        {
            var currentPos = UsedNPC.Position;
            GetBestWeapon();

            _target = MonsterTracker.Find(currentPos, 1, WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

            if (_target != null) return currentPos;

            _target = MonsterTracker.Find(PatrolPoints[_currentPatrolPos], 10,
                                          WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

            if (_target != null)
            {
                KeyLocation = new Vector3Int(_target.Position).Add(1, 0, 0);
                KeyLocation = AIManager.ClosestPosition(KeyLocation, currentPos);

                if (!AIManager.CanStandAt(KeyLocation))
                    _waitingFor++;
                else
                    return KeyLocation;
            }
            else
            {
                _waitingFor++;
            }


            if (_waitingFor > 1)
            {
                currentPos = PatrolPoints[_currentPatrolPos];
                _timeAtPatrol++;
                _waitingFor = 0;
            }

            if (_timeAtPatrol > 1)
            {
                _timeAtPatrol = 0;
                _waitingFor   = 0;

                if (PatrolPoints.Count > 1)
                    if (PatrolType == PatrolType.RoundRobin || _forward && PatrolType == PatrolType.Zipper)
                    {
                        _currentPatrolPos++;

                        if (_currentPatrolPos > PatrolPoints.Count - 1)
                            if (_forward && PatrolType == PatrolType.Zipper)
                            {
                                _currentPatrolPos -= 2;
                                _forward          =  false;
                            }
                            else
                            {
                                _currentPatrolPos = 0;
                            }
                    }
                    else
                    {
                        _currentPatrolPos--;

                        if (_currentPatrolPos < 0)
                        {
                            _currentPatrolPos = 0;
                            _currentPatrolPos++;
                            _forward = true;
                        }
                    }

                currentPos = PatrolPoints[_currentPatrolPos];

                // if our flag is gone, remove the job.
                if (World.TryGetTypeAt(PatrolPoints[_currentPatrolPos], out var objType) &&
                    objType != PatrolTool.PatrolFlag.ItemIndex)
                {
                    var stockPile = Stockpile.GetStockPile(Owner);

                    UsedNPC.ClearJob();
                    Knights[Owner].Remove(this);

                    if (((JobTracker.JobFinder) JobTracker.GetOrCreateJobFinder(Owner)).openJobs.Contains(this))
                        ((JobTracker.JobFinder) JobTracker.GetOrCreateJobFinder(Owner)).openJobs.Remove(this);

                    foreach (var flagPoint in PatrolPoints)
                        if (World.TryGetTypeAt(flagPoint, out var flagType) &&
                            flagType == PatrolTool.PatrolFlag.ItemIndex)
                        {
                            ServerManager.TryChangeBlock(flagPoint, BuiltinBlocks.Air);
                            stockPile.Add(PatrolTool.PatrolFlag.ItemIndex);
                        }
                }
            }

            return currentPos;
        }

        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (CheckTime() && UsedNPC != null)
            {
                if (_inv == null)
                    _inv = SettlerInventory.GetSettlerInventory(UsedNPC);

                if (_stock == null)
                    _stock = UsedNPC.Colony.UsedStockpile;

                ArmorFactory.GetBestArmorForNPC(_stock, UsedNPC, _inv, 0);

                try
                {
                    var currentposition = UsedNPC.Position;

                    if (_target == null || !_target.IsValid ||
                        !Physics.CanSee(UsedNPC.Position.Vector, _target.Position))
                        _target = MonsterTracker.Find(currentposition, 1, WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

                    if (_target != null && Physics.CanSee(UsedNPC.Position.Vector, _target.Position))
                    {
                        state.SetIndicator(new IndicatorState(COOLDOWN, _inv.Weapon.Id));
                        UsedNPC.LookAt(_target.Position);
                        ServerManager.SendAudio(_target.PositionToAimFor, "punch");

                        _target.OnHit(WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());
                        _waitingFor = 0;
                    }
                    else
                    {
                        state.SetIndicator(new IndicatorState(COOLDOWN, GameLoader.MissingMonster_Icon, true));
                        _waitingFor++;
                        _target = null;
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }
        }

        public void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            var hasItem = GetBestWeapon();

            if (!hasItem)
                state.SetIndicator(new IndicatorState(COOLDOWN, WeaponFactory.WeaponLookup.FirstOrDefault().Key, true));
        }

        public NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return _knightNPCSettings;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Knight.Init")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            NPCType.AddSettings(_knightNPCSettings);
            KnightNPCType = NPCType.GetByKeyNameOrDefault(_knightNPCSettings.keyName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Jobs.Knight.OnNPCHit")]
        [ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Armor.OnNPCHit")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData box)
        {
            if (npc != null && npc.Job != null && npc.Job.GetType() == typeof(Knight))
                box.ResultDamage = box.ResultDamage - box.ResultDamage * .75f;
        }

        public void OnAssignedNPC(NPCBase npc)
        {
            UsedNPC      = npc;
            _tmpVals     = npc.GetTempValues(true);
            _colony      = npc.Colony;
            _inv         = SettlerInventory.GetSettlerInventory(npc);
            _playerState = PlayerState.GetPlayerState(_colony.Owner);
            _stock       = Stockpile.GetStockPile(_colony.Owner);
        }

        public void OnRemovedNPC()
        {
            UsedNPC = null;
            JobTracker.Add(this);
        }

        protected void OverrideCooldown(double cooldownLeft)
        {
            _timeJob = Time.SecondsSinceStartDouble + cooldownLeft;
        }

        protected virtual bool CheckTime()
        {
            var timeNow = Time.SecondsSinceStartDouble;

            if (timeNow < _timeJob)
                return false;

            _timeJob = timeNow + COOLDOWN;

            return true;
        }

        private bool GetBestWeapon()
        {
            var hasItem = false;

            try
            {
                if (UsedNPC != null)
                {
                    if (_inv == null)
                        _inv = SettlerInventory.GetSettlerInventory(UsedNPC);

                    if (_stock == null)
                        _stock = UsedNPC.Colony.UsedStockpile;

                    hasItem = !_inv.Weapon.IsEmpty();
                    IWeapon bestWeapon = null;

                    if (hasItem)
                        bestWeapon = WeaponFactory.WeaponLookup[_inv.Weapon.Id];

                    foreach (var wep in WeaponFactory.WeaponLookup.Values)
                        if (_stock.Contains(wep.ItemType.ItemIndex) && bestWeapon == null ||
                            _stock.Contains(wep.ItemType.ItemIndex) && bestWeapon != null &&
                            bestWeapon.Damage.TotalDamage() < wep.Damage.TotalDamage())
                            bestWeapon = wep;

                    if (bestWeapon != null)
                        if (hasItem && _inv.Weapon.Id != bestWeapon.ItemType.ItemIndex || !hasItem)
                        {
                            hasItem = true;
                            _stock.TryRemove(bestWeapon.ItemType.ItemIndex);

                            if (!_inv.Weapon.IsEmpty())
                                _stock.Add(_inv.Weapon.Id);

                            _inv.Weapon = new SettlerInventory.ArmorState
                            {
                                Id         = bestWeapon.ItemType.ItemIndex,
                                Durability = bestWeapon.Durability
                            };
                        }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            return hasItem;
        }

        public virtual void OnRemove()
        {
            IsValid = false;

            if (UsedNPC != null)
            {
                UsedNPC.ClearJob();
                UsedNPC = null;
            }
        }
    }
}