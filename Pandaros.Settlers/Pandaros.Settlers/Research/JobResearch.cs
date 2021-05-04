using Pandaros.API;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Research;
using Science;
using Shared;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{

    public class JobResearch
    {
        private const string SCIENCEBAGREQ = ColonyBuiltIn.Research.TECHNOLOGISTTABLE;

        public class MerchantTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.MERCHANT;

            public string IconDirectory => GameLoader.ICON_PATH;

            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }


            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class TailorTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.LINENBAG.Id, 3),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.TAILOR;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class BloomeryTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Id, 3 ),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE

                    }
                }
            };

            public int BaseIterationCount => 10;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.BLOOMERYJOB;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class FineryForgeTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOWBOLT.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.FINERYFORGEJOB;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class FurnaceTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRICKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
                
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.SMELTER;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class GrinderTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.FLOUR.Id, 5)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }

            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.GRINDER;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class GunSmithTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.LEADBULLET.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
         
            };

            public int BaseIterationCount => 10;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.GUNSMITHJOB;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class KilnTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.CHARCOAL.Id, 3),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
                
            };

            public int BaseIterationCount => 10;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.KILNJOB;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MetalSmithTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id, 3),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.METALSMITHJOB;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MintTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id, 3)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.MINTER;
            public string IconDirectory => GameLoader.ICON_PATH;

            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }
            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class OvenTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BREAD.Id, 3),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.BAKER;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class WoodcutterTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.WOODCUTTER;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }
        public class WorkBenchTrainingResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.WORKBENCH.Id, 3 ),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id)
                    }
                }
            };
            public Dictionary<int, List<IResearchableCondition>> Conditions => null;
            public int NumberOfLevels => 5;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public float BaseValue => 0.05f;

            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SCIENCEBAGREQ,
                        ColonyBuiltIn.Research.FINERYFORGE
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.CRAFTER;
            public string IconDirectory => GameLoader.ICON_PATH;

            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {
                
            }

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MasterOfAllResearch : IPandaResearch
        {
            public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGMILITARY.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10),
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
                        new ColonistCountCondition() { Threshold = 1000 }
                    }
                }
            };
            public int NumberOfLevels => 10;
            public Dictionary<int, List<RecipeUnlock>> Unlocks => null;
            public float BaseValue => 0.03f;
            public string IconDirectory => GameLoader.ICON_PATH;
            public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        ColonyBuiltIn.NpcTypes.MERCHANT + "5",
                        ColonyBuiltIn.NpcTypes.TAILOR + "5",
                        ColonyBuiltIn.NpcTypes.BLOOMERYJOB + "5",
                        ColonyBuiltIn.NpcTypes.FINERYFORGEJOB + "5",
                        ColonyBuiltIn.NpcTypes.SMELTER + "5",
                        ColonyBuiltIn.NpcTypes.GRINDER + "5",
                        ColonyBuiltIn.NpcTypes.GUNSMITHJOB + "5",
                        ColonyBuiltIn.NpcTypes.KILNJOB + "5",
                        ColonyBuiltIn.NpcTypes.METALSMITHJOB + "5",
                        ColonyBuiltIn.NpcTypes.MINTER + "5",
                        ColonyBuiltIn.NpcTypes.BAKER + "5",
                        ColonyBuiltIn.NpcTypes.WOODCUTTER + "5",
                        ColonyBuiltIn.NpcTypes.CRAFTER + "5"
                    }
                }
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => GameLoader.NAMESPACE + ".MasterOfAll";

            public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

            public void BeforeRegister()
            {

            }

            public void OnRegister()
            {

            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                ColonistManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }
    }
}
