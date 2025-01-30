using Dalamud.Game.ClientState.Conditions;
using System.Collections.Generic;

namespace ExplorersIcebox.Scheduler.Tasks.GroupTask
{
    internal static class GroupIslandTask
    {
        internal static int LoopAmount;
        internal static int LoopCount;
        internal unsafe static void Enqueue(List<RouteEntry> routeEntries)
        {
            //
            displayCurrentRoute = RouteDataPoint[C.routeSelected].Name;
            LoopAmount = 0;
            LoopCount = 0;
            TaskReturn.Enqueue();
            UpdateTableDict();
            TableSellUpdate(GetTable(C.routeSelected));
            if (TotalSellItems(GetTable(C.routeSelected)) > 0)
            {
                P.taskManager.Enqueue(() => UpdateDisplayText("Selling to the shop"));
                if (Svc.Condition[ConditionFlag.Mounted])
                    TaskDisMount.Enqueue();
                P.taskManager.EnqueueDelay(1000);
                TaskMoveTo.Enqueue(mammetExportPos, "Mammet Export", false, 1);
                // TaskSellTo.Enqueue(); old targeting code, keeping it here for reference
                TaskInteract.Enqueue(ExportMammetID);
                TaskCallback.Enqueue("SelectString", true, 0);
                for (var i = 0; i < routeEntries.Count; i++)
                {
                    if (routeEntries[i].Sell > 0)
                    {
                        var itemID = routeEntries[i].ID;
                        var currentItemCount = GetItemCount(itemID);
                        PluginLog($"{itemID} has enough to sell");
                        PluginLog($"{routeEntries[i].Sell} <-- selling this much");
                        P.taskManager.Enqueue(() => displayCurrentTask = $"Selling item with this ID: {itemID}");
                        P.taskManager.EnqueueDelay(200);
                        TaskCallback.Enqueue("MJIDisposeShop", true, 12, IslandItemDict[itemID].Callback);
                        P.taskManager.EnqueueDelay(200);
                        TaskCallback.Enqueue("MJIDisposeShopShipping", true, 11, routeEntries[i].Sell);
                        P.taskManager.EnqueueDelay(200);
                        P.taskManager.Enqueue(() => !IsAddonActive("MJIDisposeShopShipping"));
                        P.taskManager.EnqueueDelay(500);
                        P.taskManager.Enqueue(() => DidAmountChange(currentItemCount, GetItemCount(itemID)));
                    }
                }
                P.taskManager.Enqueue(() => UpdateDisplayText("Leaving the shop now"));
                P.taskManager.EnqueueDelay(20);
                TaskCallback.Enqueue("MJIDisposeShop", true, 1);
                P.taskManager.Enqueue(() => !IsAddonActive("MJIDisposeShop"));
                P.taskManager.EnqueueDelay(20);
                P.taskManager.Enqueue(() => SchedulerMain.CurrentProcess = "Back to entrance");
                TaskMoveTo.Enqueue(workshopExitPos, "Entrance of shop", false, 1);
            }
            P.taskManager.Enqueue(() => UpdateDisplayText("Heading to the route destination"));
            TaskVislandTemp.Enqueue(RouteDataPoint[C.routeSelected].Location, RouteDataPoint[C.routeSelected].Name);
            while (LoopAmount < RouteAmount(C.routeSelected, SchedulerMain.WorkshopSelected))
            {
                TaskVislandTemp.Enqueue(RouteDataPoint[C.routeSelected].Base64Export, $"Enabling the following Route: {RouteDataPoint[C.routeSelected].Name}");
                P.taskManager.EnqueueDelay(100);
                P.taskManager.Enqueue(() => LoopCount = LoopCount + 1);
                P.taskManager.Enqueue(() => PluginLog($"Loop amount is currently: {LoopAmount}"));
                P.taskManager.Enqueue(() => UpdateDisplayText($"Loop {LoopCount} / {RouteAmount(C.routeSelected, SchedulerMain.WorkshopSelected)}"));
                P.taskManager.Enqueue(() => P.visland.IsRouteRunning() == false, $"{RouteDataPoint[C.routeSelected].Name} is currently running", configuration: DConfig);
                LoopAmount++;
            }
            P.taskManager.Enqueue(() => PluginLog("A full cycle has been completed!"));
            P.taskManager.Enqueue(() => displayCurrentRoute = "");
            P.taskManager.Enqueue(() => displayCurrentTask = "");
            TaskReturn.Enqueue();
        }
    }
}
