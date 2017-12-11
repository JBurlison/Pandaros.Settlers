using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz;
using Server.NPCs;

namespace Pandaros.Settlers.Jobs
{
    public class Sickness : IJob
    {
        private Players.Player _owner;
        private Vector3Int _pos;
        private NPCBase _npc;

        public bool IsValid => true;

        public Players.Player Owner => _owner;

        public Vector3Int KeyLocation => _pos;

        public bool NeedsNPC => _npc == null || !_npc.IsValid;

        public NPCType NPCType => throw new NotImplementedException();

        public InventoryItem RecruitementItem => InventoryItem.Empty;

        public List<Illness.ISickness> Illness { get; private set; } = new List<Jobs.Illness.ISickness>();
        
        public NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            
        }

        public Vector3Int GetJobLocation()
        {
            
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
