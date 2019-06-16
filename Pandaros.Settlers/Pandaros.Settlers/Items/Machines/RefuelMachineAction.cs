using BlockTypes;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public class RefuelMachineAction : IRoamingJobObjectiveAction
    {
        public static Dictionary<ItemId, float> FuelValues = new Dictionary<ItemId, float>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.SetFuelValues")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void SetFuelValues()
        {
            FuelValues[ColonyBuiltIn.ItemTypes.COALORE] = .20f;
            FuelValues[ColonyBuiltIn.ItemTypes.FIREWOOD] = .10f;
            FuelValues[ColonyBuiltIn.ItemTypes.STRAW] = .05f;
            FuelValues[ColonyBuiltIn.ItemTypes.LEAVESTEMPERATE] = .02f;
            FuelValues[ColonyBuiltIn.ItemTypes.LEAVESTAIGA] = .02f;
        }

        public string name => MachineConstants.REFUEL;

        public float TimeToPreformAction => 4;

        public string AudioKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Refuel");

        public ItemId PreformAction(Colony colony, RoamingJobState machineState)
        {
            if (!colony.OwnerIsOnline() && SettlersConfiguration.OfflineColonies || colony.OwnerIsOnline())
            {
                if (machineState.GetActionEnergy(MachineConstants.REFUEL) < .75f)
                {
                    var stockpile = colony.Stockpile;

                    foreach (var item in FuelValues)
                        while ((stockpile.AmountContained(item.Key) > 100 ||
                                item.Key == ColonyBuiltIn.ItemTypes.FIREWOOD ||
                                item.Key == ColonyBuiltIn.ItemTypes.COALORE) &&
                                machineState.GetActionEnergy(MachineConstants.REFUEL) < RoamingJobState.GetActionsMaxEnergy(MachineConstants.REFUEL, colony, MachineConstants.MECHANICAL))
                        {
                            stockpile.TryRemove(item.Key);
                            machineState.AddToActionEmergy(MachineConstants.REFUEL, item.Value);
                        }

                    if (machineState.GetActionEnergy(MachineConstants.REFUEL) < RoamingJobState.GetActionsMaxEnergy(MachineConstants.REFUEL, colony, MachineConstants.MECHANICAL))
                        return FuelValues.First().Key;
                }
            }

            return ObjectiveLoadEmptyIcon;
        }
    }
}
