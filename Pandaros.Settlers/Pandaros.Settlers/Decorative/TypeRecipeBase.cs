using Pandaros.API.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Decorative
{
    public class TypeRecipeBase : ICSRecipe
    {
        public string name { get; set; }

        public List<RecipeItem> requires { get; set; } = new List<RecipeItem>();

        public List<RecipeResult> results { get; set; } = new List<RecipeResult>();

        public CraftPriority defaultPriority { get; set; } = CraftPriority.Low;

        public bool isOptional { get; set; } = false;

        public int defaultLimit { get; set; } = 0;

        public string Job { get; set; } = Jobs.DecorBuilderRegister.JOB_NAME;

        public List<string> JobBlock => new List<string>();
    }
}
