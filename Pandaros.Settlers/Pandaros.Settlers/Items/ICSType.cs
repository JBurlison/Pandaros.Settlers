using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Pipliz.JSON;
using UnityEngine;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items
{
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Artifact = 3
    }

    public interface ICSType : INameable
    {
        bool? blocksPathing { get; }
        List<string> categories { get; }
        Colliders colliders { get; }
        string color { get; }
        dynamic customData { get; }
        int? destructionTime { get; }
        string icon { get; }
        bool? isDestructible { get; }
        bool? isFertile { get; }
        bool? isPlaceable { get; }
        bool? isRotatable { get; }
        bool? isSolid { get; }
        bool? requiresFertileBelow { get; }
        int? maxStackSize { get; }
        float? luxuryHours { get; }
        string mesh { get; }
        MeshRotationEuler meshRotationEuler { get; }
        MeshOffset meshOffset { get; }
        MeshScale meshScale { get; }
        bool? needsBase { get; }
        float? foodValue { get; }
        float? happiness { get; }
        float? dailyFoodFractionOptimal { get; }
        string onPlaceAudio { get; }
        List<OnRemove> onRemove { get; }
        string onRemoveAmount { get; }
        string onRemoveAudio { get; }
        string onRemoveChance { get; }
        string onRemoveType { get; }
        string parentType { get; }
        [JsonProperty("rotatablex-")]
        string rotatablexn { get; }
        [JsonProperty("rotatablex+")]
        string rotatablexp { get; }
        [JsonProperty("rotatablez-")]
        string rotatablezn { get; }
        [JsonProperty("rotatablez+")]
        string rotatablezp { get; }
        string sideall { get; }
        [JsonProperty("sidex-")]
        string sidexn { get; }
        [JsonProperty("sidex+")]
        string sidexp { get; }
        [JsonProperty("sidey-")]
        string sideyn { get; }
        [JsonProperty("sidey+")]
        string sideyp { get; }
        [JsonProperty("sidez-")]
        string sidezn { get; }
        [JsonProperty("sidez+")]
        string sidezp { get; }
        ItemRarity Rarity { get; }
        StaticItem StaticItemSettings { get; }
        OpenMenuSettings OpensMenuSettings { get; }
    }
}