using BlockTypes.Builtin;
using NPC;
using Pipliz;
using Pipliz.JSON;
using Server.AI;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    public class HerbalistJob : AreaJob
    {
        private bool shouldDumpInventory;

        public override NPCType NPCType
        {
            get
            {
                // return NPCType.GetByKeyNameOrDefault("pipliz.flaxfarmer");
            }
        }

        public override bool NeedsItems
        {
            get
            {
                return this.shouldDumpInventory;
            }
        }

        public HerbalistJob(Bounds box, Players.Player player, int npcID)
        {
            base.InitializeAreaJob(new Vector3Int(box.min), new Vector3Int(box.max), player, npcID);
            base.SetLayer(BuiltinBlocks.Dirt, -1);
        }

        public HerbalistJob()
        {
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            base.InitializeAreaJob(player, node);
            return this;
        }

        public override void TakeItems(ref NPCBase.NPCState state)
        {
            base.TakeItems(ref state);
            this.shouldDumpInventory = false;
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            state.JobIsDone = true;
            if (this.positionSub.IsValid)
            {
                ushort num;
                if (World.TryGetTypeAt(this.positionSub, out num))
                {
                    if (num == 0)
                    {
                        if (state.Inventory.TryGetOneItem(BuiltinBlocks.FlaxStage1) || this.usedNPC.Colony.UsedStockpile.TryRemove(BuiltinBlocks.FlaxStage1, 1)) // ,-------------------------------------------------------------------------------------------------------------------
                        {
                            ServerManager.TryChangeBlock(this.positionSub, BuiltinBlocks.FlaxStage1, ServerManager.SetBlockFlags.DefaultAudio); // ,-------------------------------------------------------------------------------------------------------------------
                            state.SetCooldown(1.0);
                            this.shouldDumpInventory = false;
                        }
                        else
                        {
                            state.SetIndicator(NPCIndicatorType.MissingItem, 2f, BuiltinBlocks.FlaxStage1);
                            this.shouldDumpInventory = (state.Inventory.UsedCapacity > 0f);
                        }
                    }
                    else if (num == BuiltinBlocks.FlaxStage2) // ,-------------------------------------------------------------------------------------------------------------------
                    {
                        if (ServerManager.TryChangeBlock(this.positionSub, 0, ServerManager.SetBlockFlags.DefaultAudio))
                        {
                            this.usedNPC.Inventory.Add(ItemTypes.GetType(BuiltinBlocks.FlaxStage2).OnRemoveItems); // ,-------------------------------------------------------------------------------------------------------------------
                        }
                        state.SetCooldown(1.0);
                        this.shouldDumpInventory = false;
                    }
                    else
                    {
                        state.SetCooldown(5.0);
                        this.shouldDumpInventory = (state.Inventory.UsedCapacity > 0f);
                    }
                }
                else
                {
                    state.SetCooldown((double)Pipliz.Random.NextFloat(3f, 6f));
                }
                this.positionSub = Vector3Int.invalidPos;
            }
            else
            {
                state.SetCooldown(10.0);
            }
        }

        public override void CalculateSubPosition()
        {
            bool flag = this.usedNPC.Colony.UsedStockpile.Contains(BuiltinBlocks.FlaxStage1, 1); // ,-------------------------------------------------------------------------------------------------------------------
            bool flag2 = false;
            Vector3Int positionSub = Vector3Int.invalidPos;
            for (int i = this.positionMin.x; i <= this.positionMax.x; i++)
            {
                int num = (!flag2) ? this.positionMin.z : this.positionMax.z;
                while ((!flag2) ? (num <= this.positionMax.z) : (num >= this.positionMin.z))
                {
                    Vector3Int vector3Int = new Vector3Int(i, this.positionMin.y, num);
                    ushort num2;
                    if (!AIManager.Loaded(vector3Int) || !World.TryGetTypeAt(vector3Int, out num2))
                    {
                        return;
                    }
                    if (num2 == 0)
                    {
                        if (!flag && !positionSub.IsValid)
                        {
                            positionSub = vector3Int;
                        }
                        if (flag)
                        {
                            this.positionSub = vector3Int;
                            return;
                        }
                    }
                    if (num2 == BuiltinBlocks.FlaxStage2) // ,-------------------------------------------------------------------------------------------------------------------
                    {
                        this.positionSub = vector3Int;
                        return;
                    }
                    num = ((!flag2) ? (num + 1) : (num - 1));
                }
                flag2 = !flag2;
            }
            if (positionSub.IsValid)
            {
                this.positionSub = positionSub;
                return;
            }
            int a = Pipliz.Random.Next(this.positionMin.x, this.positionMax.x + 1);
            int c = Pipliz.Random.Next(this.positionMin.z, this.positionMax.z + 1);
            this.positionSub = new Vector3Int(a, this.positionMin.y, c);
        }
    }
}
