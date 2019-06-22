﻿using AI;
using Jobs;
using Newtonsoft.Json;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = System.Random;

namespace Pandaros.Settlers
{
    public static class ExtentionMethods
    {
        public static bool IsConnected(this Players.Player p)
        {
            return p.ConnectionState == Players.EConnectionState.Connected || p.ConnectionState == Players.EConnectionState.Connecting;
        }

        public static double NextDouble(this Random rng, double min, double max)
        {
            return rng.NextDouble() * (max - min) + min;
        }

        public static bool TakeItemFromInventory(this Players.Player player, ushort itemType)
        {
            var hasItem = false;
            var invRef = player.Inventory;

            if (invRef != null)
                invRef.TryRemove(itemType);

            return hasItem;
        }

        public static Vector3Int GetClosestPositionWithinY(this Vector3Int goalPosition, Vector3Int currentPosition, int minMaxY)
        {
            var pos = currentPosition;

            if (PathingManager.TryCanStandNear(goalPosition, out var canStand, out pos) && !canStand)
            {
                var y    = -1;
                var negY = minMaxY * -1;

                while (PathingManager.TryCanStandNear(goalPosition.Add(0, y, 0), out var canStandNow, out pos) && !canStandNow)
                {
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

        public static void Heal(this NPCBase nPC, float heal)
        {
            if (nPC != null)
            {
                nPC.health += heal;

                if (nPC.health > nPC.Colony.NPCHealthMax)
                    nPC.health = nPC.Colony.NPCHealthMax;

                nPC.Update();
            }
        }

        public static void Heal(this Players.Player pc, float heal)
        {
            pc.Health += heal;

            if (pc.Health > pc.HealthMax)
                pc.Health = pc.HealthMax;

            pc.SendHealthPacket();
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            var Arr = (T[]) Enum.GetValues(src.GetType());
            var j   = Array.IndexOf(Arr, src) + 1;
            return Arr.Length == j ? Arr[0] : Arr[j];
        }

        public static T CallAndReturn<T>(this object o, string methodName, params object[] args)
        {
            var retVal = default(T);
            var mi     = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (mi != null)
                retVal = (T) mi.Invoke(o, args);

            return retVal;
        }

        public static object Call(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null) return mi.Invoke(o, args);
            return null;
        }

        public static rT GetFieldValue<rT, oT>(this object o, string fieldName)
        {
            return (rT) typeof(oT).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o);
        }

        public static void SetFieldValue<oT>(this object o, string fieldName, object fieldValue)
        {
            typeof(oT).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, fieldValue);
        }

        public static float TotalDamage(this Dictionary<DamageType, float> damage)
        {
            return damage.Sum(kvp => kvp.Value);
        }

        private static byte ToByte(float f)
        {
            f = UnityEngine.Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static void ForEachOwner(this Colony source, Action<Players.Player> action)
        {
            foreach (var item in source.Owners)
                action(item);
        }

        public static bool OwnerIsOnline(this Colony source)
        {
            return source.Owners.Any(o => o.IsConnected());
        }

        public static T GetRandomItem<T>(this List<T> l)
        {
            return l[Pipliz.Random.Next(l.Count)];
        }

        public static bool TryGetItem(this Dictionary<string, ushort> itemInedex, string itemName, out ItemTypes.ItemType itemType)
        {
            itemType = null;

            if (itemInedex.TryGetValue(itemName, out ushort itemId) && ItemTypes.TryGetType(itemId, out itemType))
            {
                return true;
            }

            return false;
        }

        public static void Merge(this JSONNode oldNode, JSONNode newNode)
        {
            if (newNode.NodeType != NodeType.Array && oldNode.NodeType != NodeType.Array)
            {
                foreach (var node in newNode.LoopObject())
                {
                    if (oldNode.TryGetChild(node.Key, out JSONNode existingChild))
                        Merge(existingChild, node.Value);
                    else
                        oldNode.SetAs(node.Key, node.Value);
                }
            }
        }

        public static bool IsWithinBounds(this Vector3Int pos, Vector3Int boundsPos, BoundsInt bounds)
        {
            var boundsMax = boundsPos.Add(bounds.Size.x, bounds.Size.y, bounds.Size.z);

            return pos.x >= boundsPos.x && pos.y >= boundsPos.y && pos.z >= boundsPos.z &&
                    pos.x <= boundsMax.x && pos.y <= boundsMax.y && pos.z <= boundsMax.z;
        }

        public static JSONNode JsonSerialize<T>(this T obj)
        {
            var objStr = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            PandaLogger.LogToFile(objStr);
            var json = JSON.DeserializeString(objStr);
            return json;
        }

        public static T JsonDeerialize<T>(this JSONNode node)
        {
            return JsonConvert.DeserializeObject<T>(node.ToString(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

        public static Vector3Int GetBlockOffset(this Vector3Int vector, BlockSides blockSide)
        {
            switch (blockSide)
            {
                case BlockSides.XpZpYp:
                    return vector.Add(1, 1, 1);
                case BlockSides.XpYp:
                    return vector.Add(1, 1, 0);
                case BlockSides.XpZnYp:
                    return vector.Add(1, 1, -1);
                case BlockSides.ZpYp:
                    return vector.Add(0, 1, 1);
                case BlockSides.Yp:
                    return vector.Add(0, 1, 0);
                case BlockSides.ZnYp:
                    return vector.Add(0, 1, -1);
                case BlockSides.XnZpYp:
                    return vector.Add(-1, 1, 1);
                case BlockSides.XnYp:
                    return vector.Add(-1, 1, 0);
                case BlockSides.XnZnYp:
                    return vector.Add(-1, 1, -1);
                case BlockSides.XpZp:
                    return vector.Add(1, 0 ,1);
                case BlockSides.Xp:
                    return vector.Add(1, 0, 0);
                case BlockSides.XpZn:
                    return vector.Add(1, 0, -1);
                case BlockSides.Zp:
                    return vector.Add(0, 0, 1);
                case BlockSides.Zn:
                    return vector.Add(0, 0, -1);
                case BlockSides.XnZp:
                    return vector.Add(-1, 0, 1);
                case BlockSides.Xn:
                    return vector.Add(-1, 0, 0);
                case BlockSides.XnZn:
                    return vector.Add(-1, 0, -1);
                case BlockSides.XpZpYn:
                    return vector.Add(1, -1, 1);
                case BlockSides.XpYn:
                    return vector.Add(1, -1, 0);
                case BlockSides.XpZnYn:
                    return vector.Add(1, -1, 1);
                case BlockSides.ZpYn:
                    return vector.Add(0, -1, 1);
                case BlockSides.Yn:
                    return vector.Add(0, -1, 0);
                case BlockSides.ZnYn:
                    return vector.Add(0, -1, -1);
                case BlockSides.XnZpYn:
                    return vector.Add(-1, -1, 1);
                case BlockSides.XnYn:
                    return vector.Add(-1, -1, 0);
                case BlockSides.XnZnYn:
                    return vector.Add(-1, -1, -1);
                default:
                    return vector;
            }
        }
    }
}
