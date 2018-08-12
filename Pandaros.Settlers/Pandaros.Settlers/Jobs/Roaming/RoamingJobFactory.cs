using BlockEntities;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Roaming
{
    [BlockEntityAutoLoader]
    public class RoamingJobFactory : ILoadedWithDataByPositionType, IChangedWithType, IMultiBlockEntityMapping
    {
        public IEnumerable<ItemTypes.ItemType> TypesToRegister { get; } = new List<ItemTypes.ItemType>();

        public void OnChangedWithType(Players.Player player, Vector3Int blockPosition, ItemTypes.ItemType typeOld, ItemTypes.ItemType typeNew)
        {
            
        }

        public void OnLoadedWithDataPosition(Vector3Int blockPosition, ushort type, ByteReader reader)
        {
           throw new NotImplementedException();
        }
    }
}
