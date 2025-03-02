using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskGrabPrioLeve
    {
        internal static void Enqueue(uint leveID, uint npcID, int classButton)
        {
            TaskInteract.Enqueue(npcID);
            P.taskManager.Enqueue(() => GrabLeve(npcID, classButton), DConfig);
            P.taskManager.Enqueue(() => LeaveLeveVendor(npcID), DConfig);
            P.taskManager.Enqueue(() => PlayerNotBusy(), DConfig);
        }

        internal static unsafe bool? GrabLeve(uint npcID, int classButton)
        {
            uint matchingLeveID = 0;
            var craftButton = LeveNPCDict[npcID].CrafterButton;

            if (matchingLeveID != 0 && IsAccepted(matchingLeveID))
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
                string matchingLeveName = null;

                foreach (var l in m.Levequests)
                {
                    // Iterate over priority-ordered LeveIDs that exist in currentLeveIDs
                    foreach (var leveID in CrafterLeves
                        .Where(kv => SelectedLeves.Contains(kv.Key)) // Keep only existing LeveIDs
                        .OrderBy(kv => kv.Value) // Sort by priority (lower = higher priority)
                        .Select(kv => kv.Key)) // Select only the IDs
                    {
                        // Get the LeveName for this ID
                        string leveName = CrafterLeves[leveID].LeveName;

                        // If it matches, store it and break out of the loop
                        if (l.Name == leveName)
                        {
                            matchingLeveID = leveID;
                            matchingLeveName = leveName;
                            hasLeve = true;
                            if (TryGetAddonMaster<AddonMaster.JournalDetail>("JournalDetail", out var det) && det.IsAddonReady)
                            {
                                if (GetNodeText("JournalDetail", 19) != matchingLeveName)
                                {
                                    if (EzThrottler.Throttle("Selecting the Leve"))
                                        l.Select();
                                }
                                else if (GetNodeText("JournalDetail", 19) == matchingLeveName)
                                {
                                    if (EzThrottler.Throttle("Accepting leve"))
                                    {
                                        GenericHandlers.FireCallback("JournalDetail", true, 3, (ushort)matchingLeveID);
                                    }
                                }
                            }
                        }
                    }

                    // If no matching LeveID was found, continue to the next Levequest
                    if (matchingLeveID == 0)
                        continue;


                }
                if (!hasLeve)
                {
                    PluginLog($"Could not find a viable leve to grab for Gathering Mode");
                    PluginLog($"This could be due to the following problems: ");
                    PluginLog($"1: Leve level is to low or");
                    PluginLog($"2: It's a gathering leve and the vendor doesn't have this leve currently.");
                    PluginLog($"In the case of #2, please do other gathering leves of this type to be able to do this leve potentionally");
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
