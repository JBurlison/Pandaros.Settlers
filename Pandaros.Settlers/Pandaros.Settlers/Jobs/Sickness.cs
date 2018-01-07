using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz;
using Server.NPCs;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class SicknessRegister
    {
        private static NPCTypeStandardSettings _settings;

        public static NPCTypeStandardSettings NPCTypeSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new NPCTypeStandardSettings
                    {
                        keyName = GameLoader.NAMESPACE + ".Sickness",
                        printName = "Illness",
                        maskColor1 = new Color32(147, 8, 8, 255),
                        type = NPCTypeID.GetNextID()
                    };

                return _settings;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Sickness.Init"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            NPCType.AddSettings(NPCTypeSettings);
        }
    }


    public class Sickness : IJob
    {
        private Players.Player _owner;
        private Vector3Int _pos;
        private NPCBase _npc;
        private IJob _prevJob;
        private float _invaild = 999999999;
        private double _healedTime = double.NaN;

        public bool IsValid => true;

        public Players.Player Owner => _owner;

        public Vector3Int KeyLocation => _pos;

        public bool NeedsNPC => _npc == null || !_npc.IsValid;

        public NPCType NPCType => NPCType.GetByKeyNameOrDefault(SicknessRegister.NPCTypeSettings.keyName);

        public NPCBase NPC => _npc;

        public InventoryItem RecruitementItem => InventoryItem.Empty;

        public List<Illness.ISickness> Illness { get; private set; }

        public Sickness(NPCBase npc, List<Illness.ISickness> illness, IJob job)
        {
            Illness = illness;
            _prevJob = job;
            _owner = npc.Colony.Owner;
            _npc = npc;
            _pos = npc.Position;
        }

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            GetHurt(ref state);

            foreach (var npc in _npc.Colony.Followers)
            {
                if (npc.Job != null &&
                    typeof(PhysicianJob) == npc.Job.GetType())
                {
                    return NPCBase.NPCGoal.Job;
                }
            }

            if (_healedTime == double.NaN)
            {
                _healedTime = TimeCycle.TotalTime + 12;
            }
            else if (_healedTime <= TimeCycle.TotalTime)
            {
                Heal();
                return NPCBase.NPCGoal.Job;
            }

            return NPCBase.NPCGoal.Bed; 
        }

        public Vector3Int GetJobLocation()
        {
            var loc = KeyLocation;
            float closest = _invaild;

            foreach (var npc in _npc.Colony.Followers)
            {
                if (npc.Job != null &&
                    typeof(PhysicianJob) == npc.Job.GetType())
                {
                    var dist = Vector3.Distance(KeyLocation.Vector, npc.Position.Vector);

                    if (dist < closest)
                    {
                        loc = npc.Position;
                        closest = dist;
                    }
                }
            }

            return loc;
        }

        public void OnAssignedNPC(NPCBase npc)
        {
            _npc = npc;
        }

        private void GetHurt(ref NPCBase.NPCState state)
        {
            if (Illness.Count > 0)
            {
                state.SetIndicator(NPCIndicatorType.Crafted, 1f, Illness.FirstOrDefault().IndicatorIcon);
                state.SetCooldown(1f);

                foreach (var ill in Illness)
                {
                    _npc.health -= ill.DamagePerSecond;

                    if (_npc.health <= 0)
                        _npc.OnDeath();
                }
            }
        }


        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            GetHurt(ref state);
            Heal();
        }

        public void Heal()
        {
            if (Illness.Count == 0)
            {
                _npc.ClearJob();

                if (_prevJob.NeedsNPC)
                    _prevJob.OnAssignedNPC(_npc);

                OnRemovedNPC();
            }
        }

        public void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            
        }

        public void OnRemovedNPC()
        {
            _npc = null;
        }
    }
}
