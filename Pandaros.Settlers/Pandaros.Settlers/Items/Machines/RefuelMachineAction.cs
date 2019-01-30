using BlockTypes;
using Pandaros.Settlers.Jobs.Roaming;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public class RefuelMachineAction : IRoamingJobObjectiveAction
    {
        public static Dictionary<ushort, float> FuelValues = new Dictionary<ushort, float>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.SetFuelValues")]
        public static void SetFuelValues()
        {
            FuelValues[ColonyBuiltIn.ItemTypes.COALORE] = .20f;
            FuelValues[ColonyBuiltIn.ItemTypes.FIREWOOD] = .10f;
            FuelValues[ColonyBuiltIn.ItemTypes.STRAW] = .05f;
            FuelValues[ColonyBuiltIn.ItemTypes.LEAVESTEMPERATE] = .02f;
            FuelValues[ColonyBuiltIn.ItemTypes.LEAVESTAIGA] = .02f;
        }

        string INameable.name => MachineConstants.REFUEL;

        float IRoamingJobObjectiveAction.TimeToPreformAction => 4;

        string IRoamingJobObjectiveAction.AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        ushort IRoamingJobObjectiveAction.ObjectiveLoadEmptyIcon => GameLoader.Refuel_Icon;

        ushort IRoamingJobObjectiveAction.PreformAction(Colony colony, RoamingJobState machineState)
        {
            if (!colony.OwnerIsOnline() && Configuration.OfflineColonies || colony.OwnerIsOnline())
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

            return GameLoader.Refuel_Icon;
            
        }
    }
}
