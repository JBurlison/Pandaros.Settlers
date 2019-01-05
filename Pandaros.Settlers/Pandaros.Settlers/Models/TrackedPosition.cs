using Pandaros.Settlers.Database;
using Pipliz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class TrackedPositionContext : PandaContext
    {
        public DbSet<TrackedPosition> Positions { get; set; }
    }


    public class TrackedPosition
    {
        public DateTime TimeTracked { get; set; }   
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int BlockId { get; set; }
        [Key]
        public int id { get; set; }
        public string PlayerId { get; set; }
        public string ColonyId { get; set; }

        public Vector3Int GetVector()
        {
            return new Vector3Int(X, Y, Z);
        }

        public bool Equals(TrackedPosition other)
        {
            if (other == null)
                return false;

            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public bool Equals(Vector3Int other)
        {
            if (other == null)
                return false;

            return other.x == X && other.y == Y && other.z == Z;
        }

    }
}
