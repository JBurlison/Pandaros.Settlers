using BlockTypes;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using Pipliz;
using Pipliz.JSON;
using Shared.Networking;
using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class PlayerState
    {
        private static readonly Dictionary<Players.Player, PlayerState> _playerStates = new Dictionary<Players.Player, PlayerState>();
        private static string _Enviorment = GameLoader.NAMESPACE + ".Enviorment";

        public PlayerState(Players.Player p)
        {
            Player = p;
            Rand = new Random();
            SetupArmor();

            HealingOverTimePC.NewInstance += HealingOverTimePC_NewInstance;
            _playerVariables = JSON.Deserialize("gamedata/settings/serverperclient.json");
        }

        public JSONNode _playerVariables = new JSONNode();
        public Random Rand { get; set; }
        public static List<HealingOverTimePC> HealingSpells { get; } = new List<HealingOverTimePC>();
        public Players.Player Player { get; }
        public List<Vector3Int> FlagsPlaced { get; set; } = new List<Vector3Int>();
        public Vector3Int TeleporterPlaced { get; set; } = Vector3Int.invalidPos;
        public EventedDictionary<ArmorFactory.ArmorSlot, ItemState> Armor { get; set; } = new EventedDictionary<ArmorFactory.ArmorSlot, ItemState>();
        public Dictionary<ushort, int> ItemsPlaced { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<ushort, int> ItemsRemoved { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<ushort, int> ItemsInWorld { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<string, double> Stats { get; set; } = new Dictionary<string, double>();
        public bool MusicEnabled { get; set; } = true;
        public ItemState Weapon { get; set; } = new ItemState();
        public BuildersWand.WandMode BuildersWandMode { get; set; }
        public int BuildersWandCharge { get; set; } = BuildersWand.DURABILITY;
        public int BuildersWandMaxCharge { get; set; }
        public List<IPlayerMagicItem> MagicItems { get; set; } = new List<IPlayerMagicItem>();
        public List<Vector3Int> BuildersWandPreview { get; set; } = new List<Vector3Int>();
        public ushort BuildersWandTarget { get; set; } = BuiltinBlocks.Air;
        public long NextMusicTime { get; set; }
        public bool Connected { get; set; }
        public int MaxMagicItems { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
        private void HealingOverTimePC_NewInstance(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimePC;

            lock (HealingSpells)
            {
                HealingSpells.Add(healing);
            }

            healing.Complete += Healing_Complete;
        }

        private void Healing_Complete(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimePC;

            lock (HealingSpells)
            {
                HealingSpells.Remove(healing);
            }

            healing.Complete -= Healing_Complete;
        }

        private void SetupArmor()
        {
            Weapon = new ItemState();
            Weapon.IdChanged += Weapon_IdChanged;

            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
            {
                var armorState = new ItemState();
                armorState.IdChanged += ArmorState_IdChanged;
                Armor.Add(armorType, armorState);
            }

            Armor.OnDictionaryChanged += Armor_OnDictionaryChanged;
        }

        public void IncrimentStat(string name, double count = 1)
        {
            if (!Stats.ContainsKey(name))
                Stats.Add(name, 0);

            Stats[name] += count;
        }

        private void ArmorState_IdChanged(object sender, ItemStateChangedEventArgs e)
        {
            var state = sender as ItemState;
            RecaclculateMagicItems();

            if (state != null && 
                ArmorFactory.ArmorLookup.TryGetValue(state.Id, out var armor))
            {
                if (armor.HPBoost != 0)
                {
                    var tempVal = Player.GetTempValues(true);
                    tempVal.Set("pipliz.healthmax", tempVal.GetOrDefault<float>("pipliz.healthmax", 100) + armor.HPBoost);
                }
            }


            UpdatePlayerVariables();
        }

        private void Weapon_IdChanged(object sender, ItemStateChangedEventArgs e)
        {
            RecaclculateMagicItems();
        }

        private void RecaclculateMagicItems()
        {
            ResetPlayerVars();

            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(Weapon.Id, out Items.Weapons.IWeapon wep) &&
                            wep is IPlayerMagicItem playerWep)
            {
                AddMagicEffect(playerWep);
            }

            foreach(var arm in Armor)
            {
                if(ArmorFactory.ArmorLookup.TryGetValue(arm.Value.Id, out var armor) &&
                    armor is IPlayerMagicItem playerArmor)
                {
                    AddMagicEffect(playerArmor);
                }
            }

            UpdatePlayerVariables();
        }

        private void ResetPlayerVars()
        {
            _playerVariables = JSON.Deserialize("gamedata/settings/serverperclient.json");
        }

        private void AddMagicEffect(IPlayerMagicItem playerMagicItem)
        {
            _playerVariables.SetAs("MovementSpeed", _playerVariables.GetAs<float>("MovementSpeed") + playerMagicItem.MovementSpeed);
            _playerVariables.SetAs("JumpPower", _playerVariables.GetAs<float>("JumpPower") + playerMagicItem.JumpPower);
            _playerVariables.SetAs("FlySpeed", _playerVariables.GetAs<float>("FlySpeed") + playerMagicItem.FlySpeed);
            _playerVariables.SetAs("MoveSpeed", _playerVariables.GetAs<float>("MoveSpeed") + playerMagicItem.MoveSpeed);
            _playerVariables.SetAs("LightRange", _playerVariables.GetAs<float>("LightRange") + playerMagicItem.MovementSpeed);
            _playerVariables.SetAs("FallDamage", _playerVariables.GetAs<float>("FallDamage") + playerMagicItem.FallDamage);
            _playerVariables.SetAs("FallDamagePerUnit", _playerVariables.GetAs<float>("FallDamagePerUnit") + playerMagicItem.FallDamagePerUnit);
            _playerVariables.SetAs("BuildDistance", _playerVariables.GetAs<float>("BuildDistance") + playerMagicItem.BuildDistance);
        }

        private void Armor_OnDictionaryChanged(object sender, DictionaryChangedEventArgs<ArmorFactory.ArmorSlot, ItemState> e)
        {
            RecaclculateMagicItems();
        }

        private void UpdatePlayerVariables()
        {
            using (ByteBuilder bRaw = ByteBuilder.Get())
            {
                bRaw.Write(ClientMessageType.ReceiveServerPerClientSettings);
                using (ByteBuilder b = ByteBuilder.Get())
                {
                    b.Write(_playerVariables.ToString());
                    bRaw.WriteCompressed(b);
                }
                NetworkWrapper.Send(bRaw.ToArray(), Player.ID);
            }
        }

        public static PlayerState GetPlayerState(Players.Player p)
        {
            if (p != null)
            {
                if (!_playerStates.ContainsKey(p))
                    _playerStates.Add(p, new PlayerState(p));

                return _playerStates[p];
            }

            return null;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Entities.PlayerState.OnUpdate")]
        public static void OnUpdate()
        {
            foreach (var p in Players.PlayerDatabase.Values)
            {
                if (p.IsConnected)
                {
                    try
                    {
                        var ps = GetPlayerState(p);

                        if (ps.Connected && ps.MusicEnabled && Time.MillisecondsSinceStart > ps.NextMusicTime)
                        {
                            ServerManager.SendAudio(GameLoader.NAMESPACE + ".Environment", p);
                            ps.NextMusicTime = 175700 + Time.MillisecondsSinceStart;
                        }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }
                }
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".Entities.PlayerState.OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            _playerStates[p].Connected = false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Entities.PlayerState.OnPlayerConnectedSuperLate")]
        public static void OnPlayerConnectedSuperLate(Players.Player p)
        {
            _playerStates[p].Connected = true;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (!_playerStates.ContainsKey(p))
                _playerStates.Add(p, new PlayerState(p));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".PlayerState", out var stateNode))
            {
                if (stateNode.TryGetAs(nameof(ItemsPlaced), out JSONNode ItemsPlacedNode))
                    foreach (var aNode in ItemsPlacedNode.LoopObject())
                        _playerStates[p].ItemsPlaced.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs(nameof(ItemsRemoved), out JSONNode ItemsRemovedNode))
                    foreach (var aNode in ItemsRemovedNode.LoopObject())
                        _playerStates[p].ItemsRemoved.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs(nameof(ItemsInWorld), out JSONNode ItemsInWorldNode))
                    foreach (var aNode in ItemsInWorldNode.LoopObject())
                        _playerStates[p].ItemsInWorld.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs("Armor", out JSONNode armorNode) && armorNode.NodeType == NodeType.Object)
                    foreach (var aNode in armorNode.LoopObject())
                        _playerStates[p].Armor[(ArmorFactory.ArmorSlot) Enum.Parse(typeof(ArmorFactory.ArmorSlot), aNode.Key)] =
                            new ItemState(aNode.Value);

                if (stateNode.TryGetAs("FlagsPlaced", out JSONNode flagsPlaced) &&
                    flagsPlaced.NodeType == NodeType.Array)
                    foreach (var aNode in flagsPlaced.LoopArray())
                        _playerStates[p].FlagsPlaced.Add(aNode.ToVector3Int());

                if (stateNode.TryGetAs("TeleporterPlaced", out JSONNode teleporterPlaced))
                    _playerStates[p].TeleporterPlaced = teleporterPlaced.ToVector3Int();

                if (stateNode.TryGetAs("Weapon", out JSONNode wepNode))
                    _playerStates[p].Weapon = new ItemState(wepNode);

                if (stateNode.TryGetAs(nameof(BuildersWandMode), out string wandMode))
                    _playerStates[p].BuildersWandMode = (BuildersWand.WandMode) Enum.Parse(typeof(BuildersWand.WandMode), wandMode);

                if (stateNode.TryGetAs(nameof(BuildersWandCharge), out int wandCharge))
                    _playerStates[p].BuildersWandCharge = wandCharge;

                if (stateNode.TryGetAs(nameof(BuildersWandTarget), out ushort wandTarget))
                    _playerStates[p].BuildersWandTarget = wandTarget;

                if (stateNode.TryGetAs(nameof(MusicEnabled), out bool music))
                    _playerStates[p].MusicEnabled = music;

                if (stateNode.TryGetAs(nameof(JoinDate), out string joindate))
                    _playerStates[p].JoinDate = DateTime.Parse(joindate);

                _playerStates[p].BuildersWandPreview.Clear();

                if (stateNode.TryGetAs(nameof(BuildersWandPreview), out JSONNode wandPreview))
                    foreach (var node in wandPreview.LoopArray())
                        _playerStates[p].BuildersWandPreview.Add(node.ToVector3Int());

                if (stateNode.TryGetAs(nameof(MagicItems), out JSONNode magicItems))
                    foreach (var magicItem in magicItems.LoopArray())
                        if (MagicItemsCache.PlayerMagicItems.TryGetValue(magicItem.GetAs<string>(), out var pmi))
                            _playerStates[p].MagicItems.Add(pmi);

                if (stateNode.TryGetAs(nameof(Stats), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        _playerStates[p].Stats[skill.Key] = skill.Value.GetAs<double>();
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_playerStates.ContainsKey(p))
            {
                var node                = new JSONNode();
                var armorNode           = new JSONNode();
                var ItemsPlacedNode     = new JSONNode();
                var ItemsRemovedNode    = new JSONNode();
                var ItemsInWorldNode    = new JSONNode();
                var flagsPlaced         = new JSONNode(NodeType.Array);
                var buildersWandPreview = new JSONNode(NodeType.Array);
                var equiptMagicItems    = new JSONNode(NodeType.Array);

                foreach (var magicItem in _playerStates[p].MagicItems)
                    equiptMagicItems.AddToArray(new JSONNode(magicItem.Name));

                foreach (var kvp in _playerStates[p].ItemsPlaced)
                    ItemsPlacedNode.SetAs(kvp.Key.ToString(), kvp.Value);

                foreach (var kvp in _playerStates[p].ItemsRemoved)
                    ItemsRemovedNode.SetAs(kvp.Key.ToString(), kvp.Value);

                foreach (var kvp in _playerStates[p].ItemsInWorld)
                    ItemsInWorldNode.SetAs(kvp.Key.ToString(), kvp.Value);

                foreach (var armor in _playerStates[p].Armor)
                    armorNode.SetAs(armor.Key.ToString(), armor.Value.ToJsonNode());

                foreach (var flag in _playerStates[p].FlagsPlaced)
                    flagsPlaced.AddToArray(flag.ToJSONNode());

                foreach (var preview in _playerStates[p].BuildersWandPreview)
                    buildersWandPreview.AddToArray(preview.ToJSONNode());

                var statsNode = new JSONNode();

                foreach (var job in _playerStates[p].Stats)
                    statsNode[job.Key] = new JSONNode(job.Value);

                node.SetAs(nameof(Stats), statsNode);

                node.SetAs("Armor", armorNode);
                node.SetAs("Weapon", _playerStates[p].Weapon.ToJsonNode());
                node.SetAs("FlagsPlaced", flagsPlaced);
                node.SetAs("TeleporterPlaced", _playerStates[p].TeleporterPlaced.ToJSONNode());
                node.SetAs(nameof(BuildersWandPreview), buildersWandPreview);
                node.SetAs(nameof(BuildersWandMode), _playerStates[p].BuildersWandMode.ToString());
                node.SetAs(nameof(BuildersWandCharge), _playerStates[p].BuildersWandCharge);
                node.SetAs(nameof(BuildersWandTarget), _playerStates[p].BuildersWandTarget);
                node.SetAs(nameof(ItemsPlaced), ItemsPlacedNode);
                node.SetAs(nameof(ItemsRemoved), ItemsRemovedNode);
                node.SetAs(nameof(ItemsInWorld), ItemsInWorldNode);
                node.SetAs(nameof(MusicEnabled), _playerStates[p].MusicEnabled);
                node.SetAs(nameof(MagicItems), equiptMagicItems);
                node.SetAs(nameof(JoinDate), _playerStates[p].JoinDate);

                n.SetAs(GameLoader.NAMESPACE + ".PlayerState", node);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameLoader.NAMESPACE + "Entities.PlayerState.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            menu.Items.Add(new NetworkUI.Items.DropDown("Music", _Enviorment, new List<string>() { "Disabled", "Enabled" }));
            var ps = PlayerState.GetPlayerState(player);

            if (ps != null)
                menu.LocalStorage.SetAs(_Enviorment, Convert.ToInt32(ps.MusicEnabled));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameLoader.NAMESPACE + "Entities.PlayerState.ChangedSetting")]
        public static void ChangedSetting(TupleStruct<Players.Player, JSONNode, string> data)
        {
            switch (data.item3)
            {
                case "world_settings":
                    var ps = PlayerState.GetPlayerState(data.item1);

                    if (ps != null)
                    {
                        var def = Convert.ToInt32(ps.MusicEnabled);
                        var enabled = data.item2.GetAsOrDefault(_Enviorment, def);

                        if (def != enabled)
                        {
                            ps.MusicEnabled = enabled != 0;
                            PandaChat.Send(data.item1, "Music is now " + (ps.MusicEnabled ? "on" : "off"), ChatColor.green);

                            if (!ps.MusicEnabled)
                                PandaChat.Send(data.item1, "Music can take up to 3 minutes to turn off.", ChatColor.green);
                        }
                    }

                    break;
            }
        }
    }
}