using ChilledLeves.Scheduler;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Collections.Generic;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_GuildLeves
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<GuildLeve>("GuildLeve", out var guild) && guild.IsAddonReady)
            {
                if (ImGui.Button("Dump Tree Info"))
                {
                    DumpGuildLeveTreeList();
                }
                ImGui.SameLine();
                if (ImGui.Button("Dry Test"))
                {
                    Update_PotentionalMulti();
                    P.taskManager.Enqueue(() => CheckOtherLeves(), "Checking Multi: Debug");
                }
                ImGui.SameLine();
                if (ImGui.Button("Stop Task"))
                {
                    P.taskManager.Abort();
                }

                if (ImGui.BeginTable("Leve Details: Debugger", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Level");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Select Leve");

                    foreach (var leve in guild.Levequests)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{leve.Level}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{leve.Name}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"Select##Select_{leve.Name}"))
                        {
                            leve.Select();
                        }
                    }

                    ImGui.EndTable();
                }
            }
        }

        private unsafe static void DumpGuildLeveTreeList()
        {
            var addonPtr = Svc.GameGui.GetAddonByName("GuildLeve");
            if (addonPtr == nint.Zero) { return; }
            var addon = (AddonGuildLeve*)addonPtr.Address;
            if (addon == null)
            {
                PluginLog.Warning("[LeveDump] AddonGuildLeve not found — is the window open?");
                return;
            }

            var treeList = addon->AtkComponentTreeList228;
            if (treeList == null)
            {
                PluginLog.Warning("[LeveDump] AtkComponentTreeList228 is null");
                return;
            }

            PluginLog.Information($"[LeveDump] ItemCount={treeList->ItemCount} VisibleItemCount={treeList->VisibleItemCount}");

            for (var i = 0; i < treeList->ItemCount; i++)
            {
                var item = treeList->Items[i].Value;
                if (item == null)
                {
                    PluginLog.Information($"[LeveDump] [{i}] <null item>");
                    continue;
                }

                var uintCount = (int)item->UIntValues.Count();
                var strCount = (int)item->StringValues.Count();

                var uintsDump = string.Join(", ",
                    Enumerable.Range(0, uintCount).Select(j => item->UIntValues[j].ToString()));

                var strsDump = string.Join(" | ",
                    Enumerable.Range(0, strCount).Select(j => item->StringValues[j].ToString()));

                PluginLog.Information(
                    $"[LeveDump] [{i}] Type={item->Type} Depth={item->Depth} " +
                    $"UInts=[{uintsDump}] Strings=[{strsDump}]");
            }
        }

        private static List<uint> ValidLeves = new();
        private static int ValidAmount = 0;
        private static int LastCost = 1;
        private static List<uint> GrabbedLeves = [];

        private static void Update_PotentionalMulti()
        {
            string tag = "Check MultiLeve";

            uint currentNpcId = Player.Object.TargetObject.BaseId;
            IceLogging.Verbose($"Current Target: {currentNpcId}", tag);

            var leveList = C.LeveOrder;

            ValidLeves.Clear();
            ValidAmount = Utils.Allowances;

            foreach (var leve in leveList)
            {
                if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo))
                {
                    if (sheetInfo.Npc_Vendor != currentNpcId)
                    {
                        IceLogging.Verbose($"Leve: {leve} | Not the same npc", tag);
                        continue;
                    }

                    if (sheetInfo.AllowanceCost != LastCost)
                    {
                        IceLogging.Verbose($"Leve: {leve} | Not the same cost", tag);
                        continue;
                    }

                    bool enoughLeves = ValidAmount >= sheetInfo.AllowanceCost;

                    if (Utils.PotentionalLeve(leve, tag) && enoughLeves)
                    {
                        ValidLeves.Add(leve);
                        ValidAmount -= sheetInfo.AllowanceCost;
                    }
                }
            }
            IceLogging.Verbose($"Current Leve Count: {ValidLeves.Count()}", tag);
        }
        private static bool CheckOtherLeves()
        {
            string tag = "Debug: Check Multi Leves";

            if (C.GrabMulti)
            {
                if (ValidLeves.FirstOrDefault(x => !GrabbedLeves.Contains(x) && !Utils.Leve_IsAccepted(x)) is var multiLeve && multiLeve != 0)
                {
                    if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
                    {
                        if (multiLeve == guildLeve.SelectedLeveId)
                        {
                            GrabbedLeves.Add(multiLeve);
                                // GenericHandlers.FireCallback("JournalDetail", true, 3, (int)goalLeve);

                            return false;
                        }

                        var goalLeve = LeveInfo.Leve_SheetInfo[multiLeve];
                        var goalJob = goalLeve.Job;

                        if (EzThrottler.Throttle("Current Status of Multi Leve", 1000))
                            IceLogging.Verbose($"Currently attempting to grab leve. Goal Leve: {multiLeve} | Job: {goalJob}", tag);

                        if (!guildLeve.SelectJob(goalJob))
                        {
                            IceLogging.Verbose("We're on the wrong job tab, so going to fix that", tag);
                            return false;
                        }

                        foreach (var leve in guildLeve.Levequests)
                        {
                            var selectedLeve = LeveInfo.Leve_SheetInfo.Where(x => x.Value.LeveName == leve.Name).FirstOrNull();

                            if (selectedLeve != null)
                            {
                                var selectedJob = selectedLeve.Value.Value.Job;
                                var goalName = goalLeve.LeveName;

                                if (leve.Name == goalName)
                                {
                                    if (EzThrottler.Throttle("Leve_CorrectJob", 1000))
                                    {
                                        IceLogging.Verbose($"Selecting leve: {leve.Name}", tag);
                                        leve.Select();
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    IceLogging.Verbose("We have grabbed all potentional leves from this vendor, continuing on", tag);
                    IceLogging.ChatInfo($"Dryfire Complete. Grabbed Leves:", tag);
                    foreach (var leve in GrabbedLeves)
                    {
                        IceLogging.ChatInfo($"{leve}", tag);
                    }
                    return true;
                }
            }
            else
            {
                IceLogging.Verbose("We were told that Grab Multi-Leves was disabled, so we're going to stop", tag);
                return true;
            }

            return false;
        }
    }
}
