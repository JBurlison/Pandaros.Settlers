using BlockTypes.Builtin;
using Pipliz.APIProvider.Jobs;
using Pipliz.JSON;
using NPC;
using Pipliz;
using Server.AI;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class HerbalistRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Herbalist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".HerbalistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        public static string HERB_NAME = GameLoader.NAMESPACE + ".Herbs";
        public static string HERB1_NAME = HERB_NAME + "Stage1";
        public static string HERB2_NAME = HERB_NAME + "Stage2";

        public static ItemTypesServer.ItemTypeRaw HerbBench;
        public static ItemTypesServer.ItemTypeRaw HerbItem;
        public static ItemTypesServer.ItemTypeRaw HerbStage1;
        public static ItemTypesServer.ItemTypeRaw HerbStage2;
        private static BlockTracker<HerbalistJob> _areaJobTracker;
        
        static NPCTypeStandardSettings _settings;

        public static NPCType NPCType;
        public static NPCTypeStandardSettings NPCTypeSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new NPCTypeStandardSettings
                    {
                        keyName = JOB_NAME,
                        printName = "Herbalist",
                        maskColor1 = new Color32(84, 115, 37, 255),
                        type = NPCTypeID.GetNextID()
                    };

                return _settings;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(NPCTypeSettings);
            NPCType = NPCType.GetByKeyNameOrDefault(NPCTypeSettings.keyName);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/HerbalistBench.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/HerbalistBench.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/HerbalistBench.png";

            ItemTypesServer.SetTextureMapping(JOB_ITEM_KEY, textureMapping);
            ItemTypesServer.SetTextureMapping(HERB1_NAME, new ItemTypesServer.TextureMapping(new JSONNode())
            {
                AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/herbs.png"
            });

            ItemTypesServer.SetTextureMapping(HERB2_NAME, new ItemTypesServer.TextureMapping(new JSONNode())
            {
                AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/herbs.png"
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var herbName = HERB_NAME;
            var herbNode = new JSONNode();
            herbNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/herb.png");
            herbNode["isPlaceable"] = new JSONNode(false);

            HerbItem = new ItemTypesServer.ItemTypeRaw(herbName, herbNode);
            itemTypes.Add(herbName, HerbItem);

            HerbBench = new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, new JSONNode()
              .SetAs("icon", System.IO.Path.Combine(GameLoader.ICON_FOLDER_PANDA, "HerbalistBench.png"))
              .SetAs("onPlaceAudio", "woodPlace")
              .SetAs("onRemoveAudio", "woodDeleteLight")
              .SetAs("sideall", JOB_ITEM_KEY)
              .SetAs("npcLimit", 0)
            );

            itemTypes.Add(JOB_ITEM_KEY, HerbBench);

            HerbStage1 = new ItemTypesServer.ItemTypeRaw(HERB1_NAME, new JSONNode()
              .SetAs("icon", System.IO.Path.Combine(GameLoader.ICON_FOLDER_PANDA, "herbstage1.png"))
              .SetAs("onPlaceAudio", "grassDelete")
              .SetAs("onRemoveAudio", "grassDelete")
              .SetAs("sideall", HERB1_NAME)
              .SetAs("isSolid", false)
              .SetAs("maxStackSize", 1200)
              .SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/herbstage1.obj")
              .SetAs("onRemove", new JSONNode(NodeType.Array)
                                  .AddToArray(new JSONNode()
                                              .SetAs("type", HERB1_NAME)
                                              .SetAs("amount", 1)
                                              .SetAs("chance", .8))
                                  )
            );

            itemTypes.Add(HERB1_NAME, HerbStage1);

            HerbStage2 = new ItemTypesServer.ItemTypeRaw(HERB2_NAME, new JSONNode()
              .SetAs("onPlaceAudio", "grassDelete")
              .SetAs("onRemoveAudio", "grassDelete")
              .SetAs("sideall", HERB2_NAME)
              .SetAs("isSolid", false)
              .SetAs("maxStackSize", 1200)
              .SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/herbstage2.obj")
              .SetAs("onRemove", new JSONNode(NodeType.Array)
                                  .AddToArray(new JSONNode()
                                              .SetAs("type", HERB1_NAME)
                                              .SetAs("amount", 1)
                                              .SetAs("chance", 1)
                                  ).AddToArray(new JSONNode()
                                              .SetAs("type", HERB1_NAME)
                                              .SetAs("amount", 1)
                                              .SetAs("chance", .3)
                                  ).AddToArray(new JSONNode()
                                              .SetAs("type", HERB_NAME)
                                              .SetAs("amount", 1)
                                              .SetAs("chance", 1)
                                  )
                     )
            );

            itemTypes.Add(HERB2_NAME, HerbStage2);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var iron = new InventoryItem(BuiltinBlocks.IronBlock, 1);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.CoatedPlanks, 6);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { iron, tools, planks },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.LoadUpdatables"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadupdatableblocks")]
        private static void LoadUpdatables()
        {
            if (JSON.Deserialize(GameLoader.GetUpdatableBlocksJSONPath(), out JSONNode jSONNode, false))
            {
                if (jSONNode.TryGetChild(Items.UpdatableBlocks.Herbs.NAME, out JSONNode array))
                {
                    Items.UpdatableBlocks.Herbs.Load(array);
                }
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.TypeNew == HerbStage1.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
                Items.UpdatableBlocks.Herbs.OnAdd(d.VoxelToChange, HerbStage1.ItemIndex, d.requestedBy);
            else if (d.TypeNew == BuiltinBlocks.Air && (d.typeTillNow == HerbStage1.ItemIndex || d.typeTillNow == HerbStage2.ItemIndex))
                Items.UpdatableBlocks.Herbs.OnRemove(d.VoxelToChange, d.typeTillNow, d.requestedBy);
            else if (d.TypeNew == HerbBench.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
                _areaJobTracker.Add(new HerbalistJob(new Bounds(d.VoxelToChange.Vector, new Vector3(6, 0, 6)), d.requestedBy, 0));
            else if (d.TypeNew == BuiltinBlocks.Air && d.typeTillNow == HerbBench.ItemIndex)
            {
                var bounds = new Bounds(d.VoxelToChange.Vector, new Vector3(6, 0, 6));
                var jobPos = new Vector3Int(bounds.min);

                _areaJobTracker.GetList(d.requestedBy)[jobPos].OnRemove();
                _areaJobTracker.Remove(jobPos);
            }

            return true;
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.LoadHerbAreaJob"), ModLoader.ModCallbackDependsOnAttribute("pipliz.server.loadnpcs")]
        private static void Load()
        {
            _areaJobTracker = new BlockTracker<HerbalistJob>(JOB_NAME);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnQuit, GameLoader.NAMESPACE + ".Jobs.HerbalistRegister.SaveHerbAreaJob")]
        private static void Save()
        {
            if (_areaJobTracker != null)
            {
                _areaJobTracker.Save();
            }
        }
    }

    public class HerbalistJob : AreaJob
    {
        private bool shouldDumpInventory;

        public override NPCType NPCType
        {
            get
            {
                return HerbalistRegister.NPCType;
            }
        }

        public override bool NeedsItems
        {
            get
            {
                return shouldDumpInventory;
            }
        }

        public HerbalistJob(Bounds box, Players.Player player, int npcID)
        {
            InitializeAreaJob(new Vector3Int(box.min), new Vector3Int(box.max), player, npcID);
            SetLayer(BuiltinBlocks.Dirt, -1);
        }

        public HerbalistJob()
        {
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            base.InitializeAreaJob(player, node);
            return this;
        }

        public override void TakeItems(ref NPCBase.NPCState state)
        {
            base.TakeItems(ref state);
            shouldDumpInventory = false;
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            state.JobIsDone = true;
            if (positionSub.IsValid)
            {
                ushort num;
                if (World.TryGetTypeAt(positionSub, out num))
                {
                    if (num == 0)
                    {
                        if (state.Inventory.TryGetOneItem(HerbalistRegister.HerbStage1.ItemIndex) || usedNPC.Colony.UsedStockpile.TryRemove(HerbalistRegister.HerbStage1.ItemIndex, 1))
                        {
                            ServerManager.TryChangeBlock(positionSub, HerbalistRegister.HerbStage1.ItemIndex, ServerManager.SetBlockFlags.DefaultAudio);
                            Items.UpdatableBlocks.Herbs.OnAdd(positionSub, HerbalistRegister.HerbStage1.ItemIndex, Owner);
                            state.SetCooldown(1.0);
                            shouldDumpInventory = false;
                        }
                        else
                        {
                            state.SetIndicator(NPCIndicatorType.MissingItem, 2f, HerbalistRegister.HerbStage1.ItemIndex);
                            shouldDumpInventory = (state.Inventory.UsedCapacity > 0f);
                        }
                    }
                    else if (num == HerbalistRegister.HerbStage2.ItemIndex)
                    {

                        if (ServerManager.TryChangeBlock(positionSub, 0, ServerManager.SetBlockFlags.DefaultAudio))
                        {
                            usedNPC.Inventory.Add(ItemTypes.GetType(HerbalistRegister.HerbStage2.ItemIndex).OnRemoveItems);
                            Items.UpdatableBlocks.Herbs.OnRemove(positionSub, HerbalistRegister.HerbStage2.ItemIndex, Owner);
                        }

                        state.SetCooldown(1.0);
                        shouldDumpInventory = false;
                    }
                    else
                    {
                        state.SetCooldown(5.0);
                        shouldDumpInventory = (state.Inventory.UsedCapacity > 0f);
                    }
                }
                else
                    state.SetCooldown((double)Pipliz.Random.NextFloat(3f, 6f));

                positionSub = Vector3Int.invalidPos;
            }
            else
            {
                state.SetCooldown(10.0);
            }
        }

        public override void CalculateSubPosition()
        {
            bool flag = usedNPC.Colony.UsedStockpile.Contains(HerbalistRegister.HerbStage1.ItemIndex, 1);
            bool flag2 = false;
            Vector3Int newSubPos = Vector3Int.invalidPos;

            for (int i = positionMin.x; i <= positionMax.x; i++)
            {
                int num = (!flag2) ? positionMin.z : positionMax.z;

                while ((!flag2) ? (num <= positionMax.z) : (num >= positionMin.z))
                {
                    Vector3Int vector3Int = new Vector3Int(i, positionMin.y, num);
                    ushort num2;

                    if (!AIManager.Loaded(vector3Int) || !World.TryGetTypeAt(vector3Int, out num2))
                        return;

                    if (num2 == 0)
                    {
                        if (!flag && !newSubPos.IsValid)
                            newSubPos = vector3Int;

                        if (flag)
                        {
                            positionSub = vector3Int;
                            return;
                        }
                    }

                    if (num2 == HerbalistRegister.HerbStage2.ItemIndex)
                    {
                        positionSub = vector3Int;
                        return;
                    }

                    num = ((!flag2) ? (num + 1) : (num - 1));
                }

                flag2 = !flag2;
            }

            if (newSubPos.IsValid)
            {
                positionSub = newSubPos;
                return;
            }

            int a = Pipliz.Random.Next(positionMin.x, positionMax.x + 1);
            int c = Pipliz.Random.Next(positionMin.z, positionMax.z + 1);
            positionSub = new Vector3Int(a, positionMin.y, c);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Jobs.HerbalistJob.PlaceJob")]
        public static void PlaceJob(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.typeTillNow == HerbalistRegister.HerbBench.ItemIndex)
            {
            }
        }
    }
}
