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
    public class SchematicBuilderLoader : IConstructionLoader
    {
        public static readonly string NAME = GameLoader.NAMESPACE + ".SchematicBuilder";

        public string JobName => NAME;

        public void ApplyTypes(ConstructionArea area, JSONNode node)
        {
            if (node == null)
                return;

           if (node.TryGetAs(NAME + ".SchematicName", out string schematic))
            {
                area.SetIterationType(new SchematicIterator(area, schematic));
                area.SetConstructionType(new SchematicBuilder());
            }
        }

        public void SaveTypes(ConstructionArea area, JSONNode node)
        {
            var itt = area.IterationType as SchematicIterator;

            if (itt != null)
                node.SetAs(NAME + ".SchematicName", itt.SchematicName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicBuilderLoader")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadareajobs")]
        public static void Register()
        {
            ConstructionArea.RegisterLoader(new SchematicBuilderLoader());
        }
    }
}
