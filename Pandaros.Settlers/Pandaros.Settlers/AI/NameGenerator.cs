using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pandaros.Settlers.AI
{
    public static class NameGenerator
    {
        private static readonly Random _r = new Random();
        private static readonly List<string> _firstNames;
        private static readonly List<string> _lastNames;

        static NameGenerator()
        {
            // Get first and last names
            var fnames = File.ReadAllText(GameLoader.MOD_FOLDER + "/AI/FirstNames.csv").Replace("\r", "");
            fnames.Replace(" ", "");
            var afirstNames = fnames.Split('\n');
            _firstNames = afirstNames.OfType<string>().ToList();

            var lnames = File.ReadAllText(GameLoader.MOD_FOLDER + "/AI/LastNames.csv").Replace("\r", "");
            lnames.Replace(" ", "");
            var alastNames = lnames.Split('\n');
            _lastNames = alastNames.OfType<string>().ToList();
        }

        public static string GetName()
        {
            var findex = _r.Next(_firstNames.Count);
            var lindex = _r.Next(_lastNames.Count);
            return string.Concat(_firstNames[findex], " ", _lastNames[lindex]);
        }
    }
}