using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Healing;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Research
{

    [ModLoader.ModManager]
    public class GeneralResearch
    {
        public const string Settlement = "Settlement";
        public const string Machines = "Machines";
        public const string ReducedWaste = "ReducedWaste";
        public const string ArmorSmithing = "ArmorSmithing";
        public const string SwordSmithing = "SwordSmithing";
        public const string ColonistHealth = "ColonistHealth";
        public const string Knights = "Knights";
        public const string Apothecary = "Apothecaries";
        public const string ImprovedDurability = "ImprovedDurability";
        public const string ImprovedFuelCapacity = "ImprovedFuelCapacity";
        public const string IncreasedCapacity = "IncreasedCapacity";
        public const string AdvancedApothecary = "AdvancedApothecary";
        public const string Mana = "Mana";
        public const string Elementium = "Elementium";
        public const string BuildersWand = "BuildersWand";
        public const string BetterBuildersWand = "BetterBuildersWand";
        public const string Teleporters = "Teleportation";
        public const string MaxSettlers = "MaxSettlers";
        public const string MinSettlers = "MinSettlers";
        public const string SettlerChance = "SettlerChance";
        public const string NumberSkilledLaborer = "NumberSkilledLaborer";
        public const string SkilledLaborer = "SkilledLaborer";
        private static readonly Dictionary<string, float> _baseSpeed = new Dictionary<string, float>();

        public static string GetResearchKey(string researchName)
        {
            return GameLoader.NAMESPACE + "." + researchName;
        }

        /// <summary>
        ///     This is reqquired to make sure jobs get registered before research
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, "BLOCKNPCS_WORKAROUND")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        static void Dummy()
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            var researchDic = new Dictionary<ushort, int>();
            PandaLogger.Log("Registering Panda Research.");

            AddMaxSettlers(researchDic);
            AddMinSettlers(researchDic);
            AddSettlerChance(researchDic);
            AddSkilledLaborer(researchDic);
            AddNumberSkilledLaborer(researchDic);
            AddReducedWaste(researchDic);
            AddArmorSmithing(researchDic);
            AddColonistHealth(researchDic);
            AddSwordSmithing(researchDic);
            AddKnightResearch(researchDic);
            AddImprovedSlings(researchDic);
            AddImprovedBows(researchDic);
            AddImprovedCrossbows(researchDic);
            AddImprovedMatchlockgun(researchDic);
            AddMachines(researchDic);
            AddImprovedDuarability(researchDic);
            AddImprovedFuelCapacity(researchDic);
            AddIncreasedCapacity(researchDic);
            AddApocthResearch(researchDic);
            AddAdvanceApocthResearch(researchDic);
            AddManaResearch(researchDic);
            AddElementiumResearch(researchDic);
            AddTeleporters(researchDic);
            AddBuildersWandResearch(researchDic);
            AddBetterBuildersWandResearch(researchDic);

            PandaLogger.Log("Panda Research Registering Complete!");
        }

        private static void AddReducedWaste(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BERRY.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BREAD.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.SCIENCEBAGLIFE
            };

            var research = new PandaResearch(researchDic, 1, ReducedWaste, 0.001f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ReducedWaste_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ReducedWaste, 0.001f, GameLoader.ICON_PATH, requirements);
                research.ResearchComplete += ReducedWaste_ResearchComplete;
                
            }
        }

        private static void ReducedWaste_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.UpdateFoodUse(ColonyState.GetColonyState(e.Manager.Colony));
        }

        private static void AddArmorSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id, 3);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BRONZEANVIL
            };

            RegisterArmorSmithng(researchDic, 1, requirements);

            researchDic.Remove(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZEAXE.Id, 1);
            RegisterArmorSmithng(researchDic, 2);

            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONRIVET.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONSWORD.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGMILITARY.Id, 1);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.BRONZEAXE.Id);
            RegisterArmorSmithng(researchDic, 3);

            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELPARTS.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.IRONRIVET.Id);
            RegisterArmorSmithng(researchDic, 4);
        }

        private static void RegisterArmorSmithng(Dictionary<ushort, int> researchDic, int level,
                                                 List<string> requirements = null)
        {
            var research = new PandaResearch(researchDic, level, ArmorSmithing, 1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += Research_ResearchComplete;
            
        }

        private static void Research_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            var armor = new List<IArmor>();

            switch (e.Research.Level)
            {
                case 1:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Copper")));
                    break;
                case 2:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Bronze")));
                    break;
                case 3:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Iron")));
                    break;
                case 4:
                    armor.AddRange(ArmorFactory.ArmorLookup.Values.Where(a => a.name.Contains("Steel")));
                    break;
            }

            foreach (var item in armor.Where(a => a.IsMagical == false))
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(item.name));
        }

        private static void AddSwordSmithing(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id, 3);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BRONZEANVIL
            };

            RegisterSwordmithng(researchDic, 1, requirements);

            researchDic.Remove(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZEAXE.Id, 1);
            RegisterSwordmithng(researchDic, 2);

            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONRIVET.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONSWORD.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGMILITARY.Id, 1);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Id);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.BRONZEAXE.Id);
            RegisterSwordmithng(researchDic, 3);

            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELPARTS.Id, 3);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);
            researchDic.Remove(ColonyBuiltIn.ItemTypes.IRONRIVET.Id);
            RegisterSwordmithng(researchDic, 4);
        }

        private static void RegisterSwordmithng(Dictionary<ushort, int> researchDic, int level,
                                                List<string> requirements = null)
        {
            var research = new PandaResearch(researchDic, level, SwordSmithing, 1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += SwordResearch_ResearchComplete;
            
        }

        private static void SwordResearch_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            switch (e.Research.Level)
            {
                case 1:
                    e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".CopperSword"));
                    break;
                case 2:
                    e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".BronzeSword"));
                    break;
                case 3:
                    e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".IronSword"));
                    break;
                case 4:
                    e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SteelSword"));
                    break;
            }

        }

        private static void AddColonistHealth(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.LINEN.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id, 10);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.SCIENCEBAGLIFE
            };

            var research = new PandaResearch(researchDic, 1, ColonistHealth, 10f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += Research_ResearchComplete1;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ColonistHealth, 10f, GameLoader.ICON_PATH);
                research.ResearchComplete += Research_ResearchComplete1;
                
            }
        }

        private static void Research_ResearchComplete1(object sender, ResearchCompleteEventArgs e)
        {
            var maxHp = e.Manager.Colony.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MAXCOLONISTHP", e.Manager.Colony.NPCHealthMax);
            e.Manager.Colony.NPCHealthMax = maxHp + e.Research.Level * e.Research.BaseValue;

            foreach (var follower in e.Manager.Colony.Followers)
                follower.health = e.Manager.Colony.NPCHealthMax;
        }

        private static void AddKnightResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.LINEN.Id, 2);

            var requirements = new List<string>
            {
                GetResearchKey(SwordSmithing + "1"),
                ColonyBuiltIn.Research.SCIENCEBAGBASIC
            };

            var research = new PandaResearch(researchDic, 1, Knights, 1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += Knights_ResearchComplete;
            
        }

        private static void Knights_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.ForEachOwner(o => PatrolTool.GivePlayerPatrolTool(o));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(PatrolTool.PatrolFlag.name));
        }

        private static void AddApocthResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 4);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.HERBFARMING,
                ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                ColonyBuiltIn.Research.OLIVEFARMER
            };

            var research = new PandaResearch(researchDic, 1, Apothecary, 1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += Apocth_ResearchComplete;
            
        }

        private static void Apocth_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ApothecaryRegister.JOB_RECIPE));
        }

        private static void AddAdvanceApocthResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 4);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 2);

            var requirements = new List<string>
            {
                GetResearchKey(Apothecary + "1")
            };

            var research = new PandaResearch(researchDic, 1, AdvancedApothecary, 1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += AdvanceApocth_ResearchComplete;
            
        }

        private static void AdvanceApocth_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Anitbiotic.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(TreatedBandage.Item.name));
        }

        private static void AddManaResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.ALKANET.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.WOLFSBANE.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.HOLLYHOCK.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GYPSUM.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.CRYSTAL.Id, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Apothecary + "1")
            };

            var research = new PandaResearch(researchDic, 1, Mana, 1f, GameLoader.ICON_PATH, requirements, 50);
            research.ResearchComplete += Mana_ResearchComplete;
            
        }

        private static void Mana_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Items.Mana.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Aether.Item.name));
        }

        private static void AddElementiumResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPER.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONORE.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.TIN.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDORE.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GALENASILVER.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GALENALEAD.Id, 20);

            var requirements = new List<string>
            {
                GetResearchKey(Mana + "1")
            };

            var research = new PandaResearch(researchDic, 1, Elementium, 1f, GameLoader.ICON_PATH, requirements, 50);
            research.ResearchComplete += Elementium_ResearchComplete;
            
        }

        private static void Elementium_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Items.Elementium.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(EarthStone.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(FireStone.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(WaterStone.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(AirStone.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ElementalTurrets.AIRTURRET_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ElementalTurrets.FIRETURRET_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ElementalTurrets.EARTHTURRET_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ElementalTurrets.WATERTURRET_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ElementalTurrets.VOIDTURRET_NAMESPACE));
        }

        private static void AddBuildersWandResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDINGOT.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SILVERINGOT.Id, 10);
            researchDic.Add(Items.Elementium.Item.ItemIndex, 1);

            var requirements = new List<string>
            {
                GetResearchKey(Elementium + "1")
            };

            var research = new PandaResearch(researchDic, 1, BuildersWand, 1f, GameLoader.ICON_PATH, requirements, 50);
            research.ResearchComplete += BuildersWand_ResearchComplete;
            
        }

        private static void BuildersWand_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Items.BuildersWand.Item.name));
        }

        private static void AddBetterBuildersWandResearch(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Aether.Item.ItemIndex, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDINGOT.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SILVERINGOT.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLANKS.Id, 10);

            var requirements = new List<string>
            {
                GetResearchKey(BuildersWand + "1")
            };

            var research = new PandaResearch(researchDic, 1, BetterBuildersWand, 250f, GameLoader.ICON_PATH, requirements, 50);
            research.ResearchComplete += BetterBuildersWand_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research =
                    new PandaResearch(researchDic, i, BetterBuildersWand, 250f, GameLoader.ICON_PATH, requirements, 50);

                research.ResearchComplete += BetterBuildersWand_ResearchComplete;
                
            }
        }

        private static void BetterBuildersWand_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            // TODO: make global
            foreach (var p in e.Manager.Colony.Owners)
            {
                var ps = PlayerState.GetPlayerState(p);
                ps.BuildersWandMaxCharge = (int)e.Research.Value;
                ps.BuildersWandCharge += ps.BuildersWandMaxCharge;
            }
        }

        private static void AddTeleporters(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(Items.Mana.Item.ItemIndex, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STONEBRICKS.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.CRYSTAL.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 20);

            var requirements = new List<string>
            {
                GetResearchKey(Mana + "1"),
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, Teleporters, 1f, GameLoader.ICON_PATH, requirements, 100, false);
            research.ResearchComplete += Teleporters_ResearchComplete;
            
        }

        private static void Teleporters_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(TeleportPad.Item.name));
        }

        private static void AddImprovedSlings(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SLING.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SLINGBULLET.Id, 5);

            for (var i = 1; i <= 5; i++)
            {
                var research = new PandaResearch(researchDic, i, ColonyBuiltIn.NpcTypes.GUARDSLINGERDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedSlings_ResearchComplete;
                
            }
        }

        private static void ImprovedSlings_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }

        private static void AddImprovedBows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.BOW.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRONZEARROW.Id, 5);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.ARCHERY
            };

            var research = new PandaResearch(researchDic, 1, ColonyBuiltIn.NpcTypes.GUARDBOWDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ImprovedBows_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ColonyBuiltIn.NpcTypes.GUARDBOWDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedBows_ResearchComplete;
                
            }
        }

        private static void ImprovedBows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }

        private static void AddImprovedCrossbows(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.CROSSBOW.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.CROSSBOWBOLT.Id, 5);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.CROSSBOW
            };

            var research = new PandaResearch(researchDic, 1, ColonyBuiltIn.NpcTypes.GUARDCROSSBOWDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ColonyBuiltIn.NpcTypes.GUARDCROSSBOWDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedCrossbows_ResearchComplete;
                
            }
        }

        private static void ImprovedCrossbows_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }

        private static void AddImprovedMatchlockgun(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.MATCHLOCKGUN.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.LEADBULLET.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GUNPOWDERPOUCH.Id, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.MATCHLOCKGUN
            };

            var research = new PandaResearch(researchDic, 1, ColonyBuiltIn.NpcTypes.GUARDMATCHLOCKDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ColonyBuiltIn.NpcTypes.GUARDMATCHLOCKDAY.Replace("day", ""), .05f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedMatchlockguns_ResearchComplete;
                
            }
        }

        private static void ImprovedMatchlockguns_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            SettlerManager.ApplyJobCooldownsToNPCs(e.Manager.Colony);
        }

        private static void AddMachines(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.LINEN.Id, 2);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BLOOMERY
            };

            var research = new PandaResearch(researchDic, 1, Machines, 1f, GameLoader.ICON_PATH, requirements, 20, false);
            research.ResearchComplete += Machiness_ResearchComplete;
            
        }

        private static void Machiness_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".Miner"));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GateLever.Item.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GateLever.GateItem.name));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Turret.BRONZEARROW_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Turret.STONE_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Turret.CROSSBOW_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Turret.MATCHLOCK_NAMESPACE));
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(AdvancedCrafterRegister.JOB_RECIPE));
        }

        private static void AddImprovedDuarability(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedDurability, .1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ImprovedDuarability_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedDurability, .1f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedDuarability_ResearchComplete;
                
            }
        }

        private static void ImprovedDuarability_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.REPAIR, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddImprovedFuelCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, ImprovedFuelCapacity, .1f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, ImprovedFuelCapacity, .1f, GameLoader.ICON_PATH);
                research.ResearchComplete += ImprovedFuelCapacity_ResearchComplete;
                
            }
        }

        private static void ImprovedFuelCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.REFUEL, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddIncreasedCapacity(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10);

            var requirements = new List<string>
            {
                GetResearchKey(Machines + "1")
            };

            var research = new PandaResearch(researchDic, 1, IncreasedCapacity, .2f, GameLoader.ICON_PATH, requirements);
            research.ResearchComplete += IncreasedCapacity_ResearchComplete;
            

            for (var i = 2; i <= 5; i++)
            {
                research = new PandaResearch(researchDic, i, IncreasedCapacity, .2f, GameLoader.ICON_PATH);
                research.ResearchComplete += IncreasedCapacity_ResearchComplete;
                
            }
        }

        private static void IncreasedCapacity_ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            RoamingJobState.SetActionsMaxEnergy(MachineConstants.RELOAD, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
        }

        private static void AddMaxSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.PLASTERBLOCK.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONINGOT.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BED.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 20);

            var requirements = new List<string>
            {
                GetResearchKey(SettlerChance) + "1"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MaxSettlers, 1f, GameLoader.ICON_PATH, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MaxSettlers, 1f, GameLoader.ICON_PATH));
        }

        private static void AddMinSettlers(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BRICKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BED.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 20);

            var requirements = new List<string>
            {
                GetResearchKey(MaxSettlers) + "3"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, MinSettlers, 1f, GameLoader.ICON_PATH, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, MinSettlers, 1f, GameLoader.ICON_PATH));
        }

        private static void AddSettlerChance(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 1);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.TORCH.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.STONEBRICKS.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.BED.Id, 5);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 20);

            var requirements = new List<string>
            {
                ColonyBuiltIn.Research.BANNERRADIUS2
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SettlerChance, 0.1f, GameLoader.ICON_PATH, requirements));

            for (var i = 2; i <= 5; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SettlerChance, 0.1f, GameLoader.ICON_PATH));
        }

        private static void AddSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id, 2);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 30);

            var requirements = new List<string>
            {
                GetResearchKey(SettlerChance) + "2",
                GetResearchKey(ReducedWaste) + "2"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, SkilledLaborer, 0.02f, GameLoader.ICON_PATH, requirements));

            for (var i = 2; i <= 10; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, SkilledLaborer, 0.02f, GameLoader.ICON_PATH));
        }

        private static void AddNumberSkilledLaborer(Dictionary<ushort, int> researchDic)
        {
            researchDic.Clear();
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERPARTS.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.COPPERNAILS.Id, 30);
            researchDic.Add(ColonyBuiltIn.ItemTypes.TIN.Id, 10);
            researchDic.Add(ColonyBuiltIn.ItemTypes.IRONRIVET.Id, 20);
            researchDic.Add(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 30);

            var requirements = new List<string>
            {
                GetResearchKey(SkilledLaborer) + "1"
            };

            ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, 1, NumberSkilledLaborer, 1f, GameLoader.ICON_PATH, requirements));

            for (var i = 2; i <= 5; i++)
                ServerManager.ScienceManager.RegisterResearchable(new PandaResearch(researchDic, i, NumberSkilledLaborer, 1f, GameLoader.ICON_PATH));
        }
    }
}
