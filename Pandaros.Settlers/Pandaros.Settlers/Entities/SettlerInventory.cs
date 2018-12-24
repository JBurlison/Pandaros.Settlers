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

                if (baseNode.TryGetAs(nameof(BonusProcs), out JSONNode skills))
                    foreach (var skill in skills.LoopObject())
                        if (ushort.TryParse(skill.Key, out ushort item))
                        BonusProcs[item] = skill.Value.GetAs<long>();

                if (baseNode.TryGetAs(nameof(Stats), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        Stats[skill.Key] = skill.Value.GetAs<double>();

                foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                    Armor[armorType].FromJsonNode(armorType.ToString(), baseNode);
            }
        }

        public double MagicItemUpdateTime { get; set; } = Pipliz.Time.SecondsSinceStartDouble + Pipliz.Random.Next(1, 10);

        public int SettlerId { get; set; }
        
        public NPCBase NPC { get; private set; }

        public string SettlerName { get; set; }

        public Dictionary<ushort, long> BonusProcs { get; set; } = new Dictionary<ushort, long>();

        public Dictionary<string, double> Stats { get; set; } = new Dictionary<string, double>();

        public EventedDictionary<ArmorFactory.ArmorSlot, ItemState> Armor { get; set; } =  new EventedDictionary<ArmorFactory.ArmorSlot, ItemState>();

        public ItemState Weapon { get; set; } = new ItemState();

        public void IncrimentStat(string name, double count = 1)
        {
            if (!Stats.ContainsKey(name))
                Stats.Add(name, 0);

            Stats[name] += count;
        }

        private void SetupArmor()
        {
            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                Armor.Add(armorType, new ItemState());

            Armor.OnDictionaryChanged += Armor_OnDictionaryChanged;
        }

        public void AddBonusProc(ushort item, long count = 1)
        {
            if (!BonusProcs.ContainsKey(item))
                BonusProcs.Add(item, 0);

            BonusProcs[item] += count;
        }

        public float GetSkillModifier()
        {
            var totalSkill = 0f;

            if (NPC.CustomData.TryGetAs(GameLoader.ALL_SKILLS, out float allSkill))
                totalSkill = allSkill;

            foreach (var armor in Armor)
                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                    totalSkill += a.Skilled;

            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(Weapon.Id, out var w))
                totalSkill += w.Skilled;

            return totalSkill;
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

            foreach (var job in BonusProcs)
                skills[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(BonusProcs)] = skills;

            var statsNode = new JSONNode();

            foreach (var job in Stats)
                statsNode[job.Key] = new JSONNode(job.Value);

            baseNode[nameof(statsNode)] = statsNode;

            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                baseNode[armorType.ToString()] = Armor[armorType].ToJsonNode();

            return baseNode;
        }

        public static SettlerInventory GetSettlerInventory(NPCBase npc)
        {
            SettlerInventory inv = null;

            if (npc.CustomData == null)
                npc.CustomData = new JSONNode();

            try
            {
                if (!npc.CustomData.TryGetAs(GameLoader.SETTLER_INV, out inv) || inv == null)
                {
                    inv = new SettlerInventory(npc);
                    npc.CustomData.SetAs(GameLoader.SETTLER_INV, inv);
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            if (inv == null)
            {
                inv = new SettlerInventory(npc);
            }

            return inv;
        }

        
    }
}