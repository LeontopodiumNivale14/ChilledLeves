using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks.Old_Task
{
    internal static class TaskGrabPrioLeve
    {
        private static uint leveId = 0;
        internal static void Enqueue(uint npcID, int classButton)
        {
            P.taskManager.Enqueue(() => UpdateItemAmounts(), "Updating Item Count");
            TaskInteract.Enqueue(npcID);
            P.taskManager.Enqueue(() => GrabLeve(npcID, classButton), "Grabbing Leve from NPC", DConfig);
            P.taskManager.Enqueue(() => LeaveLeveVendor(npcID), "Leaving the Leve NPC", DConfig);
            P.taskManager.Enqueue(() => PlayerNotBusy(), "Waiting for player to not be busy" ,DConfig);
            P.taskManager.Enqueue(() => CheckLeve(leveId), "Checking if leve has been grabbed");
        }

        internal static unsafe bool? GrabLeve(uint npcID, int classButton)
        {
            var craftButton = LeveNPCDict[npcID].CrafterButton;

            if (leveId != 0 && IsAccepted(leveId))
            {
                PluginInfo($"LeveID {leveId} has been accepted. Exiting the NPC");
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
            else if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var SelectYesno) && IsAddonReady(SelectYesno))
            {
                if (EzThrottler.Throttle("Accepting the leve", 500))
                {
                    GenericHandlers.FireCallback("SelectYesno", true, 0);
                }
            }
            else if (TryGetAddonMaster<GuildLeve>("GuildLeve", out var m) && m.IsAddonReady)
            {
                HashSet<string> PotentionalLeves = new HashSet<string>();

                foreach (var l in m.Levequests)
                {
                    PotentionalLeves.Add(l.Name);
                    PluginDebug($"Potentional Leves: {l.Name}");
                }

                string? prioLeve = GetLowestPriorityLeveName(PotentionalLeves);



                if (prioLeve != null)
                {
                    foreach (var l in m.Levequests)
                    {
                        if (l.Name == prioLeve)
                        {
                            if (TryGetAddonMaster<AddonMaster.JournalDetail>("JournalDetail", out var det) && det.IsAddonReady)
                            {
                                if (GetNodeText("JournalDetail", 19) != prioLeve)
                                {
                                    if (EzThrottler.Throttle("Selecting the Leve"))
                                        l.Select();
                                }
                                else if (GetNodeText("JournalDetail", 19) == prioLeve)
                                {
                                    if (EzThrottler.Throttle("Accepting leve"))
                                    {
                                        {
                                            PluginDebug($"Grabbing the leve: {leveId}");
                                            GenericHandlers.FireCallback("JournalDetail", true, 3, (int)leveId);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                else if (prioLeve == null)
                {
                    PluginInfo("No leves are potentional to grab");
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

        internal static unsafe bool? CheckLeve(uint leveId)
        {
            if (!IsAccepted(leveId))
            {
                PluginVerbos("You can't do anymore leves, stopping the process");
                Svc.Chat.Print("No more potentional leves, stopping gathering mode");
                SchedulerMain.DisablePlugin();
            }
            else
                return true;

            return false;
        }

        private static string? GetLowestPriorityLeveName(HashSet<string> leveNames)
        {
            var leve = LeveDictionary
                        .Where(kvp => leveNames.Contains(kvp.Value.LeveName) && (CraftDictionary[kvp.Key].TurninAmount <= CraftDictionary[kvp.Key].CurrentItemAmount)) // Filter by HashSet and check the TurninAmount
                        .OrderBy(kvp => kvp.Value.Priority) // Sort by Priority
                        .FirstOrDefault(); // Get the first (lowest priority) or null if none found

            if (!LeveDictionary.ContainsKey(leve.Key))
            {
                leveId = 0;
                return null;
            }

            leveId = leve.Key; // Output the LeveID
            return leve.Value.LeveName; // Return the LeveName
        }

        private static void UpdateItemAmounts()
        {
            if (SelectedLeves.Count > 0)
            {
                foreach (var leve in SelectedLeves)
                {
                    int ItemId = (int)CraftDictionary[leve].ItemID;
                    CraftDictionary[leve].CurrentItemAmount = GetItemCount(ItemId);
                }
            }
        }
    }
}
