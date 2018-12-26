using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Research
{
    public class MaxMagicItems : IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
        {
            {  ItemId.GetItemId(Items.PlayerMagicItems.HealthBooster.NAME), 1 }
        };

        public int NumberOfLevels => 2;

        public float BaseValue => 1;

        public List<string> Dependancies => new List<string>()
        {
            Jobs.SorcererRegister.JOB_NAME + "1"
        };

        public int BaseIterationCount => 20;

        public bool AddLevelToName => true;

        public string Name => GameLoader.NAMESPACE + ".MaxMagicItems";

        public void OnRegister()
        {
            
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            foreach (var p in e.Manager.Colony.Owners)
            {
                var ps = PlayerState.GetPlayerState(p);
                ps.MaxMagicItems++;
            }
        }
    }
}
