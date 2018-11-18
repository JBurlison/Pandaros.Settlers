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
        public EAreaType AreaType => EAreaType.BuilderArea;

        public EAreaMeshType AreaTypeMesh => EAreaMeshType.ThreeD;

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            Block block = default(Block);

            try
            {
                var bpi = iterationType as BlueprintIterator;
                var adjX = iterationType.CurrentPosition.x - bpi.BuilderSchematic.StartPos.x;
                var adjY = iterationType.CurrentPosition.y - bpi.BuilderSchematic.StartPos.y;
                var adjZ = iterationType.CurrentPosition.z - bpi.BuilderSchematic.StartPos.z;

                if (bpi != null &&
                    bpi.BuilderSchematic.XMax > adjX &&
                    bpi.BuilderSchematic.YMax > adjY &&
                    bpi.BuilderSchematic.ZMax > adjZ)
                    block = bpi.BuilderSchematic.Blocks[adjX, adjY, adjZ];
                else
                    PandaLogger.Log(ChatColor.yellow, "Unable to find scematic position {0}", iterationType.CurrentPosition);
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

            
        }
    }
}
