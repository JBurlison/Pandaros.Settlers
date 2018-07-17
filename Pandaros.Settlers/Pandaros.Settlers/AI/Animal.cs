using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.AI
{
    public class Animal
    {
        public Animal(ushort type)
        {
            ItemType = ItemTypes.GetType(type);
        }

        
        ItemTypes.ItemType ItemType { get; set; }
        public virtual List<uint> OkStatus { get; } = new List<uint>();
        public RoamingJobState TargetObjective { get; set; }
        public RoamingJobState PreviousObjective { get; set; }

        public virtual void Roam()
        {

        }

        public virtual void MoveToPosition(Vector3Int pos)
        {

        }
    }
}
