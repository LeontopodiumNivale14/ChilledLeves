using ChilledLeves.Scheduler;
using ChilledLeves.Scheduler.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Debug_TaskTest
    {
        public static uint selectedLeve = 1647;

        public static void Draw()
        {
            ImGui.Text($"Running task: {P.taskManager.NumQueuedTasks != 0} | Amount of queue'd task: {P.taskManager.NumQueuedTasks}");
            string currentTask = P.taskManager.CurrentTask?.Name ?? "";
            ImGui.Text($"Current task running: {currentTask}");
            ImGui.Text($"Current State: {Leve_Helper.State}");
            ImGui.Text($"Task Count: {P.taskManager.Tasks.Count}");

            ImGui.Separator();

            ImGui.SetNextItemWidth(150);
            ImGui.InputUInt("Leve to grab", ref selectedLeve);
            if (ImGui.Button("Stop Task"))
            {
                P.taskManager.Tasks.Clear();
                P.taskManager.Abort();
            }
            if (ImGui.Button("Test Interact w/ Npc"))
            {
                Leve_Helper.LeveToGrab = selectedLeve;
                Task_Travel.Grab_Enqueue();
            }

            if (ImGui.Button("Test Grab Leve"))
            {
                Leve_Helper.LeveToGrab = selectedLeve;
                P.taskManager.Enqueue(() => Task_GrabLeve.GrabLeve(), "Grabbing Leve from vendor");
            }
        }
    }
}
