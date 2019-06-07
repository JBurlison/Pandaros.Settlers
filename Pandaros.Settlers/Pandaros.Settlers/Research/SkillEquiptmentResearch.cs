using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Reagents;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Research
{
    public class SkillEquiptmentResearch : IPandaResearch
    {
        public Dictionary<ItemId, int> RequiredItems => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Adamantine.NAME), 1 },
            { ItemId.GetItemId(AirStone.Item.name), 1 },
            { ItemId.GetItemId(EarthStone.Item.name), 1 },
            { ItemId.GetItemId(WaterStone.Item.name), 1 },
            { ItemId.GetItemId(FireStone.Item.name), 1 },
        };

        public int NumberOfLevels => 2;

        public float BaseValue => 1;

        public List<string> Dependancies => new List<string>()
        {
            Jobs.SorcererRegister.JOB_NAME + "1"
        };

        public int BaseIterationCount => 200;

        public bool AddLevelToName => true;

        public string name => "SkillEquiptmentResearch";

        public void OnRegister()
        {
            
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (e.Research.Level == 1)
            {
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledBoots1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledChest1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledGloves1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledHelm1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledLegs1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledShield1"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledSword1"));
            }
            else
            {
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledBoots2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledChest2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledGloves2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledHelm2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledLegs2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledShield2"));
                e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(GameLoader.NAMESPACE + ".SkilledSword2"));
            }
        }
    }
}
