using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz.Mods.BaseGame.BlockNPCs;
using Server.Science;

namespace Pandaros.Settlers.Research
{
    [ModLoader.ModManagerAttribute]
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

        private const string SCIENCEBAGREQ = ColonyBuiltIn.ScienceBagBasic;
        private const int BAG_COST = 2;
        private const int COIN_COST = 5;

        private static readonly Dictionary<string, float> _defaultValues = new Dictionary<string, float>();

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnAddResearchables,
            GameLoader.NAMESPACE + ".Research.JobResearch.OnAddResearchables")]
        [ModLoader.ModCallbackDependsOnAttribute(GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
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
            _defaultValues[nameof(ShopJob)] = ShopJob.StaticCraftingCooldown;

            researchDic.Clear();

            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MerchantTraining, .05f, requirements);
            research.ResearchComplete += TrainedMerchant_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, MerchantTraining, .05f);
                research.ResearchComplete += TrainedMerchant_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedMerchant_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ShopJob.StaticCraftingCooldown =
                _defaultValues[nameof(ShopJob)] - _defaultValues[nameof(ShopJob)] * e.Research.Value;
        }

        private static void AddTailorTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(TailorJob)] = TailorJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LinenBag, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, TailorTraining, .05f, requirements);
            research.ResearchComplete += TrainedTailor_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, TailorTraining, .05f);
                research.ResearchComplete += TrainedTailor_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedTailor_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            TailorJob.StaticCraftingCooldown = _defaultValues[nameof(TailorJob)] -
                                               _defaultValues[nameof(TailorJob)] * e.Research.Value;
        }

        private static void AddBloomeryTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(BloomeryJob)] = BloomeryJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronWrought, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, BloomeryTraining, .05f, requirements);
            research.ResearchComplete += BloomeryTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, BloomeryTraining, .05f);
                research.ResearchComplete += BloomeryTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void BloomeryTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            BloomeryJob.StaticCraftingCooldown = _defaultValues[nameof(BloomeryJob)] -
                                                 _defaultValues[nameof(BloomeryJob)] * e.Research.Value;
        }

        private static void AddFineryForgeTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(FineryForgeJob)] = FineryForgeJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CrossbowBolt, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, FineryForgeTraining, .05f, requirements);
            research.ResearchComplete += FineryForgeTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, FineryForgeTraining, .05f);
                research.ResearchComplete += FineryForgeTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FineryForgeTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            FineryForgeJob.StaticCraftingCooldown = _defaultValues[nameof(FineryForgeJob)] -
                                                    _defaultValues[nameof(FineryForgeJob)] * e.Research.Value;
        }

        private static void AddFurnaceTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(FurnaceJob)] = FurnaceJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bricks, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, FurnaceTraining, .05f, requirements);
            research.ResearchComplete += FurnaceTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, FurnaceTraining, .05f);
                research.ResearchComplete += FurnaceTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FurnaceTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            FurnaceJob.StaticCraftingCooldown = _defaultValues[nameof(FurnaceJob)] -
                                                _defaultValues[nameof(FurnaceJob)] * e.Research.Value;
        }

        private static void AddGrinderTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(GrinderJob)] = GrinderJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Flour, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, GrinderTraining, .05f, requirements);
            research.ResearchComplete += GrinderTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, GrinderTraining, .05f);
                research.ResearchComplete += GrinderTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GrinderTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            GrinderJob.StaticCraftingCooldown = _defaultValues[nameof(GrinderJob)] -
                                                _defaultValues[nameof(GrinderJob)] * e.Research.Value;
        }

        private static void AddGunSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(GunSmithJob)] = GunSmithJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, GunSmithTraining, .05f, requirements);
            research.ResearchComplete += GunSmithTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, GunSmithTraining, .05f);
                research.ResearchComplete += GunSmithTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GunSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            GunSmithJob.StaticCraftingCooldown = _defaultValues[nameof(GunSmithJob)] -
                                                 _defaultValues[nameof(GunSmithJob)] * e.Research.Value;
        }

        private static void AddKilnTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(KilnJob)] = KilnJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Charcoal, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, KilnTraining, .05f, requirements);
            research.ResearchComplete += KilnTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, KilnTraining, .05f);
                research.ResearchComplete += KilnTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void KilnTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            KilnJob.StaticCraftingCooldown =
                _defaultValues[nameof(KilnJob)] - _defaultValues[nameof(KilnJob)] * e.Research.Value;
        }

        private static void AddMetalSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(MetalSmithJob)] = MetalSmithJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.BronzePlate, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MetalSmithTraining, .05f, requirements);
            research.ResearchComplete += MetalSmithTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, MetalSmithTraining, .05f);
                research.ResearchComplete += MetalSmithTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MetalSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            MetalSmithJob.StaticCraftingCooldown = _defaultValues[nameof(MetalSmithJob)] -
                                                   _defaultValues[nameof(MetalSmithJob)] * e.Research.Value;
        }

        private static void AddMintTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(MintJob)] = MintJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, MintTraining, .05f, requirements);
            research.ResearchComplete += MintTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, MintTraining, .05f);
                research.ResearchComplete += MintTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MintTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            MintJob.StaticCraftingCooldown =
                _defaultValues[nameof(MintJob)] - _defaultValues[nameof(MintJob)] * e.Research.Value;
        }

        private static void AddOvenTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(OvenJob)] = OvenJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bread, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, OvenTraining, .05f, requirements);
            research.ResearchComplete += OvenTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, OvenTraining, .05f);
                research.ResearchComplete += OvenTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void OvenTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            OvenJob.StaticCraftingCooldown =
                _defaultValues[nameof(OvenJob)] - _defaultValues[nameof(OvenJob)] * e.Research.Value;
        }

        private static void AddWoodcutterTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(WoodcutterJob)] = WoodcutterJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Planks, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, WoodcutterTraining, .05f, requirements);
            research.ResearchComplete += WoodcutterTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, WoodcutterTraining, .05f);
                research.ResearchComplete += WoodcutterTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WoodcutterTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            WoodcutterJob.StaticCraftingCooldown = _defaultValues[nameof(WoodcutterJob)] -
                                                   _defaultValues[nameof(WoodcutterJob)] * e.Research.Value;
        }

        private static void AddWorkBenchTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(WorkBenchJob)] = WorkBenchJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.WorkBench, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagBasic, BAG_COST);
            researchDic.Add(BuiltinBlocks.BronzeCoin, COIN_COST);

            var requirements = new List<string>
            {
                SCIENCEBAGREQ,
                ColonyBuiltIn.CoinMinting
            };

            var research = new PandaResearch(researchDic, 1, WorkBenchTraining, .05f, requirements);
            research.ResearchComplete += WorkBenchTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 5; i++)
            {
                research                  =  new PandaResearch(researchDic, i, WorkBenchTraining, .05f);
                research.ResearchComplete += WorkBenchTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WorkBenchTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            WorkBenchJob.StaticCraftingCooldown = _defaultValues[nameof(WorkBenchJob)] -
                                                  _defaultValues[nameof(WorkBenchJob)] * e.Research.Value;
        }

        private static void AddMasterOfAll(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(WorkBenchJob)] = WorkBenchJob.StaticCraftingCooldown;

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
            ScienceManager.RegisterResearchable(research);

            for (var i = 2; i <= 10; i++)
            {
                research                  =  new PandaResearch(researchDic, i, MasterOfAll, .03f, null, 200);
                research.ResearchComplete += MasterOfAll_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MasterOfAll_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            var tmpValues = e.Manager.Player.GetTempValues(true);

            WorkBenchJob.StaticCraftingCooldown = _defaultValues[nameof(WorkBenchJob)] -
                                                  _defaultValues[nameof(WorkBenchJob)] *
                                                  (e.Research.Value +
                                                   tmpValues
                                                      .GetOrDefault(PandaResearch.GetResearchKey(WorkBenchTraining),
                                                                    0f));

            WoodcutterJob.StaticCraftingCooldown = _defaultValues[nameof(WoodcutterJob)] -
                                                   _defaultValues[nameof(WoodcutterJob)] *
                                                   (e.Research.Value +
                                                    tmpValues
                                                       .GetOrDefault(PandaResearch.GetResearchKey(WoodcutterTraining),
                                                                     0f));

            OvenJob.StaticCraftingCooldown = _defaultValues[nameof(OvenJob)] - _defaultValues[nameof(OvenJob)] *
                                             (e.Research.Value +
                                              tmpValues.GetOrDefault(PandaResearch.GetResearchKey(OvenTraining), 0f));

            MintJob.StaticCraftingCooldown = _defaultValues[nameof(MintJob)] - _defaultValues[nameof(MintJob)] *
                                             (e.Research.Value +
                                              tmpValues.GetOrDefault(PandaResearch.GetResearchKey(MintTraining), 0f));

            MetalSmithJob.StaticCraftingCooldown = _defaultValues[nameof(MetalSmithJob)] -
                                                   _defaultValues[nameof(MetalSmithJob)] *
                                                   (e.Research.Value +
                                                    tmpValues
                                                       .GetOrDefault(PandaResearch.GetResearchKey(MetalSmithTraining),
                                                                     0f));

            KilnJob.StaticCraftingCooldown = _defaultValues[nameof(KilnJob)] - _defaultValues[nameof(KilnJob)] *
                                             (e.Research.Value +
                                              tmpValues.GetOrDefault(PandaResearch.GetResearchKey(KilnTraining), 0f));

            GunSmithJob.StaticCraftingCooldown = _defaultValues[nameof(GunSmithJob)] -
                                                 _defaultValues[nameof(GunSmithJob)] *
                                                 (e.Research.Value +
                                                  tmpValues.GetOrDefault(PandaResearch.GetResearchKey(GunSmithTraining),
                                                                         0f));

            GrinderJob.StaticCraftingCooldown = _defaultValues[nameof(GrinderJob)] -
                                                _defaultValues[nameof(GrinderJob)] *
                                                (e.Research.Value +
                                                 tmpValues.GetOrDefault(PandaResearch.GetResearchKey(GrinderTraining),
                                                                        0f));

            FurnaceJob.StaticCraftingCooldown = _defaultValues[nameof(FurnaceJob)] -
                                                _defaultValues[nameof(FurnaceJob)] *
                                                (e.Research.Value +
                                                 tmpValues.GetOrDefault(PandaResearch.GetResearchKey(FurnaceTraining),
                                                                        0f));

            FineryForgeJob.StaticCraftingCooldown = _defaultValues[nameof(FineryForgeJob)] -
                                                    _defaultValues[nameof(FineryForgeJob)] *
                                                    (e.Research.Value +
                                                     tmpValues
                                                        .GetOrDefault(PandaResearch.GetResearchKey(FineryForgeTraining),
                                                                      0f));

            BloomeryJob.StaticCraftingCooldown = _defaultValues[nameof(BloomeryJob)] -
                                                 _defaultValues[nameof(BloomeryJob)] *
                                                 (e.Research.Value +
                                                  tmpValues.GetOrDefault(PandaResearch.GetResearchKey(BloomeryTraining),
                                                                         0f));

            TailorJob.StaticCraftingCooldown = _defaultValues[nameof(TailorJob)] - _defaultValues[nameof(TailorJob)] *
                                               (e.Research.Value +
                                                tmpValues.GetOrDefault(PandaResearch.GetResearchKey(TailorTraining), 0f)
                                               );

            ShopJob.StaticCraftingCooldown = _defaultValues[nameof(ShopJob)] - _defaultValues[nameof(ShopJob)] *
                                             (e.Research.Value +
                                              tmpValues.GetOrDefault(PandaResearch.GetResearchKey(MerchantTraining), 0f)
                                             );
        }
    }
}