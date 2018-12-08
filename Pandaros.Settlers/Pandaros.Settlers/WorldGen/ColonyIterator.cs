using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;

namespace Pandaros.Settlers.WorldGen
{
    public class ColonyIterator : IIterationType
    {
        protected Vector3Int positionMin;
        protected Vector3Int positionMax;

        protected Vector3Int cursor;
        protected Vector3Int iterationChunkLocation;
        protected int iterationIndex;

        public ColonyIterator(RawSchematicSize size)
        {
            positionMin = new Vector3Int(0,0,0);
            positionMax = new Vector3Int(size.XMax, size.YMax, size.ZMax);

            iterationChunkLocation = positionMin;
            cursor = positionMin;
            iterationIndex = -1;
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
