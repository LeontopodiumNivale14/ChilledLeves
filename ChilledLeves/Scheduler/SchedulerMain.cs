using ChilledLeves.Scheduler.Tasks;

namespace ChilledLeves.Scheduler
{
    internal static unsafe class SchedulerMain
    {
        internal static bool AreWeTicking;
        internal static bool EnableTicking
        {
            get => AreWeTicking;
            private set => AreWeTicking = value;
        }
        internal static bool EnablePlugin()
        {
            EnableTicking = true;
            CurrentRun = 0;
            return true;
        }
        internal static bool DisablePlugin()
        {
            EnableTicking = false;
            CurrentRun = 0;
            P.taskManager.Abort();
            return true;
        }

        internal static string CurrentProcess = "";


        // dummy values so I can get the logic in 
        private static uint RunAmount = 0;
        private static uint CurrentRun = 0;
        private static uint ClassLevel = 0;
        private static uint TurninZone = 0;
        private static uint Dummy = 0;
        private static bool Synth = true;

        public static int SynthRunAmount = 0;

        internal static void Tick()
        {
            if (AreWeTicking)
            {
                if (GenericThrottle)
                {
                    if (!P.taskManager.IsBusy)
                    {
                        uint leve = 0;
                        bool noLeves = false;
                        foreach (var entry in C.workList)
                        {
                            if (entry.InputValue != 0)
                            {
                                PluginLog($"Worklist has found that: {entry.LeveID} has an amount that isn't set to 0. Starting to work on this turnin process");
                                leve = entry.LeveID;
                                break;
                            }
                            else
                                noLeves = true;
                        }
                        if (C.workList.Count > 0 && !noLeves)
                        {
                            if (CrafterLeves[leve].Level >= ClassLevel)
                                if (!IsAccepted(leve))
                                {
                                    var npc = CrafterLeves[leve].LeveVendorID;
                                    var zoneID = LeveNPCDict[npc].ZoneID;
                                    var aetheryte = LeveNPCDict[npc].Aetheryte;

                                    if (AethernetDict.ContainsKey(zoneID))
                                    {
                                        if (!IsInZone(AethernetDict[zoneID].TeleportZone))
                                        {
                                            TaskUseAethernet.Enqueue(zoneID);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                    // Use Aethernet if need to get to 2nd zone
                                    // Move to NPC
                                    // TargetNPC
                                    // Interact with NPC
                                    // THIS IS SOMETHING TO THINK ABOUT BECUASE SOMEONE'S GOING TO BE FUCKING DUMB
                                    // If (SelectIconString is visible)
                                    // Need to check and see *-which-* node contains the leve name
                                    // 0 is the top obviously, 
                                }
                                else if (!IsAccepted(leve))
                                {
                                    // Teleport to Zone
                                    // Use Aethernet if need to get to the 2nd zone
                                    // Move to the NPC
                                    // Change Class
                                    // Target NPC
                                    // Skip Through Dialog
                                    // Open Tradecraft Leves (1)
                                    // Check to make sure that the JournalName == LeveName
                                    // Accept
                                    // Close Leve Menu
                                    // Get out of Dialog
                                }
                        }
                        else
                        {
                            // Need to add a prompt to say that you're not the required level for this leve and skips it
                        }
                    }
                }
            }
        }
    }
}
