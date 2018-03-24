using NPC;
using Pipliz;
using Server.AI;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters
{
    public class PandaZombie : Zombie
    {
        public override float MovementSpeed => MovementSpeedPanda;
        public float MovementSpeedPanda { get; set; }

        public PandaZombie(NPCType nPCType, Path path, Players.Player originalGoal) :
            base(nPCType, path, originalGoal)
        {

        }

        public override bool Update()
        {
            bool result = false;

            if (!isValid)
            {
                OnDiscard();
                return false;
            }

            if (Pipliz.Time.SecondsSinceStartDouble < nextUpdate)
            {
                result = true;
            }
            else
            {
                try
                {
                    if (!decision.IsValid)
                    {
                        decision = default(ZombieDecision);
                        ReconsiderDecision();
                    }

                    if (decision.ShouldReconsider)
                    {
                        ReconsiderDecision();
                    }

                    if (!decision.IsValid)
                    {
                        OnRagdoll();
                        return false;
                    }

                    if (!Do())
                    {
                        OnDiscard();
                        return false;
                    }

                    result = true;
                }
                finally
                {
                    UpdateChunkPosition();
                }
            }

            if (Pipliz.Time.SecondsSinceStartDouble - lastSend > MaxTimeBetweenUpdateSends)
                SendUpdate();

            return result;
        }

        public bool Do()
        {
            switch (decision.GoalType)
            {
                case Zombie.ZombieGoal.Banner:
                    break;
                case Zombie.ZombieGoal.NPC:
                    if (Position == decision.GetFieldValue<NPCBase, ZombieDecision>("goalNPC").Position.Vector)
                    {
                        return decision.CallAndReturn<bool>("OnReachedGoal", null);
                    }
                    break;
                case Zombie.ZombieGoal.Player:
                    if (Position == decision.GoalPlayer.VoxelPosition.Vector)
                    {
                        return decision.CallAndReturn<bool>("OnReachedGoal", null);
                    }
                    break;
                default:
                    return false;
            }
            var path = decision.GetFieldValue<Path, ZombieDecision>("path");
            var pindex = decision.GetFieldValue<int, ZombieDecision>("pathIndex");

            int index = Pipliz.Math.Min(pindex + 1, path.Positions.Count - 1);

            Vector3Int position;

            while (!path.ValidMove(index) && path.Positions.Count - 1 > index)
                index = Pipliz.Math.Min(index + 1, path.Positions.Count - 1);

            if (path.ValidMove(index))
            {
                position = path.Positions[index];
                decision.SetFieldValue<ZombieDecision>("pathIndex", index);
            }
            else
            {
                Vector3Int vector3Int = path.Goal;

                if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(Position), vector3Int, out path, 2000000000) != EPathFindingResult.Success)
                {
                    decision.SetFieldValue<ZombieDecision>("path", null);
                    OnRagdoll();
                    return false;
                }

                decision.SetFieldValue<ZombieDecision>("pathIndex", 0);
                position = path.Positions[0];
            }

            SetPosition(position);
            SendUpdate();
            SetCooldown((double)(1f / MovementSpeed));

            return path == null || decision.GetFieldValue<int, ZombieDecision>("pathIndex") != path.Positions.Count - 1 || decision.CallAndReturn<bool>("OnReachedGoal", null);
        }

        protected override void ReconsiderDecision()
        {
            switch (decision.GoalType)
            {
                case ZombieGoal.Banner:

                    if ((!decision.IsValid || decision.PathDistance > MinDistanceToReconsiderBanner) && (ConsiderPlayerTarget(ref decision) || ConsiderNPCTarget(ref decision)))
                        return;

                    if (!BannerTracker.Contains(decision.GoalLocation))
                    {
                        Banner closest = BannerTracker.GetClosest(originalGoal, position);

                        if (closest != null && AIManager.ZombiePathFinder.TryFindPath(position, closest.KeyLocation, out Path path, 2000000000) == EPathFindingResult.Success)
                        {
                            decision.Clear();
                            decision = new ZombieDecision(this, path);
                            return;
                        }
                        else
                            SetCooldown(3.0);
                    }

                    break;

                case ZombieGoal.NPC:
                    if (!decision.IsValid || decision.PathDistance > MinDistanceToReconsiderNPC)
                    {
                        if (ConsiderPlayerTarget(ref decision))
                            return;

                        NPCBase nPCBase;
                        if (NPCTracker.TryGetNear(position.Vector, MaxRadiusToNPCToConsider, out nPCBase))
                        {
                            if (decision.IsGoingTo(nPCBase) && IsMovingTargetPathOkay(nPCBase.Position))
                            {
                                decision.Reconsidered();
                                return;
                            }

                            if (AIManager.ZombiePathFinder.TryFindPath(position, nPCBase.Position, out Path path2, decision.PathDistance / 2) == EPathFindingResult.Success)
                            {
                                decision.Clear();
                                decision = new ZombieDecision(this, nPCBase, path2);
                            }
                        }
                        if (ConsiderBannerTarget(ref decision))
                        {
                            return;
                        }
                    }

                    break;

                case ZombieGoal.Player:

                    if (Players.FindClosestAlive(position.Vector, out Players.Player player, out double num))
                    {
                        if (decision.IsGoingTo(player) && IsMovingTargetPathOkay(player.VoxelPosition))
                        {
                            decision.Reconsidered();
                            return;
                        }

                        if (num < (double)MaxSqrdRadiusToPlayerToConsider && AIManager.CanStandAt(player.VoxelPosition) && AIManager.ZombiePathFinder.TryFindPath(position, player.VoxelPosition, out Path path3, 2000000000) == EPathFindingResult.Success)
                        {
                            decision.Clear();
                            decision = new ZombieDecision(this, player, path3);
                        }
                    }

                    if (ConsiderNPCTarget(ref decision) || ConsiderBannerTarget(ref decision))
                        return;

                    break;


                default:

                    if (ConsiderPlayerTarget(ref decision) || ConsiderNPCTarget(ref decision) || ConsiderBannerTarget(ref decision))
                        return;

                    break;
            }

            decision.Reconsidered();
        }
    }
}
