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
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
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

        public string Name => "SkillEquiptmentResearch";

        public void OnRegister()
        {
            
        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (e.Research.Level == 1)
            {
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledBoots1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledChest1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledGloves1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledHelm1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledLegs1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledShield1Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Weapons.SkilledSword1Recipe.NAME), true);
            }
            else
            {
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledBoots2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledChest2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledGloves2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledHelm2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledLegs2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Armor.Magical.SkilledShield2Recipe.NAME), true);
                e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(Items.Weapons.SkilledSword2Recipe.NAME), true);
            }
        }
    }
}
