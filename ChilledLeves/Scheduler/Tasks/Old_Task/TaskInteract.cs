using Dalamud.Game.ClientState.Objects.Types;

namespace ChilledLeves.Scheduler.Tasks.Old_Task
{
    internal static class TaskInteract
    {
        public static void Enqueue(ulong dataID)
        {
            IGameObject? gameObject = null;
            P.taskManager.Enqueue(PlayerNotBusy, "Waiting for player to not be busy");
            P.taskManager.Enqueue(() => TryGetObjectByDataId(dataID, out gameObject), "Getting Objec by DataId");
            P.taskManager.Enqueue(() => PluginVerbos($"Data ID of the target is: {dataID}"), "Plugin Verbose");
            P.taskManager.Enqueue(() => PluginVerbos($"Interacting w/ {gameObject?.Name}"), "Plugin Verbose");
            P.taskManager.Enqueue(() => InteractWithObject(gameObject), "Interacting with Object");
            P.taskManager.Enqueue(() => PluginVerbos("Interacted w/ target now"));
        }
    }
}
