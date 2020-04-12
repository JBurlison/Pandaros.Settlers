
using Newtonsoft.Json;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pandaros.API.Models;
using Pandaros.API;
using Chatting;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Pandaros.Settlers.Decorative
{
    public static class CollederStorage
    {

        public static Dictionary<string, List<Colliders.Boxes>> Colliders_Dict = new Dictionary<string, List<Colliders.Boxes>>();

        static CollederStorage()
        {
            Colliders_Dict.Add(nameof(Stairs), Stairs);
            Colliders_Dict.Add(nameof(StairsCornerInverted), StairsCornerInverted);
            Colliders_Dict.Add(nameof(StairsCorner), StairsCorner);
            Colliders_Dict.Add(nameof(InvertedStairs), InvertedStairs);
            Colliders_Dict.Add(nameof(InvertedStairsCornerInverted), InvertedStairsCornerInverted);
            Colliders_Dict.Add(nameof(InvertedStairsCorner), InvertedStairsCorner);
            Colliders_Dict.Add(nameof(VerticalSlab), VerticalSlab);
            Colliders_Dict.Add(nameof(Ramp), Ramp);
            Colliders_Dict.Add(nameof(RampCornerInverted), RampCornerInverted);
            Colliders_Dict.Add(nameof(RampCorner), RampCorner);
            Colliders_Dict.Add(nameof(InvertedRamp), InvertedRamp);
            Colliders_Dict.Add(nameof(InvertedRampCornerInverted), InvertedRampCornerInverted);
            Colliders_Dict.Add(nameof(InvertedRampCorner), InvertedRampCorner);
            Colliders_Dict.Add(nameof(QuarterBlock), QuarterBlock);
            Colliders_Dict.Add(nameof(RaisedQuarterBlock), RaisedQuarterBlock);
            Colliders_Dict.Add(nameof(EdgeBlock), EdgeBlock);
            Colliders_Dict.Add(nameof(RaisedEdgeBlock), RaisedEdgeBlock);
            Colliders_Dict.Add(nameof(CornerBlock), CornerBlock);
            Colliders_Dict.Add(nameof(RaisedCornerBlock), RaisedCornerBlock);
            Colliders_Dict.Add(nameof(SlabUp), SlabUp);
            Colliders_Dict.Add(nameof(SlabDown), SlabDown);
        }

        public static List<Colliders.Boxes> Slab = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f })
                };

        public static List<Colliders.Boxes> Stairs = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ -0.25f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0.25f, 0.25f, -0.5f })
                };
        public static List<Colliders.Boxes> StairsCornerInverted = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.25f, 0f, 0.25f }, new List<float>(){ -0.5f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ -0.25f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0.25f, 0f }, new List<float>(){ -0.5f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.25f, 0.5f, -0.25f }, new List<float>(){ -0.5f, 0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0.25f, 0.25f, -0.5f })
                };
        public static List<Colliders.Boxes> StairsCorner = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.25f }, new List<float>(){ -0.25f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, -0.25f }, new List<float>(){ 0.25f, 0.25f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedStairs = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0.5f }, new List<float>(){ -0.25f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, 0.5f }, new List<float>(){ 0.25f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedStairsCornerInverted = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.25f, 0.25f, 0.25f }, new List<float>(){ -0.5f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0.5f }, new List<float>(){ -0.25f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0f, 0f }, new List<float>(){ -0.5f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.25f, -0.25f, -0.25f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, 0.5f }, new List<float>(){ 0.25f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedStairsCorner = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.25f, 0.25f }, new List<float>(){ -0.25f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0f }, new List<float>(){ 0f, -0.25f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.25f, -0.25f }, new List<float>(){ 0.25f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> VerticalSlab = new List<Colliders.Boxes>()
                {
                new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> Ramp = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, 0.5f }, new List<float>(){ -0.4f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, 0.5f }, new List<float>(){ -0.3f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, 0.5f }, new List<float>(){ -0.2f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ -0.1f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, 0.5f }, new List<float>(){ 0.1f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, 0.5f }, new List<float>(){ 0.2f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, 0.5f }, new List<float>(){ 0.3f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0.4f, 0.4f, -0.5f })
                };
        public static List<Colliders.Boxes> RampCornerInverted = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.4f, -0.3f, 0.4f }, new List<float>(){ -0.5f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, 0.5f }, new List<float>(){ -0.4f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.3f, -0.2f, 0.3f }, new List<float>(){ -0.5f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, 0.5f }, new List<float>(){ -0.3f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, 0.5f }, new List<float>(){ -0.2f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.2f, -0.1f, 0.2f }, new List<float>(){ -0.5f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ -0.1f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.1f, 0f, 0.1f }, new List<float>(){ -0.5f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0.1f, 0f }, new List<float>(){ -0.5f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, 0.5f }, new List<float>(){ 0.1f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.1f, 0.2f, -0.1f }, new List<float>(){ -0.5f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, 0.5f }, new List<float>(){ 0.2f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.2f, 0.3f, -0.2f }, new List<float>(){ -0.5f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, 0.5f }, new List<float>(){ 0.3f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.3f, 0.4f, -0.3f }, new List<float>(){ -0.5f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0.4f, 0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.4f, 0.5f, -0.4f }, new List<float>(){ -0.5f, 0.4f, -0.5f })
                };
        public static List<Colliders.Boxes> RampCorner = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, 0.5f }, new List<float>(){ -0.5f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, 0.4f }, new List<float>(){ -0.4f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, 0.3f }, new List<float>(){ -0.3f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, 0.2f }, new List<float>(){ -0.2f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.1f }, new List<float>(){ -0.1f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, -0.1f }, new List<float>(){ 0.1f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, -0.2f }, new List<float>(){ 0.2f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, -0.3f }, new List<float>(){ 0.3f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, -0.4f }, new List<float>(){ 0.4f, 0.4f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedRamp = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, 0.5f }, new List<float>(){ -0.4f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, 0.5f }, new List<float>(){ -0.3f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, 0.5f }, new List<float>(){ -0.2f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0.5f }, new List<float>(){ -0.1f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, 0.5f }, new List<float>(){ 0.1f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, 0.5f }, new List<float>(){ 0.2f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, 0.5f }, new List<float>(){ 0.3f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, 0.5f }, new List<float>(){ 0.4f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedRampCornerInverted = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.4f, 0.4f, 0.4f }, new List<float>(){ -0.5f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, 0.5f }, new List<float>(){ -0.4f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.3f, 0.3f, 0.3f }, new List<float>(){ -0.5f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, 0.5f }, new List<float>(){ -0.3f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, 0.5f }, new List<float>(){ -0.2f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.2f, 0.2f, 0.2f }, new List<float>(){ -0.5f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0.5f }, new List<float>(){ -0.1f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ -0.1f, 0.1f, 0.1f }, new List<float>(){ -0.5f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0f, 0f }, new List<float>(){ -0.5f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, 0.5f }, new List<float>(){ 0.1f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.1f, -0.1f, -0.1f }, new List<float>(){ -0.5f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, 0.5f }, new List<float>(){ 0.2f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.2f, -0.2f, -0.2f }, new List<float>(){ -0.5f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, 0.5f }, new List<float>(){ 0.3f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.3f, -0.3f, -0.3f }, new List<float>(){ -0.5f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, 0.5f }, new List<float>(){ 0.4f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.4f, -0.4f, -0.4f }, new List<float>(){ -0.5f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> InvertedRampCorner = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.4f, 0.4f }, new List<float>(){ -0.4f, 0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.3f, 0.3f }, new List<float>(){ -0.3f, 0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.2f, 0.2f }, new List<float>(){ -0.2f, 0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.1f, 0.1f }, new List<float>(){ -0.1f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0f }, new List<float>(){ 0f, -0.1f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.1f, -0.1f }, new List<float>(){ 0.1f, -0.2f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.2f, -0.2f }, new List<float>(){ 0.2f, -0.3f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.3f, -0.3f }, new List<float>(){ 0.3f, -0.4f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0.5f, -0.4f, -0.4f }, new List<float>(){ 0.4f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> QuarterBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> RaisedQuarterBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f })
                };
        public static List<Colliders.Boxes> EdgeBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0f }, new List<float>(){ 0f, -0.5f, -0.5f })
                };
        public static List<Colliders.Boxes> RaisedEdgeBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0f }, new List<float>(){ 0f, 0f, -0.5f })
                };
        public static List<Colliders.Boxes> CornerBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0f, 0.5f }, new List<float>(){ 0f, -0.5f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0f, 0.5f }, new List<float>(){ -0.5f, -0.5f, 0f })
                };
        public static List<Colliders.Boxes> RaisedCornerBlock = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ 0.5f, 0.5f, 0.5f }, new List<float>(){ 0f, 0f, -0.5f }),
                    new Colliders.Boxes(new List<float>(){ 0f, 0.5f, 0.5f }, new List<float>(){ -0.5f, 0f, 0f })
                };
        public static List<Colliders.Boxes> SlabUp = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ -0.5f, 0f, -0.5f }, new List<float>(){ 0.5f, 0.5f, 0.5f })
                };
        public static List<Colliders.Boxes> SlabDown = new List<Colliders.Boxes>()
                {
                    new Colliders.Boxes(new List<float>(){ -0.5f, -0.5f, -0.5f }, new List<float>(){ 0.5f, 0f, 0.5f })
                };
    }
}
