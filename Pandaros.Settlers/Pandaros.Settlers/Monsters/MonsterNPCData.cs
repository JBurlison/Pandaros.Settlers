using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipliz.JSON;
using Newtonsoft.Json;

namespace Pandaros.Settlers.Monsters
{
    public class MonsterNPCData : IMonsterNPCData
    {
        public string keyName { get; set; }

        public string npcType { get; set; }

        public string printName { get; set; }
    }
}
