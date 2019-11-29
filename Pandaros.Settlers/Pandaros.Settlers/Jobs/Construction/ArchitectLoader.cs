using Jobs.Implementations.Construction;
using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class ArchitectLoader : IConstructionLoader
    {
        public const string NAME = GameLoader.NAMESPACE + ".Architect";
        public string JobName => NAME;

        public void ApplyTypes(ConstructionArea area, JSONNode node)
        {
            if (node == null)
                return;

            if (node.TryGetAs(NAME + ".ArchitectSchematicName", out string schematic))
            {
                area.IterationType = new ArchitectIterator(area, schematic);

                if (node.TryGetAs<JSONNode>(NAME + "PreviousPosition", out var jSONNodePos))
                    ((ArchitectIterator)area.IterationType).PreviousPosition = (Vector3Int)jSONNodePos;

                area.ConstructionType = new ArchitectBuilder();
            }
        }

        public void SaveTypes(ConstructionArea area, JSONNode node)
        {
            var itt = area.IterationType as ArchitectIterator;

            if (itt != null)
            {
                node.SetAs(NAME + ".ArchitectSchematicName", itt.SchematicName);
                node.SetAs(NAME + "PreviousPosition", (JSONNode)((ArchitectIterator)area.IterationType).PreviousPosition);
                SchematicReader.SaveSchematic(area.Owner, itt.BuilderSchematic);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Construction.ArchitectLoader.AfterItemTypesDefined")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            ConstructionArea.RegisterLoader(new ArchitectLoader());
        }
    }
}
