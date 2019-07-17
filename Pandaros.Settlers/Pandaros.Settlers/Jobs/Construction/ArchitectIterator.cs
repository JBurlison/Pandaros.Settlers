using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class ArchitectIterator : IIterationType
    {
        protected ConstructionArea area;
        protected Vector3Int positionMin;
        protected Vector3Int positionMax;

        protected Vector3Int cursor;
        protected Vector3Int iterationChunkLocation;
        protected int iterationIndex;

        public string SchematicName { get; private set; }
        public Schematic BuilderSchematic { get; private set; }

        public ArchitectIterator(ConstructionArea area, string schematicName)
        {
            this.area = area;

            positionMin = area.Minimum;
            positionMax = area.Maximum;

            iterationChunkLocation = positionMin;
            cursor = positionMin;
            iterationIndex = -1;

            SchematicName = schematicName;

            if (SchematicReader.TryGetSchematic(SchematicName, area.Owner.ColonyID, iterationChunkLocation, out var schematic))
                BuilderSchematic = schematic;
            else
            {
                var calcSize = area.Maximum - area.Minimum;
                BuilderSchematic = new Schematic(SchematicName, calcSize.x, calcSize.y, calcSize.z);
                BuilderSchematic.Blocks = new SchematicBlock[calcSize.x + 1, calcSize.y + 1, calcSize.z + 1];

                for (int Y = 0; Y < BuilderSchematic.YMax; Y++)
                {
                    for (int Z = 0; Z < BuilderSchematic.ZMax; Z++)
                    {
                        for (int X = 0; X < BuilderSchematic.XMax; X++)
                        {
                            BuilderSchematic.Blocks[X, Y, Z] = new SchematicBlock()
                            {
                                CSBlock = true,
                                X = X,
                                Y = Y,
                                Z = Z,
                                BlockID = ColonyBuiltIn.ItemTypes.AIR
                            };
                        }
                    }
                }
            }

            BuilderSchematic.StartPos = positionMin;
        }

        public Vector3Int CurrentPosition { get { return cursor; } }
        public Vector3Int PreviousPosition { get; set; } = Vector3Int.invalidPos;

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

            PreviousPosition = cursor;
            cursor = next;

            if (IsInBounds(cursor))
                return true;
            else
                return false;

        }
    }
}
