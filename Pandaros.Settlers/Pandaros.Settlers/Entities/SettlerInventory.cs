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

                var skills = baseNode[nameof(JobSkills)];

                foreach (var skill in skills.LoopObject())
                    JobSkills[skill.Key] = skill.Value.GetAs<float>();
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

            return baseNode;
        }
    }
}
