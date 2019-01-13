using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Monsters
{
    public interface IMonsterNPCType
    {
        IMonsterNPCData data { get; }
        string albedo { get; }
        string initialHealth { get; }
        string movementSpeed { get; }
        string normal { get; }
        string punchCooldownMS { get; }
        string punchDamage { get; }
        string special { get; }
    }

    public interface IMonsterNPCData
    {
        string keyName { get; }
        string npcType { get; }
        string printName { get; }
    }
}
