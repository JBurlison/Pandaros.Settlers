using Pandaros.Settlers.AI;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Entities
{
    public class SettlerInventory
    {
        [Serializable]
        public class ItemState
        {
            public ushort Id { get; set; }

            public int Durability { get; set; }

            public bool IsEmpty()
            {
                return Id == default(ushort);
            }

            public void FromJsonNode(string nodeName, JSONNode node)
            {
                if (node.TryGetAs(nodeName, out JSONNode stateNode))
                {
                    if (stateNode.TryGetAs(nameof(Id), out ushort id))
                        Id = id;

                    if (stateNode.TryGetAs(nameof(Durability), out int durablility))
                        Durability = durablility;
                }
            }

            public JSONNode ToJsonNode()
            {
                var baseNode = new JSONNode();

                baseNode[nameof(Id)] = new JSONNode(Id);
                baseNode[nameof(Durability)] = new JSONNode(Durability);

                return baseNode;
            }
        }

        public int SettlerId { get; set; }

        public string SettlerName { get; set; }

        public Dictionary<string, float> JobSkills { get; set; } = new Dictionary<string, float>();

        public Dictionary<string, int> JobItteration { get; set; } = new Dictionary<string, int>();

        public Dictionary<Items.Armor.ArmorSlot, ItemState> Armor { get; set; } = new Dictionary<Items.Armor.ArmorSlot, ItemState>();

        public ItemState Weapon { get; set; } = new ItemState();

        public SettlerInventory(int id)
        {
            SettlerId = id;
            SettlerName = NameGenerator.GetName();
            SetupArmor();
        }

        public SettlerInventory(JSONNode baseNode)
        {
            if (baseNode.TryGetAs<int>(nameof(SettlerId), out var settlerId))
            {
                SetupArmor();
                SettlerId = settlerId;

                baseNode.TryGetAs<string>(nameof(SettlerName), out var name);
                SettlerName = name;
 
                if (baseNode.TryGetAs(nameof(JobSkills), out JSONNode skills))
                    foreach (var skill in skills.LoopObject())
                        JobSkills[skill.Key] = skill.Value.GetAs<float>();

                if (baseNode.TryGetAs(nameof(JobItteration), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        JobItteration[skill.Key] = skill.Value.GetAs<int>();

                foreach (Items.Armor.ArmorSlot armorType in Items.Armor.ArmorSlotEnum)
                    Armor[armorType].FromJsonNode(armorType.ToString(), baseNode);
            }
        }

        private void SetupArmor()
        {
            foreach (Items.Armor.ArmorSlot armorType in Items.Armor.ArmorSlotEnum)
                Armor.Add(armorType, new ItemState());
        }

        public JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            baseNode[nameof(SettlerId)] = new JSONNode(SettlerId);
            baseNode[nameof(SettlerName)] = new JSONNode(SettlerName);

            var skills = new JSONNode();

            foreach (var job in JobSkills)
                skills[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(JobSkills)] = skills;
            
            var itterations = new JSONNode();

            foreach (var job in JobItteration)
                itterations[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(itterations)] = itterations;

            foreach (Items.Armor.ArmorSlot armorType in Items.Armor.ArmorSlotEnum)
                baseNode[armorType.ToString()] = Armor[armorType].ToJsonNode();

            return baseNode;
        }

        public static SettlerInventory GetSettlerInventory(NPC.NPCBase npc)
        {
            var tempVals = npc.GetTempValues(true);
            if (!tempVals.TryGet(GameLoader.SETTLER_INV, out SettlerInventory inv))
            {
                inv = new SettlerInventory(npc.ID);
                tempVals.Set(GameLoader.SETTLER_INV, inv);
            }
            return inv;
        }
    }
}
