using System.Collections.Generic;
using BlockTypes.Builtin;

namespace Pandaros.Settlers.Seasons
{
    public static class BlockTypeRegistry
    {
        public const string GRASS = "Grass";
        public const string LEAVES = "Leaves";

        static BlockTypeRegistry()
        {
            Mappings.Add(GRASS, new List<ushort>());
            Mappings.Add(LEAVES, new List<ushort>());

            Mappings[GRASS].Add(BuiltinBlocks.GrassRainForest);
            Mappings[GRASS].Add(BuiltinBlocks.GrassSavanna);
            Mappings[GRASS].Add(BuiltinBlocks.GrassTaiga);
            Mappings[GRASS].Add(BuiltinBlocks.GrassTemperate);
            Mappings[GRASS].Add(BuiltinBlocks.GrassTundra);
            Mappings[GRASS].Add(BuiltinBlocks.Snow);

            Mappings[LEAVES].Add(BuiltinBlocks.LeavesTemperate);
        }

        public static Dictionary<string, List<ushort>> Mappings { get; } = new Dictionary<string, List<ushort>>();
    }
}