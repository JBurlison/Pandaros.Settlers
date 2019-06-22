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
        List<BlockSides> AvailableBlockSides { get; }
    }
}
