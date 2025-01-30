namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskUpdateShopID
    {
        public static void Enqueue()
        {
            if (!IsAddonActive("MJIPouch"))
            {
                TaskCallback.Enqueue("MJIHud", true, 13);
            }
            P.taskManager.EnqueueDelay(100);
            P.taskManager.Enqueue(() => UpdateShopCallback());
            P.taskManager.EnqueueDelay(100);
            TaskCallback.Enqueue("MJIPouch", true, 1);
        }
    }
}
