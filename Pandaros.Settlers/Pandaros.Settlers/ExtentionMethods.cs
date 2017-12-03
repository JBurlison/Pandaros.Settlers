using BlockTypes.Builtin;
using General.Networking;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers
{
    public static class ExtentionMethods
    {
        public static double NextDouble(this System.Random rng, double min, double max)
        {
            return rng.NextDouble() * (max - min) + min;
        }

        public static bool TakeItemFromInventory(this Players.Player player, ushort itemType)
        {
            var hasItem = false;
            var invRef = Inventory.GetInventory(player);

            if (invRef != null)
            {
                var playerInv = invRef.Items;

                if (playerInv != null)
                {
                    for (int i = 0; i < playerInv.Length - 1; i++)
                    {
                        if (playerInv[i].Type == itemType && playerInv[i].Amount > 0)
                        {
                            hasItem = true;
                            var count = playerInv[i].Amount - 1;

                            if (count >= 1)
                                playerInv[i] = new InventoryItem(itemType, count);
                            else
                                playerInv[i] = InventoryItem.Empty;
                        }
                    }

                    if (hasItem)
                        invRef.SendUpdate();
                }
            }

            return hasItem;
        }

        public static Vector3Int GetClosestPositionWithinY(this Vector3Int goalPosition, Vector3Int currentPosition, int minMaxY)
        {
            Vector3Int pos = Server.AI.AIManager.ClosestPosition(goalPosition, currentPosition);

            if (pos == Vector3Int.invalidPos)
            {
                int y = -1;
                var negY = (minMaxY * -1);

                while (pos == Vector3Int.invalidPos)
                {
                    pos = Server.AI.AIManager.ClosestPosition(goalPosition.Add(0, y, 0), currentPosition);

                    if (y > 0)
                    {
                        y++;

                        if (y > minMaxY)
                            break;
                    }
                    else
                    {
                        y--;

                        if (y < negY)
                            y = 1;
                    }
                }
            }

            return pos;
        }

        public static void Heal(this NPC.NPCBase nPC, float heal)
        {
            nPC.health += heal;

            if (nPC.health > NPC.NPCBase.MaxHealth)
                nPC.health = NPC.NPCBase.MaxHealth;

            nPC.Update();
        }

        public static void Heal(this Players.Player pc, float heal)
        {
            pc.Health += heal;

            if (pc.Health > pc.HealthMax)
                pc.Health = pc.HealthMax;

            pc.SendHealthPacket();
        }
    }
}
