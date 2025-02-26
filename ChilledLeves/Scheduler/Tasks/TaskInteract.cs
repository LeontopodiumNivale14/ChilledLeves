using Dalamud.Game.ClientState.Objects.Types;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskInteract
    {
        public static void Enqueue(ulong dataID)
        {
            IGameObject? gameObject = null;
            P.taskManager.Enqueue(PlayerNotBusy);
            P.taskManager.Enqueue(() => TryGetObjectByDataId(dataID, out gameObject));
            P.taskManager.Enqueue(() => PluginLog($"Data ID of the target is: {dataID}"));
            P.taskManager.Enqueue(() => PluginLog($"Interacting w/ {gameObject?.Name}"));
            P.taskManager.Enqueue(() => InteractWithObject(gameObject));
            P.taskManager.Enqueue(() => PluginLog("Interacted w/ target now"));
        }
    }
}
