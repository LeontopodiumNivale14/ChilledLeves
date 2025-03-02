using ChilledLeves.Scheduler;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
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
            ("Worklist Planner", MainPlanner, null, true),
            ("Gathering Planner", GatheringMode, null, true)
            );
    }

    #region Main Planner Window 

    private void MainPlanner()
    {
        ImGui.Text($"Amount of Accepted Leves: {GetNumAcceptedLeveQuests()}");

        ImGui.Text("Keep list after finishing list?");
        ImGui.SameLine(0, 10);
        ImGui.Checkbox("###ChilledLevesKeepList", ref SchedulerMain.KeepLeves);

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

        ImGui.NewLine();

        if (ListCycled.Count > 0)
        {
            if (ImGui.BeginTable($"Completed Leves:", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders, new Vector2(350, 50)))
            {
                // Columns for the crafters
                ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Leve Completed###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, 300);

                // Custom header row
                ImGui.TableHeadersRow();

                foreach (var kdp in ListCycled)
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


                    ImGui.PopID();

                }
            }
            ImGui.EndTable();
        }
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

    #endregion

    #region Gathering Specific Planner

    private List<uint> npcIds = CrafterLeves.Values
        .Select(x => x.LeveVendorID)
        .Distinct()
        .ToList();

    private uint selectedNpcId = 1000970;
    private string selectedNPCName = NPCName(1000970);
    private string LocationName = ZoneName(128);

    private HashSet<int> GatheringClasses = new HashSet<int>() { 2, 3, 4 };
    private List<string> classSelectList = new List<string>() { LeveTypeDict[2].LeveClassType, LeveTypeDict[3].LeveClassType, LeveTypeDict[4].LeveClassType };
    private List<string> RunUntilList = new List<string>() { "Lv.", "All Leves Complete" };

    private string ClassSelected = LeveTypeDict[4].LeveClassType;
    private string RunUntilSelected = "Lv.";
    private int SliderInput = 1;

    void CenterTextV2(string text)
    {
        float columnWidth = ImGui.GetColumnWidth();
        float textWidth = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - textWidth) * 0.5f);
        ImGui.Text(text);
    }


    private void GatheringMode()
    {
        if (ImGui.BeginTable("Gathering Leve Settings", 2, ImGuiTableFlags.Borders))
        {
            float tableColumn1Width = 0f;

            ImGui.TableSetupColumn("Text Information", ImGuiTableColumnFlags.WidthFixed, tableColumn1Width);
            ImGui.TableSetupColumn("Setting Selection", ImGuiTableColumnFlags.WidthFixed, 500);

            ImGui.TableNextRow();

            // Row 1
            ImGui.TableSetColumnIndex(0);
            string Option1Text = "Levement NPC:";
            float Option1Width = ImGui.CalcTextSize(Option1Text).X;
            ImGui.Text(Option1Text);
            if (Option1Width > tableColumn1Width)
                tableColumn1Width = Option1Width;

            ImGui.TableNextColumn();
            string selectableText = $"{selectedNPCName} → {LocationName}";
            if (ImGui.Selectable(selectableText))
            {
                ImGui.OpenPopup("NPC Selection Popup"); // Open the popup when the selectable is clicked
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Left click to select Leve NPC");
                ImGui.EndTooltip();
            }

            // Create the popup
            if (ImGui.BeginPopup("NPC Selection Popup"))
            {
                SelectedLeves.Clear();
                // Create a table inside the popup
                ImGui.BeginTable("NPC Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);

                // Define column widths
                ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableHeadersRow();

                // Loop through each NPC ID and display their name and location in the table
                foreach (var npcId in npcIds)
                {
                    string npcName = NPCName(npcId); // Get NPC name
                    string npcLocation = ZoneName(LeveNPCDict[npcId].ZoneID); // Get NPC location

                    // Create the row
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    if (ImGui.Selectable(npcName, npcId == selectedNpcId)) // Check if it's the selected item
                    {
                        selectedNpcId = npcId; // Update selected NPC ID
                        selectedNPCName = npcName; // Update selected NPC Name
                        LocationName = npcLocation;
                        ImGui.CloseCurrentPopup(); // Close the popup after selection
                    }

                    ImGui.TableSetColumnIndex(1);

                    ImGui.Text(npcLocation); // Display NPC location in the second column
                }

                ImGui.EndTable(); // End the table

                ImGui.EndPopup(); // End the popup
            }

            ImGui.TableNextRow();
            //Row 2
            ImGui.TableSetColumnIndex(0);
            string Option2Text = "Class";
            float Option2Width = ImGui.CalcTextSize(Option2Text).X;
            ImGui.Text("Class");
            if (Option2Width > tableColumn1Width)
                tableColumn1Width = Option2Width;

            ImGui.TableNextColumn();
            float combo2MaxWidth = 0f;
            foreach (string Class in classSelectList)
            {
                float textWidth2 = ImGui.CalcTextSize(Class).X;
                if (textWidth2 > combo2MaxWidth)
                    combo2MaxWidth = textWidth2;
            }
            combo2MaxWidth += 35;
            ImGui.SetNextItemWidth(combo2MaxWidth);
            if (!classSelectList.Contains(ClassSelected))
            {
                ClassSelected = classSelectList.FirstOrDefault();
            }
            if (ImGui.BeginCombo("###Class Selection", ClassSelected))
            {
                for (int i = 0; i < classSelectList.Count; i++)
                {
                    int iconSlot = i + 2;
                    bool isSelected = (classSelectList[i] == ClassSelected);

                    ImGui.PushID(iconSlot);

                    if (ImGui.Selectable("###hidden", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedLeves.Clear();
                        ClassSelected = classSelectList[i];
                        IconSlot = (uint)iconSlot;
                    }

                    ImGui.SameLine();

                    ImGui.Image(LeveTypeDict[(uint)iconSlot].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(20, 20));

                    ImGui.SameLine();
                    ImGui.TextUnformatted(classSelectList[i]);

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();

                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            ImGui.Image(LeveTypeDict[IconSlot].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(20, 20));

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            string Option3Text = "Run Until: ";
            float Option3Width = ImGui.CalcTextSize(Option3Text).X;
            ImGui.Text(Option3Text);
            if (Option3Width > tableColumn1Width)
                tableColumn1Width = Option3Width;

            ImGui.TableNextColumn();
            float combo3MaxWidth = 0f;
            foreach (string RunUntil in RunUntilList)
            {
                float textWidth3 = ImGui.CalcTextSize(RunUntil).X;
                if (textWidth3 > combo3MaxWidth)
                {
                    combo3MaxWidth = textWidth3;
                }
            }

            combo3MaxWidth += 35;
            ImGui.SetNextItemWidth(combo3MaxWidth);
            if (ImGui.BeginCombo("###Run Until Combo", RunUntilSelected))
            {
                foreach (var Selected in RunUntilList)
                {
                    bool isSelected = (RunUntilSelected == Selected);
                    if (ImGui.Selectable(Selected, isSelected))
                    {
                        RunUntilSelected = Selected;
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            if (RunUntilSelected == RunUntilList[0])
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderInt("###Lv Slider", ref SliderInput, 1, 100))
                {
                    if (SliderInput > 100)
                        SliderInput = 100;
                    else if (SliderInput < 1)
                        SliderInput = 1;
                }
            }

            ImGui.EndTable();

            
        }

        ImGui.Text("List of Leves:");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            if (SelectedLeves.Count > 0)
            {
                foreach (var leve in SelectedLeves)
                {
                    ImGui.Text($"leve: {leve} | Prio: {CrafterLeves[leve].Priority}");
                }
            }
            else
            {
                ImGui.Text("No possible leves");
            }
            ImGui.EndTooltip();
        }

        using (ImRaii.Disabled(SchedulerMain.AreWeTicking))
        {
            if (ImGui.Button("Start Gathering Mode"))
            {
                SchedulerMain.GatheringMode = true;
                SchedulerMain.EnablePlugin();
            }
        }
        using (ImRaii.Disabled(!SchedulerMain.AreWeTicking))
        {
            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                SchedulerMain.DisablePlugin();
            }
        }

        if (ImGui.BeginTable($"Crafting Workshop List", 7, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            // Columns for the crafters
            ImGui.TableSetupColumn("Prio###GatheringLevePrio", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Complete?###GatheringLeveComplete", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Level###GatheringLeveLevel", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Leve Name###GatheringLeveName", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Item Turnin###GatheringTurninItems", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Need###GatheringAmountNecessary", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Have###GatheringCompleteCheck", ImGuiTableColumnFlags.WidthFixed, 100);

            // Custom header row
            ImGui.TableHeadersRow();

            foreach (var kdp in CrafterLeves)
            {
                var leveID = kdp.Key;
                var leveLevel = kdp.Value.Level;
                var leveName = kdp.Value.LeveName;
                var leveVendorId = kdp.Value.LeveVendorID;
                var leveVendorName = kdp.Value.LeveVendorName;
                var jobAssignment = kdp.Value.JobAssignmentType;
                var zoneId = LeveNPCDict[leveVendorId].ZoneID;
                var priority = kdp.Value.Priority;

                var ItemImage = kdp.Value.ItemIcon.GetWrapOrEmpty();
                var itemName = kdp.Value.ItemName;
                var itemNeed = kdp.Value.TurninAmount;
                var itemId = kdp.Value.ItemID;

                if (leveVendorId != selectedNpcId || jobAssignment != IconSlot)
                {
                    continue;
                }
                else
                {
                    if (!SelectedLeves.Contains(leveID))
                    {
                        SelectedLeves.Add(leveID);
                    }
                }

                ImGui.TableNextRow();

                ImGui.PushID((int)leveID);
                ImGui.TableSetColumnIndex(0);
                if (ImGui.DragInt("###GatheringPriority", ref priority))
                {
                    kdp.Value.Priority = priority;
                }

                ImGui.TableNextColumn();
                bool leveCompleted = IsComplete(leveID);
                FancyCheckmark(leveCompleted);
                // Need to add a checkmark here, to lazy at the second
                

                ImGui.TableNextColumn();
                CenterTextV2($"{leveLevel}");

                ImGui.TableNextColumn();
                ImGui.Text($"{leveName}");

                ImGui.TableNextColumn();
                ImGui.Image(ItemImage.ImGuiHandle, new Vector2(20, 20));
                ImGui.SameLine(0, 5);
                ImGui.Text($"{itemName}");

                ImGui.TableNextColumn();
                CenterTextV2($"{itemNeed}");

                ImGui.TableNextColumn();
                var itemHave = GetItemCount((int)kdp.Value.ItemID);
                CenterTextV2($"{itemHave}");
            }

            ImGui.EndTable();
        }

    }

    #endregion
}
