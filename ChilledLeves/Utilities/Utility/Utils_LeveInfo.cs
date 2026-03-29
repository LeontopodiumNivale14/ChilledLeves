using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;

namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public static unsafe int Allowances => QuestManager.Instance()->NumLeveAllowances;
    public static unsafe TimeSpan NextAllowances => QuestManager.GetNextLeveAllowancesDateTime() - DateTime.Now;

    public static unsafe LeveWork* GetLeveWork(uint leveId)
    {
        var leveQuests = QuestManager.Instance()->LeveQuests;

        for (var i = 0; i < leveQuests.Length; i++)
        {
            if (leveQuests[i].LeveId == (ushort)leveId)
            {
                return leveQuests.GetPointer(i);
            }
        }

        return null;
    }
    
    // Noting some things down here, or else I'll forget
    // Material Leves (Crafter + Fisher) Always return a sequence of 1 once picked up.
    // They do track as completed once they've been completed once but
    // 
    // Gathering (Min + Btn) work differently, the grab/turnin is from the same NPC
    // Grabbing it put it in a state of 1
    // Failing puts it in a state of 3, but you can also retry said mission ifyou have the allowance for it (good for re-attempting)
    // Completing it puts it in a state of 255


    public static unsafe int Leve_Sequence(uint leveId)
    {
        var leveWork = GetLeveWork((ushort)leveId);
        if (leveWork == null)
            return 0;

        return leveWork->Sequence;
    }

    public static unsafe bool Leve_IsStarted(uint leveId)
    {
        var leveWork = GetLeveWork((ushort)leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
    }
    public static unsafe bool Leve_IsComplete(uint leveID)
    {
        return QuestManager.Instance()->IsLevequestComplete((ushort)leveID);
    }
    public static bool Leve_IsAccepted(uint leveID)
    {
        return Leve_ActiveIds().Any(id => id == (ushort)leveID);
    }
    public static unsafe IEnumerable<ushort> Leve_ActiveIds()
    {
        var leveIds = new HashSet<ushort>();

        foreach (ref var entry in QuestManager.Instance()->LeveQuests)
        {
            if (entry.LeveId != 0)
                leveIds.Add(entry.LeveId);
        }

        return leveIds;
    }
}
