using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using Lumina.Excel.Sheets;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Travel
    {
        public static void Grab_Enqueue()
        {
            string tag = "Task Travel: Grab Enqueue";

            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                if (LeveInfo.LeveNpc_Info.TryGetValue(sheetInfo.Npc_Vendor, out var vendorInfo))
                {
                    P.taskManager.EnqueueMulti
                    (
                        new(() => ZoneCheck(vendorInfo), "Checking Zone Requirements"),
                        // new(() => DistanceCheck(vendorInfo), "Distance check to npc"),
                        new(() => OpenLeveWindow(vendorInfo, sheetInfo.Npc_Vendor)),
                        new(() => 
                        {
                            Leve_Helper.State = Leve_State.Grab_StandardLeve;
                            return true;
                        }, "Changing state to grab leve")
                    );
                }
                else
                {
                    IceLogging.Error($"Missing NPC info on the following: {Leve_Helper.LeveToGrab}. Gave Id: {sheetInfo.Npc_Vendor}", tag);
                    Leve_Helper.State = Leve_State.Idle;
                }
            }
            else
            {
                IceLogging.Error($"We seem to be missing a leve out of the sheets? {Leve_Helper.LeveToGrab}. Please report back to me on this", tag);
                Leve_Helper.State = Leve_State.Idle;
            }
        }

        private static bool ZoneCheck(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Travel: Zone Check";

            if (Player.Available)
            {
                var territoryId = Player.Territory.RowId;
                if (vendorInfo.TerritoryId == territoryId)
                {
                    IceLogging.Verbose("We're currently in our designated zone, so going to check for distance to leve vendor", tag);
                    P.taskManager.Insert(() => AethernetTask(vendorInfo), "Starting Travel Segment");
                }
                else
                {
                    // Add Foundation to this
                    HashSet<uint> Limsa = new() { 128, 129 };
                    HashSet<uint> UlDah = new() { 132, 133 };
                    HashSet<uint> Gridania = new() { 130, 131 };
                    HashSet<uint> Foundation = new() { };

                    uint LimsaUpperId = 128;

                    if (Limsa.Contains(territoryId))
                    {
                        IceLogging.Verbose("We're in limsa, but just on the wrong floor. Using the aethernet to get us to the right space", tag);
                    }
                    else if (UlDah.Contains(territoryId))
                    {
                        IceLogging.Verbose("We're in Ul' Dah, but just on the wrong section. Using the aethernet to get us to the right space", tag);
                    }
                    else if (Gridania.Contains(territoryId))
                    {
                        IceLogging.Verbose("We're in Gridania, but just on the wrong section. Using the aethernet to get us to the right space", tag);
                    }
                    else if (Foundation.Contains(territoryId))
                    {
                        IceLogging.Verbose("We're in Foundation [Somewhere], but just on the wrong section. Using the aethernet to get us to the right space", tag);
                    }
                    else
                    {
                        IceLogging.Verbose("We're just not in the right area. Going to queue up a teleport task, then check for anything post", tag);
                        if (vendorInfo.TerritoryId == LimsaUpperId)
                        {
                            // Special case, since we're teleporting to lower and need to get to upper. Need to teleport -> force use the cross city aethernet

                        }
                        else
                        {
                            // Just a normal teleport -> check to see if we can even use the aethernet
                        }
                    }

                    if (MainCityMulti.Contains(territoryId))
                    {
                        IceLogging.Verbose("We're in a main city where there's multiple areas within (woo). Going to use the aethernet to travel to the closest point", tag);
                        P.taskManager.Insert(() => AethernetTask(vendorInfo), "Travel: Aethernet City Travel");
                        return true;
                    }
                    else
                    {
                        IceLogging.Verbose("We just need to directly teleport. Then we need to check for post aethernet travel.", tag);
                        P.taskManager.Insert(() => TeleportTask(vendorInfo), "Travel: Teleporting");
                        return true;
                    }
                }
            }

            return false;
        }
        private static bool TeleportTask(LeveInfo.VendorInfo vendorInfo)
        {
            string tag = "Travel: Teleport Task";

            // Grab the vendor TerritoryId / Aetheryte
            // Have it initiate the task to teleport
            // Have... some sanity check on attempting to teleport. This is going to be a minor pita
            // Once it sucessfully teleports to said aetheryte, check to see if it needs to travel to the other zone via aethernet (Limsa Upper)
            // If yes, use aethernet to travel up north
            // If no, we JUST need to travel via navmesh. 
            // - If in city, no mount flying
            // - If outside of city (ARR areas) use mount / flying version of the task

            return false;
        }

        private static bool AethernetTask(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Travel: Navmesh Check";

            // IDEALLY... this will be just for teleporting to the floor that the npc is at. 
            // Which in other words, is used to get to limsa upper and limsa lower. 
            // Will need to check the paths to see which is closer to the goal ones, and have it travel to those...

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                if (Player.DistanceTo(vendorInfo.Npc_InteractZone) < 2)
                {
                    IceLogging.Verbose("We're close enough to the interact zone we don't need to queue up anything. Continuing to grab leve", tag);
                    return true;
                }
                else
                {
                    IceLogging.Verbose("Queueing up navigation task", tag);
                    navTask.Enqueue(() => Task_Navmesh.QueueCityAethernet(vendorInfo));
                }
            }
            else
            {
                
            }

            return false;
        }

        private static int talkCooldown = 0;

        private static bool OpenLeveWindow(LeveInfo.VendorInfo npcInfo, uint npcId)
        {
            string tag = "Open Leve Window";

            if (GenericHelpers.TryGetAddonMaster<GuildLeve>("GuildLeve", out var guildLeve) && guildLeve.IsAddonReady)
            {
                IceLogging.Debug("We should be in the journal tab now! So we're going to collect our leve", tag);
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Talk Window Visibility", 10))
                    talkCooldown += 1;

                if (talkCooldown > 1)
                {
                    talk.Click();
                    talkCooldown = 0;
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting Addon Button", 1000))
                    SelectLeveKind(selectString);
            }
            else
            {
                if (!Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
                {
                    if (Utils.TryGetObjectByDataId(npcId, out var gameObject))
                    {
                        if (EzThrottler.Throttle("Interact/Target NPC"))
                        {
                            Utils.TargetgameObject(gameObject);
                            Utils.InteractWithObject(gameObject);
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Npc doesn't exist log", 2000))
                            IceLogging.Error($"NPC: {npcId} doesn't seem to exist in [{Player.Territory.RowId}].\n" +
                                             $"Player Position: {Player.Position:N2}", tag);
                    }
                }
            }

            return false;
        }

        public static void SelectLeveKind(SelectString addon)
        {
            string tag = "Select Leve: Addon";
            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                var kind = sheetInfo.LeveType;

                string NormalizeForComparison(string input)
                {
                    return System.Text.RegularExpressions.Regex.Replace(input, @"\d+", "").Trim();
                }

                if (!LeveInfo.Leve_SelectText.TryGetValue(kind, out var targetText))
                {
                    if (EzThrottler.Throttle("Error Message Dictionary: LeveKind", 2000))
                        IceLogging.Error($"No text was found to match up in the dictionary. Please report this. {kind}", tag);
                    return;
                }

                var normalizedTarget = NormalizeForComparison(targetText);

                SelectString.Entry? match = addon.Entries.Cast<AddonMaster.SelectString.Entry?>()
                        .FirstOrDefault(e => NormalizeForComparison(e!.Value.Text)
                        .Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));

                if (match is null)
                {
                    if (EzThrottler.Throttle("Error Message: LeveKind", 2000))
                        IceLogging.Error($"No text was found to match up in the addon itself. Please report this. {kind}", tag);
                    return;
                }

                if (EzThrottler.Throttle("Positive Kind Message", 2000))
                    IceLogging.Verbose($"We managed to find a kind to match up! Selecting it now [{match.Value.Text}] {kind}", tag);
                match.Value.Select();
            }
            else
            {
                if (EzThrottler.Throttle("Error Message: Sheet Info", 2000))
                    IceLogging.Error($"Hey! We've somehow gotten an invalid leve that doesn't exist in the sheets... {Leve_Helper.LeveToGrab}", tag);
            }
        }
    }
}
