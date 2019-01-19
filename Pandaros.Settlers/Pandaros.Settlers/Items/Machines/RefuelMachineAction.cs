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
            FuelValues[BuiltinBlocks.Coalore] = .20f;
            FuelValues[BuiltinBlocks.Firewood] = .10f;
            FuelValues[BuiltinBlocks.Straw] = .05f;
            FuelValues[BuiltinBlocks.LeavesTemperate] = .02f;
            FuelValues[BuiltinBlocks.LeavesTaiga] = .02f;
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
                                item.Key == BuiltinBlocks.Firewood ||
                                item.Key == BuiltinBlocks.Coalore) &&
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
