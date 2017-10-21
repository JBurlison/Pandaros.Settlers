using General.Settings;
using NPC;
using Pipliz;
using Pipliz.Assertions;
using Pipliz.Collections;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pandaros.Settlers.Monsters
{
    public class PandaZombie : IMonster
    {
        private struct ZombieDecision
        {
            private PandaZombie.ZombieGoal goal;

            private float pathIndex;

            private Path path;

            private NPCBase goalNPC;

            private Players.Player goalPlayer;

            private long lastDecisionTime;

            private ushort decisionInterval;

            public Players.Player GoalPlayer
            {
                get
                {
                    return this.goalPlayer;
                }
            }

            public Vector3Int GoalLocation
            {
                get
                {
                    return this.path.Goal;
                }
            }

            public int PathDistance
            {
                get
                {
                    return (!this.IsValid) ? 2000000000 : (this.path.Count - (int)this.pathIndex);
                }
            }

            public bool IsAtBlockCenter
            {
                get
                {
                    float num = MonsterTracker.deltaUpdate * PandaZombie.variables.WalkingSpeed;
                    return (int)this.pathIndex != (int)(this.pathIndex + num);
                }
            }

            public bool ShouldReconsider
            {
                get
                {
                    bool flag = this.lastDecisionTime != 0L && Pipliz.Time.MillisecondsSinceStart - this.lastDecisionTime > (long)this.decisionInterval;
                    bool result;
                    if (this.goal == PandaZombie.ZombieGoal.NPC)
                    {
                        if (this.IsAtBlockCenter)
                        {
                            result = (flag || !this.goalNPC.IsValid || this.path.Goal != new Vector3Int(this.goalNPC.Position));
                            return result;
                        }
                    }
                    else if (this.goal == PandaZombie.ZombieGoal.Player)
                    {
                        if (this.IsAtBlockCenter)
                        {
                            result = (flag || !this.goalPlayer.ShouldSendData || this.goalPlayer.Health <= 0f || this.path.Goal != this.goalPlayer.VoxelPosition);
                            return result;
                        }
                    }
                    result = flag;
                    return result;
                }
            }

            public PandaZombie.ZombieGoal GoalType
            {
                get
                {
                    return this.goal;
                }
            }

            public bool IsValid
            {
                get
                {
                    bool result;
                    switch (this.goal)
                    {
                        case PandaZombie.ZombieGoal.Banner:
                            result = (this.path != null);
                            break;
                        case PandaZombie.ZombieGoal.NPC:
                            result = (this.path != null && this.goalNPC != null && this.goalNPC.IsValid);
                            break;
                        case PandaZombie.ZombieGoal.Player:
                            result = (this.path != null && this.goalPlayer != null && this.goalPlayer.ShouldSendData);
                            break;
                        default:
                            result = false;
                            break;
                    }
                    return result;
                }
            }

            public ZombieDecision(Path path)
            {
                this.path = path;
                this.goal = PandaZombie.ZombieGoal.Banner;
                this.goalNPC = null;
                this.goalPlayer = null;
                this.pathIndex = 0f;
                this.lastDecisionTime = Pipliz.Time.MillisecondsSinceStart;
                this.decisionInterval = (ushort)Pipliz.Random.Next(PandaZombie.variables.ReconsiderDecisionTimeMinimum, PandaZombie.variables.ReconsiderDecisionTimeMaximum);
            }

            public ZombieDecision(NPCBase goalNPC, Path path)
            {
                this.path = path;
                this.goal = PandaZombie.ZombieGoal.NPC;
                this.goalNPC = goalNPC;
                this.goalPlayer = null;
                this.pathIndex = 0f;
                this.lastDecisionTime = Pipliz.Time.MillisecondsSinceStart;
                this.decisionInterval = (ushort)Pipliz.Random.Next(PandaZombie.variables.ReconsiderDecisionTimeMinimum, PandaZombie.variables.ReconsiderDecisionTimeMaximum);
            }

            public ZombieDecision(Players.Player goalPlayer, Path path)
            {
                this.path = path;
                this.goal = ZombieGoal.Player;
                this.goalNPC = null;
                this.goalPlayer = goalPlayer;
                this.pathIndex = 0f;
                this.lastDecisionTime = Pipliz.Time.MillisecondsSinceStart;
                this.decisionInterval = (ushort)Pipliz.Random.Next(PandaZombie.variables.ReconsiderDecisionTimeMinimum, PandaZombie.variables.ReconsiderDecisionTimeMaximum);
            }

            public void Clear()
            {
                Path.Pool.Return(this.path);
                this.path = null;
            }

            public void Reconsidered()
            {
                this.lastDecisionTime = Pipliz.Time.MillisecondsSinceStart;
            }

            public bool IsGoingTo(NPCBase npc)
            {
                return this.goalNPC == npc;
            }

            public bool IsGoingTo(Players.Player player)
            {
                return this.goalPlayer == player;
            }

            public bool Do(PandaZombie zombie)
            {
                bool result;
                switch (this.goal)
                {
                    case PandaZombie.ZombieGoal.Banner:
                        break;
                    case PandaZombie.ZombieGoal.NPC:
                        if ((double)(zombie.Position - this.goalNPC.Position).sqrMagnitude < 0.3)
                        {
                            result = this.OnReachedGoal(zombie);
                            return result;
                        }
                        break;
                    case PandaZombie.ZombieGoal.Player:
                        if ((double)(zombie.Position - goalPlayer.Position).sqrMagnitude < 0.3)
                        {
                            result = this.OnReachedGoal(zombie);
                            return result;
                        }
                        break;
                    default:
                        result = false;
                        return result;
                }
                float num = MonsterTracker.deltaUpdate * PandaZombie.variables.WalkingSpeed;
                if (this.pathIndex + num >= (float)(this.path.Count - 1))
                {
                    result = this.OnReachedGoal(zombie);
                }
                else
                {
                    if ((int)(this.pathIndex + 0.5f) != (int)(this.pathIndex + num + 0.5f))
                    {
                        if (!this.path.ValidMove((int)(this.pathIndex + num + 0.5f)))
                        {
                            Vector3Int vector3Int = this.path.Goal;
                            Profiler.BeginSample("Recalculating route");
                            this.Clear();
                            if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(zombie.Position), vector3Int, out this.path) != EPathFindingResult.Success)
                            {
                                Profiler.EndSample();
                                this.path = null;
                                zombie.OnRagdoll();
                                result = false;
                                return result;
                            }
                            this.pathIndex = 0f;
                            Profiler.EndSample();
                        }
                    }
                    Vector3 position = zombie.Position;
                    float num2 = this.pathIndex;
                    if (!this.path.MovePath(ref position, ref this.pathIndex, true, num))
                    {
                        zombie.OnRagdoll();
                        result = false;
                    }
                    else
                    {
                        Profiler.BeginSample("SetPosition");
                        zombie.SetPosition(position);
                        Profiler.EndSample();
                        if ((int)(num2 + 0.95f) != (int)(num2 + num + 0.95f))
                        {
                            Profiler.BeginSample("SendUpdate");
                            zombie.SendUpdate(this.path.GetAt((int)(num2 + num + 0.95f)));
                            Profiler.EndSample();
                        }
                        result = true;
                    }
                }
                return result;
            }

            private bool OnReachedGoal(PandaZombie zombie)
            {
                long millisecondsSinceStart = Pipliz.Time.MillisecondsSinceStart;
                PandaZombie.ZombieGoal zombieGoal = this.goal;
                bool result;
                if (zombieGoal != PandaZombie.ZombieGoal.NPC)
                {
                    if (zombieGoal != PandaZombie.ZombieGoal.Player)
                    {
                        if (zombieGoal != PandaZombie.ZombieGoal.Banner)
                        {
                            result = false;
                        }
                        else
                        {
                            Banner banner = BannerTracker.Get(zombie.OriginalGoal);
                            if (banner == null || banner.KeyLocation != this.GoalLocation)
                            {
                                result = false;
                            }
                            else if (millisecondsSinceStart - zombie.lastPunch > (long)PandaZombie.variables.PunchCooldownMS)
                            {
                                zombie.SendUpdate(zombie.Position);
                                zombie.lastPunch = millisecondsSinceStart;
                                Colony colony = Colony.Get(zombie.OriginalGoal);
                                if (colony.FollowerCount > 0)
                                {
                                    bool flag = Colony.Get(zombie.OriginalGoal).TakeMonsterHit(millisecondsSinceStart, PandaZombie.variables.PunchCooldownMS * 25);
                                    ServerManager.SendAudio(zombie.Position, "punch");
                                    if (flag)
                                    {
                                        if (zombie.killedBefore)
                                        {
                                            zombie.OnRagdoll();
                                            result = false;
                                            return result;
                                        }
                                        zombie.killedBefore = true;
                                    }
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            else
                            {
                                result = true;
                            }
                        }
                    }
                    else
                    {
                        Vector3 a = this.goalPlayer.Position - zombie.Position;
                        if (a.sqrMagnitude < 1f)
                        {
                            if (millisecondsSinceStart - zombie.lastPunch > (long)PandaZombie.variables.PunchCooldownMS)
                            {
                                zombie.SendUpdate(zombie.Position);
                                zombie.lastPunch = millisecondsSinceStart;
                                Players.TakeHit(this.goalPlayer, PandaZombie.variables.PlayerHitDamage, true);
                                ServerManager.SendAudio(zombie.Position, "punch");
                                a = a.normalized;
                                a.y += PandaZombie.variables.PunchHeight;
                                ServerManager.SendForce(this.goalPlayer.ID, a * PandaZombie.variables.PunchForce);
                            }
                        }
                        else
                        {
                            this.path = null;
                            this.pathIndex = 0f;
                        }
                        result = true;
                    }
                }
                else
                {
                    if ((this.goalNPC.Position - zombie.Position).sqrMagnitude < 1f)
                    {
                        if (millisecondsSinceStart - zombie.lastPunch > (long)PandaZombie.variables.PunchCooldownMS)
                        {
                            zombie.SendUpdate(zombie.Position);
                            zombie.lastPunch = millisecondsSinceStart;
                            //Chat.Send(this.goalNPC.Colony.Owner, string.Format("One of your colonists with job {0} has died.", this.goalNPC.NPCType), ChatSenderType.Server);
                            ServerManager.SendAudio(zombie.Position, "punch");
                            ServerManager.SendAudio(this.goalNPC.Position, "grunt");
                            this.goalNPC.OnDeath();
                            this.goalNPC = null;
                            if (zombie.killedBefore)
                            {
                                zombie.OnRagdoll();
                                result = false;
                                return result;
                            }
                            zombie.killedBefore = true;
                        }
                    }
                    else
                    {
                        this.path = null;
                        this.pathIndex = 0f;
                    }
                    result = true;
                }
                return result;
            }
        }

        private enum ZombieGoal
        {
            Invalid,
            Banner,
            NPC,
            Player
        }

        [AutoVariable]
        public class Variables : VariablesHelper
        {
            public int TargetingRange;

            public int ReconsiderDecisionTimeMinimum;

            public int ReconsiderDecisionTimeMaximum;

            public float WalkingSpeed;

            public float PlayerTargetingRangeSqr;

            public float PlayerHitDamage;

            public int PunchCooldownMS;

            public float PunchForce;

            public float PunchHeight;

            public Variables() : base("gamedata/settings/server.json", "Zombies")
            {
            }
        }

        private int id;

        private NPCBody body;

        private NPCType type;

        private Vector3Int lastChunkPosition;

        private PandaZombie.ZombieDecision decision;

        private Players.Player originalGoal;

        private float health = 100f;

        private bool killedBefore = false;

        private long lastPunch;

        private BoxedDictionary tempValues;

        private static PandaZombie.Variables variables = new PandaZombie.Variables();

        public Vector3 Position
        {
            get;
            private set;
        }

        public Vector3 PositionToAimFor
        {
            get
            {
                return this.Position + Vector3.up;
            }
        }

        public Vector3 Direction
        {
            get;
            private set;
        }

        public bool IsValid
        {
            get
            {
                return this.body != null;
            }
        }

        public Players.Player OriginalGoal
        {
            get
            {
                return this.originalGoal;
            }
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }

        public PandaZombie(Path path, Players.Player originalGoal)
        {
            this.originalGoal = originalGoal;
            this.decision = new PandaZombie.ZombieDecision(path);
            this.id = ZombieID.GetNext();
            this.Position = path.Start.Vector;
            this.body = new NPCBody(this.Position);
            this.body.SetIdentifier(NPCBodyIdentifier.BodyType.Zombie, this.id);
            this.lastChunkPosition = path.Start.ToChunk();
            this.type = NPCType.GetByKeyNameOrDefault("pipliz.monster");
        }

        public BoxedDictionary GetTempValues(bool allocateIfNull = false)
        {
            if (allocateIfNull && this.tempValues.BaseDictionary == null)
            {
                this.tempValues.BaseDictionary = new Dictionary<string, object>(3);
            }
            return this.tempValues;
        }

        public void SetTempValues(BoxedDictionary dict)
        {
            this.tempValues = dict;
        }

        public void InternalSetID(int ID)
        {
            this.id = ID;
        }

        public void OnHit(float damage)
        {
            ModLoader.TriggerCallbacks<IMonster, float>(ModLoader.EModCallbackType.OnMonsterHit, this, damage);
            this.health -= damage;
            if (this.health <= 0f)
            {
                this.OnRagdoll();
            }
        }

        public void OnRagdoll()
        {
            if (this.IsValid)
            {
                ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterDied, this);
                using (ByteBuilder byteBuilder = ByteBuilder.Get())
                {
                    byteBuilder.Write(13);
                    byteBuilder.Write(this.id);
                    Players.SendToNearby(this.lastChunkPosition, byteBuilder.ToArray(), 156, NetworkMessageReliability.ReliableWithBuffering);
                }
                if (this.body != null)
                {
                    this.body.OnDestroy();
                    this.body = null;
                }
                this.decision = default(PandaZombie.ZombieDecision);
            }
        }

        public bool Update()
        {
            bool result;
            if (!this.IsValid)
            {
                this.OnDiscard();
                result = false;
            }
            else
            {
                try
                {
                    if (!this.decision.IsValid)
                    {
                        this.decision = default(PandaZombie.ZombieDecision);
                        this.ReconsiderDecision();
                    }
                    if (this.decision.ShouldReconsider)
                    {
                        this.ReconsiderDecision();
                    }
                    if (this.decision.IsValid)
                    {
                        if (this.decision.Do(this))
                        {
                            result = true;
                        }
                        else
                        {
                            this.OnDiscard();
                            result = false;
                        }
                    }
                    else
                    {
                        this.OnRagdoll();
                        result = false;
                    }
                }
                finally
                {
                    this.UpdateChunkPosition();
                }
            }
            return result;
        }

        public void SendUpdate(Vector3Int position)
        {
            this.SendUpdate(position.Vector);
        }

        public void SendUpdate(Vector3 pos)
        {
            Vector3 direction = this.Direction;
            direction.y = 0f;
            Quaternion quaternion;
            if (direction != Vector3.zero)
            {
                quaternion = Quaternion.LookRotation(direction, Vector3.up);
            }
            else
            {
                quaternion = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
            byte b = (byte)((quaternion.x + 1f) * 127f);
            byte b2 = (byte)((quaternion.y + 1f) * 127f);
            byte b3 = (byte)((quaternion.z + 1f) * 127f);
            byte b4 = (byte)((quaternion.w + 1f) * 127f);
            using (ByteBuilder byteBuilder = ByteBuilder.Get())
            {
                byteBuilder.Write(10);
                byteBuilder.Write(this.type.Type);
                byteBuilder.Write(this.id);
                byteBuilder.Write(pos);
                byteBuilder.Write(b);
                byteBuilder.Write(b2);
                byteBuilder.Write(b3);
                byteBuilder.Write(b4);
                Players.SendToNearby(new Vector3Int(pos), byteBuilder.ToArray(), 106, NetworkMessageReliability.Unreliable);
            }
        }

        public void OnDiscard()
        {
            if (this.IsValid)
            {
                using (ByteBuilder byteBuilder = ByteBuilder.Get())
                {
                    byteBuilder.Write(11);
                    byteBuilder.Write(this.id);
                    if (this.body != null)
                    {
                        this.body.OnDestroy();
                        this.body = null;
                    }
                    Players.SendToNearby(this.lastChunkPosition, byteBuilder.ToArray(), 156, NetworkMessageReliability.ReliableWithBuffering);
                }
                this.decision = default(PandaZombie.ZombieDecision);
            }
        }

        private void SetPosition(Vector3 newPosition)
        {
            Vector3 position = this.Position;
            this.Position = newPosition;
            this.Direction = this.Position - position;
            this.body.SetPositionAndDirection(newPosition, this.Direction);
        }

        private void UpdateChunkPosition()
        {
            Vector3Int vector3Int = new Vector3Int(this.Position);
            Vector3Int vector3Int2 = vector3Int.ToChunk();
            if (vector3Int2 != this.lastChunkPosition)
            {
                MonsterTracker.UpdateMonsterChunk(this.lastChunkPosition, vector3Int2, this.ID);
                this.lastChunkPosition = vector3Int2;
                if (World.GetChunk(this.lastChunkPosition) == null)
                {
                    this.OnDiscard();
                }
            }
        }

        private void ReconsiderDecision()
        {
            switch (this.decision.GoalType)
            {
                case PandaZombie.ZombieGoal.Banner:
                    if (!this.ConsiderPlayerTarget(ref this.decision))
                    {
                        if (!this.ConsiderNPCTarget(ref this.decision))
                        {
                            Banner banner = BannerTracker.Get(this.originalGoal);
                            if (banner != null)
                            {
                                if (banner.KeyLocation == this.decision.GoalLocation)
                                {
                                    this.decision.Reconsidered();
                                    break;
                                }
                                Path path;
                                if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), banner.KeyLocation, out path) == EPathFindingResult.Success && path.Count < this.decision.PathDistance)
                                {
                                    this.decision.Clear();
                                    this.decision = new PandaZombie.ZombieDecision(path);
                                    break;
                                }
                            }
                            else if (Pipliz.Random.Next(0, 10) == 9)
                            {
                                this.OnRagdoll();
                                break;
                            }
                            if (this.decision.IsValid)
                            {
                                this.decision.Reconsidered();
                            }
                            else
                            {
                                this.OnRagdoll();
                                Log.Write<int, Vector3>("No goal for zombie {0} at {1}", this.id, this.Position);
                            }
                        }
                    }
                    break;
                case PandaZombie.ZombieGoal.NPC:
                    if (!this.ConsiderPlayerTarget(ref this.decision))
                    {
                        NPCBase nPCBase;
                        if (NPCTracker.TryGetNear(this.Position, PandaZombie.variables.TargetingRange, out nPCBase))
                        {
                            if (this.decision.IsGoingTo(nPCBase) && this.IsMovingTargetPathOkay(new Vector3Int(nPCBase.Position)))
                            {
                                this.decision.Reconsidered();
                                break;
                            }
                            Path path2;
                            if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), new Vector3Int(nPCBase.Position), out path2) == EPathFindingResult.Success && path2.Count < this.decision.PathDistance)
                            {
                                this.decision.Clear();
                                this.decision = new PandaZombie.ZombieDecision(nPCBase, path2);
                                break;
                            }
                        }
                        if (this.decision.IsValid)
                        {
                            this.decision.Reconsidered();
                        }
                        else
                        {
                            this.OnRagdoll();
                            Log.Write<int, Vector3>("No goal for zombie {0} at {1}", this.id, this.Position);
                        }
                    }
                    break;
                case PandaZombie.ZombieGoal.Player:
                    if (this.decision.GoalPlayer.IsConnected && this.IsMovingTargetPathOkay(this.decision.GoalPlayer.VoxelPosition))
                    {
                        this.decision.Reconsidered();
                    }
                    else
                    {
                        Players.Player player;
                        double num;
                        if (Players.FindClosestAlive(this.Position, out player, out num))
                        {
                            if (this.decision.IsGoingTo(player) && this.IsMovingTargetPathOkay(player.VoxelPosition))
                            {
                                this.decision.Reconsidered();
                                break;
                            }
                            if (num < (double)PandaZombie.variables.PlayerTargetingRangeSqr)
                            {
                                if (AIManager.CanStandAt(player.VoxelPosition))
                                {
                                    Path path3;
                                    if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), player.VoxelPosition, out path3) == EPathFindingResult.Success && path3.Count < this.decision.PathDistance)
                                    {
                                        this.decision.Clear();
                                        this.decision = new PandaZombie.ZombieDecision(player, path3);
                                        break;
                                    }
                                }
                            }
                        }
                        if (!this.ConsiderNPCTarget(ref this.decision))
                        {
                            if (!this.ConsiderBannerTarget(ref this.decision))
                            {
                                if (!this.decision.IsValid)
                                {
                                    this.OnRagdoll();
                                    Log.Write<int, Vector3>("No goal for zombie {0} at {1}", this.id, this.Position);
                                }
                            }
                        }
                    }
                    break;
                default:
                    if (!this.ConsiderPlayerTarget(ref this.decision))
                    {
                        if (!this.ConsiderNPCTarget(ref this.decision))
                        {
                            if (!this.ConsiderBannerTarget(ref this.decision))
                            {
                                if (this.decision.IsValid)
                                {
                                    this.decision.Reconsidered();
                                }
                                else
                                {
                                    this.OnRagdoll();
                                    Log.Write<int, Vector3>("No goal for zombie {0} at {1}", this.id, this.Position);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private bool IsMovingTargetPathOkay(Vector3Int newTarget)
        {
            bool result;
            if (this.decision.GoalLocation == newTarget)
            {
                result = true;
            }
            else
            {
                int pathDistance = this.decision.PathDistance;
                result = (pathDistance < 20 || (newTarget - this.decision.GoalLocation).SqrMagnitudeFloat < 0.2f * (float)pathDistance);
            }
            return result;
        }

        private bool ConsiderPlayerTarget(ref PandaZombie.ZombieDecision decision)
        {
            Players.Player player;
            double num;
            bool result;
            if (Players.FindClosestAlive(this.Position, out player, out num))
            {
                if (num < (double)PandaZombie.variables.PlayerTargetingRangeSqr)
                {
                    if (AIManager.CanStandAt(player.VoxelPosition))
                    {
                        Path path;
                        if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), player.VoxelPosition, out path) == EPathFindingResult.Success && path.Count < decision.PathDistance)
                        {
                            decision.Clear();
                            decision = new PandaZombie.ZombieDecision(player, path);
                            result = true;
                            return result;
                        }
                    }
                }
            }
            result = false;
            return result;
        }

        private bool ConsiderNPCTarget(ref PandaZombie.ZombieDecision decision)
        {
            NPCBase nPCBase;
            bool result;
            if (NPCTracker.TryGetNear(this.Position, PandaZombie.variables.TargetingRange, out nPCBase))
            {
                Path path;
                if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), new Vector3Int(nPCBase.Position), out path) == EPathFindingResult.Success && path.Count < decision.PathDistance)
                {
                    decision.Clear();
                    decision = new PandaZombie.ZombieDecision(nPCBase, path);
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }

        private bool ConsiderBannerTarget(ref PandaZombie.ZombieDecision decision)
        {
            Banner banner = BannerTracker.Get(this.originalGoal);
            bool result;
            Path path;
            if (banner == null)
            {
                result = false;
            }
            else if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(this.Position), banner.KeyLocation, out path) == EPathFindingResult.Success && path.Count < decision.PathDistance)
            {
                decision.Clear();
                decision = new PandaZombie.ZombieDecision(path);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
