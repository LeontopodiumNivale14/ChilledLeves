using ChilledLeves.Utilities;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Navmesh
    {
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
            bool mounted = Player.Mounted;
            float minMountDistance = 15;
            float dismountDistance = 3;

            if (!P.navmesh.Installed)
            {
                PluginLog.Information("We seem to be missing navmesh... so we're just going to exit here");
                return true;
            }
            else if (P.navmesh.IsRunning())
            {
                // TODO: FIX THIS
                if (true)
                {
                    if (CheckAndHandleStuck())
                    {
                        return false; // Let the stuck handler take control
                    }
                }

                if (!mounted && Player.DistanceTo(pos) > minMountDistance)
                {
                    if (true)
                    {
                        if (EzThrottler.Throttle("Using mount"))
                            Utils.MountAction();
                    }
                }

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
                        PluginLog.Verbose("We're currently moving, and we were told to wait for us to NOT be moving so... yeah, we waiting");

                    return false;
                }
                else if (!waitForBusy && Player.DistanceTo(new Vector2(pos.X, pos.Z)) <= distance)
                {
                    if (EzThrottler.Throttle("Telling navmesh to stop"))
                    {
                        PluginLog.Debug("We're within stopping distance, so stopping navmesh");
                        P.navmesh.PathStop();
                    }
                }
            }
            else if (!P.navmesh.IsReady())
            {
                if (EzThrottler.Throttle("Waiting on navmesh", 1000))
                {
                    var navProgress = P.navmesh.BuildProgress();
                    PluginLog.Debug($"Waiting for navmesh to finish building. Currently at: {navProgress:N2}");
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
                        PluginLog.Debug("We've met the distance threshold, continuing on");
                        ResetInfo();
                        return true;
                    }
                }
                else
                {
                    if (EzThrottler.Throttle("Telling navmesh to start"))
                    {
                        P.navmesh.SetTolerance(0.25f);
                        PluginLog.Debug("We're setting the tolerance to 0.25f here");
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
