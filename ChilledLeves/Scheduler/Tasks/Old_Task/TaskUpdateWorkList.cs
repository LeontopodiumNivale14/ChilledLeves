namespace ChilledLeves.Scheduler.Tasks.Old_Task
{
    internal static class TaskUpdateWorkList
    {
        internal unsafe static void Enqueue(uint leveID)
        {
            P.taskManager.Enqueue(() => UpdateWorkList(leveID), "Updating the worklist");
        }

        internal unsafe static bool? UpdateWorkList(uint leveID)
        {
            #pragma warning disable CS8600
            LeveEntry foundEntry = C.workList.FirstOrDefault(entry => entry.LeveID == leveID);

            if (C.workList.Count == 1 && C.RepeatLastLeve)
            {
                LeveEntry OnlyEntry = C.workList.FirstOrDefault();
                if (OnlyEntry.InputValue != 1)
                {
                    OnlyEntry.InputValue = OnlyEntry.InputValue - 1;
                }
                else
                {
                    OnlyEntry.InputValue = 1;
                }
            }
            else if (foundEntry != null)
            {
                foundEntry.InputValue = foundEntry.InputValue - 1;
                PluginVerbos($"Updated {leveID} to now have {foundEntry.InputValue}");
                if (foundEntry.InputValue == 0)
                {
                    if (foundEntry.InputValue == 0)
                        foundEntry.InputValue = 1;
                    ListCycled.Add(new LeveEntry { LeveID = foundEntry.LeveID, InputValue = 0 });
                    PluginDebug($"List Cycled entry added {foundEntry.LeveID}");
                    C.workList.Remove(foundEntry);
                }
            }

            return true;
        }
    }
}
