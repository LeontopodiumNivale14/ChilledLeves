using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Collections.Generic;

namespace ChilledLeves.Ui.DebugTabs;

internal static unsafe class Ui_LeveInfo
{
    public static void Draw()
    {
        using (var child = ImRaii.Child("Leve List Info", default, true))
        {

            if (ImGui.Button("Check Handlers"))
            {
                LeveInfo.DebugDumpHandlerState();
            }

            var handler = LeveInfo.GetHandler();
            if (handler == null)
            {
                ImGui.TextDisabled("No active GuildleveAssignmentEventHandler found.");
                return;
            }

            ImGui.Text($"Current type: {handler->CurrentLeveType}");
            ImGui.Text($"Selected leve id: {handler->SelectedLeveId}");

            if (Svc.Targets.Target != null)
            {
                var target = Svc.Targets.Target;
                if (ImGui.Button($"Name: {target.Name} | ID: {target.BaseId}"))
                {
                    ImGui.SetClipboardText($"{target.BaseId}");
                }
            }
            ImGui.Separator();

            DrawLeveTable(handler);
        }
    }

    private static void DrawLeveTable(GuildleveAssignmentEventHandler* handler)
    {
        var leves = LeveInfo.GetVisibleLeves(handler);

        if (leves.Count() != 0)
        {
            var target = Svc.Targets.Target;
            if (target != null)
            {
                if (GetNewLeves(leves, target.BaseId).Count() != 0)
                {
                    if (ImGui.Button("Copy missing ID's only"))
                    {
                        ImGui.SetClipboardText(AddMissingLeves(leves));
                    }
                    ImGui.SameLine();
                }

                if (ImGui.Button("Copy all leves [old + new]"))
                {
                    var list = GetAllLevesSorted(leves);
                    string newList = string.Join(", ", list);

                    ImGui.SetClipboardText(newList);
                }
            }
        }

        using (var table = ImRaii.Table("LeveTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY))
        {
            if (!table.Success)
                return;

            ImGui.TableSetupColumn("Leve ID");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Level");
            ImGui.TableSetupColumn("Genre Icon");
            ImGui.TableSetupColumn("Select");

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            foreach (var leve in leves)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(leve.LeveId.ToString());

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(leve.Name.ToString());

                ImGui.TableNextColumn();
                ImGui.Text(leve.ClassJobLevel.ToString());

                ImGui.TableNextColumn();
                GameIcons.DrawInlineOrIcon(leve.GenreIcon, FontAwesomeIcon.Book);

                ImGui.TableNextColumn();
                if (ImGui.Button($"Select##{leve.LeveId}"))
                {
                    if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
                    {
                        guildLeve.SelectProperLeve(guildLeve, leve.LeveId);
                    }
                }
            }
        }
    }

    private static List<uint> GetNewLeves(List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> leves, uint baseId)
    {
        List<uint> newLeves = new();

        if (!LeveInfo.Levemete_Info.TryGetValue(baseId, out var vendorInfo))
            return newLeves;

        foreach (var leve in leves)
        {
            if (!vendorInfo.Leves.Contains(leve.LeveId))
                newLeves.Add(leve.LeveId);
        }

        return newLeves;
    }

    private static string AddMissingLeves(List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> leves)
    {
        var baseId = Svc.Targets.Target.BaseId;
        var newLeves = GetNewLeves(leves, baseId);

        newLeves.Sort();

        return string.Join(", ", newLeves);
    }

    private static List<uint> GetAllLevesSorted(List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> leves)
    {
        var baseId = Svc.Targets.Target.BaseId;

        if (!LeveInfo.Levemete_Info.TryGetValue(baseId, out var vendorInfo))
            return GetNewLeves(leves, baseId);

        List<uint> allLeves = new(vendorInfo.Leves);
        allLeves.AddRange(GetNewLeves(leves, baseId));

        allLeves.Sort();

        return allLeves;
    }
}