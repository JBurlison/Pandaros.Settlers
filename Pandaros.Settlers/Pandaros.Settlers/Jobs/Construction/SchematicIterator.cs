using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;

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

        public SchematicIterator(ConstructionArea area, string schematicName, Schematic.Rotation rotation)
        {
            this.area = area;
            
            positionMin = area.Minimum;
            positionMax = area.Maximum;

            iterationChunkLocation = positionMin;
            cursor = positionMin;
            iterationIndex = -1;

            SchematicName = schematicName;

            if (SchematicReader.TryGetSchematic(SchematicName, area.Owner.ColonyID, iterationChunkLocation, out var schematic))
            {
                BuilderSchematic = schematic;

                if (rotation >= Schematic.Rotation.Right)
                    BuilderSchematic.Rotate();

                if (rotation >= Schematic.Rotation.Back)
                    BuilderSchematic.Rotate();

                if (rotation >= Schematic.Rotation.Left)
                    BuilderSchematic.Rotate();
            }
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
            var next = cursor.Add(1, 0, 0);

            if (next.x > positionMax.x)
            {
                next.x = positionMin.x;
                next = next.Add(0, 0, 1);
            }

            if (next.z > positionMax.z)
            {
                next.z = positionMin.z;
                next = next.Add(0, 1, 0);
            }

            cursor = next;

            if (IsInBounds(cursor))
                return true;
            else
                return false;

        }
    }
}
