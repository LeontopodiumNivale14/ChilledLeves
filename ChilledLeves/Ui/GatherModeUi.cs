using ChilledLeves.Scheduler;
using ChilledLeves.Utilities;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui;

internal class GatherModeUi : Window
{
    public GatherModeUi() :
        base($"Gather Grind Mode Window {P.GetType().Assembly.GetName().Version} ###GatherModeWorkshopWindow")
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

    public override void Draw()
    {
        // Check for Ice theme
        bool usingIceTheme = C.UseIceTheme;
        
        // Begin theming with the improved style boundary
        ThemeHelper.BeginTheming(usingIceTheme);
        
        GatheringMode();
        
        // End theming properly
        ThemeHelper.EndTheming(usingIceTheme);
    }

    #region Gathering Specific Planner

    private static List<uint> npcIds = LeveDictionary.Values
        .Select(x => x.LeveVendorID)
        .Distinct()
        .ToList();

    private static HashSet<int> GatheringClasses = new HashSet<int>() { 2, 3, 4 };
    private static List<string> classSelectList = new List<string>() { LeveTypeDict[2].LeveClassType, LeveTypeDict[3].LeveClassType, LeveTypeDict[4].LeveClassType };
    private static List<string> RunUntilList = new List<string>() { "Lv.", "All Leves Complete" };

