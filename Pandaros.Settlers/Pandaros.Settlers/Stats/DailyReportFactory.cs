using Monsters;
using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModLoader;
using static Players;

namespace Pandaros.Settlers.Stats
{
    [ModLoader.ModManager]
    public class DailyReportFactory
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterSpawned, GameLoader.NAMESPACE + ".Stats.DailyReport.OnMonsterSpawned")]
        public void OnMonsterSpawned(IMonster monster)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterDied, GameLoader.NAMESPACE + ".Stats.DailyReport.MonsterDied")]
        public void MonsterDied(IMonster monster)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Stats.DailyReport.MonsterHit")]
        public void MonsterHit(IMonster monster, OnHitData hitData)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Stats.DailyReport.OnNPCHit")]
        public void OnNPCHit(NPCBase npc, OnHitData hitData)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".Stats.DailyReport.OnNPCDied")]
        public void OnNPCDied(NPCBase npc)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Stats.DailyReport.OnPlayerHit")]
        public void OnPlayerHit(Player player, OnHitData hitData)
        {

        }
    }
}
