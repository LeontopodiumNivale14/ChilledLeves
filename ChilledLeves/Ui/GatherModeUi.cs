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
        base($"Priority Leve Grind Window {P.GetType().Assembly.GetName().Version} ###GatherModeWorkshopWindow")
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
        
        // Reset style tracking at the start of each frame when debugging
        if (usingIceTheme)
        {
            ThemeHelper.ResetStyleTracking();
        }
        
        // Use try/finally to guarantee style cleanup
        int styleCount = 0;
        try
        {
            // Begin theming
            styleCount = ThemeHelper.BeginTheming(usingIceTheme);
            
            // Draw all UI content
            GatheringMode();
        }
        finally
        {
            // Always clean up styles, even if exceptions occur
            ThemeHelper.EndTheming(usingIceTheme, styleCount);
        }
    }

    #region Gathering Specific Planner

    private static List<uint> npcIds = LeveDictionary.Values
        .Select(x => x.LeveVendorID)
        .Distinct()
        .ToList();

    private static List<string> classSelectList = new List<string>() { LeveTypeDict[2].LeveClassType, LeveTypeDict[3].LeveClassType, LeveTypeDict[4].LeveClassType };
    private static List<string> RunUntilList = new List<string>() { "Lv.", "All Leves Complete", "Just Because" };

    private static void CenterTextV2(string text)
    {
        float columnWidth = ImGui.GetColumnWidth();
        float textWidth = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - textWidth) * 0.5f);
        ImGui.Text(text);
    }

    private static void GatheringMode()
    {
        // Get font scaling metrics
        float textLineHeight = ImGui.GetTextLineHeight();
        float fontScale = ImGui.GetIO().FontGlobalScale;
        float scaledSpacing = ImGui.GetStyle().ItemSpacing.Y * fontScale;
        
        // Get theme setting (but don't set window styles - Draw method handles that)
        bool usingIceTheme = C.UseIceTheme;
        
        // Populate SelectedLeves based on current selections
        // This ensures the leve list is always up to date
        SelectedLeves.Clear();
        foreach (var kdp in LeveDictionary)
        {
            var leveID = kdp.Key;
            var leveVendorId = kdp.Value.LeveVendorID;
            var jobAssignment = kdp.Value.JobAssignmentType;
            
            if (leveVendorId == C.SelectedNpcId && jobAssignment == C.ClassJobType)
            {
                if (!SelectedLeves.Contains(leveID))
                {
                    SelectedLeves.Add(leveID);
                }
            }
        }
        
        // Navigation buttons section with StyleScope for guaranteed cleanup
        if (usingIceTheme)
        {
            using var navStyle = new ThemeHelper.StyleScope();
            
            // Apply button styling
            navStyle.PushColor(ImGuiCol.Button, ThemeHelper.ButtonBg)
                   .PushColor(ImGuiCol.ButtonHovered, ThemeHelper.ButtonHovered)
                   .PushColor(ImGuiCol.ButtonActive, ThemeHelper.ButtonActive)
                   .PushVar(ImGuiStyleVar.FrameRounding, 4.0f);
            
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
            float windowWidth = ImGui.GetWindowWidth() - 20; // To give a little bit of room for the text in case of scroll bar
            string allowancesInfo = $"Allowances: {Allowances}/100 | Next in: {NextAllowances:hh\\:mm\\:ss}";
            float infoWidth = ImGui.CalcTextSize(allowancesInfo).X;
            
            ImGui.SameLine(windowWidth - infoWidth - btnPadding);
            
            // Add ice theme styling for the text
            using var textStyle = new ThemeHelper.StyleScope();
            textStyle.PushColor(ImGuiCol.Text, ThemeHelper.IceBlue);
            ImGui.Text(allowancesInfo);
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
        
        // Main settings table with StyleScope for complete style isolation
        if (usingIceTheme)
        {
            using var tableStyles = new ThemeHelper.StyleScope();
            
            // Apply comprehensive styling using StyleScope instead of individual pushes
            tableStyles.PushColor(ImGuiCol.ChildBg, ThemeHelper.ChildBg)
                      .PushColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue)
                      .PushColor(ImGuiCol.Header, ThemeHelper.HeaderBg)
                      .PushColor(ImGuiCol.HeaderHovered, ThemeHelper.HeaderHovered)
                      .PushColor(ImGuiCol.HeaderActive, ThemeHelper.HeaderActive);
                
            if (ImGui.BeginTable("Gathering Leve Settings", 2, ImGuiTableFlags.Borders))
            {
                // Use dynamic column width based on font size
                float tableColumn1Width = Math.Max(150, textLineHeight * 10);
                float tableColumn2Width = Math.Max(500, textLineHeight * 30);

                ImGui.TableSetupColumn("Text Information", ImGuiTableColumnFlags.WidthFixed, tableColumn1Width);
                ImGui.TableSetupColumn("Setting Selection", ImGuiTableColumnFlags.WidthFixed, tableColumn2Width);
                
                ImGui.TableHeadersRow();
        
                // Now continue with the table content
                DrawGatheringTableRows(textLineHeight, fontScale, usingIceTheme);
                
                ImGui.EndTable();
            }
            // No need for cleanup with StyleScope - it's handled by the using statement
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
        
        // Add spacing
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Action Buttons with StyleScope for guaranteed cleanup
        if (usingIceTheme)
        {
            using var btnStyle = new ThemeHelper.StyleScope();
            
            btnStyle.PushColor(ImGuiCol.Button, ThemeHelper.ButtonBg)
                   .PushColor(ImGuiCol.ButtonHovered, ThemeHelper.ButtonHovered)
                   .PushColor(ImGuiCol.ButtonActive, ThemeHelper.ButtonActive)
                   .PushVar(ImGuiStyleVar.FrameRounding, 4.0f);
            
            float buttonHeight = textLineHeight * 1.5f;
            DrawGatheringButtons(buttonHeight);
        }
        else
        {
            float buttonHeight = textLineHeight * 1.5f;
            DrawGatheringButtons(buttonHeight);
        }

        // Draw the Crafting Workshop table with StyleScope for proper cleanup
        ImGui.Dummy(new Vector2(0, 7));
        
        // Draw detailed leve table - completely isolated with StyleScope
        using (var scope = new ThemeHelper.StyleScope())
        {
            ImGui.PushID("CraftingWorkshopTableScope");
            
            if (usingIceTheme)
            {
                // Apply styles for themed table using StyleScope
                scope.PushColor(ImGuiCol.ChildBg, ThemeHelper.ChildBg)
                     .PushColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue)
                     .PushColor(ImGuiCol.Header, ThemeHelper.HeaderBg)
                     .PushColor(ImGuiCol.HeaderHovered, ThemeHelper.HeaderHovered)
                     .PushColor(ImGuiCol.HeaderActive, ThemeHelper.HeaderActive);
                
                if (ImGui.BeginTable($"Crafting Workshop List", 8, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit))
                {
                    // Columns for the crafters
                    ImGui.TableSetupColumn("Prio###GatheringLevePrio", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Complete?###GatheringLeveComplete", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Level###GatheringLeveLevel", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Leve Name###GatheringLeveName", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Item Turnin###GatheringTurninItems", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Need###GatheringAmountNecessary", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Have###GatheringCompleteCheck", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Triple?");
                    
                    ImGui.TableHeadersRow();
                    
                    DrawGatheringTableContent();
                    
                    ImGui.EndTable();
                }
            }
            else
            {
                if (ImGui.BeginTable($"Crafting Workshop List", 8, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit))
                {
                    // Columns for the crafters
                    ImGui.TableSetupColumn("Prio###GatheringLevePrio", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Complete?###GatheringLeveComplete", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Level###GatheringLeveLevel", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Leve Name###GatheringLeveName", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Item Turnin###GatheringTurninItems", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Need###GatheringAmountNecessary", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Have###GatheringCompleteCheck", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Triple?");
                    
                    ImGui.TableHeadersRow();
                    
                    DrawGatheringTableContent();
                    
                    ImGui.EndTable();
                }
            }
            
            ImGui.PopID();
        }
    }

    // Extract table content drawing to separate method for better organization
    private static void DrawGatheringTableContent()
    {
        foreach (var kdp in LeveDictionary)
        {
            var leveID = kdp.Key;
            var leveLevel = kdp.Value.Level;
            var leveName = kdp.Value.LeveName;
            var leveVendorId = kdp.Value.LeveVendorID;
            var leveVendorName = kdp.Value.LeveVendorName;
            var jobAssignment = kdp.Value.JobAssignmentType;
            var zoneId = LeveNPCDict[leveVendorId].ZoneID;
            var priority = kdp.Value.Priority;
            
            if (leveVendorId != C.SelectedNpcId || jobAssignment != C.ClassJobType)
            {
                continue;
            }
            
            ImGui.TableNextRow();
            
            ImGui.PushID((int)leveID);
            ImGui.TableSetColumnIndex(0);
            if (ImGui.DragInt("###GatheringPriority", ref priority))
            {
                kdp.Value.Priority = priority;
                if (!C.LevePriority.ContainsKey(leveID))
                {
                    C.LevePriority.Add(leveID, priority);
                }
                else if (C.LevePriority.ContainsKey(leveID))
                {
                    C.LevePriority[leveID] = priority;
                }
                C.Save();
            }
            
            ImGui.TableNextColumn();
            bool leveCompleted = IsComplete(leveID);
            if (leveCompleted)
            {
                ImGui.Image(LeveStatusDict[1].GetWrapOrDefault().ImGuiHandle, new Vector2(20, 20));
            }
            if (!leveCompleted)
            {
                ImGui.Image(LeveStatusDict[2].GetWrapOrDefault().ImGuiHandle, new Vector2(20, 20));
            }
            
            ImGui.TableNextColumn();
            CenterTextV2($"{leveLevel}");
            
            ImGui.TableNextColumn();
            ImGui.Text($"{leveName}");

            if (CraftFisherJobs.Contains(jobAssignment))
            {
                var ItemImage = CraftDictionary[leveID].ItemIcon.GetWrapOrEmpty();
                var itemName = CraftDictionary[leveID].ItemName;
                var itemNeed = CraftDictionary[leveID].TurninAmount;
                var itemId = CraftDictionary[leveID].ItemID;

                ImGui.TableNextColumn();
                ImGui.Image(ItemImage.ImGuiHandle, new Vector2(20, 20));
                ImGui.SameLine(0, 5);
                ImGui.Text($"{itemName}");

                ImGui.TableNextColumn();
                CenterTextV2($"{itemNeed}");

                ImGui.TableNextColumn();
                var itemHave = GetItemCount((int)CraftDictionary[leveID].ItemID);
                CenterTextV2($"{itemHave}");

                ImGui.TableNextColumn();
                bool tripleTurnin = CraftDictionary[leveID].RepeatAmount > 1;
                FancyCheckmark(tripleTurnin);
            }
            
            ImGui.PopID();
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
                SchedulerMain.GatheringMode = true;
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

    private static void DrawGatheringTableRows(float textLineHeight, float fontScale, bool usingIceTheme)
    {

        // Row 1 - NPC Selection
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Levequest NPC:");

        ImGui.TableNextColumn();
        string selectableText = $"{C.SelectedNpcName} → {C.LocationName}";
        
        // Use a themed text color for the NPC selection
        if (usingIceTheme)
        {
            using var textStyle = new ThemeHelper.StyleScope();
            textStyle.PushColor(ImGuiCol.Text, ThemeHelper.FrostWhite);

            if (ImGui.Button("Open NPC Selection"))
            {
                ImGui.OpenPopup("NPC Selection Popup"); // Open the popup when the selectable is clicked
            }
            ImGui.SameLine();
            ImGui.Text(selectableText);
        }
        else
        {
            if (ImGui.Button("Open NPC Selection"))
            {
                ImGui.OpenPopup("NPC Selection Popup"); // Open the popup when the selectable is clicked
            }

            ImGui.SameLine();
            ImGui.Text(selectableText);
        }

        // NPC Selection Popup - NO ID SCOPE to avoid breaking popup behavior
        if (ImGui.BeginPopup("NPC Selection Popup"))
        {
            SelectedLeves.Clear();
            
            // Create a table inside the popup
            if (usingIceTheme)
            {
                // Use StyleScope for guaranteed cleanup of styles
                using (var tableStyles = new ThemeHelper.StyleScope())
                {
                    // Add styling for the table with proper cleanup
                    tableStyles.PushColor(ImGuiCol.TableHeaderBg, ThemeHelper.DeepIceBlue)
                              .PushColor(ImGuiCol.Header, ThemeHelper.HeaderBg)
                              .PushColor(ImGuiCol.HeaderHovered, ThemeHelper.HeaderHovered)
                              .PushColor(ImGuiCol.HeaderActive, ThemeHelper.HeaderActive);
                    
                    if (ImGui.BeginTable("NPC Table", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                    {
                        // Define column widths
                        ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, 200);
                        ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, 50);
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
                            
                            // Highlight the selected NPC with the ice theme
                            bool isSelected = npcId == C.SelectedNpcId;
                            if (isSelected)
                            {
                                using (var rowStyle = new ThemeHelper.StyleScope())
                                {
                                    rowStyle.PushColor(ImGuiCol.Header, ThemeHelper.HeaderBg)
                                           .PushColor(ImGuiCol.HeaderHovered, ThemeHelper.HeaderHovered)
                                           .PushColor(ImGuiCol.HeaderActive, ThemeHelper.HeaderActive);
                                           
                                    if (ImGui.Selectable(npcName, true))
                                    {
                                        C.SelectedNpcId = npcId; // Update selected NPC ID
                                        C.SelectedNpcName = npcName; // Update selected NPC Name
                                        C.LocationName = npcLocation;
                                        C.Save();
                                        ImGui.CloseCurrentPopup(); // Close the popup after selection
                                    }
                                }
                            }
                            else
                            {
                                if (ImGui.Selectable(npcName, false))
                                {
                                    C.SelectedNpcId = npcId; // Update selected NPC ID
                                    C.SelectedNpcName = npcName; // Update selected NPC Name
                                    C.LocationName = npcLocation;
                                    C.Save();
                                    ImGui.CloseCurrentPopup(); // Close the popup after selection
                                }
                            }

                            ImGui.TableSetColumnIndex(1);
                            string leveLevels = "";
                            if (C.ClassJobType == 4)
                            {
                                leveLevels = LeveNPCDict[npcId].FshLeveLevels;
                            }
                            else if (C.ClassJobType == 3)
                            {
                                leveLevels = LeveNPCDict[npcId].BtnLeveLevels;
                            }
                            else if (C.ClassJobType == 2)
                            {
                                leveLevels = LeveNPCDict[npcId].MinLeveLevels;
                            }
                            CenterTextV2(leveLevels);
        
                            ImGui.TableSetColumnIndex(2);
                            ImGui.Text(npcLocation); // Display NPC location in the second column
                        }
        
                        ImGui.EndTable(); // End the table
                    }
                } // StyleScope automatically cleans up all styles when disposed
            }
            else
            {
                // Non-themed table
                if (ImGui.BeginTable("NPC Table", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    // Define column widths
                    ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, 200);
                    ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, 50);
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
                        if (ImGui.Selectable(npcName, npcId == C.SelectedNpcId)) // Check if it's the selected item
                        {
                            C.SelectedNpcId = npcId; // Update selected NPC ID
                            C.SelectedNpcName = npcName; // Update selected NPC Name
                            C.LocationName = npcLocation;
                            C.Save();
                            ImGui.CloseCurrentPopup(); // Close the popup after selection
                        }

                        ImGui.TableSetColumnIndex(1);
                        string leveLevels = "";
                        if (C.ClassJobType == 4)
                        {
                            leveLevels = LeveNPCDict[npcId].FshLeveLevels;
                        }
                        else if (C.ClassJobType == 3)
                        {
                            leveLevels = LeveNPCDict[npcId].BtnLeveLevels;
                        }
                        else if (C.ClassJobType == 2)
                        {
                            leveLevels = LeveNPCDict[npcId].MinLeveLevels;
                        }
                        CenterTextV2(leveLevels);

                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text(npcLocation); // Display NPC location in the second column
                    }
    
                    ImGui.EndTable(); // End the table
                }
            }
            
            ImGui.EndPopup(); // End the popup
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
        
        // Apply control styling for combo box with proper ID scope
        ImGui.PushID("ClassComboScope");
        try 
        {
            if (usingIceTheme)
            {
                // Use StyleScope for safer style management
                using (var comboStyles = new ThemeHelper.StyleScope())
                {
                    comboStyles.PushColor(ImGuiCol.FrameBg, ThemeHelper.FrameBg)
                               .PushColor(ImGuiCol.FrameBgHovered, ThemeHelper.FrameBgHovered)
                               .PushColor(ImGuiCol.FrameBgActive, ThemeHelper.FrameBgActive)
                               .PushColor(ImGuiCol.CheckMark, ThemeHelper.IceBlue);
                    
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
                                C.ClassJobType = (uint)iconSlot;
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
                } // StyleScope automatically disposes and cleans up styles
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
                            C.ClassJobType = (uint)iconSlot;
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
        }
        finally
        {
            ImGui.PopID();
        }
        
        ImGui.SameLine();
        // Scale icon size based on font size
        float classIconSize = Math.Max(20, textLineHeight * 1.2f);
        ImGui.Image(LeveTypeDict[C.ClassJobType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(classIconSize, classIconSize));

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
        
        // Apply control styling for the combo box with proper ID scope
        ImGui.PushID("RunUntilComboScope");
        try
        {
            if (usingIceTheme)
            {
                // Use StyleScope for safer style management
                using (var comboStyles = new ThemeHelper.StyleScope())
                {
                    comboStyles.PushColor(ImGuiCol.FrameBg, ThemeHelper.FrameBg)
                               .PushColor(ImGuiCol.FrameBgHovered, ThemeHelper.FrameBgHovered)
                               .PushColor(ImGuiCol.FrameBgActive, ThemeHelper.FrameBgActive)
                               .PushColor(ImGuiCol.CheckMark, ThemeHelper.IceBlue);
                    
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
                } // StyleScope automatically disposes and cleans up styles
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
        finally
        {
            ImGui.PopID();
        }
        
        if (C.RunUntilSelected == RunUntilList[0])
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.SliderInt("###Lv Slider", ref C.LevelSliderInput, 1, 100))
            {
                if (C.LevelSliderInput > 100)
                    C.LevelSliderInput = 100;
                else if (C.LevelSliderInput < 1)
                    C.LevelSliderInput = 1;
                C.Save();
            }
        }
    }

    #endregion
}
