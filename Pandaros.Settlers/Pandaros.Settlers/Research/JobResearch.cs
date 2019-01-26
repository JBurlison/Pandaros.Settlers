using BlockTypes;
using Pandaros.Settlers.ColonyManager;
using Pandaros.Settlers.Managers;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{

    public class JobResearch
    {
        public const string MasterOfAll = "MasterOfAll";

        private const string SCIENCEBAGREQ = ColonyBuiltIn.Research.SCIENCEBAGBASIC;
        private const int BAG_COST = 2;
        private const int COIN_COST = 5;

        public class MerchantTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.MERCHANT;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class TailorTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.LinenBag, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.TAILOR;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class BloomeryTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.IronWrought, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.BLOOMERYJOB;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class FineryForgeTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.CrossbowBolt, 5 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.FINERYFORGEJOB;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class FurnaceTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.Bricks, 5 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.SMELTER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class GrinderTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.Flour, 5 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.GRINDER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class GunSmithTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.LeadBullet, 5 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.GUNSMITHJOB;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class KilnTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.Charcoal, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.KILNJOB;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MetalSmithTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.BronzePlate, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.METALSMITHJOB;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MintTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.MINTER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class OvenTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.Bread, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.BAKER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class WoodcutterTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.Planks, 5 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.WOODCUTTER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }
        public class WorkBenchTrainingResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.WorkBench, 3 },
                { BuiltinBlocks.ScienceBagBasic, BAG_COST },
                { BuiltinBlocks.BronzeCoin, COIN_COST }
            };

            public int NumberOfLevels => 5;

            public float BaseValue => 0.05f;

            public List<string> Dependancies => new List<string>()
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.FINERYFORGE
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => ColonyBuiltIn.NpcTypes.CRAFTER;

            public void OnRegister()
            {
                
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }

        public class MasterOfAllResearch : IPandaResearch
        {
            public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
            {
                { BuiltinBlocks.ScienceBagBasic, 1 },
                { BuiltinBlocks.ScienceBagLife, 1 },
                { BuiltinBlocks.ScienceBagAdvanced, 1 },
                { BuiltinBlocks.ScienceBagColony, 1 },
                { BuiltinBlocks.ScienceBagMilitary, 1 },
                { BuiltinBlocks.GoldCoin, 10 },
                { BuiltinBlocks.BronzeCoin, 10 }
            };

            public int NumberOfLevels => 10;

            public float BaseValue => 0.03f;

            public List<string> Dependancies => new List<string>()
            {
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.MERCHANT + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.TAILOR + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.BLOOMERYJOB + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.FINERYFORGEJOB + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.SMELTER + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.GRINDER + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.GUNSMITHJOB + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.KILNJOB + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.METALSMITHJOB + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.MINTER + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.BAKER + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.WOODCUTTER + "5"),
                PandaResearch.GetResearchKey(ColonyBuiltIn.NpcTypes.CRAFTER + "5")
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => MasterOfAll;

            public void OnRegister()
            {

            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
            }
        }
    }
}
