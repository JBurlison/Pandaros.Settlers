using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.localization
{
    public class LocalizationHelper
    {
        public string Prefix { get; set; }

        public LocalizationHelper(string prefix)
        {
            Prefix = prefix;
        }

        public string LocalizeOrDefault(string key, Players.Player p, string defaultString = "")
        {
            string fullKey = GetLocalizationKey(key);
            var newVal = Localization.GetSentence(p.LastKnownLocale, key);

            if (newVal == fullKey)
                if (!string.IsNullOrEmpty(defaultString))
                    return defaultString;
                else
                    return key;
            else
                return newVal;
        }

        public string GetLocalizationKey(string key)
        {
            return string.Concat(GameLoader.NAMESPACE, ".", Prefix, ".", key);
        }
    }
}
