using Pipliz.JSON;
using Pipliz.Mods.BaseGame.Construction;
using Pipliz.Mods.BaseGame.Construction.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class BlueprintBuilderLoader : IConstructionLoader
    {
        public static readonly string NAME = GameLoader.NAMESPACE + ".BlueprintBuilder";

        public string JobName => NAME;

        public void ApplyTypes(ConstructionArea area, JSONNode node)
        {
            if (node == null)
                return;

           if (node.TryGetAs(NAME + ".BlueprintName", out string blueprint))
            {
                area.SetIterationType(new BottomToTop(area));
                area.SetConstructionType(new BlueprintBuilder(blueprint));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Construction.BlueprintBuilderLoader")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadareajobs")]
        public static void Register()
        {
            ConstructionArea.RegisterLoader(new BlueprintBuilderLoader());
        }
    }
}
