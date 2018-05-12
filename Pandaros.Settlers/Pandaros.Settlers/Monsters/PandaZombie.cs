using NPC;
using Pipliz;
using Server.AI;
using Server.NPCs;

namespace Pandaros.Settlers.Monsters
{
    public class PandaZombie : Zombie
    {
        public PandaZombie(NPCType nPCType, Path path, Players.Player originalGoal) :
            base(nPCType, path, originalGoal)
        {
        }

        public override float MovementSpeed => MovementSpeedPanda;
        public float MovementSpeedPanda { get; set; }

        public override bool Update()
        {
            var result = false;

            if (!isValid)
            {
                OnDiscard();
                return false;
            }

            if (Time.SecondsSinceStartDoubleThisFrame < nextUpdate)
                result = true;
            else
                try
                {
                    if (!decision.IsValid)
                    {
                        decision = default(ZombieDecision);
                        ReconsiderDecision();
                    }

                    if (decision.ShouldReconsider) ReconsiderDecision();

                    if (!decision.IsValid)
                    {
                        OnRagdoll();
                        var result2 = false;
                        return result2;
                    }

                    if (!decision.Do())
                    {
                        OnDiscard();
                        var result2 = false;
                        return result2;
                    }

                    result = true;
                }
                finally
                {
                    UpdateChunkPosition();
                }

            if (Time.SecondsSinceStartDoubleThisFrame > nextSend)
                SendUpdate();

            return result;
        }

        public bool Do()
        {
            switch (decision.GoalType)
            {
                case ZombieGoal.Banner:
                    break;
                case ZombieGoal.NPC:

                    if (Position == decision.GetFieldValue<NPCBase, ZombieDecision>("goalNPC").Position.Vector)
                        return decision.CallAndReturn<bool>("OnReachedGoal", null);

                    break;
                case ZombieGoal.Player:

                    if (Position == decision.GoalPlayer.VoxelPosition.Vector)
                        return decision.CallAndReturn<bool>("OnReachedGoal", null);

                    break;
                default:
                    return false;
            }

            var path   = decision.GetFieldValue<Path, ZombieDecision>("path");
            var pindex = decision.GetFieldValue<int, ZombieDecision>("pathIndex");

            var index = Math.Min(pindex + 1, path.Positions.Count - 1);

            Vector3Int position;

            while (!path.ValidMove(index) && path.Positions.Count - 1 > index)
                index = Math.Min(index + 1, path.Positions.Count - 1);

            if (path.ValidMove(index))
            {
                position = path.Positions[index];
                decision.SetFieldValue<ZombieDecision>("pathIndex", index);
            }
            else
            {
                var vector3Int = path.Goal;

                if (AIManager.ZombiePathFinder.TryFindPath(new Vector3Int(Position), vector3Int, out path,
                                                           2000000000) != EPathFindingResult.Success)
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
            SetCooldown(1f / MovementSpeed);

            return path == null ||
                   decision.GetFieldValue<int, ZombieDecision>("pathIndex") != path.Positions.Count - 1 ||
                   decision.CallAndReturn<bool>("OnReachedGoal", null);
        }

        protected override void ReconsiderDecision()
        {
            switch (decision.GoalType)
            {
                case ZombieGoal.Banner:

                    if ((!decision.IsValid || decision.PathDistance > MinDistanceToReconsiderBanner) &&
                        (ConsiderPlayerTarget(ref decision) || ConsiderNPCTarget(ref decision)))
                        return;

                    if (!BannerTracker.Contains(decision.GoalLocation))
                    {
                        var closest = BannerTracker.GetClosest(originalGoal, position);

                        if (closest != null &&
                            AIManager.ZombiePathFinder.TryFindPath(position, closest.KeyLocation, out var path,
                                                                   2000000000) == EPathFindingResult.Success)
                        {
                            decision.Clear();
                            decision = new ZombieDecision(this, path);
                            return;
                        }

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

                            if (AIManager.ZombiePathFinder.TryFindPath(position, nPCBase.Position, out var path2,
                                                                       decision.PathDistance / 2) ==
                                EPathFindingResult.Success)
                            {
                                decision.Clear();
                                decision = new ZombieDecision(this, nPCBase, path2);
                            }
                        }

                        if (ConsiderBannerTarget(ref decision)) return;
                    }

                    break;

                case ZombieGoal.Player:

                    if (Players.FindClosestAlive(position.Vector, out var player, out var num))
                    {
                        if (decision.IsGoingTo(player) && IsMovingTargetPathOkay(player.VoxelPosition))
                        {
                            decision.Reconsidered();
                            return;
                        }

                        if (num < MaxSqrdRadiusToPlayerToConsider && AIManager.CanStandAt(player.VoxelPosition) &&
                            AIManager.ZombiePathFinder.TryFindPath(position, player.VoxelPosition, out var path3,
                                                                   2000000000) == EPathFindingResult.Success)
                        {
                            decision.Clear();
                            decision = new ZombieDecision(this, player, path3);
                        }
                    }

                    if (ConsiderNPCTarget(ref decision) || ConsiderBannerTarget(ref decision))
                        return;

                    break;


                default:

                    if (ConsiderPlayerTarget(ref decision) || ConsiderNPCTarget(ref decision) ||
                        ConsiderBannerTarget(ref decision))
                        return;

                    break;
            }

            decision.Reconsidered();
        }
    }
}