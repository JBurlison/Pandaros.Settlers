using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Pipliz.JSON;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class MachinistRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Machinist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".MachinistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Machinist.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<MachinistJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Machinist.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/MachinistBenchTop.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/MachinistBenchTop.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/MachinistBenchTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "MachinistBenchTop", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Machinist.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, new JSONNode()
              .SetAs("icon", Path.Combine(GameLoader.ICON_FOLDER_PANDA, "MachinistBench.png"))
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("sidey+", GameLoader.NAMESPACE + "MachinistBenchTop")
              .SetAs("npcLimit", 0)
            ));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Machinist.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var gold = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { gold, tools, stone, iron },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }
    }

    public class MachinistJob : BlockJobBase, IBlockJobBase, INPCTypeDefiner
    {
        protected float cooldown = 2f;
        Items.Machines.MachineState _targetMachine;
        Vector3Int originalPosition;
        public static List<uint> OkStatus;

        public override string NPCTypeKey
        {
            get
            {
                return GameLoader.NAMESPACE + ".Machinist";
            }
        }

        public override InventoryItem RecruitementItem
        {
            get
            {
                return new InventoryItem(BuiltinBlocks.CopperTools, 1);
            }
        }

        public override bool ToSleep => false;

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

        public override Vector3Int GetJobLocation()
        {
            position = originalPosition;

            if (_targetMachine == null)
            {
                if (MachineManager.Machines.ContainsKey(owner))
                    foreach (var machine in MachineManager.Machines[Owner].Where(m => !m.Value.HasMachinist))
                    {
                        float dis = Vector3.Distance(machine.Key.Vector, position.Vector);

                        if (dis <= 21)
                        {
                            if (machine.Value.Durability < .5f ||
                                machine.Value.Fuel < .3f ||
                                machine.Value.Load < .3f)
                            {
                                _targetMachine = machine.Value;
                                _targetMachine.HasMachinist = true;
                                position = Server.AI.AIManager.ClosestPosition(_targetMachine.Position, usedNPC.Position);
                                break;
                            }
                        }
                    }
            }
            else
            {
                position = Server.AI.AIManager.ClosestPosition(_targetMachine.Position, usedNPC.Position);
            }

            return position;
        }

        const float COOLDOWN = 3f;

        public override void OnNPCAtJob(ref NPCBase.NPCState state)
        {
            ushort status = GameLoader.Waiting_Icon;
            float cooldown = COOLDOWN;

            if (_targetMachine != null && 3 > Vector3.Distance(_targetMachine.Position.Vector, usedNPC.Position.Vector))
            {
                usedNPC.LookAt(_targetMachine.Position.Vector);

                if (_targetMachine.Durability < .50f)
                {
                    status = _targetMachine.MachineSettings.Repair(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.RepairTime;
                }
                else if (_targetMachine.Fuel < .3f)
                {
                    status = _targetMachine.MachineSettings.Refuel(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.RefuelTime;
                }
                else if (_targetMachine.Load < .3f)
                {
                    status = _targetMachine.MachineSettings.Reload(Owner, _targetMachine);
                    cooldown = _targetMachine.MachineSettings.ReloadTime;
                }
                else
                {
                    _targetMachine.HasMachinist = false;
                    _targetMachine = null;
                }   
            }

            if (_targetMachine == null)
            {
                state.JobIsDone = true;
                status = GameLoader.Waiting_Icon;
            }

            if (OkStatus.Contains(status))
                state.SetIndicator(NPCIndicatorType.Crafted, cooldown, status);
            else if (status != 0)
                state.SetIndicator(NPCIndicatorType.MissingItem, cooldown, status);
            else
                state.SetIndicator(NPCIndicatorType.Crafted, cooldown, BuiltinBlocks.ErrorMissing);

            state.SetCooldown(cooldown);

            if (_targetMachine == null)
                state.JobIsDone = true;
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            return NPCBase.NPCGoal.Job;
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = this.NPCTypeKey,
                printName = "Machinist",
                maskColor1 = new Color32(242, 132, 29, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }
    }
}
