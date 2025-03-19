using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskGrabLeve
    {
        internal static void Enqueue(uint leveID, uint npcID, int classButton)
        {
            TaskInteract.Enqueue(npcID);
            P.taskManager.Enqueue(() => GrabLeve((ushort)leveID, npcID, classButton), DConfig);
            P.taskManager.Enqueue(() => LeaveLeveVendor(npcID), DConfig);
            P.taskManager.Enqueue(() => PlayerNotBusy(), DConfig);
        }

        internal static unsafe bool? GrabLeve(ushort leveID, uint npcID, int classButton)
        {
            var LeveName = LeveDictionary[leveID].LeveName;
            var craftButton = LeveNPCDict[npcID].CrafterButton;

            if (IsAccepted(leveID))
            {
                return true;
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonActive("Talk"))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    GenericHandlers.FireCallback("Talk", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
            {
                if (EzThrottler.Throttle("Opening the Levequests Window", 100))
                {
                    GenericHandlers.FireCallback("SelectString", true, classButton);
                }
            }
            else if (TryGetAddonMaster<GuildLeve>("GuildLeve", out var m) && m.IsAddonReady)
            {
                bool hasLeve = false;
                foreach (var l in m.Levequests)
                {
                    if (l.Name != LeveName)
                        { continue; }
                    else if (l.Name == LeveName)
                    {
                        hasLeve = true;
                        if (TryGetAddonMaster<AddonMaster.JournalDetail>("JournalDetail", out var det) && det.IsAddonReady)
                        {
                            if (GetNodeText("JournalDetail", 19) != LeveName)
                            {
                                if (EzThrottler.Throttle("Selecting the Leve"))
                                    l.Select();
                            }
                            else if (GetNodeText("JournalDetail", 19) == LeveName)
                            {
                                if (EzThrottler.Throttle("Accepting leve"))
                                {
                                    GenericHandlers.FireCallback("JournalDetail", true, 3, leveID);
                                }
                            }
                        }
                    }
                }
                if (!hasLeve)
                {
                    PluginVerbos($"The following leve: {LeveName} could not be found at the vendor.");
                    PluginVerbos($"This could be due to the following problems: ");
                    PluginVerbos($"1: Leve turnin area not completed (aka missing a quest)");
                    PluginVerbos($"2: It's a gathering leve and the vendor doesn't have this leve currently.");
                    PluginVerbos($"In the case of #2, please do other gathering leves of this type to be able to do this leve potentionally");
                    foreach (var kdp in C.workList)
                    {
                        if (C.workList.Any(e => e.LeveID == leveID))
                        {
                            { C.workList.Remove(kdp); }
                            break;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        internal static unsafe bool? LeaveLeveVendor(uint npcID)
        {
            var leaveButton = LeveNPCDict[npcID].LeaveButton;

            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
            {
                if (EzThrottler.Throttle("Leaving Leve Person", 500))
                {
                    GenericHandlers.FireCallback("SelectString", true, leaveButton);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("GuildLeve", out var GuildLeve) && IsAddonReady(GuildLeve))
            {
                if (EzThrottler.Throttle("Leaving Journal Window", 500))
                {
                    GenericHandlers.FireCallback("GuildLeve", true, -1);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonActive("Talk"))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    GenericHandlers.FireCallback("Talk", true, 0);
                }
            }
            else if (PlayerNotBusy())
            {
                return true;
            }

            return false;
        }
    }
}
