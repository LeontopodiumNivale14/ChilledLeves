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
            return true;
        }
        internal static bool DisablePlugin()
        {
            EnableTicking = false;
            P.navmesh.Stop();
            P.taskManager.Abort();
            return true;
        }

        internal static string CurrentProcess = "";


        // dummy values so I can get the logic in 
        private static uint ClassLevel = 100;

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
                            if (entry.InputValue == 0)
                            {
                                C.workList.Remove(entry);
                            }
                            else if (entry.InputValue != 0)
                            {
                                PluginLog($"Worklist has found that: {entry.LeveID} has an amount that isn't set to 0. Starting to work on this turnin process");
                                var templeve = entry.LeveID;
                                var currentAmount = GetItemCount((int)CrafterLeves[templeve].ItemID);
                                var necessaryAmount = CrafterLeves[templeve].TurninAmount;
                                if (currentAmount >= necessaryAmount)
                                {
                                    PluginLog("You have the necessary amount to run this leve. Grabbing/Turning In");
                                    leve = templeve;
                                    break;
                                }
                            }
                            else
                                noLeves = true;
                        }
                        if (!noLeves)
                        {
                            if (CrafterLeves[leve].Level <= ClassLevel)
                            {
                                if (IsAccepted(leve))
                                {
                                    var Turninnpc = CrafterLeves[leve].LeveTurninVendorID;
                                    var LeveName = CrafterLeves[leve].LeveName;
                                    var zoneID = LeveNPCDict[Turninnpc].ZoneID;
                                    var aetheryte = LeveNPCDict[Turninnpc].Aetheryte;
                                    var NPCLocation = LeveNPCDict[Turninnpc].NPCLocation;

                                    if (IsInZone(zoneID))
                                    {
                                        if (GetDistanceToPlayer(NPCLocation) < 1)
                                        {
                                            var worklist = (int)leve;

                                            TaskTarget.Enqueue(Turninnpc);
                                            TaskInteract.Enqueue(Turninnpc);
                                            TaskTurnin.Enqueue(LeveName, leve);
                                            TaskUpdateWorkList.Enqueue(leve);
                                        }
                                        else if (GetDistanceToPlayer(NPCLocation) > 1)
                                        {
                                            bool fly = false;

                                            if (LeveNPCDict[Turninnpc].Mount)
                                            {
                                                TaskMountUp.Enqueue();
                                                if (LeveNPCDict[Turninnpc].Fly.HasValue)
                                                {
                                                    #pragma warning disable CS8629 // Nullable value type may be null.
                                                    fly = (bool)LeveNPCDict[Turninnpc].Fly;
                                                    #pragma warning restore CS8629 // Nullable value type may be null.
                                                }
                                            }

                                            TaskMoveTo.Enqueue(NPCLocation, "LeveNPC", fly, 1);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                }
                                else if (!IsAccepted(leve) && Allowances > 0)
                                {
                                    var npc = CrafterLeves[leve].LeveVendorID;
                                    var zoneID = LeveNPCDict[npc].ZoneID;
                                    var aetheryte = LeveNPCDict[npc].Aetheryte;
                                    var NPCLocation = LeveNPCDict[npc].NPCLocation;
                                    var requiredLevel = CrafterLeves[leve].Level;
                                    var jobID = CrafterLeves[leve].EcomJob;

                                    if (IsInZone(zoneID))
                                    {
                                        if (GetDistanceToPlayer(NPCLocation) < 1)
                                        {
                                            TaskTarget.Enqueue(npc);
                                            TaskGrabLeve.Enqueue(leve, npc);
                                        }
                                        else if (GetDistanceToPlayer(NPCLocation) > 1)
                                        {
                                            bool fly = false;

                                            if (LeveNPCDict[npc].Mount)
                                            {
                                                TaskMountUp.Enqueue();
                                                if (LeveNPCDict[npc].Fly.HasValue)
                                                {
                                                    #pragma warning disable CS8629 // Nullable value type may be null.
                                                    fly = (bool)LeveNPCDict[npc].Fly;
                                                    #pragma warning restore CS8629 // Nullable value type may be null.
                                                }
                                            }

                                            TaskMoveTo.Enqueue(NPCLocation, "LeveNPC", fly, 0.5f);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        TaskClassChange.Enqueue(jobID);
                                        P.taskManager.EnqueueDelay(500);
                                        TaskLevelChecker.Enqueue(requiredLevel);
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                }
                                else if (Allowances == 0)
                                {
                                    PluginLog("Somehow, you got here??? Not sure how but. Disabling the plugin");
                                    DisablePlugin();
                                }
                            }
                        }
                        else
                        {
                            if (Allowances == 0)
                                PluginLog("HEY. YOU HAVE NO LEVES FOOL.");
                            DisablePlugin();
                        }
                    }
                }
            }
        }
    }
}
