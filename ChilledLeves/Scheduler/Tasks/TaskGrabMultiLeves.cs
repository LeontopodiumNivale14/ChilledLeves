using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskGrabMultiLeves
    {
        private static SortedDictionary<uint, string> ViableLeves = new SortedDictionary<uint, string>();

        internal static void Enqueue(uint npcID, int classButton)
        {
            TaskInteract.Enqueue(npcID);
            P.taskManager.Enqueue(() => UpdateViableLeves());
            P.taskManager.Enqueue(() => GrabLeveMulti(npcID, classButton), DConfig);
            P.taskManager.Enqueue(() => LeaveLeveVendor(npcID), DConfig);
            P.taskManager.Enqueue(() => PlayerNotBusy(), DConfig);
        }

        internal static void UpdateViableLeves()
        {
            ViableLeves.Clear();
            foreach (var leve in C.workList)
            {
                if (!ViableLeves.ContainsKey(leve.LeveID))
                {
                    string leveName = CrafterLeves[leve.LeveID].LeveName;
                    ViableLeves.Add(leve.LeveID, leveName);
                }
            }
        }

        internal static unsafe bool? GrabLeveMulti(uint npcID, int classButton)
        {
            bool IsAllAccepted = false;

            if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonActive("Talk"))
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
                bool foundLeve = false;

                foreach (var l in m.Levequests)
                {
                    var leveMatch = ViableLeves.FirstOrDefault(v => v.Value == l.Name); // Checking to see and make sure that the leve exist in the Dictionary

                    if (!leveMatch.Equals(default(KeyValuePair<uint, string>))) // If a match is found/that it didn't find one, and you haven't accepted the quest yet
                    {
                        PluginLog($"Leve: {leveMatch.Value} [{leveMatch.Key}] was found");
                        var leveId = leveMatch.Key;
                        var leveName = leveMatch.Value;
                        var itemId = CrafterLeves[leveId].ItemID;
                        var itemAmountRequired = CrafterLeves[leveId].TurninAmount;
                        var itemAmountHave = GetItemCount((int)itemId);

                        if (GenericThrottle)
                            PluginLog($"Leve {leveMatch.Value}: Accepted? {IsAccepted(leveId)}");
                        // if 3 <= 2
                        if (itemAmountRequired <= itemAmountHave && !IsAccepted(leveId) && Allowances > 0)
                        {
                            if (GenericThrottle)
                            PluginLog($"Leve: {leveName} [{leveId}] has not been picked up yet");

                            foundLeve = true;
                            if (TryGetAddonMaster<AddonMaster.JournalDetail>("JournalDetail", out var det) && det.IsAddonReady)
                            {
                                if (GetNodeText("JournalDetail", 19) != leveName)
                                {
                                    if (EzThrottler.Throttle("Selecting the Leve", 100))
                                    {
                                        l.Select();
                                        PluginLog($"Selecting leve: {leveName}");
                                    }
                                }
                                else if (GetNodeText("JournalDetail", 19) == leveName)
                                {
                                    if (EzThrottler.Throttle("Accepting leve", 100))
                                    {
                                        GenericHandlers.FireCallback("JournalDetail", true, 3, (int)leveId);
                                        PluginLog($"Accepting the leve: {leveName}");
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                if (!foundLeve)
                {
                    PluginLog("All possible leves at this vendor has been accepted, continuing on w/ the process");
                    IsAllAccepted = true;
                }
            }

            return IsAllAccepted;
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
