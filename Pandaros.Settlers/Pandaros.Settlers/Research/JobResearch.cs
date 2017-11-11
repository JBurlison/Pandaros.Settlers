using BlockTypes.Builtin;
using Pipliz.BlockNPCs.Implementations;
using Server.Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Research
{

    public static class JobResearch
    {
        public const string MerchantTraining = "MerchantTraining";
        public const string TailorTraining = "TailorTraining";
        public const string BloomeryTraining = "BloomeryTraining";
        public const string FineryForgeTraining = "FineryForgeTraining";
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
        }

        private static void AddMerchantTraining(Dictionary<ushort, int> researchDic)
        {
            _defaultValues[nameof(ShopJob)] = ShopJob.StaticCraftingCooldown;

            researchDic.Clear();
            researchDic.Add(BuiltinBlocks.ScienceBagColony, 3);
            researchDic.Add(BuiltinBlocks.GoldCoin, COLONY_BAG_COST);

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
    }
}
