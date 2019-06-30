using Newtonsoft.Json;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Weapons;
using Pandaros.Settlers.Models;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ItemsProvider : IAfterWorldLoad, IAddItemTypes
    {
        StringBuilder _sb = new StringBuilder();

        List<string> _fixRelativePaths = new List<string>()
        {
            "icon",
            "mash"
        };

        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSType);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.name))
                    {
                        ItemCache.CSItems[itemType.name] = itemType;

                        var permutations = ConnectedBlockCalculator.GetPermutations(itemType);

                        foreach (var permutation in permutations)
                            ItemCache.CSItems[permutation.name] = permutation;
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

            var settings = GameLoader.GetJSONSettingPaths(GameLoader.NAMESPACE + ".CSItems");
            
            foreach (var modInfo in settings)
            {
                foreach (var path in modInfo.Value)
                {
                    try
                    {
                        var jsonFile = JSON.Deserialize(modInfo.Key + "/" + path);

                        if (jsonFile.NodeType == NodeType.Object && jsonFile.ChildCount > 0)
                            foreach (var item in jsonFile.LoopObject())
                            {
                                foreach (var property in _fixRelativePaths)
                                    if (item.Value.TryGetAs(property, out string propertyPath) && propertyPath.StartsWith("./"))
                                        item.Value[property] = new JSONNode(modInfo.Key + "/" + propertyPath.Substring(2));

                                if (item.Value.TryGetAs("Durability", out int durability))
                                {
                                    var ma = item.Value.JsonDeerialize<MagicArmor>();
                                    ItemCache.CSItems[ma.name] = ma;
                                }
                                else if (item.Value.TryGetAs("WepDurability", out bool wepDurability))
                                {
                                    var mw = item.Value.JsonDeerialize<MagicWeapon>();
                                    ItemCache.CSItems[mw.name] = mw;
                                }
                                else if (item.Value.TryGetAs("IsMagical", out bool isMagic))
                                {
                                    var mi = item.Value.JsonDeerialize<PlayerMagicItem>();
                                    ItemCache.CSItems[mi.name] = mi;
                                }
                                else
                                {
                                    var newItem = item.Value.JsonDeerialize<CSType>();
                                    ItemCache.CSItems[newItem.name] = newItem;

                                    var permutations = ConnectedBlockCalculator.GetPermutations(newItem);

                                    foreach (var permutation in permutations)
                                        ItemCache.CSItems[permutation.name] = permutation;
                                }
                            }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }
                }
            }

            foreach (var itemType in ItemCache.CSItems.Values)
            {
                if (itemType.TrainConfiguration != null && itemType.TrainConfiguration.playerSeatOffset != null)
                    Transportation.Train.TrainTypes[itemType.name] = itemType;

                ConnectedBlockSystem.AddConnectedBlock(itemType);

                var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.name, itemType.JsonSerialize());

                if (itemTypes.ContainsKey(itemType.name))
                {
                    PandaLogger.Log(ChatColor.yellow, "Item {0} already loaded...Overriding item.", itemType.name);
                    itemTypes[itemType.name] = rawItem;
                }
                else
                    itemTypes.Add(itemType.name, rawItem);

                if (itemType.StaticItemSettings != null && !string.IsNullOrWhiteSpace(itemType.StaticItemSettings.Name))
                    StaticItems.List.Add(itemType.StaticItemSettings);

                if (itemType is IPlayerMagicItem pmi)
                    MagicItemsCache.PlayerMagicItems[pmi.name] = pmi;

                if (itemType.OpensMenuSettings != null && !string.IsNullOrEmpty(itemType.OpensMenuSettings.ItemName))
                    Help.UIManager.OpenMenuItems.Add(itemType.OpensMenuSettings);

                _sb.Append($"{itemType.name}, ");
                i++;

                if (i > 5)
                {
                    i = 0;
                    _sb.AppendLine();
                }
            }

        }

        public void AfterWorldLoad()
        {
            PandaLogger.LogToFile("-------------------Items Loaded----------------------");
            PandaLogger.LogToFile(_sb.ToString());
            PandaLogger.LogToFile("------------------------------------------------------"); 
        }
    }
}
