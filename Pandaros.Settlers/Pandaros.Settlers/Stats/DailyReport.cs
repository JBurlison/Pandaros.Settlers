using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Stats
{
    public class DailyReport
    {
        public int ColonyId { get; set; }
        public long ColonistDeathFromMonsters { get; set; }
        public long ColonistDeathFromPlayers { get; set; }
        public long ColonistDeathFromStarvation { get; set; }
        public long ColonistLeavingFromNoBed { get; set; }
        public long ColonistLeavingFromNoJob { get; set; }
        public long ColonistsBought { get; set; }
        public long ColonistDamageTaken { get; set; }
        public long ColonistDamageDone { get; set; }
        public long TurretDamageDone { get; set; }
        public long SettlersJoined { get; set; }
        public long MonstersSpawned { get; set; }
        public long MonstersKilled { get; set; }
        public long MonsterDamageTaken { get; set; }
        public long BossesSpawned { get; set; }
        public long BossesKilled { get; set; }
        public long BossDamageTaken { get; set; }
    }
}
