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

// This isn't currently wired up to anything. Can actually use this to place all the general settings for all the windows...
internal class SettingsWindow : Window
{
    public SettingsWindow() :
        base($"Worklist Window {P.GetType().Assembly.GetName().Version} ###ChilledLevesWorkshopWindow")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(2000, 2000),
        };
        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose() { }

#nullable disable
    public override void Draw()
    {
        // Check for Ice theme
        bool usingIceTheme = C.UseIceTheme;
        
        // Begin theming this window only
        int styleCount = ThemeHelper.BeginTheming(usingIceTheme);
        
        ImGuiEx.EzTabBar("ChilledLeves Settings Window",
            ("Worklist Planner", MainPlanner, null, true)//,
            //("Gathering Planner", GatheringMode, null, true)
            );
        
        // End theming
        ThemeHelper.EndTheming(usingIceTheme, styleCount);
    }

    #region Main Planner Window 

    private void MainPlanner()
    {
        // Get font scaling metrics
        float textLineHeight = ImGui.GetTextLineHeight();
        float fontScale = ImGui.GetIO().FontGlobalScale;
        float scaledSpacing = ImGui.GetStyle().ItemSpacing.Y * fontScale;
        
        // Get theme setting (but don't re-apply window styles - those are set by Draw method)
        bool usingIceTheme = C.UseIceTheme;
        
        // Add navigation buttons to other windows at the top
        if (usingIceTheme)
        {
            int navigationBtnStyleCount = ThemeHelper.PushButtonStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);
            
            float navButtonHeight = textLineHeight * 1.5f;
            float btnPadding = 8 * fontScale;
            float mainBtnWidth = ImGui.CalcTextSize("Main Window").X + btnPadding * 2;
            float worklistBtnWidth = ImGui.CalcTextSize("Worklist Window").X + btnPadding * 2;
            float gatherBtnWidth = ImGui.CalcTextSize("Priority Leve Grind Window").X + btnPadding * 2;
            
            // Left-aligned buttons
            ImGui.BeginGroup();
            if (ImGui.Button("Main Window", new Vector2(mainBtnWidth, navButtonHeight)))
            {
                P.mainWindow.IsOpen = true;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Worklist Window", new Vector2(worklistBtnWidth, navButtonHeight)))
            {
                P.workListUi.IsOpen = true;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Priority Leve Grind Window", new Vector2(gatherBtnWidth, navButtonHeight)))
            {
                P.gatherModeUi.IsOpen = true;
            }
            ImGui.EndGroup();
            
            // Right-aligned allowances info
            float windowWidth = ImGui.GetWindowWidth();
            string allowancesInfo = $"Allowances: {Allowances}/100 | Next in: {NextAllowances:hh\\:mm\\:ss}";
            float infoWidth = ImGui.CalcTextSize(allowancesInfo).X;
            
            ImGui.SameLine(windowWidth - infoWidth - btnPadding);
            
            // Add ice theme styling for the text
            int textStyleCount = ThemeHelper.PushHeadingTextStyle();
            ImGui.Text(allowancesInfo);
            ImGui.PopStyleColor(textStyleCount);
            
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(navigationBtnStyleCount);
        }
        else
        {
            float navButtonHeight = textLineHeight * 1.5f;
            float btnPadding = 8 * fontScale;
            float mainBtnWidth = ImGui.CalcTextSize("Main Window").X + btnPadding * 2;
            float worklistBtnWidth = ImGui.CalcTextSize("Worklist Window").X + btnPadding * 2;
            float gatherBtnWidth = ImGui.CalcTextSize("Priority Leve Grind Window").X + btnPadding * 2;
            
            // Left-aligned buttons
            ImGui.BeginGroup();
            if (ImGui.Button("Main Window", new Vector2(mainBtnWidth, navButtonHeight)))
            {
                P.mainWindow.IsOpen = true;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Worklist Window", new Vector2(worklistBtnWidth, navButtonHeight)))
            {
                P.workListUi.IsOpen = true;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Priority Leve Grind Window", new Vector2(gatherBtnWidth, navButtonHeight)))
            {
                P.gatherModeUi.IsOpen = true;
            }
            ImGui.EndGroup();
            
            // Right-aligned allowances info
            float windowWidth = ImGui.GetWindowWidth();
            string allowancesInfo = $"Allowances: {Allowances}/100 | Next in: {NextAllowances:hh\\:mm\\:ss}";
            float infoWidth = ImGui.CalcTextSize(allowancesInfo).X;
            
            ImGui.SameLine(windowWidth - infoWidth - btnPadding);
            ImGui.Text(allowancesInfo);
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text($"Amount of Accepted Leves: {GetNumAcceptedLeveQuests()}");

        // Apply control styling for checkboxes
        if (usingIceTheme)
        {
            int styleCount = ThemeHelper.PushControlStyle();
            ImGui.Checkbox("###ChilledLevesKeepList", ref SchedulerMain.KeepLeves);
            ImGui.PopStyleColor(styleCount);
        }
        else
        {
            ImGui.Checkbox("###ChilledLevesKeepList", ref SchedulerMain.KeepLeves);
        }
        
        ImGui.SameLine();
        ImGui.Text("Keep list after completion?");
        
        if (usingIceTheme)
        {
            int styleCount = ThemeHelper.PushControlStyle();
            ImGui.Checkbox("###Delay grabbing leves", ref C.IncreaseDelay);
            ImGui.PopStyleColor(styleCount);
        }
        else
        {
            ImGui.Checkbox("###Delay grabbing leves", ref C.IncreaseDelay);
        }
        
        ImGui.SameLine();
        ImGui.Text("Increase delay between leves");
        
        if (usingIceTheme)
        {
            int styleCount = ThemeHelper.PushControlStyle();
            ImGui.Checkbox("###GrabMultiLeve", ref C.GrabMulti);
            ImGui.PopStyleColor(styleCount);
        }
        else
        {
            ImGui.Checkbox("###GrabMultiLeve", ref C.GrabMulti);
        }
        
        ImGui.SameLine();
        ImGui.Text("Grab multiple leve's from vendor");

        // Begin the worklist table with themed headers
        if (usingIceTheme)
        {
            // Apply child background
            int childStyleCount = ThemeHelper.PushChildStyle();
            
            // Apply table headers styling
            ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue);
            int headerStyleCount = ThemeHelper.PushHeaderStyle();
            
            if (ImGui.BeginTable($"Crafting Workshop List", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                // Columns for the crafters - sizes scaled according to font size
                float levelColWidth = Math.Max(50, textLineHeight * 3);
                float nameColWidth = Math.Max(200, textLineHeight * 12);
                float runColWidth = Math.Max(100, textLineHeight * 6);
                float itemColWidth = Math.Max(100, textLineHeight * 6);
                float needColWidth = Math.Max(50, textLineHeight * 3);
                float haveColWidth = Math.Max(100, textLineHeight * 6);
                
                ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, levelColWidth);
                ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, nameColWidth);
                ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, runColWidth);
                ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, itemColWidth);
                ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, needColWidth);
                ImGui.TableSetupColumn("Have?###CrafterCompleteCheck", ImGuiTableColumnFlags.WidthFixed, haveColWidth);

                ImGui.TableHeadersRow();
                
                // Pop the styling after the headers are drawn
                ImGui.PopStyleColor(headerStyleCount + 1); // +1 for TableHeaderBg
                
                // Draw the table rows
                DrawWorklistRows(textLineHeight, fontScale, usingIceTheme);
                
                ImGui.EndTable();
            }
            
            // Pop child background
            ImGui.PopStyleColor(childStyleCount);
        }
        else
        {
            if (ImGui.BeginTable($"Crafting Workshop List", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                // Columns for the crafters - sizes scaled according to font size
                float levelColWidth = Math.Max(50, textLineHeight * 3);
                float nameColWidth = Math.Max(200, textLineHeight * 12);
                float runColWidth = Math.Max(100, textLineHeight * 6);
                float itemColWidth = Math.Max(100, textLineHeight * 6);
                float needColWidth = Math.Max(50, textLineHeight * 3);
                float haveColWidth = Math.Max(100, textLineHeight * 6);
                
                ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, levelColWidth);
                ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, nameColWidth);
                ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, runColWidth);
                ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, itemColWidth);
                ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, needColWidth);
                ImGui.TableSetupColumn("Have?###CrafterCompleteCheck", ImGuiTableColumnFlags.WidthFixed, haveColWidth);

                ImGui.TableHeadersRow();
                
                // Draw the table rows
                DrawWorklistRows(textLineHeight, fontScale, usingIceTheme);
                
                ImGui.EndTable();
            }
        }
    }
    
    private void DrawWorklistRows(float textLineHeight, float fontScale, bool usingIceTheme)
    {
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
            // Scale icon size based on text line height
            float iconSize = Math.Max(25, textLineHeight * 1.5f);
            ImGui.Image(jobIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(iconSize, iconSize));
            ImGui.SameLine(0, 5 * fontScale); // Scale spacing
            ImGui.AlignTextToFramePadding();
            CenterTextInHeight($"{LeveName}");

            ImGui.TableNextColumn();
            var input = kdp.InputValue > 0 ? kdp.InputValue.ToString() : "0";
            
            // Apply control styling for input field
            if (usingIceTheme)
            {
                int controlStyleCount = ThemeHelper.PushControlStyle();
                CenterInputTextInHeight("###Level", ref input, 3);
                ImGui.PopStyleColor(controlStyleCount);
            }
            else
            {
                CenterInputTextInHeight("###Level", ref input, 3);
            }
            
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
        float fontScale = ImGui.GetIO().FontGlobalScale;

        float cursorY = ImGui.GetCursorPosY() + (rowHeight - inputHeight) * 0.5f;
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY()); // Prevent negative padding

        ImGui.SetCursorPosY(cursorY);
        // Scale the input field width based on font scale
        ImGui.SetNextItemWidth(75 * fontScale);
        ImGui.InputText(label, ref input, maxLength);
    }

    #endregion
}
