using Pandaros.Settlers.Buildings.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Pipliz.Mods.BaseGame.Construction.Iterators;
using System.IO;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class BlueprintIterator : IIterationType
    {
        protected ConstructionArea area;
        protected Vector3Int positionMin;
        protected Vector3Int positionMax;

        protected Vector3Int cursor;
        protected Vector3Int iterationChunkLocation;
        protected int iterationIndex;

        public string BlueprintName { get; private set; }
        public Schematic BuilderSchematic { get; private set; }

        public BlueprintIterator(ConstructionArea area, string blueprintName)
        {
            this.area = area;

            positionMin = area.Minimum;
            positionMax = area.Maximum;

            iterationChunkLocation = new Vector3Int(positionMin.x & -16, positionMin.y & -16, positionMin.z & -16);
            iterationIndex = -1;

            BlueprintName = blueprintName;

            if (File.Exists(GameLoader.BLUEPRINT_SAVE_LOC + BlueprintName))
                BuilderSchematic = SchematicReader.LoadSchematic(GameLoader.BLUEPRINT_SAVE_LOC + BlueprintName, iterationChunkLocation);
            else if (File.Exists(GameLoader.BLUEPRINT_DEFAULT_LOC + BlueprintName))
                BuilderSchematic = SchematicReader.LoadSchematic(GameLoader.BLUEPRINT_DEFAULT_LOC + BlueprintName, iterationChunkLocation);
            else
                PandaLogger.Log(ChatColor.red, "Cannot find blueprint {0}!", BlueprintName);

            MoveNext();
        }

        public Vector3Int CurrentPosition { get { return cursor; } }

        public bool IsInBounds(Vector3Int location)
        {
            return location.x >= positionMin.x && location.x <= positionMax.x
                && location.y >= positionMin.y && location.y <= positionMax.y
                && location.z >= positionMin.z && location.z <= positionMax.z;
        }

        public bool MoveNext()
        {
            while (true)
            {
                iterationIndex++;

                if (iterationIndex >= 16 * 16 * 16)
                {
                    iterationIndex = 0;
                    iterationChunkLocation.z += 16;

                    if (iterationChunkLocation.z > (positionMax.z & -16))
                    {
                        iterationChunkLocation.z = (positionMin.z & -16);
                        iterationChunkLocation.x += 16;

                        if (iterationChunkLocation.x > (positionMax.x & -16))
                        {
                            iterationChunkLocation.x = (positionMin.x & -16);
                            iterationChunkLocation.y += 16;

                            if (iterationChunkLocation.y > (positionMax.y & -16))
                            {
                                SchematicReader.UnloadSchematic(GameLoader.BLUEPRINT_SAVE_LOC + BlueprintName);
                                SchematicReader.UnloadSchematic(GameLoader.BLUEPRINT_DEFAULT_LOC + BlueprintName);
                                return false;
                            }
                        }
                    }
                }

                cursor = IteratorHelper.ZOrderToPosition(iterationIndex).ToWorld(iterationChunkLocation);

                if (IsInBounds(cursor))
                {
                    return true;
                }
            }
        }
    }
}
