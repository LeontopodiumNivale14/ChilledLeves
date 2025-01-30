using Dalamud.Game.ClientState.Objects.Types;
using ExplorersIcebox.Util;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskInteract
    {
        public static void Enqueue(uint dataID)
        {
            IGameObject? gameObject = null;
            P.taskManager.Enqueue(() => TargetUtil.TryGetObjectByDataId(dataID, out gameObject));
            P.taskManager.Enqueue(() => TargetUtil.InteractWithObject(gameObject));
        }
    }
}
