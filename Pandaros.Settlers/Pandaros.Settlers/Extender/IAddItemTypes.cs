using System.Collections.Generic;

namespace Pandaros.Settlers.Extender
{
    public interface IAddItemTypes : ISettlersExtension
    {
        void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes);
    }
}
