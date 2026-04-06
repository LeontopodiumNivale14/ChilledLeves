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
                        new(() => DistanceCheck(vendorInfo), "Distance check to npc"),
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

        private static bool ZoneCheck(LeveInfo.VendorInfo npcInfo)
        {
            string tag = "Task Travel: Zone Check";

            var currentTerritory = Player.Territory.RowId;

            if (!P.navmesh.NavRunning())
            {
                if (currentTerritory == npcInfo.TerritoryId)
                {
                    IceLogging.Debug("We're in the correct zone, going to find the closest way to get to the npc", tag);
                    return true;
                }
                else
                {
                    if (EzThrottler.Throttle("Telling smartnav to path us to our destination"))
                    {
                        IceLogging.Verbose($"Kicking off smartnav for traveling to: [{npcInfo.TerritoryId}] -> [{npcInfo.Npc_InteractZone:N2}]", tag);
                        P.navmesh.SmartPath(npcInfo.TerritoryId, npcInfo.Npc_InteractZone);
                    }
                }
            }
            else
            {
                if (EzThrottler.Throttle("Navmesh throttle message", 2000))
                {
                    IceLogging.Verbose($"Smartnav/Navmesh is currently running. Waiting for it to finish pathing", tag);
                }
            }

            return false;
        }

        private static bool DistanceCheck(LeveInfo.VendorInfo npcInfo)
        {
            string tag = "Task Travel: Distance Check";

            if (Task_Navmesh.Task_GroundTo(npcInfo.Npc_InteractZone, false, 1))
            {
                IceLogging.Debug("We're close enough to the npc to interact w/ them. So we're going to do so", tag);
                return true;
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
