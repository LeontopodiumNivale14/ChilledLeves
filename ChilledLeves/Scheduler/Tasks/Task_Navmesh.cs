using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ChilledLeves.Utilities.LeveData.LeveInfo;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Navmesh
    {
        // The test idea behind this is to have a separate task to queue up navmesh task.
        // Not because running it on the main task is bad, but maybe having a way to handle traveling (So we can know the process of it)
        // seperately wouldn't be WORST idea... this might work, this might not *-shrugs-*

        /// <summary>
        /// Aethernet / Direct travel for the same city. Teleport task is done before this (keeps it clean..)
        /// </summary>
        /// <param name="vendorInfo"></param>
        public static void QueueCityAethernet(LeveInfo.VendorInfo vendorInfo)
        {
            P.navTask.EnqueueMulti
            (
                new(() => Paths_Clear(), "Clearing Navmesh Paths"),
                new(() => City_CalculateDirect(vendorInfo), "Calculating Direct Path"),
                new(() => City_CalculateAethernet(vendorInfo), "Calculating Aethernet Path"),
                new(() => City_BestTravel(vendorInfo), "Figuring Best Travel")
            );
        }

        /// <summary>
        /// Used post city teleport, or in the case we're in a multi-city and need to get to the proper side...
        /// </summary>
        /// <param name="vendorInfo"></param>
        public static void Queue_TeleportAethernet(LeveInfo.VendorInfo vendorInfo)
        {
            P.navTask.EnqueueMulti
                (
                    new(() => Paths_Clear(), "Clearing Navmesh Paths"),
                    new(() => City_ClosestAethernet(), "Calculating Aethernet Path"),
                    new(() => City_BestTravel(vendorInfo), "Figuring Best Travel")
                );
        }



        // Trying to take what worked out of cosmic and make it more refined here. So if need to reference again, check cosmic navmesh task

        // - - - Navmesh Info Storage - - - //
        public enum TravelTypes
        {
            Direct,
            Aethernet,
        }
        public class PathInfo
        {
            public float Distance { get; set; } = 0;
            public List<Vector3> PathTo { get; set; } = null;
            public List<Vector3> PathFrom { get; set; } = null;
            public uint Aethernet_TravelTo { get; set; } = 0;
            public uint Aethernet_TravelFrom { get; set; } = 0;
        }
        public static Dictionary<TravelTypes, PathInfo> TravelMethods = new()
        {
            [TravelTypes.Direct] = new(),
            [TravelTypes.Aethernet] = new(),
        };

        private static Task? _PathCalculations = null;

        private static List<Utils.AethershardInfo> _CalcCandidatesQueue;
        private static List<CandidatePathResult> _CalcCandidateResults;
        private static Task<List<Vector3>> _CalcCurrentTask;
        private static Utils.AethershardInfo _CalcCurrentCandidate;
        private static Task<List<Vector3>> _CalcDestinationTask;
        private static List<Vector3> _CalcDestinationPath;
        private static List<Utils.AethershardInfo> _ClosestCandidatesQueue;
        private static List<CandidatePathResult> _ClosestResults;
        private static Task<List<Vector3>> _ClosestCurrentTask;
        private static Utils.AethershardInfo _ClosestCurrentCandidate;

        private readonly record struct CandidatePathResult(Utils.AethershardInfo Candidate, List<Vector3> PathTo);

        // - - - Navmesh Task Themselves... - - - //

        public static bool TeleportCheck(VendorInfo vendorInfo)
        {
            const string tag = "Navmesh: Teleport";

            if (!Player.Available)
                return false;

            IceLogging.Verbose("- - - Starting Teleport Check - - -", tag);

            var territoryId = Player.Territory.RowId;

            if (vendorInfo.TerritoryId == territoryId)
            {
                IceLogging.Info("Nice and simple. We just need to check to see how to travel to our npc", tag);
                QueueCityAethernet(vendorInfo);
                return true;
            }

            if (InSameCity(territoryId, vendorInfo.TerritoryId))
            {
                IceLogging.Info("We're just in the wrong part of the city, so going to use the aethernet to get to the proper side", tag);
                Queue_TeleportAethernet(vendorInfo);
                return true;
            }

            IceLogging.Debug("We're not in the correct area... at all. So going to queue up a teleport task, then use the travel system post", tag);
            var vendorAetheryte = vendorInfo.Aetheryte;
            IceLogging.Verbose($"Teleporting to: {vendorAetheryte}", tag);

            P.navTask.Enqueue(() => TeleportToArea(vendorAetheryte), "Teleporting to aethernet");
            if (Utils.Aethernet.Any(x => x.Value.TerritoryId == vendorInfo.TerritoryId))
            {
                IceLogging.Info("Teleport has been queue'd up, and it seems like we're traveling to a city. So also queueing up the aethernet system", tag);
                QueueCityAethernet(vendorInfo);
                return true;
            }
            else
            {
                IceLogging.Info($"Teleporting to a vendor outside the city, using normal navmesh travel system. Destination: [{vendorInfo.TerritoryId}]", tag);
                // TODO: Actually put navmesh navigation here... need to check for flying available vs not (and being able to set mount distance and the hoozah)
            }
            return true;
        }

        private static readonly HashSet<uint>[] MultiZoneCities =
        {
            new() { 128, 129 },       // Limsa
            new() { 130, 131 },       // Gridania
            new() { 132, 133 },       // Ul'dah
            new() { 418, 419 },       // Foundation
        };

        private static bool InSameCity(uint territoryId, uint vendorTerritoryId)
        {
            foreach (var city in MultiZoneCities)
            {
                if (city.Contains(territoryId) && city.Contains(vendorTerritoryId))
                    return true;
            }

            return false;
        }
        private static bool Paths_Clear()
        {
            const string tag = "Navmesh: Resetting Dictionaries";

            foreach (var path in TravelMethods)
            {
                path.Value.Distance = 0;
                path.Value.PathTo = null;
                path.Value.PathFrom = null;
                path.Value.Aethernet_TravelTo = 0;
                path.Value.Aethernet_TravelFrom = 0;
            }

            IceLogging.Verbose("Resetting dictionaries has been completed, all are default", tag);
            return true;
        }

        /// <summary>
        /// Used to find the most direct path to the destination. Very Point A -> B
        /// </summary>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        private static bool City_CalculateDirect(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Navmesh: Calculate Direct";
            var playerPosition = Player.Position;
            var destination = vendorInfo.Npc_InteractZone;
            var method = TravelMethods[TravelTypes.Direct];

            if (_PathCalculations == null)
            {
                _PathCalculations = Task.Run(async () =>
                {
                    method.PathTo = await FindPath(playerPosition, destination);
                });
                if (EzThrottler.Throttle("Started Task"))
                    IceLogging.Verbose("Started Direct Path Calculations", tag);

                return false;
            }

            // Now waiting for the task to complete..
            if (!_PathCalculations.IsCompleted)
            {
                if (EzThrottler.Throttle("Still Calculating", 1000))
                    IceLogging.Verbose("We're still waiting on navmesh to calculate our direct path", tag);
                return false;
            }

            // Woo it's done!
            _PathCalculations = null; // Resetting
            if (method.PathTo != null)
            {
                method.Distance = PathDistance(method.PathTo);
            }
            IceLogging.Debug("Direct Pathing Complete", tag);
            return true;
        }

        /// <summary>
        /// Used to find the closest aetheryte to the player, and the one that is closest to the vendor npc <br></br>
        /// Same territroyID, but for things like old sharlayan, crystarium... where taking the aethernet to the vendor would be quicker
        /// </summary>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        private static bool City_CalculateAethernet(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Navmesh: Aethernet Calculation";
            Vector3 destination = vendorInfo.Npc_InteractZone;
            uint destinationShardId = vendorInfo.ClosestShard;

            if (!Utils.Aethernet.TryGetValue(destinationShardId, out var destinationShard))
            {
                IceLogging.Info($"Could not resolve destination shard {destinationShardId}, continuing", tag);
                return true;
            }

            var curTerritoryId = Player.Territory.RowId;
            var validShards = Utils.Aethernet.Values
                .Where(x => x.TerritoryId == curTerritoryId)
                .ToList();

            if (validShards.Count == 0)
            {
                IceLogging.Info($"There was no aethershards in this area [or none recorded] for {curTerritoryId}", tag);
                IceLogging.Info("Continuing onwards", tag);
                return true;
            }

            if (validShards.Count == 1 && validShards[0].ShardId == destinationShard.ShardId)
            {
                IceLogging.Info("Only valid shard, is the same one we're using as our destination. So going to just exit", tag);
                return true;
            }

            var aethernet = TravelMethods[TravelTypes.Aethernet];
            var playerPosition = Player.Position;

            // Phase 1 setup: build candidate queue
            if (_CalcCandidatesQueue == null)
            {
                _CalcCandidatesQueue = new List<Utils.AethershardInfo>(validShards);
                _CalcCandidateResults = new List<CandidatePathResult>();

                if (EzThrottler.Throttle("Started candidate task"))
                    IceLogging.Verbose($"Started calculating paths for {validShards.Count} candidate shards", tag);

                return false;
            }

            // Phase 1: work through candidates one at a time
            if (_CalcCandidateResults.Count < validShards.Count)
            {
                if (_CalcCurrentTask == null)
                {
                    _CalcCurrentCandidate = _CalcCandidatesQueue[0];
                    _CalcCandidatesQueue.RemoveAt(0);
                    _CalcCurrentTask = FindPath(playerPosition, _CalcCurrentCandidate.MoveTo);
                    return false;
                }

                if (!_CalcCurrentTask.IsCompleted)
                {
                    if (EzThrottler.Throttle("Calculating path message", 1000))
                        IceLogging.Verbose("Still calculating candidate shard paths", tag);
                    return false;
                }

                List<Vector3> path = null;
                try
                {
                    path = _CalcCurrentTask.Result;
                }
                catch (Exception ex)
                {
                    IceLogging.Error($"Path calc failed for shard {_CalcCurrentCandidate.ShardId}: {ex}", tag);
                }

                _CalcCandidateResults.Add(new CandidatePathResult(_CalcCurrentCandidate, path));
                _CalcCurrentTask = null;
                return false;
            }

            // Phase 2: destination leg, only starts once the queue is free
            if (_CalcDestinationTask == null)
            {
                _CalcDestinationTask = FindPath(destinationShard.MoveTo, destination);
                return false;
            }

            if (!_CalcDestinationTask.IsCompleted)
            {
                if (EzThrottler.Throttle("Calculating destination path message", 1000))
                    IceLogging.Verbose("Still calculating destination leg", tag);
                return false;
            }

            try
            {
                _CalcDestinationPath = _CalcDestinationTask.Result;
            }
            catch (Exception ex)
            {
                IceLogging.Error($"Destination path calc failed: {ex}", tag);
                _CalcDestinationPath = null;
            }

            // Done - pick the best
            var destinationLegDistance = PathDistance(_CalcDestinationPath);
            var candidateResults = _CalcCandidateResults;
            var destinationPath = _CalcDestinationPath;

            _CalcCandidatesQueue = null;
            _CalcCandidateResults = null;
            _CalcCurrentTask = null;
            _CalcCurrentCandidate = default;
            _CalcDestinationTask = null;
            _CalcDestinationPath = null;

            var best = candidateResults
                .Where(r => r.PathTo != null)
                .Select(r => new
                {
                    r.Candidate,
                    r.PathTo,
                    TotalDistance = PathDistance(r.PathTo) + destinationLegDistance,
                })
                .OrderBy(r => r.TotalDistance)
                .FirstOrDefault();

            if (best == null)
            {
                IceLogging.Info("None of the candidate shards had a valid path, continuing", tag);
                return true;
            }

            aethernet.PathTo = best.PathTo;
            aethernet.PathFrom = destinationPath;
            aethernet.Distance = best.TotalDistance;
            aethernet.Aethernet_TravelTo = best.Candidate.ShardId;
            aethernet.Aethernet_TravelFrom = destinationShard.ShardId;

            IceLogging.Verbose($"Calulations have been completed. Here's the best aetherytes to take: To: {best.Candidate.ShardId} | From: {destinationShard.ShardId}", tag);

            return true;
        }

        /// <summary>
        /// Used to find the closest aethernet crystal there is in the city.<br></br>
        /// Limsa, Uldah, Gridania, and Idyllshire are the ones where this impacts the most.<br></br>
        /// Where we need to use the aethernet to get to the other part of the city but is in a different territory
        /// </summary>
        /// <returns></returns>
        private static bool City_ClosestAethernet()
        {
            const string tag = "Navmesh: Closest Aethernet Calculation";

            var curTerritoryId = Player.Territory.RowId;
            var validShards = Utils.Aethernet.Values
                .Where(x => x.TerritoryId == curTerritoryId)
                .ToList();

            if (validShards.Count == 0)
            {
                IceLogging.Info("No valid aethershards in this territory, continuing", tag);
                return true;
            }

            var aethernet = TravelMethods[TravelTypes.Aethernet];
            var playerPosition = Player.Position;

            // First tick: set up the queue
            if (_ClosestCandidatesQueue == null)
            {
                _ClosestCandidatesQueue = new List<Utils.AethershardInfo>(validShards);
                _ClosestResults = new List<CandidatePathResult>();

                if (EzThrottler.Throttle("Started closest task"))
                    IceLogging.Verbose($"Started calculating paths for {validShards.Count} candidate shards", tag);

                return false; // Keep checking
            }

            // No task running - kick off the next candidate in the queue
            if (_ClosestCurrentTask == null)
            {
                if (_ClosestCandidatesQueue.Count == 0)
                {
                    // All candidates processed - pick the best
                    var best = _ClosestResults
                        .Where(r => r.PathTo != null)
                        .Select(r => new
                        {
                            r.Candidate,
                            r.PathTo,
                            Distance = PathDistance(r.PathTo),
                        })
                        .OrderBy(r => r.Distance)
                        .FirstOrDefault();

                    _ClosestCandidatesQueue = null;
                    _ClosestResults = null;

                    if (best == null)
                    {
                        IceLogging.Info("None of the candidate shards had a valid path, continuing", tag);
                        return true;
                    }

                    aethernet.PathTo = best.PathTo;
                    aethernet.PathFrom = null;
                    aethernet.Distance = best.Distance;
                    aethernet.Aethernet_TravelTo = best.Candidate.ShardId;
                    aethernet.Aethernet_TravelFrom = 0;

                    return true;
                }

                _ClosestCurrentCandidate = _ClosestCandidatesQueue[0];
                _ClosestCandidatesQueue.RemoveAt(0);
                _ClosestCurrentTask = FindPath(playerPosition, _ClosestCurrentCandidate.MoveTo);

                return false; // Keep checking
            }

            // Task running - wait for it
            if (!_ClosestCurrentTask.IsCompleted)
            {
                if (EzThrottler.Throttle("Calculating closest path message", 1000))
                    IceLogging.Verbose("Still calculating candidate shard paths", tag);

                return false; // Still calculating
            }

            // This candidate's task finished - record result and clear for next tick
            List<Vector3> path = new();
            try
            {
                path = _ClosestCurrentTask.Result;
            }
            catch (Exception ex)
            {
                IceLogging.Error($"Path calc failed for shard {_ClosestCurrentCandidate.ShardId}: {ex}", tag);
            }

            _ClosestResults.Add(new CandidatePathResult(_ClosestCurrentCandidate, path));
            _ClosestCurrentTask = null;

            return false; // Keep checking, more candidates (or the final pick) to go
        }
        private static bool City_BestTravel(VendorInfo vendorInfo)
        {
            const string tag = "Navmesh: City Best Travel";

            var bestTravel = TravelMethods.Where(x => x.Value.Distance != 0)
                                          .OrderBy(x => x.Value.Distance)
                                          .FirstOrDefault();

            IceLogging.Verbose("Reporting back all travel methods and the distance for them", tag);
            foreach (var travelKind in TravelMethods)
            {
                IceLogging.Verbose($"[{travelKind.Key}] = {travelKind.Value.Distance}", tag);
            }

            if (bestTravel.Key == TravelTypes.Direct)
            {
                IceLogging.Verbose("Best travel kind was directly to our destination. So we're just going to run there", tag);
                P.navTask.Enqueue(() => Task_GroundTo(vendorInfo.Npc_InteractZone), "Moving to Npc");
            }
            else if (bestTravel.Key is TravelTypes.Aethernet)
            {
                IceLogging.Verbose("Best travel kind was by the aethernet, so we're going to use those to get where we're needed", tag);
                var shardInfo = Utils.Aethernet[bestTravel.Value.Aethernet_TravelTo];
                P.navTask.EnqueueMulti
                (
                    new(() => Task_GroundToAetheryte(shardInfo.MoveTo, shardInfo.Position, false, shardInfo.InteractDistance), "Moving to Aethershard"),
                    new(() => UseAethernet(bestTravel.Value.Aethernet_TravelTo, vendorInfo), "Using the aethernet"),
                    new(() => Task_GroundTo(vendorInfo.Npc_InteractZone), "Moving to npc")
                );
            }

            return true;
        }
        private static bool TeleportToArea(uint aetheryteId)
        {
            const string tag = "Navmesh: Teleporting";

            if (Utils.Aethernet.TryGetValue(aetheryteId, out var aetherInfo))
            {
                var territoryId = aetherInfo.TerritoryId;
                var currentTerritory = Player.Territory.RowId;
                if (Player.Available)
                {
                    if (currentTerritory == territoryId)
                    {
                        IceLogging.Verbose("We are currently in the correct territory, continuing on", tag);
                        return true;
                    }
                    else
                    {
                        bool isBusy = Player.IsBusy;
                        bool inBetweenAreas = Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];

                        if (!isBusy && !inBetweenAreas)
                        {
                            InvokeTeleport(aetheryteId, territoryId, tag);
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Waiting for teleport", 1000))
                                IceLogging.Verbose("Waiting for teleport to finish so we can check the state..", tag);
                        }
                    }
                }
            }
            else
            {
                if (EzThrottler.Throttle("Aethernet Teleport Error", 2000))
                    IceLogging.Error($"No aetheryte was found under ID: [{aetheryteId}]", tag);
            }

            return false;
        }
        private static unsafe bool UseAethernet(uint aetherShard, VendorInfo vendorInfo)
        {
            const string tag = "Navmesh: Using Aethernet";

            var destinationShard = vendorInfo.ClosestShard;

            if (Utils.Aethernet.TryGetValue(destinationShard, out var destinationInfo) && Utils.Aethernet.TryGetValue(aetherShard, out var goalShard))
            {
                var agent = AgentTelepotTown.Instance();
                if (Player.DistanceTo(destinationInfo.Position) < destinationInfo.InteractDistance + 5)
                {
                    IceLogging.Debug("We've reached our destination", tag);
                    return true;
                }
                if (agent != null && agent->Data != null)
                {
                    if (EzThrottler.Throttle("Finding proper aethernet", 1000))
                    {
                        var data = agent->Data;

                        for (byte i = 0; i < data->AetheryteCount; i++)
                        {
                            var entry = data->Entries[i];
                            if (entry.AetheryteId == destinationShard)
                            {
                                IceLogging.Verbose($"We found the destination we were looking for: [{aetherShard}]. Initializing teleport", tag);
                                agent->TeleportToAetheryte(i);
                            }
                        }
                    }
                }
                else if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
                {
                    // This is always going to be the top entry for aethernet travel, so can just hardcode this (can't wait for evercold to bite me in the ass for this)
                    var menu = selectString.Entries[0];
                    if (EzThrottler.Throttle("Selecting entry"))
                    {
                        IceLogging.Verbose("Selecting the aethernet selection from the select string", tag);
                        menu.Select();
                    }

                }
                else
                {
                    var aethernet = Svc.Objects.Where(x => x.BaseId == aetherShard).FirstOrDefault();
                    bool validShard = aethernet != null;
                    bool interactable = validShard && Player.DistanceTo(aethernet.Position) <= goalShard.InteractDistance;

                    if (aethernet != null && Player.DistanceTo(aethernet) <= goalShard.InteractDistance)
                    {
                        if (Player.Mounted || Player.IsJumping)
                        {
                            Utils.Dismount();
                            return false;
                        }
                        else if (!Player.IsBusy)
                        {
                            if (EzThrottler.Throttle("Selecting aetheryte"))
                                IceLogging.Verbose("Selecting the aetheryte for travel", tag);

                            Utils.TargetgameObject(aethernet);
                            Utils.InteractWithObject(aethernet);
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Targeting aethershard"))
                        {
                            IceLogging.Debug($"Waiting... Shard Valid? {validShard} | Interact Distance? {interactable}", tag);
                            if (aethernet != null)
                            {
                                IceLogging.Debug($"{Player.DistanceTo(aethernet)} <= {goalShard.InteractDistance}", tag);
                            }
                        }
                    }
                }
            }
            else
            {
                bool destinationValid = Utils.Aethernet.ContainsKey(destinationShard);
                bool goalValid = Utils.Aethernet.ContainsKey(aetherShard);

                if (EzThrottler.Throttle("Error Aethernet", 1000))
                    IceLogging.Error($"We... seem to be missing aethernet info. Destination [Vendor] = {destinationValid} | Goal Shard [MoveTo] = {goalValid}", tag);
            }

            return false;
        }
        public static float PathDistance(List<Vector3> path)
        {
            if (path == null || path.Count < 2)
                return 0f;

            float distance = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                distance += Vector3.Distance(path[i], path[i + 1]);

            return distance;
        }
        private static unsafe void InvokeTeleport(uint aetheryteId, uint territoryId, string tag)
        {
            if (EzThrottler.Throttle("Attempting to teleport"))
            {
                IceLogging.Verbose($"Initializing the teleport to: {territoryId}", tag);
                Telepo.Instance()->Teleport(aetheryteId, 0);
            }
        }

        private static async Task<List<Vector3>> FindPath(Vector3 position, Vector3 destination)
        {
            return await P.navmesh.Pathfind(position, destination, false);
        }

        // - - - Old code to sort through - - - //

        private static Vector3? _pointOnGround = null;

        public static bool Task_FlyTo(Vector3 pos, bool waitForBusy = true, float distance = 2.0f, bool stayMounted = false)
        {
            bool isFlying = Svc.Condition[ConditionFlag.InFlight];
            bool mounted = Player.Mounted;

            bool JumpIfStuck = true;

            if (!P.navmesh.Installed)
                return true;

            else if (!P.navmesh.IsReady())
            {
                if (EzThrottler.Throttle("Waiting on navmesh", 1000))
                {
                    var navProgress = P.navmesh.BuildProgress();
                }
            }
            else if (P.navmesh.IsRunning())
            {
                if (JumpIfStuck)
                {
                    if (CheckAndHandleStuck())
                    {
                        return false;
                    }
                }

                if (!mounted)
                {
                    // We should never be not mounting here, so going to just stop the current navmesh and restart it
                    if (EzThrottler.Throttle("Emergency Navmesh Stop | Fly"))
                        P.navmesh.PathStop();

                    return false;
                }

                if (Player.IsMoving && waitForBusy)
                {
                    // We were told to wait for us to stop moving, so we're going to do so
                    return false;
                }
                else if (!waitForBusy && Player.DistanceTo(pos) <= distance)
                {
                    if (EzThrottler.Throttle("Telling navmesh to stop"))
                    {
                        P.navmesh.PathStop();
                    }
                }

            }
            else if (!P.navmesh.IsRunning())
            {
                if (Player.DistanceTo(new Vector2(pos.X, pos.Z)) < distance)
                // if (Player.DistanceTo(pos) < distance)
                {
                    // We're close enough to the area, time to check for mounting/flying
                    if (mounted && !stayMounted)
                    {
                        Utils.Dismount();
                        return false;
                    }
                    else if (Player.IsJumping)
                    {
                        return false;
                    }
                    else
                    {
                        ResetInfo();
                        return true;
                    }
                }
                else if (!Player.Mounted)
                {
                    // We have fly set to true, but not mounted to actually fly so, starting with that
                    if (EzThrottler.Throttle("Telling us to mount"))
                        Utils.MountAction();

                    return false;
                }
                else
                {
                    // All other conditions have been met to start navmesh moving
                    // And we're not close enough to the point, so starting to move now
                    if (EzThrottler.Throttle("Commence Navmesh Movement"))
                    {
                        P.navmesh.SetTolerance(0.25f);
                        P.navmesh.PathfindAndMoveTo(pos, true);
                    }
                }
            }

            return false;
        }

        public static bool Task_GroundTo(Vector3 pos, bool waitForBusy = true, float distance = 2.0f, bool stayMounted = false)
        {
            const string tag = "Navmesh: Ground Move";

            bool mounted = Player.Mounted;
            float minMountDistance = 15;
            float dismountDistance = 3;

            if (!P.navmesh.Installed)
            {
                IceLogging.Info("We seem to be missing navmesh... so we're just going to exit here", tag);
                return true;
            }
            else if (P.navmesh.IsRunning())
            {
                if (Player.DistanceTo(pos) <= dismountDistance && !stayMounted)
                {
                    if (EzThrottler.Throttle("Dismounting the mount"))
                    {
                        Utils.Dismount();
                    }
                }

                if (Player.IsMoving && waitForBusy)
                {
                    if (EzThrottler.Throttle("Throttle message tehe"))
                        IceLogging.Verbose("We're currently moving, and we were told to wait for us to NOT be moving so... yeah, we waiting", tag);

                    return false;
                }
                else if (!waitForBusy && Player.DistanceTo(new Vector2(pos.X, pos.Z)) <= distance)
                {
                    if (EzThrottler.Throttle("Telling navmesh to stop"))
                    {
                        IceLogging.Verbose("We're within stopping distance, so stopping navmesh", tag);
                        P.navmesh.PathStop();
                    }
                }
            }
            else if (!P.navmesh.IsReady())
            {
                if (EzThrottler.Throttle("Waiting on navmesh", 1000))
                {
                    var navProgress = P.navmesh.BuildProgress();
                    IceLogging.Debug($"Waiting for navmesh to finish building. Currently at: {navProgress:N2}", tag);
                }
            }
            else if (!P.navmesh.IsRunning())
            {
                if (Player.DistanceTo(new Vector2(pos.X, pos.Z)) < distance)
                {
                    if (mounted && !stayMounted)
                    {
                        if (EzThrottler.Throttle("Dismounting the mount"))
                        {
                            Utils.Dismount();
                        }
                        return false;
                    }
                    else if (Player.IsJumping)
                    {
                        return false;
                    }
                    else
                    {
                        IceLogging.Verbose("We've met the distance threshold, continuing on", tag);
                        ResetInfo();
                        return true;
                    }
                }
                else
                {
                    if (EzThrottler.Throttle("Telling navmesh to start"))
                    {
                        P.navmesh.SetTolerance(0.25f);
                        IceLogging.Verbose("We're setting the tolerance to 0.25f here", tag);
                        P.navmesh.PathfindAndMoveTo(pos, false);
                    }
                }
            }

            return false;
        }
        public static bool Task_GroundToAetheryte(Vector3 pos, Vector3 aetheryte, bool waitForBusy = true, float distance = 2.0f)
        {
            const string tag = "Navmesh: Ground Move";

            if (!P.navmesh.Installed)
            {
                IceLogging.Info("We seem to be missing navmesh... so we're just going to exit here", tag);
                return true;
            }
            else if (P.navmesh.IsRunning())
            {
                if (Player.IsMoving && waitForBusy)
                {
                    if (EzThrottler.Throttle("Throttle message tehe"))
                        IceLogging.Verbose("We're currently moving, and we were told to wait for us to NOT be moving so... yeah, we waiting", tag);

                    return false;
                }
                else if (!waitForBusy && (Player.DistanceTo(aetheryte) <= distance) )
                {
                    if (EzThrottler.Throttle("Telling navmesh to stop"))
                    {
                        IceLogging.Verbose("We're within stopping distance, so stopping navmesh", tag);
                        P.navmesh.PathStop();
                    }
                }
            }
            else if (!P.navmesh.IsReady())
            {
                if (EzThrottler.Throttle("Waiting on navmesh", 1000))
                {
                    var navProgress = P.navmesh.BuildProgress();
                    IceLogging.Debug($"Waiting for navmesh to finish building. Currently at: {navProgress:N2}", tag);
                }
            }
            else if (!P.navmesh.IsRunning())
            {
                if (Player.DistanceTo(aetheryte) <= distance)
                {
                    IceLogging.Verbose("We've met the distance threshold, continuing on", tag);
                    ResetInfo();
                    return true;
                }
                else
                {
                    if (EzThrottler.Throttle("Telling navmesh to start"))
                    {
                        P.navmesh.SetTolerance(0.25f);
                        IceLogging.Verbose("We're setting the tolerance to 0.25f here", tag);
                        P.navmesh.PathfindAndMoveTo(pos, false);
                    }
                }
            }

            return false;
        }

        private static Vector3 _lastPosition = Vector3.Zero;
        private static DateTime _lastPositionChange = DateTime.Now;
        private static int _stuckAttempts = 0;
        private const float STUCK_DISTANCE_THRESHOLD = 1.0f; // Consider stuck if moved less than this
        private const int STUCK_TIME_THRESHOLD = 3000; // Time in ms before considering stuck

        private static unsafe bool CheckAndHandleStuck()
        {
            var currentPos = Player.Position;
            var timeSinceLastChange = (DateTime.Now - _lastPositionChange).TotalMilliseconds;

            // Check if we've moved significantly
            if (Vector3.Distance(currentPos, _lastPosition) > STUCK_DISTANCE_THRESHOLD)
            {
                // We moved, reset tracking
                ResetInfo();
                return false;
            }

            // We haven't moved much, check if we've been stuck long enough
            if (timeSinceLastChange > STUCK_TIME_THRESHOLD)
            {
                _stuckAttempts++;

                if (_stuckAttempts == 1)
                {
                    // First attempt: try jumping
                    if (EzThrottler.Throttle("Stuck - attempting jump", 1000))
                    {
                        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2);
                        _lastPositionChange = DateTime.Now; // Give it time to work
                    }
                    return true;
                }
                else if (_stuckAttempts >= 2)
                {
                    // Second attempt: stop navmesh after jump had time to execute
                    if (EzThrottler.Throttle("Stuck - stopping navmesh", 1000))
                    {
                        P.navmesh.PathStop();
                        _stuckAttempts = 0; // Reset for next time
                        _lastPositionChange = DateTime.Now;
                    }
                    return true;
                }
            }

            return false;
        }

        public static void ResetInfo()
        {
            _lastPosition = Vector3.Zero;
            _lastPositionChange = DateTime.Now;
            _stuckAttempts = 0;
        }
    }
}
