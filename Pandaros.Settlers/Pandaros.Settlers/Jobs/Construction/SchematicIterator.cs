using Pandaros.Settlers.Buildings.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Pipliz.Mods.BaseGame.Construction.Iterators;
using System.IO;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class SchematicIterator : IIterationType
    {
        protected ConstructionArea area;
        protected Vector3Int positionMin;
        protected Vector3Int positionMax;

        protected Vector3Int cursor;
        protected Vector3Int iterationChunkLocation;
        protected int iterationIndex;

        public string SchematicName { get; private set; }
        public Schematic BuilderSchematic { get; private set; }

        public SchematicIterator(ConstructionArea area, string schematicName)
        {
            this.area = area;
            
            positionMin = area.Minimum;
            positionMax = area.Maximum;

            iterationChunkLocation = new Vector3Int(positionMin.x & -16, positionMin.y & -16, positionMin.z & -16);
            iterationIndex = -1;

            SchematicName = schematicName;

            if (SchematicReader.TryGetSchematic(SchematicName, area.Owner.ColonyID, iterationChunkLocation, out var schematic))
                BuilderSchematic = schematic;

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
                                SchematicReader.UnloadSchematic(GameLoader.Schematic_SAVE_LOC + SchematicName, iterationChunkLocation);
                                SchematicReader.UnloadSchematic(GameLoader.Schematic_DEFAULT_LOC + SchematicName, iterationChunkLocation);
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
