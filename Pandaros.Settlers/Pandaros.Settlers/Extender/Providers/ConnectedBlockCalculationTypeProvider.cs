using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ConnectedBlockCalculationTypeProvider : IAfterModsLoaded
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IConnectedBlockCalculationType);
        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.LogToFile("-------------------Connected Block CalculationType Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IConnectedBlockCalculationType connectedBlockCalcType &&
                    !string.IsNullOrEmpty(connectedBlockCalcType.name))
                {
                    sb.Append($"{connectedBlockCalcType.name}, ");
                    ConnectedBlockCalculator.CalculationTypes[connectedBlockCalcType.name] = connectedBlockCalcType;
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("---------------------------------------------------------");
        }
    }
}