    private static void CenterTextV2(string text)
    {
        float columnWidth = ImGui.GetColumnWidth();
        float textWidth = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - textWidth) * 0.5f);
        ImGui.Text(text);
    }

    private static uint leveId = 0;
    private static void GatheringMode()
    {
        // Get font scaling metrics
        float textLineHeight = ImGui.GetTextLineHeight();
        float fontScale = ImGui.GetIO().FontGlobalScale;
        float scaledSpacing = ImGui.GetStyle().ItemSpacing.Y * fontScale;
        
        // Get theme setting (but don't set window styles - Draw method handles that)
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
        
        // Apply table styling if using ice theme
        if (usingIceTheme)
        {
            int childStyleCount = ThemeHelper.PushChildStyle();
            ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue);
            int headerStyleCount = ThemeHelper.PushHeaderStyle();
            
            if (ImGui.BeginTable("Gathering Leve Settings", 2, ImGuiTableFlags.Borders))
            {
                // Use dynamic column width based on font size
                float tableColumn1Width = Math.Max(150, textLineHeight * 10);
                float tableColumn2Width = Math.Max(500, textLineHeight * 30);

                ImGui.TableSetupColumn("Text Information", ImGuiTableColumnFlags.WidthFixed, tableColumn1Width);
                ImGui.TableSetupColumn("Setting Selection", ImGuiTableColumnFlags.WidthFixed, tableColumn2Width);
                
                ImGui.TableHeadersRow();
                
                // Pop style after headers are drawn
                ImGui.PopStyleColor(headerStyleCount + 1); // +1 for TableHeaderBg
        
                // Now continue with the table content
                DrawGatheringTableRows(textLineHeight, fontScale, usingIceTheme);
                
                ImGui.EndTable();
            }
            
            // Pop child background style
            ImGui.PopStyleColor(childStyleCount);
        }
        else
        {
            if (ImGui.BeginTable("Gathering Leve Settings", 2, ImGuiTableFlags.Borders))
            {
                // Use dynamic column width based on font size
                float tableColumn1Width = Math.Max(150, textLineHeight * 10);
                float tableColumn2Width = Math.Max(500, textLineHeight * 30);

                ImGui.TableSetupColumn("Text Information", ImGuiTableColumnFlags.WidthFixed, tableColumn1Width);
                ImGui.TableSetupColumn("Setting Selection", ImGuiTableColumnFlags.WidthFixed, tableColumn2Width);
                
                ImGui.TableHeadersRow();
                
                // Table content
                DrawGatheringTableRows(textLineHeight, fontScale, usingIceTheme);
                
                ImGui.EndTable();
            }
        }
        
        // The buttons section below the table
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Action Buttons
        float buttonHeight = textLineHeight * 1.5f;
        
        if (usingIceTheme)
        {
            int btnStyleCount = ThemeHelper.PushButtonStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);
            
            DrawGatheringButtons(buttonHeight);
            
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(btnStyleCount);
        }
        else
        {
            DrawGatheringButtons(buttonHeight);
        }
        
        // If leves are selected, draw the leve list
        if (SelectedLeves.Count > 0)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            if (usingIceTheme)
            {
                int textStyleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text("Selected Leves:");
                ImGui.PopStyleColor(textStyleCount);
            }
            else
            {
                ImGui.Text("Selected Leves:");
            }
            
            // Draw the selected leves table
            DrawSelectedLeves(textLineHeight, fontScale, usingIceTheme);
        }
    }

    private static void DrawGatheringButtons(float buttonHeight)
    {
        // Drawing start button
        using (ImRaii.Disabled(SchedulerMain.AreWeTicking))
        {
            if (ImGui.Button("Start Gathering Leves", new Vector2(ImGui.GetContentRegionAvail().X, buttonHeight)))
            {
                SchedulerMain.WorkListMode = false;
                SchedulerMain.EnablePlugin();
            }
        }
        
        // Drawing stop button
        using (ImRaii.Disabled(!SchedulerMain.AreWeTicking))
        {
            if (ImGui.Button("Stop Gathering Leves", new Vector2(ImGui.GetContentRegionAvail().X, buttonHeight)))
            {
                SchedulerMain.DisablePlugin();
            }
        }
    }

    private static void DrawSelectedLeves(float textLineHeight, float fontScale, bool usingIceTheme)
    {
        if (usingIceTheme)
        {
            int childStyleCount = ThemeHelper.PushChildStyle();
            ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue);
            int headerStyleCount = ThemeHelper.PushHeaderStyle();
            
            if (ImGui.BeginTable("Selected Leves Table", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // Calculate column widths based on font metrics
                float nameColWidth = Math.Max(200, textLineHeight * 10);
                float levelColWidth = Math.Max(80, textLineHeight * 5);
                
                ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, nameColWidth);
                ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, levelColWidth);
                
                ImGui.TableHeadersRow();
                
                // Pop styling after headers are drawn
                ImGui.PopStyleColor(headerStyleCount + 1); // +1 for TableHeaderBg
                
                // Draw the selected leves
                foreach (uint leveId in SelectedLeves)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(LeveDictionary[leveId].LeveName);
                    
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{LeveDictionary[leveId].Level}");
                }
                
                ImGui.EndTable();
            }
            
            ImGui.PopStyleColor(childStyleCount);
        }
        else
        {
            if (ImGui.BeginTable("Selected Leves Table", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // Calculate column widths based on font metrics
                float nameColWidth = Math.Max(200, textLineHeight * 10);
                float levelColWidth = Math.Max(80, textLineHeight * 5);
                
                ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, nameColWidth);
                ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, levelColWidth);
                
                ImGui.TableHeadersRow();
                
                // Draw the selected leves
                foreach (uint leveId in SelectedLeves)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(LeveDictionary[leveId].LeveName);
                    
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{LeveDictionary[leveId].Level}");
                }
                
                ImGui.EndTable();
            }
        }
    }

    private static void DrawGatheringTableRows(float textLineHeight, float fontScale, bool usingIceTheme)
    {
        // Row 1 - NPC Selection
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Levequest NPC:");

        ImGui.TableNextColumn();
        string selectableText = $"{C.SelectedNpcName}";
        
        // Use a themed text color for the NPC selection
        if (usingIceTheme)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ThemeHelper.IceBlue);
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, EColor.Green);
        }
        
        if (ImGui.Selectable(selectableText))
        {
            ImGui.OpenPopup("NPC Selection Popup");
        }
        ImGui.PopStyleColor();
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Left click to select Leve NPC");
            ImGui.EndTooltip();
        }

        // NPC Selection Popup
        if (ImGui.BeginPopup("NPC Selection Popup"))
        {
            SelectedLeves.Clear();
            
            if (usingIceTheme)
            {
                ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue);
                int headerStyleCount = ThemeHelper.PushHeaderStyle();
                
                float popupColumnWidth = Math.Max(200, textLineHeight * 12);
                if (ImGui.BeginTable("NPC Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, popupColumnWidth);
                    ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, popupColumnWidth);
                    ImGui.TableHeadersRow();
                    
                    ImGui.PopStyleColor(headerStyleCount + 1);
                    
                    // Render NPC rows
                    foreach (uint npcId in npcIds)
                    {
                        string npcName = GetNpcName(npcId);
                        string locationName = GetLocationName(npcId);
                        
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        
                        bool isSelected = C.SelectedNpcName == npcName;
                        if (isSelected && usingIceTheme)
                        {
                            int selectStyleCount = ThemeHelper.PushHeaderStyle();
                            if (ImGui.Selectable($"{npcName}###{npcId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                C.SelectedNpcId = npcId;
                                C.SelectedNpcName = npcName;
                                C.LocationName = locationName;
                                C.Save();
                            }
                            ImGui.PopStyleColor(selectStyleCount);
                        }
                        else
                        {
                            if (ImGui.Selectable($"{npcName}###{npcId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                C.SelectedNpcId = npcId;
                                C.SelectedNpcName = npcName;
                                C.LocationName = locationName;
                                C.Save();
                            }
                        }
                        
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(locationName);
                    }
                    
                    ImGui.EndTable();
                }
            }
            else
            {
                float popupColumnWidth = Math.Max(200, textLineHeight * 12);
                if (ImGui.BeginTable("NPC Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, popupColumnWidth);
                    ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, popupColumnWidth);
                    ImGui.TableHeadersRow();
                    
                    // Render NPC rows
                    foreach (uint npcId in npcIds)
                    {
                        string npcName = GetNpcName(npcId);
                        string locationName = GetLocationName(npcId);
                        
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        
                        bool isSelected = C.SelectedNpcName == npcName;
                        if (ImGui.Selectable($"{npcName}###{npcId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            C.SelectedNpcId = npcId;
                            C.SelectedNpcName = npcName;
                            C.LocationName = locationName;
                            C.Save();
                        }
                        
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(locationName);
                    }
                    
                    ImGui.EndTable();
                }
            }
            
            ImGui.EndPopup();
        }
        
        // Row 2 - Class Selection
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Class");

        ImGui.TableNextColumn();
        float classComboWidth = Math.Max(120, textLineHeight * 7);
        ImGui.SetNextItemWidth(classComboWidth);
        
        if (!classSelectList.Contains(C.ClassSelected))
        {
        #nullable disable
            C.ClassSelected = classSelectList.FirstOrDefault();
        }
        
        // Apply control styling for combo box
        if (usingIceTheme)
        {
            int controlStyleCount = ThemeHelper.PushControlStyle();
            
            if (ImGui.BeginCombo("###Class Selection", C.ClassSelected))
            {
                ImGui.PopStyleColor(controlStyleCount);
                
                for (int i = 0; i < classSelectList.Count; i++)
                {
                    int iconSlot = i + 2;
                    bool isSelected = (classSelectList[i] == C.ClassSelected);

                    ImGui.PushID(iconSlot);

                    if (ImGui.Selectable("###hidden", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedLeves.Clear();
                        C.ClassSelected = classSelectList[i];
                        IconSlot = (uint)iconSlot;
                        C.Save();
                    }

                    ImGui.SameLine();

                    // Scale icon size based on font size
                    float iconSize = Math.Max(20, textLineHeight * 1.2f);
                    ImGui.Image(LeveTypeDict[(uint)iconSlot].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(iconSize, iconSize));

                    ImGui.SameLine();
                    ImGui.TextUnformatted(classSelectList[i]);

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();

                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }
        else
        {
            if (ImGui.BeginCombo("###Class Selection", C.ClassSelected))
            {
                for (int i = 0; i < classSelectList.Count; i++)
                {
                    int iconSlot = i + 2;
                    bool isSelected = (classSelectList[i] == C.ClassSelected);

                    ImGui.PushID(iconSlot);

                    if (ImGui.Selectable("###hidden", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedLeves.Clear();
                        C.ClassSelected = classSelectList[i];
                        IconSlot = (uint)iconSlot;
                        C.Save();
                    }

                    ImGui.SameLine();

                    // Scale icon size based on font size
                    float iconSize = Math.Max(20, textLineHeight * 1.2f);
                    ImGui.Image(LeveTypeDict[(uint)iconSlot].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(iconSize, iconSize));

                    ImGui.SameLine();
                    ImGui.TextUnformatted(classSelectList[i]);

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();

                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }
        
        ImGui.SameLine();
        // Scale icon size based on font size
        float classIconSize = Math.Max(20, textLineHeight * 1.2f);
        ImGui.Image(LeveTypeDict[IconSlot].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(classIconSize, classIconSize));

        // Row 3 - Run Until option
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Run Until:");

        ImGui.TableNextColumn();
        float runUntilComboWidth = 0f;
        foreach (string runUntil in RunUntilList)
        {
            float textWidth = ImGui.CalcTextSize(runUntil).X;
            if (textWidth > runUntilComboWidth)
            {
                runUntilComboWidth = textWidth;
            }
        }

        runUntilComboWidth = Math.Max(runUntilComboWidth + 35, textLineHeight * 8);
        ImGui.SetNextItemWidth(runUntilComboWidth);
        
        // Apply control styling for the combo box
        if (usingIceTheme)
        {
            int controlStyleCount = ThemeHelper.PushControlStyle();
            
            if (ImGui.BeginCombo("###Run Until Combo", C.RunUntilSelected))
            {
                ImGui.PopStyleColor(controlStyleCount);
                
                foreach (var selected in RunUntilList)
                {
                    bool isSelected = (C.RunUntilSelected == selected);
                    if (ImGui.Selectable(selected, isSelected))
                    {
                        C.RunUntilSelected = selected;
                        C.Save();
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }
        else
        {
            if (ImGui.BeginCombo("###Run Until Combo", C.RunUntilSelected))
            {
                foreach (var selected in RunUntilList)
                {
                    bool isSelected = (C.RunUntilSelected == selected);
                    if (ImGui.Selectable(selected, isSelected))
                    {
                        C.RunUntilSelected = selected;
                        C.Save();
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }
    }

    // Helper methods for NPC selection
    private static string GetNpcName(uint npcId)
    {
        if (LeveNPCDict.ContainsKey(npcId))
        {
            return LeveNPCDict[npcId].Name;
        }
        return string.Empty;
    }

    private static string GetLocationName(uint npcId)
    {
        if (LeveNPCDict.ContainsKey(npcId))
        {
            var zoneId = LeveNPCDict[npcId].ZoneID;
            return ZoneName(zoneId);
        }
        return string.Empty;
    }

    #endregion
}
