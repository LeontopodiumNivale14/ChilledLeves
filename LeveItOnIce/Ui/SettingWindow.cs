using Dalamud.Interface.Colors;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui;

internal class SettingWindow : Window
{
    public SettingWindow() :
        base($"Workshop Window {P.GetType().Assembly.GetName().Version} ###ChilledLevesWorkshopWindow")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(2000, 2000),
        };
        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text($"Amount of Accepted Leves: {GetNumAcceptedLeveQuests()}");

        if (ImGui.BeginTable($"Crafting Workshop List", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            // Columns for the crafters
            ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Have?###CrafterCompleteCheck", ImGuiTableColumnFlags.WidthFixed, 100);

            // Custom header row
            ImGui.TableHeadersRow();

            foreach (var kdp in C.workList)
            {
                uint leveID = kdp.LeveID;
                uint level = CrafterLeves[leveID].Level;
                uint jobID = CrafterLeves[leveID].JobAssignmentType;
                var jobIcon = LeveTypeDict[jobID].AssignmentIcon;
                string LeveName = CrafterLeves[leveID].LeveName;
                string ItemName = CrafterLeves[leveID].ItemName;
                uint itemId = CrafterLeves[leveID].ItemID;
                ImGui.TableNextRow();

                ImGui.PushID((int)leveID);
                ImGui.TableSetColumnIndex(0);
                CenterText($"{level}");

                ImGui.TableNextColumn();
                ImGui.Image(jobIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(25, 25));
                ImGui.SameLine(0, 5);
                ImGui.AlignTextToFramePadding();
                CenterTextInHeight($"{LeveName}");

                ImGui.TableNextColumn();
                var input = kdp.InputValue > 0 ? kdp.InputValue.ToString() : "0";
                CenterInputTextInHeight("###Level", ref input, 3);
                if (uint.TryParse(input, out var num) && num > 0 && num <= 100)
                {
                    kdp.InputValue = (int)num;
                    C.Save();
                }

                ImGui.TableNextColumn();
                CenterTextInHeight(ItemName);

                ImGui.TableNextColumn();
                int amountNeeded = kdp.InputValue * CrafterLeves[leveID].TurninAmount;
                CenterText(amountNeeded.ToString());

                ImGui.TableNextColumn();
                int currentAmount = GetItemCount((int)itemId);
                bool hasEnough = (currentAmount >= amountNeeded);
                FancyCheckmark(hasEnough);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"Have: {currentAmount}");
                    ImGui.EndTooltip();
                }
                

                ImGui.PopID();

            }
        }
        ImGui.EndTable();
    }

    private static void RemoveLeve(uint leve)
    {
        ImGui.PushID((int)leve);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"###RemoveLeve", new Vector2(15, 15)))
        {
            C.workList.RemoveAll(e => e.LeveID == leve);
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.PopID();
    }

    private static void CenterText(string text)
    {
        float columnWidth = ImGui.GetColumnWidth();  // Get the width of the current column
        float textWidth = ImGui.CalcTextSize(text).X;

        float rowHeight = ImGui.GetTextLineHeightWithSpacing(); // Approximate row height
        float textHeight = ImGui.CalcTextSize(text).Y;

        float cursorX = ImGui.GetCursorPosX() + (columnWidth - textWidth) * 0.5f;
        float cursorY = ImGui.GetCursorPosY() + (rowHeight - textHeight) * 0.5f;

        cursorX = Math.Max(cursorX, ImGui.GetCursorPosX()); // Prevent negative padding
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY());

        ImGui.SetCursorPos(new Vector2(cursorX, cursorY));
        ImGui.Text(text);
    }

    private static void CenterTextInHeight(string text)
    {
        float rowHeight = ImGui.GetTextLineHeightWithSpacing(); // Approximate row height
        float textHeight = ImGui.CalcTextSize(text).Y;

        float cursorY = ImGui.GetCursorPosY() + (rowHeight - textHeight) * 0.5f;
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY()); // Prevent negative padding

        ImGui.SetCursorPosY(cursorY);
        ImGui.Text(text);
    }

    private static void CenterInputTextInHeight(string label, ref string input, uint maxLength)
    {
        float rowHeight = ImGui.GetTextLineHeightWithSpacing(); // Approximate row height
        float inputHeight = ImGui.GetFrameHeight(); // Input field height

        float cursorY = ImGui.GetCursorPosY() + (rowHeight - inputHeight) * 0.5f;
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY()); // Prevent negative padding

        ImGui.SetCursorPosY(cursorY);
        ImGui.SetNextItemWidth(75);
        ImGui.InputText(label, ref input, maxLength);
    }

    public static void FancyCheckmark(bool enabled)
    {
        /*
        float rowHeight = ImGui.GetTextLineHeightWithSpacing();  // Get the row height
        float iconHeight = ImGui.CalcTextSize($"{FontAwesome.Cross}").Y;      // Get the icon's text height
        float padding = (rowHeight - iconHeight) * 0.5f;

        padding = Math.Max(padding, 0); // Prevent negative padding
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + padding);
        */

        float columnWidth = ImGui.GetColumnWidth();  // Get column width
        float rowHeight = ImGui.GetTextLineHeightWithSpacing();  // Get row height

        Vector2 iconSize = ImGui.CalcTextSize($"{FontAwesome.Cross}"); // Get icon size
        float iconWidth = iconSize.X;
        float iconHeight = iconSize.Y;

        float cursorX = ImGui.GetCursorPosX() + (columnWidth - iconWidth) * 0.5f;
        float cursorY = ImGui.GetCursorPosY() + (rowHeight - iconHeight) * 0.5f;

        cursorX = Math.Max(cursorX, ImGui.GetCursorPosX()); // Prevent negative padding
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY());

        ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

        if (!enabled)
        {
            FontAwesome.Print(ImGuiColors.DalamudRed, FontAwesome.Cross);
        }
        else if (enabled)
        {
            FontAwesome.Print(ImGuiColors.HealerGreen, FontAwesome.Check);
        }
    }

}
