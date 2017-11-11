using BlockTypes.Builtin;
using Pipliz.BlockNPCs.Implementations;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private const int COLONY_BAG_COST = 3;

        static Dictionary<string, float> _defaultValues = new Dictionary<string, float>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.JobResearch.OnAddResearchables"),
            ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
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
        }

        private static void AddMerchantTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(ShopJob)] = ShopJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, MerchantTraining, .05f, requirements);
            research.ResearchComplete += TrainedMerchant_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MerchantTraining, .05f);
                research.ResearchComplete += TrainedMerchant_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedMerchant_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            ShopJob.StaticCraftingCooldown = _defaultValues[nameof(ShopJob)] - (_defaultValues[nameof(ShopJob)] * e.Research.Value);
        }

        private static void AddTailorTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(TailorJob)] = TailorJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LinenBag, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, TailorTraining, .05f, requirements);
            research.ResearchComplete += TrainedTailor_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, TailorTraining, .05f);
                research.ResearchComplete += TrainedTailor_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void TrainedTailor_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            TailorJob.StaticCraftingCooldown = _defaultValues[nameof(TailorJob)] - (_defaultValues[nameof(TailorJob)] * e.Research.Value);
        }

        private static void AddBloomeryTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(BloomeryJob)] = BloomeryJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.IronWrought, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, BloomeryTraining, .05f, requirements);
            research.ResearchComplete += BloomeryTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, BloomeryTraining, .05f);
                research.ResearchComplete += BloomeryTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void BloomeryTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            BloomeryJob.StaticCraftingCooldown = _defaultValues[nameof(BloomeryJob)] - (_defaultValues[nameof(BloomeryJob)] * e.Research.Value);
        }

        private static void AddFineryForgeTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(FineryForgeJob)] = FineryForgeJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.CrossbowBolt, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, FineryForgeTraining, .05f, requirements);
            research.ResearchComplete += FineryForgeTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, FineryForgeTraining, .05f);
                research.ResearchComplete += FineryForgeTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FineryForgeTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            FineryForgeJob.StaticCraftingCooldown = _defaultValues[nameof(FineryForgeJob)] - (_defaultValues[nameof(FineryForgeJob)] * e.Research.Value);
        }

        private static void AddFurnaceTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(FurnaceJob)] = FurnaceJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bricks, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, FurnaceTraining, .05f, requirements);
            research.ResearchComplete += FurnaceTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, FurnaceTraining, .05f);
                research.ResearchComplete += FurnaceTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void FurnaceTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            FurnaceJob.StaticCraftingCooldown = _defaultValues[nameof(FurnaceJob)] - (_defaultValues[nameof(FurnaceJob)] * e.Research.Value);
        }

        private static void AddGrinderTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(GrinderJob)] = GrinderJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Flour, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, GrinderTraining, .05f, requirements);
            research.ResearchComplete += GrinderTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, GrinderTraining, .05f);
                research.ResearchComplete += GrinderTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GrinderTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            GrinderJob.StaticCraftingCooldown = _defaultValues[nameof(GrinderJob)] - (_defaultValues[nameof(GrinderJob)] * e.Research.Value);
        }

        private static void AddGunSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(GunSmithJob)] = GunSmithJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, GunSmithTraining, .05f, requirements);
            research.ResearchComplete += GunSmithTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, GunSmithTraining, .05f);
                research.ResearchComplete += GunSmithTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void GunSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            GunSmithJob.StaticCraftingCooldown = _defaultValues[nameof(GunSmithJob)] - (_defaultValues[nameof(GunSmithJob)] * e.Research.Value);
        }

        private static void AddKilnTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(KilnJob)] = KilnJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.LeadBullet, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, KilnTraining, .05f, requirements);
            research.ResearchComplete += KilnTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, KilnTraining, .05f);
                research.ResearchComplete += KilnTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void KilnTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            KilnJob.StaticCraftingCooldown = _defaultValues[nameof(KilnJob)] - (_defaultValues[nameof(KilnJob)] * e.Research.Value);
        }

        private static void AddMetalSmithTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(MetalSmithJob)] = MetalSmithJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.BronzePlate, 5);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, MetalSmithTraining, .05f, requirements);
            research.ResearchComplete += MetalSmithTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MetalSmithTraining, .05f);
                research.ResearchComplete += MetalSmithTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MetalSmithTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            MetalSmithJob.StaticCraftingCooldown = _defaultValues[nameof(MetalSmithJob)] - (_defaultValues[nameof(MetalSmithJob)] * e.Research.Value);
        }

        private static void AddMintTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(MintJob)] = MintJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.BronzeCoin, 20);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, MintTraining, .05f, requirements);
            research.ResearchComplete += MintTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, MintTraining, .05f);
                research.ResearchComplete += MintTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void MintTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            MintJob.StaticCraftingCooldown = _defaultValues[nameof(MintJob)] - (_defaultValues[nameof(MintJob)] * e.Research.Value);
        }

        private static void AddOvenTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(OvenJob)] = OvenJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Bread, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, OvenTraining, .05f, requirements);
            research.ResearchComplete += OvenTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, OvenTraining, .05f);
                research.ResearchComplete += OvenTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void OvenTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            OvenJob.StaticCraftingCooldown = _defaultValues[nameof(OvenJob)] - (_defaultValues[nameof(OvenJob)] * e.Research.Value);
        }

        private static void AddWoodcutterTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(WoodcutterJob)] = WoodcutterJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.Planks, 10);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, WoodcutterTraining, .05f, requirements);
            research.ResearchComplete += WoodcutterTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, WoodcutterTraining, .05f);
                research.ResearchComplete += WoodcutterTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WoodcutterTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            WoodcutterJob.StaticCraftingCooldown = _defaultValues[nameof(WoodcutterJob)] - (_defaultValues[nameof(WoodcutterJob)] * e.Research.Value);
        }

        private static void AddWorkBenchTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(WorkBenchJob)] = WorkBenchJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.WorkBench, 3);
            researchDic.Add(BuiltinBlocks.ScienceBagColony, COLONY_BAG_COST);
            researchDic.Add(BuiltinBlocks.GoldCoin, 20);

            var requirements = new List<string>()
            {
                ColonyBuiltIn.ScienceBagColony
            };

            var research = new PandaResearch(researchDic, 1, WorkBenchTraining, .05f, requirements);
            research.ResearchComplete += WorkBenchTraining_ResearchComplete;
            ScienceManager.RegisterResearchable(research);

            for (int i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, WorkBenchTraining, .05f);
                research.ResearchComplete += WorkBenchTraining_ResearchComplete;
                ScienceManager.RegisterResearchable(research);
            }
        }

        private static void WorkBenchTraining_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            WorkBenchJob.StaticCraftingCooldown = _defaultValues[nameof(WorkBenchJob)] - (_defaultValues[nameof(WorkBenchJob)] * e.Research.Value);
        }
    }
}
