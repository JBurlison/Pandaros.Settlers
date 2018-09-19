using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    [ModLoader.ModManager]
    public static class JobResearch
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

        private const string SCIENCEBAGREQ = ColonyBuiltIn.Research.ScienceBagBasic;
        private const int BAG_COST = 2;
        private const int COIN_COST = 5;

        private static readonly Dictionary<string, float> _defaultValues = new Dictionary<string, float>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.JobResearch.OnAddResearchables")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();
            AddMerchantTraining(researchDic);
            AddTailorTraining(researchDic);
            AddBloomeryTraining(researchDic);
            AddFineryForgeTraining(researchDic);
            AddFurnaceTraining(researchDic);
            AddGrinderTraining(researchDic);
            AddGunSmithTraining(researchDic);
            AddKilnTraining(researchDic);
            AddMetalSmithTraining(researchDic);
            AddMintTraining(researchDic);
            AddOvenTraining(researchDic);
            AddWoodcutterTraining(researchDic);
            AddWorkBenchTraining(researchDic);
            AddMasterOfAll(researchDic);
        }

        private static void AddMerchantTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.MERCHANT] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MERCHANT).CraftingCooldown;

            researchDic.Clear();

            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MerchantTraining, .05f, requirements);
            research.ResearchComplete += TrainedMerchant_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MerchantTraining, .05f);
                research.ResearchComplete += TrainedMerchant_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedMerchant_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MERCHANT).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.MERCHANT] - _defaultValues[ColonyBuiltIn.Jobs.MERCHANT] * e.Research.Value;
        }

        private static void AddTailorTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.TAILOR] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.TAILOR).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LinenBag, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, TailorTraining, .05f, requirements);
            research.ResearchComplete += TrainedTailor_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, TailorTraining, .05f);
                research.ResearchComplete += TrainedTailor_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedTailor_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.TAILOR).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.TAILOR] -
                                               _defaultValues[ColonyBuiltIn.Jobs.TAILOR] * e.Research.Value;
        }

        private static void AddBloomeryTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.BLOOMERYJOB] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BLOOMERYJOB).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronWrought, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, BloomeryTraining, .05f, requirements);
            research.ResearchComplete += BloomeryTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, BloomeryTraining, .05f);
                research.ResearchComplete += BloomeryTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void BloomeryTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BLOOMERYJOB).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.BLOOMERYJOB] -
                                                 _defaultValues[ColonyBuiltIn.Jobs.BLOOMERYJOB] * e.Research.Value;
        }

        private static void AddFineryForgeTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.FINERYFORGEJOB] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.FINERYFORGEJOB).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CrossbowBolt, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, FineryForgeTraining, .05f, requirements);
            research.ResearchComplete += FineryForgeTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, FineryForgeTraining, .05f);
                research.ResearchComplete += FineryForgeTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FineryForgeTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.FINERYFORGEJOB).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.FINERYFORGEJOB] -
                                                    _defaultValues[ColonyBuiltIn.Jobs.FINERYFORGEJOB] * e.Research.Value;
        }

        private static void AddFurnaceTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.SMELTER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.SMELTER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bricks, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, FurnaceTraining, .05f, requirements);
            research.ResearchComplete += FurnaceTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, FurnaceTraining, .05f);
                research.ResearchComplete += FurnaceTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FurnaceTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.SMELTER).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.SMELTER] -
                                                _defaultValues[ColonyBuiltIn.Jobs.SMELTER] * e.Research.Value;
        }

        private static void AddGrinderTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.GRINDER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GRINDER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Flour, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, GrinderTraining, .05f, requirements);
            research.ResearchComplete += GrinderTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, GrinderTraining, .05f);
                research.ResearchComplete += GrinderTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GrinderTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GRINDER).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.GRINDER] -
                                                _defaultValues[ColonyBuiltIn.Jobs.GRINDER] * e.Research.Value;
        }

        private static void AddGunSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.GUNSMITHJOB] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GUNSMITHJOB).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, GunSmithTraining, .05f, requirements);
            research.ResearchComplete += GunSmithTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, GunSmithTraining, .05f);
                research.ResearchComplete += GunSmithTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GunSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GUNSMITHJOB).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.GUNSMITHJOB] -
                                                 _defaultValues[ColonyBuiltIn.Jobs.GUNSMITHJOB] * e.Research.Value;
        }

        private static void AddKilnTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.KILNJOB] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.KILNJOB).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Charcoal, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, KilnTraining, .05f, requirements);
            research.ResearchComplete += KilnTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, KilnTraining, .05f);
                research.ResearchComplete += KilnTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void KilnTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.KILNJOB).CraftingCooldown =
                _defaultValues[ColonyBuiltIn.Jobs.KILNJOB] - _defaultValues[ColonyBuiltIn.Jobs.KILNJOB] * e.Research.Value;
        }

        private static void AddMetalSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.METALSMITHJOB] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.METALSMITHJOB).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.BronzePlate, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MetalSmithTraining, .05f, requirements);
            research.ResearchComplete += MetalSmithTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MetalSmithTraining, .05f);
                research.ResearchComplete += MetalSmithTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MetalSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.METALSMITHJOB).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.METALSMITHJOB] -
                                                   _defaultValues[ColonyBuiltIn.Jobs.METALSMITHJOB] * e.Research.Value;
        }

        private static void AddMintTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.MINTER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MINTER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MintTraining, .05f, requirements);
            research.ResearchComplete += MintTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MintTraining, .05f);
                research.ResearchComplete += MintTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MintTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MINTER).CraftingCooldown =
                _defaultValues[ColonyBuiltIn.Jobs.MINTER] - _defaultValues[ColonyBuiltIn.Jobs.MINTER] * e.Research.Value;
        }

        private static void AddOvenTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.BAKER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BAKER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bread, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, OvenTraining, .05f, requirements);
            research.ResearchComplete += OvenTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, OvenTraining, .05f);
                research.ResearchComplete += OvenTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void OvenTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BAKER).CraftingCooldown =
                _defaultValues[ColonyBuiltIn.Jobs.BAKER] - _defaultValues[ColonyBuiltIn.Jobs.BAKER] * e.Research.Value;
        }

        private static void AddWoodcutterTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.WOODCUTTER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.WOODCUTTER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, WoodcutterTraining, .05f, requirements);
            research.ResearchComplete += WoodcutterTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, WoodcutterTraining, .05f);
                research.ResearchComplete += WoodcutterTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WoodcutterTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.WOODCUTTER).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.WOODCUTTER] -
                                                   _defaultValues[ColonyBuiltIn.Jobs.WOODCUTTER] * e.Research.Value;
        }

        private static void AddWorkBenchTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[ColonyBuiltIn.Jobs.CRAFTER] = ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.CRAFTER).CraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.WorkBench, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.Research.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, WorkBenchTraining, .05f, requirements);
            research.ResearchComplete += WorkBenchTraining_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, WorkBenchTraining, .05f);
                research.ResearchComplete += WorkBenchTraining_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WorkBenchTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.CRAFTER).CraftingCooldown = _defaultValues[ColonyBuiltIn.Jobs.CRAFTER] -
                                                  _defaultValues[ColonyBuiltIn.Jobs.CRAFTER] * e.Research.Value;
        }

        private static void AddMasterOfAll(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagLife, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagAdvanced, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 1);
            researchDic.Add(BuiltinBlocks.ScienceBagMilitary, 1);
            researchDic.Add(BuiltinBlocks.GoldCoin, 10);
            researchDic.Add(BuiltinBlocks.BronzeCoin, 10);

            var requirements = new List<string>
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

            var research = new PandaResearch(researchDic, 1, MasterOfAll, .03f, requirements, 200);
            research.ResearchComplete += MasterOfAll_ResearchComplete;
            ServerManager.ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 10; i++)
            {
                research = new PandaResearch(researchDic, i, MasterOfAll, .03f, null, 200);
                research.ResearchComplete += MasterOfAll_ResearchComplete;
                ServerManager.ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MasterOfAll_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.CRAFTER).CraftingCooldown         = _defaultValues[ColonyBuiltIn.Jobs.CRAFTER]        - _defaultValues[ColonyBuiltIn.Jobs.CRAFTER]        * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(WorkBenchTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.WOODCUTTER).CraftingCooldown      = _defaultValues[ColonyBuiltIn.Jobs.WOODCUTTER]     - _defaultValues[ColonyBuiltIn.Jobs.WOODCUTTER]     * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(WoodcutterTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BAKER).CraftingCooldown           = _defaultValues[ColonyBuiltIn.Jobs.BAKER]          - _defaultValues[ColonyBuiltIn.Jobs.BAKER]          * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(OvenTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MINTER).CraftingCooldown          = _defaultValues[ColonyBuiltIn.Jobs.MINTER]         - _defaultValues[ColonyBuiltIn.Jobs.MINTER]         * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(MintTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.METALSMITHJOB).CraftingCooldown   = _defaultValues[ColonyBuiltIn.Jobs.METALSMITHJOB]  - _defaultValues[ColonyBuiltIn.Jobs.METALSMITHJOB]  * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(MetalSmithTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.KILNJOB).CraftingCooldown         = _defaultValues[ColonyBuiltIn.Jobs.KILNJOB]        - _defaultValues[ColonyBuiltIn.Jobs.KILNJOB]        * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(KilnTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GUNSMITHJOB).CraftingCooldown     = _defaultValues[ColonyBuiltIn.Jobs.GUNSMITHJOB]    - _defaultValues[ColonyBuiltIn.Jobs.GUNSMITHJOB]    * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(GunSmithTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.GRINDER).CraftingCooldown         = _defaultValues[ColonyBuiltIn.Jobs.GRINDER]        - _defaultValues[ColonyBuiltIn.Jobs.GRINDER]        * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(GrinderTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.SMELTER).CraftingCooldown         = _defaultValues[ColonyBuiltIn.Jobs.SMELTER]        - _defaultValues[ColonyBuiltIn.Jobs.SMELTER]        * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(FurnaceTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.FINERYFORGEJOB).CraftingCooldown  = _defaultValues[ColonyBuiltIn.Jobs.FINERYFORGEJOB] - _defaultValues[ColonyBuiltIn.Jobs.FINERYFORGEJOB] * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(FineryForgeTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.BLOOMERYJOB).CraftingCooldown     = _defaultValues[ColonyBuiltIn.Jobs.BLOOMERYJOB]    - _defaultValues[ColonyBuiltIn.Jobs.BLOOMERYJOB]    * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(BloomeryTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.TAILOR).CraftingCooldown          = _defaultValues[ColonyBuiltIn.Jobs.TAILOR]         - _defaultValues[ColonyBuiltIn.Jobs.TAILOR]         * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(TailorTraining), 0f));
            ServerManager.BlockEntityCallbacks.GetCraftJobSettings(ColonyBuiltIn.Jobs.MERCHANT).CraftingCooldown        = _defaultValues[ColonyBuiltIn.Jobs.MERCHANT]       - _defaultValues[ColonyBuiltIn.Jobs.MERCHANT]       * (e.Research.Value + e.Manager.Colony.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(MerchantTraining), 0f));
        }
    }
}