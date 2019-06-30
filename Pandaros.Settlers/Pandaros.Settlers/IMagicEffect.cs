﻿using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers
{
    public interface IMagicEffect : IPandaArmor, IPandaDamage, INameable, ILucky
    {
        bool IsMagical { get; set; }
        float Skilled { get; set; }
        float HPTickRegen { get; }
        void Update();
    }
}