using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs.Roaming;

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

        string IRoamingJobObjectiveAction.Name => MachineConstants.REFUEL;

        float IRoamingJobObjectiveAction.TimeToPreformAction => 4;

        string IRoamingJobObjectiveAction.AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        ushort IRoamingJobObjectiveAction.ObjectiveLoadEmptyIcon => GameLoader.Refuel_Icon;

        ushort IRoamingJobObjectiveAction.PreformAction(Players.Player player, RoamingJobState machineState)
        {
            if (!player.IsConnected && Configuration.OfflineColonies || player.IsConnected)
            {
                var ps = PlayerState.GetPlayerState(player);

                if (machineState.ActionLoad[MachineConstants.REFUEL] < .75f)
                {
                    var stockpile = Stockpile.GetStockPile(player);

                    foreach (var item in FuelValues)
                        while ((stockpile.AmountContained(item.Key) > 100 ||
                                item.Key == BuiltinBlocks.Firewood ||
                                item.Key == BuiltinBlocks.Coalore) &&
                                machineState.ActionLoad[MachineConstants.REFUEL] < RoamingJobState.GetMaxLoad(MachineConstants.REFUEL, player, MachineConstants.MECHANICAL))
                        {
                            stockpile.TryRemove(item.Key);
                            machineState.ActionLoad[MachineConstants.REFUEL] += item.Value;
                        }

                    if (machineState.ActionLoad[MachineConstants.REFUEL] < RoamingJobState.GetMaxLoad(MachineConstants.REFUEL, player, MachineConstants.MECHANICAL))
                        return FuelValues.First().Key;
                }
            }

            return GameLoader.Refuel_Icon;
            
        }
    }
}
