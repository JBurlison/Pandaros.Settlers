﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.localization
{
    public class LocalizationHelper
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }

        public LocalizationHelper(string modNamespace, string prefix)
        {
            Prefix = prefix;
            Namespace = modNamespace;
        }

        public string LocalizeOrDefault(string key, Players.Player p)
        {
            string fullKey = GetLocalizationKey(key);
            var newVal = Localization.GetSentence(p.LastKnownLocale, fullKey);

            if (newVal == fullKey)
                return key;
            else
                return newVal;
        }

        public string GetLocalizationKey(string key)
        {
            return string.Concat(Namespace, ".", Prefix, ".", key);
        }
    }
}
