using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Extender;

namespace Pandaros.Settlers.Items
{
    public interface IPlayerMagicItem : ICSType, IMagicEffect
    {
        Players.Player Owner { get; set; }
        float MovementSpeed { get; }
        float JumpPower { get; }
        float FlySpeed { get; }
        float MoveSpeed { get; }
        float LightRange { get; }
        UnityEngine.Color LightColot { get; }
        float FallDamage { get; }
        float FallDamagePerUnit { get; }
        float BuildDistance { get; }
    }
}