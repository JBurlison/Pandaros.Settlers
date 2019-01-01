using Pipliz.JSON;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class StaticItems
    {
        [JSON.HintAutoObject]
        public class StaticItem : IJsonDeserializable, IJsonSerializable
        {
            public string Name { get; set; }
            public string RequiredScience { get; set; }
            public string RequiredPermission { get; set; }

            public void JsonDeerialize(JSONNode node)
            {
                JSON.LoadFields(this, node);
            }

            public virtual JSONNode JsonSerialize()
            {
                return JSON.SaveField(this);
            }
        }

        public static List<StaticItem> List { get; set; } = new List<StaticItem>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Items.StaticItems.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            AddStaticItemToStockpile(p);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRespawn, GameLoader.NAMESPACE + ".Items.StaticItems.OnPlayerRespawn")]
        public static void OnPlayerRespawn(Players.Player p)
        {
            AddStaticItemToStockpile(p);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnActiveColonyChanges, GameLoader.NAMESPACE + ".Items.StaticItems.OnActiveColonyChanges")]
        public static void OnActiveColonyChanges(Players.Player p, Colony previouslyActiveColony, Colony newActiveColony)
        {
            AddStaticItemToStockpile(p);
        }

        public static void AddStaticItemToStockpile(Players.Player p)
        {
            foreach (var item in List)
                if (p != null && p.Colonies != null && p.Colonies.Length != 0)
                    foreach (var c in p.Colonies)
                        if (ItemTypes.IndexLookup.TryGetIndex(item.Name, out var staticItem) && !c.Stockpile.Contains(staticItem))
                        {
                            bool canAdd = true;

                            if (!string.IsNullOrEmpty(item.RequiredScience))
                            {
                                var sk = c.ScienceData.CompletedCycles.FirstOrDefault(kvp => kvp.Key.Researchable.Researchable.GetKey() == item.RequiredScience).Key;

                                if (sk.Researchable == null)
                                    canAdd = false;
                            }

                            if (!string.IsNullOrEmpty(item.RequiredPermission))
                            {
                                if (!PermissionsManager.HasPermission(p, new PermissionsManager.Permission(item.RequiredPermission)))
                                    canAdd = false;
                            }

                            if (canAdd)
                                c.Stockpile.Add(staticItem);
                        }
        }
    }
}
