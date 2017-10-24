using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.AI
{
    public static class NameGenerator
    {
        static Random _r = new Random();
        static List<string> _firstNames;
        static List<string> _lastNames;

        static NameGenerator()
        {
            // Get first and last names
            string fnames = File.ReadAllText(GameLoader.MOD_FOLDER + "\\AI\\FirstNames.csv").Replace("\r", "");
            fnames.Replace(" ", "");
            String[] afirstNames = fnames.Split('\n');
            _firstNames = afirstNames.OfType<string>().ToList();

            string lnames = File.ReadAllText(GameLoader.MOD_FOLDER + "\\AI\\LastNames.csv").Replace("\r", "");
            lnames.Replace(" ", "");
            String[] alastNames = lnames.Split('\n');
            _lastNames = alastNames.OfType<string>().ToList();
        }

        public static string GetName()
        {
            int findex = _r.Next(_firstNames.Count);
            int lindex = _r.Next(_lastNames.Count);
            return string.Concat(_firstNames[findex], " ", _lastNames[lindex]);
        }
    }
}
