using ChilledLeves.Enums;
using ChilledLeves.Resources;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Travel
    {
        public static bool AethernetTask_Grab(LeveInfo.Info_Vendor vendorInfo)
        {
            const string tag = "Travel: Navmesh Check";

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                if (Player.DistanceTo(vendorInfo.Npc_InteractZone) < 2)
                {
                    IceLogging.Verbose("We're close enough to the interact zone we don't need to queue up anything. Continuing to grab leve", tag);
                    Leve_Helper.State = Leve_Helper.SelectedMode switch
                    {
                        ModeSelection.Standard => LeveState.Grab_StandardLeve,
                        ModeSelection.ARR_Grind => LeveState.Grab_ARRLeve,
                        _ => LeveState.Grab_StandardLeve
                    };
                    IceLogging.Info($"Exiting to grab our leves with the following mode: {Leve_Helper.State}", tag);
                    return true;
                }
                else
                {
                    IceLogging.Verbose("Queueing up navigation task", tag);
                    navTask.Enqueue(() => Task_Navmesh.TeleportCheck(vendorInfo));
                }
            }
            else
            {
                var count = navTask.Tasks.Count();

                if (EzThrottler.Throttle("Navigation is running", 10000))
                    IceLogging.Verbose($"Navigation is currently running, current task count: [{count}]", tag);
            }

            return false;
        }

        public static bool AethernetTask_Turnin(LeveInfo.Info_Vendor vendorInfo)
        {
            const string tag = "Travel: Navmesh Check";

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                if (Player.DistanceTo(vendorInfo.Npc_InteractZone) < 2)
                {
                    IceLogging.Verbose("We're close enough to the interact zone we don't need to queue up anything. Continuing to grab leve", tag);
                    Leve_Helper.State = LeveState.Turnin_Leve;
                    IceLogging.Info($"Exiting to grab our leves with the following mode: {Leve_Helper.State}", tag);
                    return true;
                }
                else
                {
                    IceLogging.Verbose("Queueing up navigation task", tag);
                    navTask.Enqueue(() => Task_Navmesh.TeleportCheck(vendorInfo));
                }
            }
            else
            {
                var count = navTask.Tasks.Count();

                if (EzThrottler.Throttle("Navigation is running", 10000))
                    IceLogging.Verbose($"Navigation is currently running, current task count: [{count}]", tag);
            }

            return false;
        }

        public static bool Aethernet_GatheringTravel(GatheringRoute routeInfo)
        {
            const string tag = "Travel: Gathering Pathing";

            void QueueGatherPath()
            {
                var node = routeInfo.NodeInfo[0];

                bool optionalFly = C.OptionalFly;
                bool minFlyDistance = C.Fly_MinDistance < Player.DistanceTo(node.Position);

                if (Utils.CanFly(routeInfo.TerritoryId) && optionalFly && minFlyDistance)
                {
                    P.navTask.Enqueue(() => Task_Navmesh.Task_FlyTo(Task_Navmesh.Gather_RandomFanPosition(node, true)), "Flying to destination");
                    P.navTask.Enqueue(() => Task_Navmesh.Task_GroundTo(Task_Navmesh.Gather_RandomFanPosition(node, false)), "Moving closer to the node");
                }
                else
                {
                    P.navTask.Enqueue(() => Task_Navmesh.Task_GroundTo(Task_Navmesh.Gather_RandomFanPosition(node, false)), "Moving closer to the node");
                }
            }

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                bool correctTerritory = Player.Territory.RowId == routeInfo.TerritoryId;
                if (correctTerritory)
                {
                    var closestNode = routeInfo.NodeInfo.OrderBy(x => Player.DistanceTo(x.Position)).FirstOrDefault();
                    if (Player.DistanceTo(closestNode.Position) < 5)
                    {
                        IceLogging.Verbose($"We're close to the gathering node. So we're going to continue on", tag);
                        IceLogging.Verbose($"Distance to node: {Player.DistanceTo(closestNode.Position):N2}", tag);
                        return true;
                    }
                    else
                    {
                        IceLogging.Verbose("We need to travel to a node still, queueing that up", tag);
                        QueueGatherPath();
                    }
                }
                else
                {
                    const uint IdyllshireAetheryteId = 75; // Idyllshire -> Drivanian AetheryteId

                    // We need to queue up a travel for both, so might as well do it prior to both of these
                    P.navTask.Enqueue(() => Task_Navmesh.TeleportToArea(routeInfo.AetheryteId), "Teleporting to aethernet");

                    if (routeInfo.AetheryteId == IdyllshireAetheryteId)
                    {
                        // The one route where we need to use special routing for handing getting there 
                        var shardInfo = Utils.Aethernet[IdyllshireAetheryteId];

                        P.navTask.Enqueue(() => Task_Navmesh.Task_GroundToAetheryte(shardInfo.Position, shardInfo.Position, false, 11), "Moving to Idyllshire Shard");
                        P.navTask.Enqueue(() => Task_Navmesh.LeaveIdyllshire(IdyllshireAetheryteId), "leaving Idllshire");
                        QueueGatherPath();
                    }
                    else
                    {
                        // Just need to queue up the gather path for this
                        QueueGatherPath();
                    }
                }
            }
            else
            {
                var count = navTask.Tasks.Count();

                if (EzThrottler.Throttle("Navigation is running", 10000))
                    IceLogging.Verbose($"Navigation is currently running, current task count: [{count}]", tag);
            }

            return false;
        }
    }
}
