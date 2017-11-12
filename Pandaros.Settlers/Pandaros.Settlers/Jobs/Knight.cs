using NPC;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using Pandaros.Settlers.Entities;
using Server.Monsters;
using Pipliz.Collections;
using BlockTypes.Builtin;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public class Knight : IJob, INPCTypeDefiner
    {
        public static Dictionary<Players.Player, List<Knight>> Knights { get; private set; } = new Dictionary<Players.Player, List<Knight>>();

        private const float COOLDOWN = 2f;
        public static NPCType KnightNPCType;

        static NPCTypeStandardSettings _knightNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".Knight",
            printName = "Knight",
            maskColor0 = UnityEngine.Color.blue,
            maskColor1 = UnityEngine.Color.black
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Knight.Init"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            NPCType.AddSettings(_knightNPCSettings);
            KnightNPCType = NPCType.GetByKeyNameOrDefault(_knightNPCSettings.keyName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Jobs.Knight.OnNPCHit"), ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Armor.OnNPCHit")]
        public static void OnNPCHit(NPC.NPCBase npc, Pipliz.Box<float> box)
        {
            if (npc != null && npc.Job != null && npc.Job.GetType() == typeof(Jobs.Knight))
            {
                box.Set(box.item1 - (box.item1 * .5f));
            }
        }

        NPC.NPCBase _usedNPC;
        bool _isValid = true;
        Vector3Int _position;
        BoxedDictionary _tmpVals;
        Colony _colony;
        PlayerState _playerState;
        Stockpile _stock;
        IMonster _target;
        SettlerInventory _inv;
        int _waitingFor;
        int _currentPatrolPos;
        private double _timeJob;
        int _timeAtPatrol;
        Players.Player _owner;
        bool _forward = true;

        public PatrolType PatrolType { get; set; }

        public List<Vector3Int> PatrolPoints { get; private set; }
        public NPC.NPCBase UsedNPC { get { return _usedNPC; } }
        public bool IsValid { get { return _isValid; } }
        public Players.Player Owner { get { return _owner; } }
        public Vector3Int KeyLocation { get { return _position; } }
        public bool NeedsNPC { get { return _usedNPC == null || !_usedNPC.IsValid; } }
        public NPCType NPCType => KnightNPCType;

        public InventoryItem RecruitementItem
        {
            get
            {
                return InventoryItem.Empty;
            }
        }

        public Knight(List<Vector3Int> potrolPoints, Players.Player owner)
        {
            if (!Knights.ContainsKey(owner))
                Knights.Add(owner, new List<Knight>());

            Knights[owner].Add(this);
            PatrolPoints = potrolPoints;
            _position = PatrolPoints[0];
            _owner = owner;
        }

        public void OnAssignedNPC(NPCBase npc)
        {
            _usedNPC = npc;
            _tmpVals = npc.GetTempValues(true);
            _colony = npc.Colony;
            _inv = SettlerInventory.GetSettlerInventory(npc);
            _playerState = PlayerState.GetPlayerState(_colony.Owner);
            _stock = Stockpile.GetStockPile(_colony.Owner);
        }

        public void OnRemovedNPC()
        {
            _usedNPC = null;
            JobTracker.Add(this);
        }

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            var inv = SettlerInventory.GetSettlerInventory(_usedNPC);
            
            if (!inv.Weapon.IsEmpty())
                return NPCBase.NPCGoal.Job;
            else
                return NPCBase.NPCGoal.Stockpile;
        }

        public Vector3Int GetJobLocation()
        {
            var currentPos = _usedNPC.Position;

            _target = MonsterTracker.Find(currentPos, 1, Items.ItemFactory.WeaponLookup[_inv.Weapon.Id].Damage);

            if (_target != null)
            {
                return currentPos;
            }
            else
            {
                _target = MonsterTracker.Find(PatrolPoints[_currentPatrolPos], 10, Items.ItemFactory.WeaponLookup[_inv.Weapon.Id].Damage);

                if (_target != null)
                {
                    _position = new Vector3Int(_target.Position).Add(1, 0, 0);
                    _position = Server.AI.AIManager.ClosestPosition(_position, currentPos);

                    if (!Server.AI.AIManager.CanStandAt(_position))
                    {
                        _waitingFor++;
                    }
                    else
                        return _position;
                }
                else
                {
                    _waitingFor++;
                }
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
                _waitingFor = 0;

                if (PatrolType == PatrolType.RoundRobin || (_forward && PatrolType == PatrolType.Zipper))
                {
                    _currentPatrolPos++;

                    if (_currentPatrolPos > PatrolPoints.Count - 1)
                        if (_forward && PatrolType == PatrolType.Zipper)
                        {
                            _currentPatrolPos -= 2;
                            _forward = false;
                        }
                        else
                            _currentPatrolPos = 0;
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
                if (World.TryGetTypeAt(PatrolPoints[_currentPatrolPos], out var objType) && objType != PatrolTool.PatrolFlag.ItemIndex)
                {
                    var stockPile = Stockpile.GetStockPile(Owner);

                    UsedNPC.ClearJob();
                    Knights[Owner].Remove(this);

                    if (((JobTracker.JobFinder)JobTracker.GetOrCreateJobFinder(Owner)).openJobs.Contains(this))
                        ((JobTracker.JobFinder)JobTracker.GetOrCreateJobFinder(Owner)).openJobs.Remove(this);
                    
                    foreach (var flagPoint in PatrolPoints)
                        if (World.TryGetTypeAt(flagPoint, out var flagType) && flagType == PatrolTool.PatrolFlag.ItemIndex)
                        {
                            ServerManager.TryChangeBlock(flagPoint, BuiltinBlocks.Air);
                            stockPile.Add(PatrolTool.PatrolFlag.ItemIndex);
                        }
                }
            }

            return currentPos;
        }

        protected void OverrideCooldown(double cooldownLeft)
        {
            _timeJob = Time.SecondsSinceStartDouble + cooldownLeft;
        }

        protected virtual bool CheckTime()
        {
            double timeNow = Time.SecondsSinceStartDouble;

            if (timeNow < _timeJob)
            {
                return false;
            }

            _timeJob = timeNow + COOLDOWN;

            return true;
        }

        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (CheckTime())
            {
                Items.Armor.GetBestArmorForNPC(_stock, _usedNPC, _inv, 0);

                try
                {
                    var currentposition = _usedNPC.Position;

                    if (_target == null || !_target.IsValid || !General.Physics.Physics.CanSee(_usedNPC.Position.Vector, _target.Position))
                        _target = MonsterTracker.Find(currentposition, 1, Items.ItemFactory.WeaponLookup[_inv.Weapon.Id].Damage);

                    if (_target != null && General.Physics.Physics.CanSee(_usedNPC.Position.Vector, _target.Position))
                    {
                        state.SetIndicator(NPCIndicatorType.Crafted, COOLDOWN, _inv.Weapon.Id);
                        _usedNPC.LookAt(_target.Position);
                        ServerManager.SendAudio(_target.PositionToAimFor, "punch");

                        _target.OnHit(Items.ItemFactory.WeaponLookup[_inv.Weapon.Id].Damage);
                        _waitingFor = 0;
                    }
                    else
                    {
                        state.SetIndicator(NPCIndicatorType.MissingItem, COOLDOWN, GameLoader.MissingMonster_Icon);
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
            bool hasItem = GetBestWeapon();

            if (!hasItem)
            {
                state.SetIndicator(NPCIndicatorType.MissingItem, COOLDOWN, Items.ItemFactory.WeaponLookup.FirstOrDefault().Key);
            }
        }

        private bool GetBestWeapon()
        {
            bool hasItem = !_inv.Weapon.IsEmpty();

            Items.WeaponMetadata bestWeapon = null;

            if (hasItem)
                bestWeapon = Items.ItemFactory.WeaponLookup[_inv.Weapon.Id];

            foreach (var wep in Items.ItemFactory.WeaponLookup.Values)
                if ((_stock.Contains(wep.ItemType.ItemIndex) && bestWeapon == null) ||
                    (_stock.Contains(wep.ItemType.ItemIndex) && bestWeapon != null && bestWeapon.Damage < wep.Damage))
                    bestWeapon = wep;

            if ((bestWeapon != null && hasItem && _inv.Weapon.Id != bestWeapon.ItemType.ItemIndex) ||
                (!hasItem && bestWeapon != null))
            {
                hasItem = true;
                _stock.TryRemove(bestWeapon.ItemType.ItemIndex);

                if (!_inv.Weapon.IsEmpty())
                    _stock.Add(_inv.Weapon.Id);

                _inv.Weapon = new SettlerInventory.ArmorState()
                {
                    Id = bestWeapon.ItemType.ItemIndex,
                    Durability = bestWeapon.Durability
                };
            }

            return hasItem;
        }

        public NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return _knightNPCSettings;
        }

        public virtual void OnRemove()
        {
            _isValid = false;

            if (_usedNPC != null)
            {
                _usedNPC.ClearJob();
                _usedNPC = null;
            }
        }
    }
}
