using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Buildings.NBT;
using Pipliz.Mods.BaseGame.Construction;
using Shared;
using System;
using System.IO;

namespace Pandaros.Settlers.Jobs.Construction
{
    //public static void CreateNewAreaJob (string identifier, Pipliz.JSON.JSONNode args, Colony colony, Vector3Int min, Vector3Int max)
    //on the areajobtracker
    //identifier = pipliz.constructionarea
    //then the args node has
    //"constructionType" : "GameLoader.NAMESPACE + ".BlueprintBuilder"
    public class BlueprintBuilder : IConstructionType
    {
        public string BlueprintName { get; private set; }
        public Schematic BuilderSchematic { get; private set; }

        public EAreaType AreaType => EAreaType.BuilderArea;

        public EAreaMeshType AreaTypeMesh => EAreaMeshType.ThreeD;

        public BlueprintBuilder(string blueprintName)
        {
            BlueprintName = blueprintName;

            if (File.Exists(GameLoader.BLUEPRINT_SAVE_LOC + BlueprintName))
                BuilderSchematic = SchematicReader.LoadSchematic(GameLoader.BLUEPRINT_SAVE_LOC + BlueprintName);
            else if (File.Exists(GameLoader.BLUEPRINT_DEFAULT_LOC + BlueprintName))
                BuilderSchematic = SchematicReader.LoadSchematic(GameLoader.BLUEPRINT_DEFAULT_LOC + BlueprintName);
            else
                PandaLogger.Log(ChatColor.red, "Cannot find blueprint {0}!", BlueprintName);
        }

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            Block block = default(Block);

            try
            {
                if (BuilderSchematic.XMax > iterationType.CurrentPosition.x &&
                    BuilderSchematic.YMax > iterationType.CurrentPosition.y &&
                    BuilderSchematic.ZMax > iterationType.CurrentPosition.z)
               block = BuilderSchematic.Blocks[iterationType.CurrentPosition.x, iterationType.CurrentPosition.y, iterationType.CurrentPosition.z];
            }
            catch (Exception) { }

            if (block == default(Block))
                block = Block.Air;

            var mapped = block.MappedBlock;

            if (World.TryGetTypeAt(iterationType.CurrentPosition, out ushort val) && mapped.CSIndex != val)
            {
                if (val != BuiltinBlocks.Air)
                    job.NPC.Colony.Stockpile.Add(val);

                
            }

            iterationType.MoveNext();
        }
    }
}
