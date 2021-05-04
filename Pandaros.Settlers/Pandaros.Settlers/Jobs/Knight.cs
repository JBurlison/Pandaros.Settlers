using AI;
using Jobs;
using Monsters;
using NPC;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.Items.Armor;
using Pandaros.API.Items.Weapons;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Pipliz;
using Science;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Jobs
{
    public class KnightResearch : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => null;

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 25 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.SWORDSMITHING1,
                    SettlersBuiltIn.Research.ARMORSMITHING1
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 1;

        public float BaseValue => 1f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".Knights";

        public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

        public void BeforeRegister()
        {
            
        }

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.ForEachOwner(o => PatrolTool.GivePlayerPatrolTool(o));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(PatrolTool.PatrolFlag.name));
        }
    }

    [ModLoader.ModManager]
    public class Knight : IJob
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Knight.Init")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Init()
        {
            NPCType.AddSettings(_knightNPCSettings);
            KnightNPCType = NPCType.GetByKeyNameOrDefault(_knightNPCSettings.keyName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Jobs.Knight.OnNPCHit")]
        [ModLoader.ModCallbackProvidesFor("Pandaros.API.Armor.OnNPCHit")]
        [ModLoader.ModCallbackDependsOn("Pandaros.API.Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData box)
        {
            if (npc != null && npc.Job != null && npc.Job.GetType() == typeof(Knight))
                box.ResultDamage = box.ResultDamage - box.ResultDamage * .75f;
        }

        private const float COOLDOWN = 2f;
        public static NPCType KnightNPCType;
        public virtual float NPCShopGameHourMinimum { get { return TimeCycle.Settings.SleepTimeEnd; } }
        public virtual float NPCShopGameHourMaximum { get { return TimeCycle.Settings.SleepTimeStart; } }

        private static readonly NPCTypeStandardSettings _knightNPCSettings = new NPCTypeStandardSettings
        {
            Type       = NPCTypeID.GetID(GameLoader.NAMESPACE + ".Knight"),
            keyName    = GameLoader.NAMESPACE + ".Knight",
            maskColor0 = UnityEngine.Color.blue,
            maskColor1 = UnityEngine.Color.black
        };

        private int _currentPatrolPos;
        private bool _forward = true;
        private ColonistInventory _inv;
        private IMonster _target;
        private int _timeAtPatrol;
        private double _timeJob;

        private int _waitingFor;

        public Knight(List<Vector3Int> potrolPoints, Colony owner)
        {
            if (!Knights.ContainsKey(owner))
                Knights.Add(owner, new List<Knight>());

            Knights[owner].Add(this);
            PatrolPoints = potrolPoints;
            KeyLocation  = PatrolPoints[0];
            Owner        = owner;
        }

        public static Dictionary<Colony, List<Knight>> Knights { get; } = new Dictionary<Colony, List<Knight>>();

        public PatrolType PatrolType { get; set; }

        public List<Vector3Int> PatrolPoints { get; }
        public NPCBase UsedNPC { get; private set; }
        public bool IsValid { get; private set; } = true;
        public Colony Owner { get; }
        public Vector3Int KeyLocation { get; private set; }
        public bool NeedsNPC => UsedNPC == null || !UsedNPC.IsValid;
        public NPCType NPCType => KnightNPCType;

        public InventoryItem RecruitmentItem => InventoryItem.Empty;

        public NPCBase NPC
        {
            get => UsedNPC;
            set => UsedNPC = value;
        }

        public Vector3Int GetJobLocation()
        {
            var currentPos = UsedNPC.Position;
            GetBestWeapon();

            _target = MonsterTracker.Find(currentPos, 1, WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

            if (_target != null) return currentPos;

            if (PatrolType == PatrolType.Zipper || PatrolType == PatrolType.RoundRobin)
                _target = MonsterTracker.Find(PatrolPoints[_currentPatrolPos], 10, WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

            if (_target != null)
            {
                if (PathingManager.TryCanStandNear(currentPos, out var canStandNear, out var newLoc))
                    KeyLocation = newLoc;

                if (!canStandNear || (!PathingManager.TryCanStandAt(KeyLocation, out var canStand) && canStand))
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
                    if ((PatrolType == PatrolType.RoundRobin || PatrolType == PatrolType.WaitRoundRobin) || 
                        _forward && (PatrolType == PatrolType.Zipper || PatrolType == PatrolType.WaitZipper))
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
                if (World.TryGetTypeAt(PatrolPoints[_currentPatrolPos], out ushort objType) &&
                    objType != PatrolTool.PatrolFlag.ItemIndex)
                {
                    UsedNPC.ClearJob();
                    Knights[Owner].Remove(this);
                    Owner.JobFinder.UnregisterJob(this);

                    foreach (var flagPoint in PatrolPoints)
                        if (World.TryGetTypeAt(flagPoint, out ushort flagType) &&
                            flagType == PatrolTool.PatrolFlag.ItemIndex)
                        {
                            ServerManager.TryChangeBlock(flagPoint, ColonyBuiltIn.ItemTypes.AIR.Id);
                            UsedNPC.Colony.Stockpile.Add(PatrolTool.PatrolFlag.ItemIndex);
                        }
                }
            }

            return currentPos;
        }

        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            state.SetCooldown(5);
            if (CheckTime() && UsedNPC != null)
            {
                if (_inv == null)
                    _inv = ColonistInventory.Get(UsedNPC);

                ArmorFactory.GetBestArmorForNPC(UsedNPC.Colony.Stockpile, UsedNPC, _inv, 0);

                try
                {
                    var currentposition = UsedNPC.Position;

                    if (_target == null || !_target.IsValid ||
                        !VoxelPhysics.CanSee(UsedNPC.Position.Vector, _target.Position))
                        _target = MonsterTracker.Find(currentposition, 1, WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());

                    if (_target != null && _target.IsValid && VoxelPhysics.CanSee(UsedNPC.Position.Vector, _target.Position))
                    {
                        state.SetIndicator(new IndicatorState(COOLDOWN, _inv.Weapon.Id));
                        UsedNPC.LookAt(_target.Position);
                        AudioManager.SendAudio(_target.PositionToAimFor, "punch");

                        _target.OnHit(WeaponFactory.WeaponLookup[_inv.Weapon.Id].Damage.TotalDamage());
                        _waitingFor = 0;
                    }
                    else
                    {
                        state.SetIndicator(new IndicatorState(COOLDOWN, ItemId.GetItemId(GameLoader.NAMESPACE + ".Monster").Id, true));
                        _waitingFor++;
                        _target = null;
                    }
                }
                catch (Exception ex)
                {
                    SettlersLogger.LogError(ex);
                }
            }
        }

        public void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            var hasItem = GetBestWeapon();
            state.SetCooldown(5);

            if (!hasItem)
                state.SetIndicator(new IndicatorState(COOLDOWN, WeaponFactory.WeaponLookup.FirstOrDefault().Key, true));
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
                        _inv = ColonistInventory.Get(UsedNPC);

                    hasItem = !_inv.Weapon.IsEmpty();
                    IWeapon bestWeapon = null;

                    if (hasItem)
                        bestWeapon = WeaponFactory.WeaponLookup[_inv.Weapon.Id];

                    foreach (var wep in WeaponFactory.WeaponLookup.Values.Where(w => w as IPlayerMagicItem == null && w is WeaponMetadata weaponMetadata && weaponMetadata.ItemType != null).Cast<WeaponMetadata>())
                        if (UsedNPC.Colony.Stockpile.Contains(wep.ItemType.ItemIndex) && bestWeapon == null ||
                            UsedNPC.Colony.Stockpile.Contains(wep.ItemType.ItemIndex) && bestWeapon != null &&
                            bestWeapon.Damage.TotalDamage() < wep.Damage.TotalDamage())
                            bestWeapon = wep;

                    if (bestWeapon != null)
                    {
                        var wepId = ItemId.GetItemId(bestWeapon.name);
                        if (hasItem && _inv.Weapon.Id != wepId || !hasItem)
                        {
                            hasItem = true;
                            UsedNPC.Colony.Stockpile.TryRemove(wepId);

                            if (!_inv.Weapon.IsEmpty())
                                UsedNPC.Colony.Stockpile.Add(_inv.Weapon.Id);

                            _inv.Weapon = new ItemState
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
                SettlersLogger.LogError(ex);
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

        public void SetNPC(NPCBase npc)
        {
            UsedNPC = npc;
            _inv = ColonistInventory.Get(npc);
        }

        public void OnNPCCouldNotPathToGoal()
        {
            
        }

        public void OnNPCUpdate(NPCBase npc)
        {
            throw new NotImplementedException();
        }
    }
}