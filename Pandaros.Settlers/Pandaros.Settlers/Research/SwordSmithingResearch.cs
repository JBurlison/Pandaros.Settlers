using Pandaros.API;
using Pandaros.API.Research;
using Science;
using Shared;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    public class SwordSmithingResearch : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                1,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id, 3)
                }
            },
            {
                2,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id, 3),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEAXE.Id, 1)
                }
            },
            {
                3,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Id, 3),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONSWORD.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGMILITARY.Id, 1)
                }
            },
            {
                4,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Id, 3),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGMILITARY.Id, 1)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                3,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 150 }
                }
            },
            {
                4,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 250 }
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.COPPERSWORD, ERecipeUnlockType.Recipe)
                }
            },
            {
                2,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.BRONZESWORD, ERecipeUnlockType.Recipe)
                }
            },
            {
                3,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.IRONSWORD, ERecipeUnlockType.Recipe)
                }
            },
            {
                4,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.STEELSWORD, ERecipeUnlockType.Recipe)
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                1,
                new List<string>()
                {
                    ColonyBuiltIn.Research.BRONZEANVIL
                }
            },
            {
                3,
                new List<string>()
                {
                    ColonyBuiltIn.Research.FINERYFORGE,
                    ColonyBuiltIn.Research.SCIENCEBAGMILITARY
                }
            }
        };

        public int NumberOfLevels => 4;

        public float BaseValue => 1;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".SwordSmithing";

        public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

        public void BeforeRegister()
        {

        }

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {

        }
    }
}
