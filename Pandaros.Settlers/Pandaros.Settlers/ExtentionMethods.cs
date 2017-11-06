using BlockTypes.Builtin;
using General.Networking;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public static class ExtentionMethods
    {
        public static double NextDouble(this System.Random rng, double min, double max)
        {
            return rng.NextDouble() * (max - min) + min;
        }

        public static void SetJobTime(this BlockJobBase job, double time)
        {
            typeof(BlockJobBase).GetField("timeJob", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(job, Pipliz.Time.SecondsSinceStartDouble + time);
        }

        public static double GetJobTime(this BlockJobBase job)
        {
            return (double)typeof(BlockJobBase).GetField("timeJob", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(job);
        }

        public static List<InventoryItem> GetInventory(this Inventory inv)
        {
            return typeof(Inventory).GetField("items", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(inv) as List<InventoryItem>;
        }

        public static void Update(this List<InventoryItem> inv, Players.Player p)
        {
            NetworkWrapper.Send(inv.GetBytes(), p, NetworkMessageReliability.ReliableWithBuffering);
        }

        public static byte[] GetBytes(this List<InventoryItem> items)
        {
            byte[] result;
            using (ByteBuilder byteBuilder = ByteBuilder.Get())
            {
                byteBuilder.Write((ushort)ClientMessageType.InventoryUpdate);

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].ToBytes(byteBuilder);
                }
                result = byteBuilder.ToArray();
            }
            return result;
        }

        public static bool TakeItemFromInventory(this Players.Player player, ushort itemType)
        {
            var hasItem = false;
            var invRef = Inventory.GetInventory(player);

            if (invRef != null)
            {
                var playerInv = invRef.GetInventory();

                if (playerInv != null)
                {
                    for (int i = 0; i < playerInv.Count - 1; i++)
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
                        playerInv.Update(player);
                }
            }

            return hasItem;
        }

        public static BedBlock GetBed(this NPCBase job)
        {
            return typeof(NPCBase).GetField("bed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(job) as BedBlock;
        }
    }
}
