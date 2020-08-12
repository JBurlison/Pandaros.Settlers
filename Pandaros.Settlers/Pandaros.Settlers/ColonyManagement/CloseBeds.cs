using BlockEntities.Implementations;
using Happiness;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.localization;
using Pipliz;
using System;
using System.Collections.Generic;
using static BlockEntities.Implementations.BedTracker;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public class CloseBeds : IHappinessCause, IOnConstructInventoryManageColonyUI
    {
        public class BedStateObject
        {
            public Colony Colony { get; set; }
            public Dictionary<Bed, CachedBedState> Beds { get; set; } = new Dictionary<Bed, CachedBedState>();
        }

        public class CachedBedState
        {
            public Vector3Int Position { get; set; }
            public bool Taken { get; set; }
            public bool IsHappy { get; set; } = true;
        }

        public static Dictionary<Colony, int> CachedHappiness { get; set; } = new Dictionary<Colony, int>();
        public static LocalizationHelper LocalizationHelper { get; private set; } = new LocalizationHelper(GameLoader.NAMESPACE, "Happiness");
        public static Dictionary<Colony, BedStateObject> BedCache { get; private set; } = new Dictionary<Colony, BedStateObject>();
        static readonly LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Beds");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Entities.PlayerState.OnPlayerConnectedSuperLate")]
        public static void OnPlayerConnectedSuperLate(Players.Player p)
        {
            foreach (var colony in p.Colonies)
                CalculateBeds(colony);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".ColonyManager.CloseBeds.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                CalculateBeds(colony);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.CloseBeds.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (IsBed(d.TypeNew.ItemIndex) || IsBed(d.TypeOld.ItemIndex))
                if (d.RequestOrigin.AsColony != null)
                    CalculateBeds(d.RequestOrigin.AsColony);
                else if (d.RequestOrigin.AsPlayer != null && d.RequestOrigin.AsPlayer.ActiveColony != null)
                    CalculateBeds(d.RequestOrigin.AsPlayer.ActiveColony);
        }

        public float Evaluate(Colony colony)
        {
            var cs = ColonyState.GetColonyState(colony);

            if (cs.Difficulty.Name != GameDifficulty.Normal.Name && CachedHappiness.TryGetValue(colony, out var value))
                return value;
            else
                return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("BedsClose", player);
        }

        public static void CalculateBeds(Colony colony)
        {
            var cs = ColonyState.GetColonyState(colony);

            if (cs.Difficulty.Name != GameDifficulty.Normal.Name)
            {
                BedStateObject bso = new BedStateObject();
                bso.Colony = colony;
                colony.BedTracker.ForeachBedInstance(ForEachAction, ref bso);

                int happiness = 0;

                foreach (var bed in bso.Beds)
                {
                    try
                    {
                        var bedEnd = bed.Value.Position.Add(-1, 0, 0);

                        if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDXP)
                        {
                            bedEnd = bed.Value.Position.Add(1, 0, 0);
                        }
                        else if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDZN)
                        {
                            bedEnd = bed.Value.Position.Add(0, 0, -1);
                        }
                        else if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDZP)
                        {
                            bedEnd = bed.Value.Position.Add(0, 0, 1);
                        }

                        if (!IsHappy(bed.Value.Position, bedEnd))
                        {
                            happiness -= 1;
                            bed.Value.IsHappy = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        SettlersLogger.LogError(ex);
                    }
                }

                CachedHappiness[colony] = happiness;
                BedCache[colony] = bso;
            }
        }

        private static bool IsHappy(Vector3Int currentPos, Vector3Int ignorePos)
        {
            int count = 0;
            int touchingBeds = 0;

            var currentN = currentPos.Add(1, 0, 0);
            var currentS = currentPos.Add(-1, 0, 0);
            var currentE = currentPos.Add(0, 0, -1);
            var currentW = currentPos.Add(0, 0, 1);

            var ignoreN = ignorePos.Add(1, 0, 0);
            var ignoreS = ignorePos.Add(-1, 0, 0);
            var ignoreE = ignorePos.Add(0, 0, -1);
            var ignoreW = ignorePos.Add(0, 0, 1);

            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentN);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentS);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentE);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentW);

            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreN);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreS);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreE);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreW);

            return count > 0 && touchingBeds < 3;
        }

        private static void EvaluateSpot(Vector3Int ignorePos, ref int count, ref int touchingBeds, Vector3Int spot)
        {
            if (ignorePos != spot)
            {
                if (World.TryGetTypeAt(spot, out ushort type) && type != ColonyBuiltIn.ItemTypes.AIR)
                {
                    if (IsBed(type))
                        touchingBeds++;
                }
                else
                    count++;
            }
        }

        private static bool IsBed(ushort type)
        {
            return type == ColonyBuiltIn.ItemTypes.BEDXN ||
                   type == ColonyBuiltIn.ItemTypes.BEDXP ||
                   type == ColonyBuiltIn.ItemTypes.BEDZN ||
                   type == ColonyBuiltIn.ItemTypes.BEDZP ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDXN ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDXP ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDZN ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDZP;
        }

        private static void ForEachAction(Vector3Int position, bool bedIsUsed, BedTracker.Bed bed, ref BedStateObject stateObject)
        {
            if (bed.Colony == stateObject.Colony && !stateObject.Beds.ContainsKey(bed))
                stateObject.Beds.Add(bed, new CachedBedState() { Position = position, Taken = bedIsUsed });
        }

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            if (player.ActiveColony != null)
                table.Item1.Rows.Add(new ButtonCallback(GameLoader.NAMESPACE + ".BedLocations", new LabelData(_localizationHelper.GetLocalizationKey("BedLocations")), 200));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".ColonyManager.CloseBeds.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".BedLocations" && 
                data.Player.ActiveColony != null &&
                BedCache.TryGetValue(data.Player.ActiveColony, out var bso))
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("BedLocations", data.Player));
                menu.Width = 800;
                menu.Height = 600;
                menu.ForceClosePopups = true;

                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();

                headerItems.Add((new Label(new LabelData(_localizationHelper.GetLocalizationKey("Position"), UnityEngine.Color.black)), 400));
                headerItems.Add((new Label(new LabelData(_localizationHelper.GetLocalizationKey("Occupied"), UnityEngine.Color.black)), 200));
                headerItems.Add((new Label(new LabelData(_localizationHelper.GetLocalizationKey("Placement"), UnityEngine.Color.black)), 200));

                menu.Items.Add(new HorizontalRow(headerItems));

                foreach (var bed in bso.Beds)
                {
                    List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();

                    items.Add((new Label(new LabelData(string.Format("x:{0} y:{1} z:{2}", bed.Value.Position.x, bed.Value.Position.y, bed.Value.Position.z), UnityEngine.Color.black)), 400));
                    items.Add((new Label(new LabelData(bed.Value.Taken ? _localizationHelper.GetLocalizationKey("Yes") : _localizationHelper.GetLocalizationKey("No"), UnityEngine.Color.black)), 200));
                    items.Add((new Label(new LabelData(bed.Value.IsHappy ? _localizationHelper.GetLocalizationKey("Good") : _localizationHelper.GetLocalizationKey("Bad"), UnityEngine.Color.black)), 200));

                    menu.Items.Add(new HorizontalRow(items));
                }

                NetworkMenuManager.SendServerPopup(data.Player, menu);
            }
        }
    }
}
