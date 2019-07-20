using Pandaros.API.Entities;
using Pandaros.API.Research;
using Science;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    public class MaxMagicItems : IPandaResearch
    {
        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem("Pandaros.Settlers.Healthbooster")
                }
            }
        };

        public int NumberOfLevels => 2;

        public float BaseValue => 1;

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.SORCERER1
                }
            }
        };

        public int BaseIterationCount => 20;

        public bool AddLevelToName => true;

        public string name =>  GameLoader.NAMESPACE + ".MaxMagicItems";
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public Dictionary<int, List<IResearchableCondition>> Conditions => null;

        public void OnRegister()
        {
            
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            foreach (var p in e.Manager.Colony.Owners)
            {
                var ps = PlayerState.GetPlayerState(p);
                ps.MaxMagicItems++;
                ps.ResizeMaxMagicItems();
            }
        }
    }
}
