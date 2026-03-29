using ChilledLeves.Utilities;
using Dalamud.Interface.Utility.Raii;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text.ReadOnly;
using System.Collections.Generic;

namespace ChilledLeves.Ui.DebugTabs;

internal class Table_RawInfo
{
    private static Language _selectedLanguage = Language.English;
    private static List<Language> languageList = new()
    {
        Language.Japanese,
        Language.English,
        Language.German,
        Language.French
    };

    public static void Draw()
    {
        ImGui.SetNextItemWidth(150);
        if (ImGui.BeginCombo("Language", _selectedLanguage.ToString()))
        {
            foreach (var lang in languageList)
            {
                if (ImGui.Selectable(lang.ToString(), _selectedLanguage == lang))
                    _selectedLanguage = lang;
            }
            ImGui.EndCombo();
        }

        var sheet = Svc.Data.Excel.GetSheet<LeveGuildleveAssignment>(language: _selectedLanguage, name: "leve/GuildleveAssignment");
        if (sheet == null) return;

        using var table = ImRaii.Table("Leve_GuildleveAssignment", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY);
        if (!table) return;

        ImGui.TableSetupColumn("RowId");
        ImGui.TableSetupColumn("Text");
        ImGui.TableHeadersRow();

        foreach (var row in sheet)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"{row.RowId}");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{row.Text0}");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text($"{row.Text1}");
            if (ImGui.IsItemClicked())
            {
                ImGui.SetClipboardText($"{row.Text1}");
            }
        }
    }
}