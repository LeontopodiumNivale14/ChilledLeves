using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.STD.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_SelectString
    {
        private static List<LeveKind> TestLeve = new()
        {
            LeveKind.Battlecraft,
            LeveKind.Fieldcraft,
            LeveKind.Tradecraft,
            LeveKind.LS_Battlecraft,
            LeveKind.LS_Fieldcraft,
            LeveKind.LS_Tradecraft,
            LeveKind.TurninLeve,
        };

        private static LeveKind selectedKind = LeveKind.Battlecraft;
        private static string Message = "Tehe";

        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                ImGui.SetNextItemWidth(150);
                if (ImGui.BeginCombo("Type", selectedKind.ToString()))
                {
                    foreach (var kind in TestLeve)
                    {
                        if (ImGui.Selectable(kind.ToString(), selectedKind == kind))
                            selectedKind = kind;
                    }
                    ImGui.EndCombo();
                }

                if (ImGui.Button("Find Kind"))
                {
                    SelectLeveKind(selectString, selectedKind);
                }
                ImGui.SameLine();
                ImGui.Text($"{Message}");

                foreach (var item in selectString.Entries)
                {
                    ImGui.Text($"[{item.Index}] - {item.Text}");
                    if (ImGui.IsItemClicked())
                    {
                        ImGui.SetClipboardText($"{item.Text}");
                    }
                }
            }
            else
            {
                ImGui.Text("Select String is not currently visible");
                ImGui.Text("Woopsie");
            }
        }

        private static string NormalizeForComparison(string input) => System.Text.RegularExpressions.Regex.Replace(input, @"\d+", "").Trim();

        public static void SelectLeveKind(SelectString addon, LeveKind kind)
        {
            if (!LeveInfo.Leve_SelectText.TryGetValue(kind, out var targetText))
            {
                Message = "No target text found in dictionary";
                return;
            }

            var normalizedTarget = NormalizeForComparison(targetText);

            SelectString.Entry? match = addon.Entries.Cast<AddonMaster.SelectString.Entry?>()
                    .FirstOrDefault(e => NormalizeForComparison(e!.Value.Text)
                    .Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                Message = $"No match found for {kind}";
                return;
            }

            Message = $"Match found: {match.Value.Text}";
            match.Value.Select();
        }
    }
}
