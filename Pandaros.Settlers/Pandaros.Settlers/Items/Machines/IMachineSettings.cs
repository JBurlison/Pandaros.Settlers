using System;
using Pandaros.Settlers.Items.Machines;

namespace Pandaros.Settlers.Items.Machines
{
    public interface IMachineSettings
    {
        Action<Players.Player, MachineState> DoWork { get; set; }
        ushort ItemIndex { get; set; }
        Func<Players.Player, MachineState, ushort> Refuel { get; set; }
        float RefuelTime { get; set; }
        Func<Players.Player, MachineState, ushort> Reload { get; set; }
        float ReloadTime { get; set; }
        Func<Players.Player, MachineState, ushort> Repair { get; set; }
        float RepairTime { get; set; }
        float WorkTime { get; set; }
    }
}