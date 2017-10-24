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
        public int SettlerId { get; set; }

        public string SettlerName { get; set; }

        public Dictionary<string, float> JobSkills { get; set; } = new Dictionary<string, float>();

        public Dictionary<string, int> JobItteration { get; set; } = new Dictionary<string, int>();

        public SettlerInventory(int id)
        {
            SettlerId = id;
            SettlerName = NameGenerator.GetName();
        }

        public SettlerInventory(JSONNode baseNode)
        {
            if (baseNode.TryGetAs<int>(nameof(SettlerId), out var settlerId))
            {
                SettlerId = settlerId;

                baseNode.TryGetAs<string>(nameof(SettlerName), out var name);
                SettlerName = name;
 
                if (baseNode.TryGetAs(nameof(JobSkills), out JSONNode skills))
                    foreach (var skill in skills.LoopObject())
                        JobSkills[skill.Key] = skill.Value.GetAs<float>();

                if (baseNode.TryGetAs(nameof(JobItteration), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        JobItteration[skill.Key] = skill.Value.GetAs<int>();
            }
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

            return baseNode;
        }
    }
}
