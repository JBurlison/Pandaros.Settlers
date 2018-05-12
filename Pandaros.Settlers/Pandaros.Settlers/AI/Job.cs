using NPC;
using Pipliz;
using Pipliz.JSON;
using Server.NPCs;

namespace Pandaros.Settlers.AI
{
    public class Job : IJob, ITrackableBlock
    {
        protected bool isValid = true;
        protected Players.Player owner;
        protected Vector3Int position;
        protected NPCBase usedNPC;

        public virtual bool ToSleep => TimeCycle.ShouldSleep;

        /// Returns true if the job requires items from the stockpile; false if it doesn't require items OR if the npc should come to dump its items.
        public virtual bool NeedsItems => false;

        public virtual Vector3Int KeyLocation => position;

        public virtual bool IsValid => isValid;

        public virtual Players.Player Owner => owner;

        public virtual bool NeedsNPC => usedNPC == null || !usedNPC.IsValid;

        public virtual NPCType NPCType => new NPCType();

        public virtual InventoryItem RecruitementItem => InventoryItem.Empty;

        public NPCBase NPC
        {
            get => usedNPC;
            set => usedNPC = value;
        }

        public virtual Vector3Int GetJobLocation()
        {
            return KeyLocation;
        }

        public virtual void OnNPCAtJob(ref NPCBase.NPCState state)
        {
        }

        public virtual void OnNPCAtStockpile(ref NPCBase.NPCState state)
        {
            if (ToSleep)
            {
                TryDumpNPCInventory(ref state);
                state.JobIsDone = true;
                state.SetCooldown(0.3);
            }
            else
            {
                TakeItems(ref state);
                state.SetCooldown(0.3);
            }
        }

        public virtual NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (ToSleep)
            {
                if (!state.Inventory.IsEmpty) return NPCBase.NPCGoal.Stockpile;
                return NPCBase.NPCGoal.Bed;
            }

            if (state.Inventory.Full || NeedsItems) return NPCBase.NPCGoal.Stockpile;
            return NPCBase.NPCGoal.Job;
        }

        public virtual ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            return this;
        }

        public virtual void OnRemove()
        {
            isValid = false;

            if (usedNPC != null)
            {
                usedNPC.ClearJob();
                usedNPC = null;
            }

            JobTracker.Remove(owner, KeyLocation);
        }

        public virtual JSONNode GetJSON()
        {
            return new JSONNode().SetAs("npcID", usedNPC == null ? 0 : usedNPC.ID);
        }

        public void InitializeJob(Players.Player owner, Vector3Int position, int desiredNPCID)
        {
            this.position = position;
            this.owner    = owner;

            if (desiredNPCID != 0 && NPCTracker.TryGetNPC(desiredNPCID, out usedNPC))
                usedNPC.TakeJob(this);
            else
                desiredNPCID = 0;

            if (usedNPC == null) JobTracker.Add(this);
        }

        public virtual void OnAssignedNPC(NPCBase npc)
        {
            usedNPC = npc;
        }

        public virtual void OnRemovedNPC()
        {
            usedNPC = null;
            JobTracker.Add(this);
        }

        public virtual void TakeItems(ref NPCBase.NPCState state)
        {
            if (TryDumpNPCInventory(ref state)) state.JobIsDone = true;
        }

        protected bool TryDumpNPCInventory(ref NPCBase.NPCState npcState)
        {
            if (!npcState.Inventory.IsEmpty) npcState.Inventory.Dump(usedNPC.Colony.UsedStockpile);
            return true;
        }
    }
}