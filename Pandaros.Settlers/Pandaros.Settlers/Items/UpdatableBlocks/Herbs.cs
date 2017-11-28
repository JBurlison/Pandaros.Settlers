using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz;
using Pipliz.JSON;
using UpdatableBlocks;

namespace Pandaros.Settlers.Items.UpdatableBlocks
{
    class Herbs : IUpdatableBlock
    {
        public const string NAME = "herbs";

        private enum HerbType : byte
        {
            Unknown,
            Invalid,
            Stage1
        }

        public Vector3Int Location { get; set; }

        private long lastUpdateRealMilliseconds;
        private float growthAccumulated;
        private double lastUpdateTimecycleHours = -1.0;
        private HerbType type;

        public Herbs(Vector3Int location)
        {
            lastUpdateRealMilliseconds = Time.MillisecondsSinceStart;
            Location = location;
        }

        public Herbs(Vector3Int location, float growth) : this(location)
		{
            growthAccumulated = growth;
        }

        public void OnRemove()
        {
            type = HerbType.Invalid;
        }

        public long GetNextUpdate()
        {
            if (type == HerbType.Invalid)
            {
                return 0L;
            }

            return this.lastUpdateRealMilliseconds + (long)Pipliz.Random.Next(2000, 4000);
        }

        private void AccumalateGrowth()
        {
            double totalTime = TimeCycle.TotalTime;
            if (this.lastUpdateTimecycleHours >= 0.0 && !TimeCycle.IsDay)
            {
                float num = (float)(totalTime - this.lastUpdateTimecycleHours);
                if (num < TimeCycle.DayLength)
                {
                    this.growthAccumulated += num;
                }
            }
            this.lastUpdateTimecycleHours = totalTime;
        }

        private void RefreshType()
        {
            ushort num;
            if (World.TryGetTypeAt(this.Location, out num))
            {
                if (num == Jobs.HerbalistRegister.HerbStage1.ItemIndex)
                {
                    this.type = HerbType.Stage1;
                }
                else
                {
                    this.type = HerbType.Invalid;
                }
            }
            else
            {
                this.type = HerbType.Unknown;
            }
        }

        public bool Update()
        {
            this.lastUpdateRealMilliseconds = Time.MillisecondsSinceStart;

            if (this.type == HerbType.Unknown)
            {
                this.RefreshType();
                if (this.type == HerbType.Unknown)
                {
                    return true;
                }
                if (this.type == HerbType.Invalid)
                {
                    return false;
                }
            }
            this.AccumalateGrowth();
            if (this.type == HerbType.Stage1 && this.growthAccumulated > 5f)
            {
                if (!ServerManager.TryChangeBlock(this.Location, Jobs.HerbalistRegister.HerbStage2.ItemIndex, ServerManager.SetBlockFlags.Default))
                {
                    this.type = HerbType.Unknown;
                }
                else
                {
                    this.type = HerbType.Invalid;
                }
                this.growthAccumulated = 0f;
            }
            return this.type != HerbType.Invalid;
        }

        public void SaveJSON(JSONNode node)
        {
            if (this.type != HerbType.Invalid)
            {
                this.AccumalateGrowth();
                JSONNode jSONNode;
                if (!node.TryGetChild(NAME, out jSONNode))
                {
                    jSONNode = new JSONNode(NodeType.Array);
                    node[NAME] = jSONNode;
                }
                jSONNode.AddToArray(new JSONNode(NodeType.Object).SetAs<JSONNode>("location", (JSONNode)this.Location).SetAs<float>("growth", this.growthAccumulated));
            }
        }

        public static void OnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            ushort num;

            if (World.TryGetTypeAt(position.Add(0, -1, 0), out num) && ItemTypes.GetType(num).IsFertile)
            {
                BlockTracker.Add(new Herbs(position));
            }
        }

        public static void OnRemove(Vector3Int position, ushort type, Players.Player player)
        {
            BlockTracker.Remove(position);
        }

        public static void OnChange(Vector3Int position, ushort typeOld, ushort typeNew, Players.Player player)
        {
            Herbs herb = BlockTracker.Get<Herbs>(position);

            if (herb != null)
            {
                if (typeNew == Jobs.HerbalistRegister.HerbStage1.ItemIndex)
                {
                    herb.type = HerbType.Stage1;
                }
                else
                {
                    herb.type = HerbType.Invalid;
                }
            }
        }

        public static void Load(JSONNode array)
        {
            if (array == null)
            {
                return;
            }
            foreach (JSONNode current in array.LoopArray())
            {
                try
                {
                    BlockTracker.Add(new Herbs((Vector3Int)current["location"], current["growth"].GetAs<float>()));
                }
                catch (Exception exc)
                {
                    Log.WriteException("Exception loading a herb block;", exc);
                }
            }
        }
    }
}
