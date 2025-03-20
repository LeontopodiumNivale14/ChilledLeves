using ChilledLeves.Scheduler;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.Throttlers;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui;

internal class SettingsWindow : Window
{
    public SettingsWindow() :
        base($"Worklist Window {P.GetType().Assembly.GetName().Version} ###ChilledLevesWorkshopWindow")
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

#nullable disable
    public override void Draw()
    {
        ImGuiEx.EzTabBar("ChilledLeves Settings Window",
            ("Worklist Planner", MainPlanner, null, true)//,
            //("Gathering Planner", GatheringMode, null, true)
            );
    }

    #region Main Planner Window 

    private void MainPlanner()
    {
        ImGui.Text($"Amount of Accepted Leves: {GetNumAcceptedLeveQuests()}");

        ImGui.Checkbox("###ChilledLevesKeepList", ref SchedulerMain.KeepLeves);
        ImGui.SameLine();
        ImGui.Text("Keep list after completion?");
        ImGui.Checkbox("###Delay grabbing leves", ref C.IncreaseDelay);
        ImGui.SameLine();
        ImGui.Text("Increase delay between leves");
        ImGui.Checkbox("###GrabMultiLeve", ref C.GrabMulti);
        ImGui.SameLine();
        ImGui.Text("Grab multiple leve's from vendor");

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
                uint level = LeveDictionary[leveID].Level;
                uint jobID = LeveDictionary[leveID].JobAssignmentType;
                var jobIcon = LeveTypeDict[jobID].AssignmentIcon;
                string LeveName = LeveDictionary[leveID].LeveName;
                string ItemName = CraftDictionary[leveID].ItemName;
                uint itemId = CraftDictionary[leveID].ItemID;
                int itemAmount = GetItemCount((int)itemId);
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
                int amountNeeded = kdp.InputValue * CraftDictionary[leveID].TurninAmount;
                if (amountNeeded > 0)
                {
                    CenterText(amountNeeded.ToString());
                }
                else if (amountNeeded < 0)
                {
                    amountNeeded = 0;
                    CenterText(amountNeeded.ToString());
                }

                ImGui.TableNextColumn();
                int currentAmount = itemAmount;
                bool hasEnough = (currentAmount >= amountNeeded);
                if (amountNeeded != 0)
                {
                    FancyCheckmark(hasEnough);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Have: {currentAmount}");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.PopID();

            }
        }
        ImGui.EndTable();
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

    #endregion
}
