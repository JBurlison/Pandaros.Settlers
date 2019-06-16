using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Models;
using Science;

namespace Pandaros.Settlers.Research
{
    public class AddReducedWaste : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BERRY.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BREAD.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 250 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 5;

        public float BaseValue => 0.001f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".ReducedWaste";

        public void OnRegister()
        {
            
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.UpdateFoodUse(ColonyState.GetColonyState(e.Manager.Colony));
        }
    }

    public class ColonistHealth : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.LINEN.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id, 10)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 300 },
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 5;

        public float BaseValue => 1;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".ColonistHealth";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            var maxHp = e.Manager.Colony.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MAXCOLONISTHP", e.Manager.Colony.NPCHealthMax);
            e.Manager.Colony.NPCHealthMax = maxHp + e.Research.Level * e.Research.BaseValue;

            foreach (var follower in e.Manager.Colony.Followers)
                follower.health = e.Manager.Colony.NPCHealthMax;
        }
    }

    public class MaxSettlers : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.PLASTERBLOCK.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BED.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 300 },
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.SETTLERCHANCE1,
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE,
                    ColonyBuiltIn.Research.BRONZEMINTING
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 10;

        public float BaseValue => 1;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".MaxSettlers";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
        }
    }

    public class MinSettlers : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BRICKS.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BED.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 300 },
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.SETTLERCHANCE1,
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE,
                    ColonyBuiltIn.Research.BRONZEMINTING
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 10;

        public float BaseValue => 1;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".MinSettlers";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
        }
    }

    public class SettlerChance : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.TORCH.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.BED.Id, 5),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 20)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 100 },
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.BANNERRADIUS2,
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE,
                    ColonyBuiltIn.Research.BRONZEMINTING
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 5;

        public float BaseValue => 0.1f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".SettlerChance";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
        }
    }

    public class SkilledLaborer : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id, 2),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 30)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 500 },
                    new HappinessCondition() { Threshold = 150 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.BANNERRADIUS5,
                    ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                    ColonyBuiltIn.Research.FINERYFORGE,
                    SettlersBuiltIn.Research.SETTLERCHANCE2,
                    SettlersBuiltIn.Research.REDUCEDWASTE2
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 10;

        public float BaseValue => 0.2f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".SkilledLaborer";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
           
        }
    }

    public class NumberSkilledLaborer : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id, 30),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.TIN.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 30)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 500 },
                    new HappinessCondition() { Threshold = 150 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.SKILLEDLABORER1
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public int NumberOfLevels => 5;

        public float BaseValue => 1f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".NumberSkilledLaborer";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
        }
    }
}
