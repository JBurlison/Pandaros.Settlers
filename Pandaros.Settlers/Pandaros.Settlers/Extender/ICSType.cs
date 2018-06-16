using System.Collections.ObjectModel;
using Pipliz.JSON;
using UnityEngine;

namespace Pandaros.Settlers.Extender
{
    public interface ICSType : IJsonConvertable
    {
        string Name { get; }
        bool? blocksPathing { get; }
        ReadOnlyCollection<string> categories { get; }
        ReadOnlyCollection<Colliders> colliders { get; }
        Color color { get; }
        JSONNode customData { get; }
        int? destructionTime { get; }
        string icon { get; }
        bool? isDestructible { get; }
        bool? isFertile { get; }
        bool? isPlaceable { get; }
        bool? isRotatable { get; }
        bool? isSolid { get; }
        int? maxStackSize { get; }
        string mesh { get; }
        bool? needsBase { get; }
        float? nutritionalValue { get; }
        string onPlaceAudio { get; }
        ReadOnlyCollection<OnRemove> onRemove { get; }
        string onRemoveAmount { get; }
        string onRemoveAudio { get; }
        string onRemoveChance { get; }
        string onRemoveType { get; }
        string parentType { get; }
        string rotatablexn { get; }
        string rotatablexp { get; }
        string rotatablezn { get; }
        string rotatablezp { get; }
        string sideall { get; }
        string sidexn { get; }
        string sidexp { get; }
        string sideyn { get; }
        string sideyp { get; }
        string sidezn { get; }
        string sidezp { get; }
    }
}