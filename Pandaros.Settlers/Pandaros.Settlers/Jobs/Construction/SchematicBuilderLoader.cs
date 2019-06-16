using Pandaros.Settlers.NBT;
using Pipliz.JSON;
using Pipliz.Mods.BaseGame.Construction;

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

           if (node.TryGetAs(NAME + ".SchematicName", out string schematic) && node.TryGetAs(NAME + ".Rotation", out Schematic.Rotation rotation))
            {
                area.IterationType = new SchematicIterator(area, schematic, rotation);
                area.ConstructionType = new SchematicBuilder();
            }
        }

        public void SaveTypes(ConstructionArea area, JSONNode node)
        {
            var itt = area.IterationType as SchematicIterator;

            if (itt != null)
                node.SetAs(NAME + ".SchematicName", itt.SchematicName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicBuilderLoader")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            ConstructionArea.RegisterLoader(new SchematicBuilderLoader());
        }
    }
}
