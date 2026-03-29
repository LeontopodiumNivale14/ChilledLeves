using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.GameHelpers;
using ECommons.Throttlers;
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
                        new(() => GrabLeve(vendorInfo), "Grabbing the leve f")
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

        private static bool OpenLeveWindow(LeveInfo.VendorInfo npcInfo)
        {
            if (GenericHelpers.TryGetAddonMaster<GuildLeve>("GuildLeve", out var guildLeve) && guildLeve.IsAddonReady)
            {

            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Talk Window Visibility", 100))
                    talkCooldown += 1;

                if (talkCooldown > 1)
                {
                    talk.Click();
                    talkCooldown = 0;
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {

            }

            return false;
        }

        private static bool GrabLeve(LeveInfo.VendorInfo npcInfo)
        {


            return false;
        }
    }
}
