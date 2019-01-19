using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{

    public class JobResearch
    {
        public const string MerchantTraining = "MerchantTraining";
        public const string TailorTraining = "TailorTraining";
        public const string BloomeryTraining = "BloomeryTraining";
        public const string FineryForgeTraining = "FineryForgeTraining";
        public const string FurnaceTraining = "FurnaceTraining";
        public const string GrinderTraining = "GrinderTraining";
        public const string GunSmithTraining = "GunSmithTraining";
        public const string KilnTraining = "KilnTraining";
        public const string MetalSmithTraining = "MetalSmithTraining";
        public const string MintTraining = "MintTraining";
        public const string OvenTraining = "OvenTraining";
        public const string WoodcutterTraining = "WoodcutterTraining";
        public const string WorkBenchTraining = "WorkBenchTraining";
        public const string MasterOfAll = "MasterOfAll";

        private const string SCIENCEBAGREQ = ColonyBuiltIn.Research.SCIENCEBAGBASIC;
        private const int BAG_COST = 2;
        private const int COIN_COST = 5;

        private static readonly Dictionary<string, float> _defaultValues = new Dictionary<string, float>();

        /// <summary>
        ///     Sets the default crafting speeds for a job that <see cref="DecreaseCraftingCooldown"/> will use to decrease a crafters cooldown by.
        /// </summary>
        /// <param name="jobNmae"></param>
        public static void SetCraftingCooldown(string jobNmae)
        {
            if (ServerManager.BlockEntityCallbacks.TryGetCraftJobSettings(jobNmae, out var settings))
                _defaultValues[jobNmae] = settings.CraftingCooldown;
        }

        /// <summary>
        ///     Decreases based on a % amount given as float ex: .5 would be 50 percent.
        /// </summary>
        /// <param name="jobNmae"></param>
        /// <param name="amount"></param>
        public static void DecreaseCraftingCooldown(string jobNmae, float amount)
        {
            if (ServerManager.BlockEntityCallbacks.TryGetCraftJobSettings(jobNmae, out var settings))
                settings.CraftingCooldown = _defaultValues[jobNmae] - _defaultValues[jobNmae] * amount; ;
        }

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

            public string name => MerchantTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.MERCHANT);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.MERCHANT, e.Research.Value);
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

            public string name => TailorTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.TAILOR);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.TAILOR, e.Research.Value);
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

            public string name => BloomeryTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.BLOOMERYJOB);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.BLOOMERYJOB, e.Research.Value);
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

            public string name => FineryForgeTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.FINERYFORGEJOB);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.FINERYFORGEJOB, e.Research.Value);
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

            public string name => FurnaceTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.SMELTER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.SMELTER, e.Research.Value);
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

            public string name => GrinderTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.GRINDER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.GRINDER, e.Research.Value);
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

            public string name => GunSmithTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.GUNSMITHJOB);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.GUNSMITHJOB, e.Research.Value);
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

            public string name => KilnTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.KILNJOB);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.KILNJOB, e.Research.Value);
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

            public string name => MetalSmithTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.METALSMITHJOB);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.METALSMITHJOB, e.Research.Value);
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

            public string name => MintTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.MINTER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.MINTER, e.Research.Value);
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

            public string name => OvenTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.BAKER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.BAKER, e.Research.Value);
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

            public string name => WoodcutterTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.WOODCUTTER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.WOODCUTTER, e.Research.Value);
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

            public string name => WorkBenchTraining;

            public void OnRegister()
            {
                SetCraftingCooldown(ColonyBuiltIn.NpcTypes.CRAFTER);
            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                DecreaseCraftingCooldown(ColonyBuiltIn.NpcTypes.CRAFTER, e.Research.Value);
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
                PandaResearch.GetResearchKey(MerchantTraining + "5"),
                PandaResearch.GetResearchKey(TailorTraining + "5"),
                PandaResearch.GetResearchKey(BloomeryTraining + "5"),
                PandaResearch.GetResearchKey(FineryForgeTraining + "5"),
                PandaResearch.GetResearchKey(FurnaceTraining + "5"),
                PandaResearch.GetResearchKey(GrinderTraining + "5"),
                PandaResearch.GetResearchKey(GunSmithTraining + "5"),
                PandaResearch.GetResearchKey(KilnTraining + "5"),
                PandaResearch.GetResearchKey(MetalSmithTraining + "5"),
                PandaResearch.GetResearchKey(MintTraining + "5"),
                PandaResearch.GetResearchKey(OvenTraining + "5"),
                PandaResearch.GetResearchKey(WoodcutterTraining + "5"),
                PandaResearch.GetResearchKey(WorkBenchTraining + "5")
            };

            public int BaseIterationCount => 10;

            public bool AddLevelToName => true;

            public string name => MasterOfAll;

            public void OnRegister()
            {

            }

            public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                SetCooldown(e, ColonyBuiltIn.NpcTypes.CRAFTER, WorkBenchTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.WOODCUTTER, WoodcutterTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.BAKER, OvenTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.MINTER, MintTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.METALSMITHJOB, MetalSmithTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.KILNJOB, KilnTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.GUNSMITHJOB, GunSmithTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.GRINDER, GrinderTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.SMELTER, FurnaceTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.FINERYFORGEJOB, FineryForgeTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.BLOOMERYJOB, BloomeryTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.TAILOR, TailorTraining);
                SetCooldown(e, ColonyBuiltIn.NpcTypes.MERCHANT, MerchantTraining);
            }

            private static void SetCooldown(ResearchCompleteEventArgs e, string name, string researchKey)
            {
                if (ServerManager.BlockEntityCallbacks.TryGetCraftJobSettings(name, out var settings))
                    settings.CraftingCooldown = _defaultValues[name] - _defaultValues[name] * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(researchKey), 0f));

            }
        }
    }
}
