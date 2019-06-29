using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public interface IConnectedBlockCalculationType : INameable
    {
        List<BlockSide> AvailableBlockSides { get; }
        List<RotationAxis> AxisRotations { get; }
        int MaxConnections { get; }
    }
}
