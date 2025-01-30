using Dalamud.Game.ClientState.Objects.Types;
using ExplorersIcebox.Util;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskTarget
    {
        public static void Enqueue(ulong dataID)
        {
            IGameObject? gameObject = null;
            P.taskManager.Enqueue(() => TargetUtil.TryGetObjectByDataId(dataID, out gameObject));
            P.taskManager.Enqueue(() => TargetUtil.TargetByID(gameObject));
        }
    }
}
