using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pipliz.JSON;
using UnityEngine;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Artifact
    }

    public interface ICSType : IJsonSerializable, INameable, IJsonDeserializable
    {
        bool? blocksPathing { get; }
        List<string> categories { get; }
        List<Colliders> colliders { get; }
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
        List<OnRemove> onRemove { get; }
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
        ItemRarity Rarity { get; }
        StaticItem StaticItemSettings { get; }
    }
}