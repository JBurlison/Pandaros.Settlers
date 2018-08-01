using System;
using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pipliz.JSON;

namespace Pandaros.Settlers.Entities
{
    public class SettlerInventory
    {
        public SettlerInventory(NPCBase id)
        {
            SettlerId   = id.ID;
            NPC = id;
            SettlerName = NameGenerator.GetName();
            SetupArmor();
        }

        public SettlerInventory(JSONNode baseNode, NPCBase nPC)
        {
            if (baseNode.TryGetAs<int>(nameof(SettlerId), out var settlerId))
            {
                NPC = nPC;
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

                foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                    Armor[armorType].FromJsonNode(armorType.ToString(), baseNode);
            }
        }

        public double MagicItemUpdateTime { get; set; } = Pipliz.Time.SecondsSinceStartDouble + Pipliz.Random.Next(1, 10);

        public int SettlerId { get; set; }
        
        public NPCBase NPC { get; private set; }

        public string SettlerName { get; set; }

        public Dictionary<string, float> JobSkills { get; set; } = new Dictionary<string, float>();

        public Dictionary<string, int> JobItteration { get; set; } = new Dictionary<string, int>();

        public EventedDictionary<ArmorFactory.ArmorSlot, ItemState> Armor { get; set; } =  new EventedDictionary<ArmorFactory.ArmorSlot, ItemState>();

        public ItemState Weapon { get; set; } = new ItemState();

        private void SetupArmor()
        {
            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                Armor.Add(armorType, new ItemState());

            Armor.OnDictionaryChanged += Armor_OnDictionaryChanged;
        }

        // TODO: apply armor
        private void Armor_OnDictionaryChanged(object sender, DictionaryChangedEventArgs<ArmorFactory.ArmorSlot, ItemState> e)
        {
            switch (e.EventType)
            {
                case DictionaryEventType.AddItem:
                    
                    break;

                case DictionaryEventType.ChangeItem:

                    break;

                case DictionaryEventType.RemoveItem:

                    break;
            }
        }

        public JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            baseNode[nameof(SettlerId)]   = new JSONNode(SettlerId);
            baseNode[nameof(SettlerName)] = new JSONNode(SettlerName);

            var skills = new JSONNode();

            foreach (var job in JobSkills)
                skills[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(JobSkills)] = skills;

            var itterations = new JSONNode();

            foreach (var job in JobItteration)
                itterations[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(itterations)] = itterations;

            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                baseNode[armorType.ToString()] = Armor[armorType].ToJsonNode();

            return baseNode;
        }

        public static SettlerInventory GetSettlerInventory(NPCBase npc)
        {
            var tempVals = npc.GetTempValues(true);

            if (!tempVals.TryGet(GameLoader.SETTLER_INV, out SettlerInventory inv))
            {
                inv = new SettlerInventory(npc);
                tempVals.Set(GameLoader.SETTLER_INV, inv);
            }

            return inv;
        }

        
    }
}