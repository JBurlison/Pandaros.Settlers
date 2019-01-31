using System.Collections.Generic;

namespace Pandaros.Settlers.Extender
{
    public interface IAddItemTypes : ISettersExtension
    {
        void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes);
    }
}
