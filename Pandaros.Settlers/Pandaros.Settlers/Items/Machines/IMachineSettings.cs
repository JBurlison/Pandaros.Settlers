using System;

namespace Pandaros.Settlers.Items.Machines
{
    public interface IMachineSettings
    {
        string Name { get; set; }
        Action<Players.Player, MachineState> DoWork { get; set; }
        ushort ItemIndex { get; set; }
        Func<Players.Player, MachineState, ushort> Refuel { get; set; }
        float RefuelTime { get; set; }
        Func<Players.Player, MachineState, ushort> Reload { get; set; }
        float ReloadTime { get; set; }
        Func<Players.Player, MachineState, ushort> Repair { get; set; }
        float RepairTime { get; set; }
        float WorkTime { get; set; }
        string MachineType { get; set; }
        string RefuelAudioKey { get; set; }
        string ReloadAudioKey { get; set; }
        string RepairAudioKey { get; set; }
    }
}