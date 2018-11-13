using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBTReader
{
    public interface ICacheTable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the value at the given index.
        /// </summary>
        /// <param name="index">The index to fetch.</param>
        T this[int index] { get; }
    }
}
