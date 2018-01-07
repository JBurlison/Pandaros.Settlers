using BlockTypes.Builtin;
using NPC;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class PhysicianRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Physician";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".PhysicianBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";
        private static NPCTypeStandardSettings _settings;

        public static NPCTypeStandardSettings NPCTypeSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new NPCTypeStandardSettings
                    {
                        keyName = JOB_NAME,
                        printName = "Physician",
                        maskColor1 = new Color32(237, 235, 234, 255),
                        type = NPCTypeID.GetNextID()
                    };

                return _settings;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Physician.Init"),
            ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        public static void Init()
        {
            NPCType.AddSettings(NPCTypeSettings);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Physician.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<PhysicianJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.PhysicianRegister.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/PhysicianBench.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/PhysicianBench.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/PhysicianBench.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "PhysicianBench", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Jobs.PhysicianRegister.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, new JSONNode()
              .SetAs("icon", System.IO.Path.Combine(GameLoader.ICON_FOLDER_PANDA, "PhysicianBench.png"))
              .SetAs("onPlaceAudio", "woodPlace")
              .SetAs("onRemoveAudio", "woodDeleteLight")
              .SetAs("sideall", GameLoader.NAMESPACE + "PhysicianBench")
              .SetAs("npcLimit", 0)
            ));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.PhysicianRegister.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var iron = new InventoryItem(BuiltinBlocks.IronBlock, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { iron, tools, planks },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".Jobs.PhysicianRegister.OnDeath")]
        public static void OnDeath(NPCBase nPC)
        {
            if (nPC.Job != null && nPC.Job.GetType() == typeof(PhysicianJob) && ((PhysicianJob)nPC.Job).Patient != null)
            {
                if (PhysicianJob.Treating.Contains(((PhysicianJob)nPC.Job).Patient))
                    PhysicianJob.Treating.Remove(((PhysicianJob)nPC.Job).Patient);

                ((PhysicianJob)nPC.Job).Patient = null;
            }
        }
    }

    public class PhysicianJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        const int MAX_DIST = 10;
        public static List<NPCBase> Treating { get; private set; } = new List<NPCBase>();
        Vector3Int originalPosition;
        public NPCBase Patient { get; set; }
        
        public override bool ToSleep => false;

        public override string NPCTypeKey
        {
            get
            {
                return PhysicianRegister.JOB_NAME;
            }
        }

        public override InventoryItem RecruitementItem => InventoryItem.Empty;

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        public override Vector3Int GetJobLocation()
        {
            var loc = KeyLocation;

            foreach (var npc in Colony.Get(owner).Followers)
                if (npc.Job != null &&
                    typeof(Sickness) == npc.Job.GetType() &&
                    !Treating.Contains(npc) &&
                    Vector3.Distance(KeyLocation.Vector, npc.Position.Vector) <= MAX_DIST)
                {
                    Treating.Add(npc);
                    Patient = npc;
                    loc = npc.Position;
                }

            if (Patient == null)
            {
                foreach (var npc in Colony.Get(owner).Followers)
                    if (npc.health < NPC.NPCBase.MaxHealth)
                    {

                    }
            }

            return loc;
        }

        public NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return PhysicianRegister.NPCTypeSettings;
        }

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            if (Patient != null)
            {
                if (Patient.Job.GetType() == typeof(Sickness) &&
                    ((Sickness)Patient.Job).Illness.Count > 0 && !((Sickness)Patient.Job).NeedsNPC)
                {
                    var stock = Stockpile.GetStockPile(((Sickness)Patient.Job).Owner);
                    ushort cureUsed = 0;

                    foreach (var ill in ((Sickness)Patient.Job).Illness)
                        foreach (var cure in ill.Cure)
                            if (stock.Contains(cure))
                            {
                                cureUsed = cure;
                                break;
                            }

                    if (cureUsed != 0)
                    {
                        state.SetCooldown(10);
                        state.SetIndicator(NPCIndicatorType.Crafted, 10, cureUsed);
                        ServerManager.SendAudio(Patient.Position.Vector, GameLoader.NAMESPACE + ".Bandage");
                    }
                    else
                    {
                        state.SetCooldown(4);
                        state.SetIndicator(NPCIndicatorType.MissingItem, 4, ((Sickness)Patient.Job).Illness.FirstOrDefault().Cure.FirstOrDefault());
                    }
                }

                if (Patient.health < NPCBase.MaxHealth)
                {
                    var stock = Stockpile.GetStockPile(((Sickness)Patient.Job).Owner);

                    if (stock.Contains(Items.Healing.Bandage.Item.ItemIndex))
                    {
                        stock.TryRemove(Items.Healing.Bandage.Item.ItemIndex);
                        var heal = new Entities.HealingOverTimeNPC(Patient, Items.Healing.Bandage.INITIALHEAL, Items.Healing.Bandage.TOTALHOT, 5);
                    }
                }
            }
            else
            {
                Patient = null;
                state.JobIsDone = true;
                state.SetCooldown(4);
                state.SetIndicator(NPCIndicatorType.MissingItem, 4);
            }
        }

        public override ITrackableBlock InitializeFromJSON(Players.Player player, JSONNode node)
        {
            originalPosition = (Vector3Int)node[nameof(originalPosition)];
            InitializeJob(player, (Vector3Int)node["position"], node.GetAs<int>("npcID"));
            return this;
        }

        public ITrackableBlock InitializeOnAdd(Vector3Int position, ushort type, Players.Player player)
        {
            originalPosition = position;
            InitializeJob(player, position, 0);
            return this;
        }

        public override JSONNode GetJSON()
        {
            return base.GetJSON().SetAs(nameof(originalPosition), (JSONNode)originalPosition);
        }
    }
}
