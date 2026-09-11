using ChilledLeves.Config_Files;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;
using System.Data;

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

    public static unsafe int NumAcceptedQuest()
    {
        return QuestManager.Instance()->NumAcceptedLeveQuests;
    }

    public static unsafe bool Leve_MaxAccepted()
    {
        return QuestManager.Instance()->NumAcceptedLeveQuests == 16;
    }

    public static bool PotentionalLeve(uint leveId, string tag)
    {
        if (LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var sheetInfo))
        {
            bool properLv = Player.GetLevel(sheetInfo.Job) >= sheetInfo.Level;

            if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
            {
                var materialInfo = sheetInfo.MaterialInfo;
                var neededAmount = Utils.Leve_RequiredAmount(materialInfo);

                bool enoughItems = Utils.GetItemCount(materialInfo.Item_Id) >= neededAmount;

                if (!properLv || !enoughItems)
                {
                    IceLogging.Verbose($"Skipping Crafting Leve: [{leveId}] due to: Proper Level? [{properLv}] | Not enough items? {enoughItems}", tag);
                }

                return enoughItems && properLv;
            }
            else if (LeveInfo.LeveJobs_Gathering.Contains(sheetInfo.Job))
            {
                if (!properLv)
                {
                    IceLogging.Verbose($"Skipping Gathering Leve: [{leveId}] due to not high enough lv", tag);
                }
                return properLv;
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    public static bool EnoughAllowance(uint leveId)
    {
        if (LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var sheetInfo))
            return sheetInfo.AllowanceCost >= Utils.Allowances;
        else
            return false;
    }

    public static int EstimateCurrentAllowance(Config.ClassInformation info)
    {
        const int cap = 100;
        const int allowancesPerTick = 3;
        const int regenIntervalHours = 12;

        if (info.Time_LastObserved == DateTime.MinValue)
            return 0;

        if (info.LastKnownAllowance >= cap)
            return cap;

        DateTime serverTime = Svc.Framework.LastUpdateUTC;

        if (serverTime <= info.Time_LastObserved)
            return info.LastKnownAllowance;

        if (serverTime < info.Time_NextTickAt)
            return info.LastKnownAllowance; // hasn't hit the next tick yet

        var hoursSinceNextTick = (serverTime - info.Time_NextTickAt).TotalHours;
        var ticks = (int)(hoursSinceNextTick / regenIntervalHours) + 1;

        return Math.Min(cap, info.LastKnownAllowance + ticks * allowancesPerTick);
    }

    public static void OnLeveDataReady(ulong characterId)
    {
        const string tag = "User Info: Update Leve";

        int currentAllowance = Allowances;
        var nextTick = DateTimeOffset
            .FromUnixTimeSeconds(QuestManager.GetNextLeveAllowancesUnixTimestamp())
            .UtcDateTime;

        if (C.CharacterInfo.TryGetValue(characterId, out var info))
        {
            bool changed = info.LastKnownAllowance != currentAllowance;

            info.LastKnownAllowance = currentAllowance;
            info.Time_LastObserved = Svc.Framework.LastUpdateUTC;
            info.Time_NextTickAt = nextTick;

            if (changed)
            {
                IceLogging.Info($"Updated Character Info for: {info.Name}", tag);
                C.SaveDebounced();
            }
            else if (EzThrottler.Throttle($"Updating last seen {characterId}", 300000))
            {
                IceLogging.Verbose("Updating the last seen so it's atleast known", tag);
                C.SaveDebounced();
            }
        }
        else
        {
            C.CharacterInfo[characterId] = new()
            {
                Name = Player.Name,
                World = Player.HomeWorld.Value.Name.ToString(),
                AllowNotification = false,
                LastKnownAllowance = currentAllowance,
                Time_LastObserved = Svc.Framework.LastUpdateUTC,
                Time_NextTickAt = nextTick
            };

            C.SaveDebounced();
        }
    }
}
