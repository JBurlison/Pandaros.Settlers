using NetworkUI;
using Pandaros.Settlers.Extender;

namespace Pandaros.Settlers.Help
{
    public interface IHelpMenuItem : INameable
    {
        IItem Item { get; }
        string MenuName { get; }
    }
}