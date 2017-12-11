using NPC;
using Pipliz;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    public class Physician : IJob
    {
        const int MAX_DIST = 10;
        static List<NPCBase> _treating = new List<NPCBase>();
        
        private Players.Player _owner;
        private Vector3Int _pos;
        private NPCBase _npc;

        public bool IsValid => true;

        public Players.Player Owner => _owner;

        public Vector3Int KeyLocation => _pos;

        public bool NeedsNPC => _npc == null || !_npc.IsValid;

        public NPCType NPCType => throw new NotImplementedException();

        public InventoryItem RecruitementItem => InventoryItem.Empty;

        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        public Vector3Int GetJobLocation()
        {
            var loc = KeyLocation;

            foreach (var npc in _npc.Colony.Followers)
                if (npc.Job != null &&
                    typeof(Sickness) == npc.Job.GetType() &&
                    !_treating.Contains(npc) &&
                    Vector3.Distance(KeyLocation.Vector, npc.Position.Vector) <= MAX_DIST)
                {
                    _treating.Add(npc);
                    loc = npc.Position;
                }

            return loc;
        }

        public void OnAssignedNPC(NPCBase npc)
        {
            _npc = npc;
        }

        public void OnNPCAtJob(ref NPCBase.NPCState state)
        {

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
