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
    public class DailyMonsterReport
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterSpawned, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.OnMonsterSpawned")]
        public void OnMonsterSpawned(IMonster monster)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterDied, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.MonsterDied")]
        public void MonsterDied(IMonster monster)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.MonsterHit")]
        public void MonsterHit(IMonster monster, OnHitData hitData)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.OnNPCHit")]
        public void OnNPCHit(NPCBase npc, OnHitData hitData)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.OnNPCDied")]
        public void OnNPCDied(NPCBase npc)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Stats.DailyMonsterReport.OnPlayerHit")]
        public void OnPlayerHit(Player player, OnHitData hitData)
        {

        }
    }
}
