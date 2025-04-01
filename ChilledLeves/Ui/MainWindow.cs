using ChilledLeves.Scheduler;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Style;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lumina.Data.Parsing.Uld.UldRoot;

namespace ChilledLeves.Ui
{
    internal class MainWindow : Window
    {

        private static int selectedLeve;

        // Ice theme color definitions are now moved to ThemeHelper class

        // Used for the navmesh button
        private string buttonText = "Need Navmesh Installed";
        private float lastClickTime = 0f;
        private const float resetDelay = 2.0f; // 2 seconds delay

        /// <summary>
        /// Constructor for the main window. Adjusts window size, flags, and initializes data.
        /// </summary>
        public MainWindow() :
            base($"Chilled Leves {P.GetType().Assembly.GetName().Version} ###ChilledLevesMainWindow")
        {

            Flags = ImGuiWindowFlags.None;

            // Set up size constraints to ensure window cannot be too small or too large.
            // Increased minimum size to better accommodate larger font sizes
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(1050, 650),
                MaximumSize = new Vector2(2000, 2000)
            };

            // Register this window with Dalamud's window system.
            P.windowSystem.AddWindow(this);

            AllowPinning = false;

            // Populate leve dictionaries and other data on creation.
            PopulateDictionary();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Primary draw method. Responsible for drawing the entire UI of the main window.
        /// </summary>
        public override void Draw()
        {
            // If user has enabled Ice Theme, push a dark-slate background color for the window.
            bool usingIceTheme = C.UseIceTheme;
            
            // Use the new ThemeHelper method to begin theming
            int themingStyleCount = ThemeHelper.BeginTheming(usingIceTheme);

            // Calculate scaling factors based on current font size
            float fontScale = ImGui.GetIO().FontGlobalScale;
            float textLineHeight = ImGui.GetTextLineHeight();
            float scaledSpacing = ImGui.GetStyle().ItemSpacing.Y * fontScale;
            float headerPadding = textLineHeight * 1.2f;

            // Draw the custom or default style header at the top:
            DrawSimpleHeader(usingIceTheme);

            // The header is dynamically sized based on font metrics - taller header now
            float headerHeight = textLineHeight + headerPadding * 2;
            float contentAreaHeight = ImGui.GetWindowHeight() - headerHeight - 4;
            float labelHeight = ImGui.GetTextLineHeightWithSpacing();
            float childHeight = ImGui.GetContentRegionAvail().Y - labelHeight;

            // Use a 3-column layout to hold Filters (left panel), Leve List (middle panel), and Details (right panel).
            ImGui.Columns(3, "MainLayout", false);

            // -----------------------------------------
            //  PANEL HEADERS (the text above each panel)
            // -----------------------------------------

            // Left panel header - dynamically sized based on font metrics
            float leftPanelWidth = Math.Max(220, textLineHeight * 14);
            ImGui.SetColumnWidth(0, leftPanelWidth);
            if (usingIceTheme)
            {
                int styleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text("Controls");
                ImGui.PopStyleColor(styleCount);
            }
            else
            {
                ImGui.Text("Controls");
            }
            ImGui.NextColumn();

            // Middle panel header - dynamically sized based on font metrics
            float middlePanelWidth = Math.Max(350, textLineHeight * 22);
            ImGui.SetColumnWidth(1, middlePanelWidth);
            if (C.OnlyFavorites)
            {
                ImGui.Text($"Showing: {C.FavoriteLeves.Count} out of {LeveDictionary.Count}");
            }
            else
            {
                ImGui.Text($"Showing: {VisibleLeves.Count} out of {LeveDictionary.Count}");
            }
            ImGui.NextColumn();

            // Right panel header
            if (usingIceTheme)
            {
                int styleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text("Selected Leve Details");
                ImGui.PopStyleColor(styleCount);
            }
            else
            {
                ImGui.Text("Selected Leve Details");
            }

            // Reset columns in order to begin actual panel content
            ImGui.Columns(1);
            ImGui.Columns(3, "MainLayoutPanels", false);

            // ------------------------------------------------
            //  LEFT PANEL: Start/Stop, Worklist, Gathering, and Filter UI
            // ------------------------------------------------
            ImGui.SetColumnWidth(0, leftPanelWidth);

            // Possibly push a child background color if ice theme is active
            int childBgStyleCount = 0;
            if (usingIceTheme)
            {
                childBgStyleCount = ThemeHelper.PushChildStyle();
            }
            ImGui.BeginChild("###FilterPanel", new Vector2(0, childHeight), true);

            // We push a rounded corner style for the top-left panel buttons (Start/Stop, Worklist, etc.)
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);

            // If Ice Theme, push a custom color for the button. If not, use default/dalamud.
            int buttonColors = 0;
            if (usingIceTheme)
            {
                buttonColors = ThemeHelper.PushButtonStyle();
            }

            // Start and Stop buttons 
            using (ImRaii.Disabled(SchedulerMain.AreWeTicking) )
            {
                if (!HasPlugin("vnavmesh"))
                {
                    // Check if button is clicked
                    if (ImGui.Button(buttonText, new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f)))
                    {
                        ImGui.SetClipboardText("Navmesh installation guide or link here");
                        buttonText = "Copied to Clipboard";
                        lastClickTime = (float)ImGui.GetTime(); // Store the current ImGui time
                    }
                    
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Press the button to copy the repo to your clipboard");
                        ImGui.EndTooltip();
                    }

                    // Check if the delay has passed and revert the text
                    if (buttonText == "Copied to Clipboard" && (ImGui.GetTime() - lastClickTime) >= resetDelay)
                    {
                        buttonText = "Need Navmesh Installed";
                    }
                }
                else if (HasPlugin("vnavmesh"))
                {
                    if (ImGui.Button("Start", new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f)))
                    {
                        SchedulerMain.WorkListMode = true;
                        SchedulerMain.EnablePlugin();
                    }
                }
            }
            using (ImRaii.Disabled(!SchedulerMain.AreWeTicking))
            {
                if (ImGui.Button("Stop", new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f)))
                {
                    SchedulerMain.DisablePlugin();
                }
            }

            // Two extra plugin UI toggles: Worklist and Gathering Grind.
            if (ImGui.Button("Open Worklist", new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f)))
            {
                P.workListUi.IsOpen = !P.workListUi.IsOpen;
            }
            if (ImGui.Button("Open Gathering Grind", new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f)))
            {
                P.gatherModeUi.IsOpen = !P.gatherModeUi.IsOpen;
            }

            // Pop the button colors if we used them for the ice theme
            if (buttonColors > 0)
            {
                ImGui.PopStyleColor(buttonColors);
            }
            // Pop the frame rounding
            ImGui.PopStyleVar();

            // Theme Toggle
            ImGui.Spacing();
            ImGui.Spacing();

            bool useIceTheme = C.UseIceTheme;
            if (ImGui.Checkbox("Use Ice Theme", ref useIceTheme))
            {
                C.UseIceTheme = useIceTheme;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Toggle between custom ice theme and Dalamud's default theme");
                ImGui.EndTooltip();
            }

            ImGui.Spacing();
            ImGui.Spacing();

            // Heading for main filter area
            if (usingIceTheme)
            {
                int styleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text("Filter Leves");
                ImGui.PopStyleColor(styleCount);
            }
            else
            {
                ImGui.Text("Filter Leves");
            }
            ImGui.Separator();

            // Independent filter checkboxes (Favorites, Completed, Incomplete).
            ImGui.Spacing();
            ImGui.Text("Filter Options:");
            ImGui.Spacing();

            #region Favorites Checkbox

            bool showFavorites = C.OnlyFavorites;
            void FavoritesOnly()
            {
                if (ImGui.Checkbox("Show Favorites Only", ref showFavorites))
                {
                    C.OnlyFavorites = showFavorites;
                    if (showFavorites)
                    {
                        C.CompleteFilter = 0; // if turning on favorites, reset the complete filter
                    }
                    C.Save();
                }
            }

            // Favorites only
            if (usingIceTheme)
            {
                int controlStyleCount = ThemeHelper.PushControlStyle();
                FavoritesOnly();
                ImGui.PopStyleColor(controlStyleCount);
            }
            else
            {
                FavoritesOnly();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Show only leves marked as favorites");
                ImGui.EndTooltip();
            }

            #endregion

            #region Show Completed

            // Show Completed
            int completeFilter = (int)C.CompleteFilter;
            bool showCompleted = completeFilter == 1;
            void ShowCompletedOnly()
            {
                if (ImGui.Checkbox("Show Completed Only", ref showCompleted))
                {
                    C.CompleteFilter = showCompleted ? 1u : 0u;
                    if (showCompleted)
                    {
                        // Turn off favorites filter and enable "completed only"
                        C.OnlyFavorites = false;
                        C.CompleteFilter = 1u;
                    }
                    C.Save();
                }
            }

            if (usingIceTheme)
            {
                int controlStyleCount = ThemeHelper.PushControlStyle();
                ShowCompletedOnly();
                ImGui.PopStyleColor(controlStyleCount);
            }
            else
            {
                ShowCompletedOnly();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Show only completed leves");
                ImGui.EndTooltip();
            }

            #endregion

            #region Show Incomplete

            // Show Incomplete
            // Used to filter out completed leves
            bool showIncomplete = completeFilter == 2;
            void ShowIncompleteOnly()
            {
                if (ImGui.Checkbox("Show Incomplete Only", ref showIncomplete))
                {
                    C.CompleteFilter = showIncomplete ? 2u : 0u;
                    if (showIncomplete)
                    {
                        // Turn off favorites and show "incomplete only"
                        C.OnlyFavorites = false;
                        C.CompleteFilter = 2u;
                    }
                    C.Save();
                }
            }

            if (usingIceTheme)
            {
                int controlStyleCount = ThemeHelper.PushControlStyle();
                ShowIncompleteOnly();
                ImGui.PopStyleColor(controlStyleCount);
            }
            else
            {
                ShowIncompleteOnly();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Show only incomplete leves");
                ImGui.EndTooltip();
            }

            #endregion

            #region Reset All Filters

            // Reset all filters button
            void ResetAllFilterButton()
            {
                if (ImGui.Button("Reset All Filters", new Vector2(ImGui.GetContentRegionAvail().X * 0.9f, 0)))
                {
                    C.OnlyFavorites = false;
                    C.CompleteFilter = 0;
                    C.LevelFilter = 0;
                    C.NameFilter = "";
                    C.RefreshJobFilter();

                    // Turn on all job filters by default:
                    C.ShowCarpenter = true;
                    C.ShowBlacksmith = true;
                    C.ShowArmorer = true;
                    C.ShowGoldsmith = true;
                    C.ShowLeatherworker = true;
                    C.ShowWeaver = true;
                    C.ShowAlchemist = true;
                    C.ShowCulinarian = true;
                    C.ShowFisher = true;
                    C.ShowMiner = true;
                    C.ShowBotanist = true;

                    C.Save();
                }
            }
            ImGui.Spacing();
            if (usingIceTheme)
            {
                int buttonStyleCount = ThemeHelper.PushButtonStyle();
                ImGui.PushStyleColor(ImGuiCol.Text, ThemeHelper.FrostWhite);
                ResetAllFilterButton();
                ImGui.PopStyleColor(buttonStyleCount + 1); // +1 for the text color
            }
            else
            {
                ResetAllFilterButton();
            }
            
            // Tooltip for Reset All Filters button
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Clear all active filters");
                ImGui.EndTooltip();
            }

            #endregion

            ImGui.Spacing();

            #region Job Filters

            string JobFilterText = "Job Filters:";

            // Job Filters heading
            if (usingIceTheme)
            {
                int styleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text(JobFilterText);
                ImGui.PopStyleColor(styleCount);
            }
            else
            {
                ImGui.Text(JobFilterText);
            }
            ImGui.Separator();

            // Crafters
            ImGui.Spacing();
            ImGui.Text("Crafters:");
            ImGui.Spacing();

            float iconSize = 32;
            float iconSpacing = 8;
            float availWidth = ImGui.GetContentRegionAvail().X;
            float startX = (availWidth - (iconSize + iconSpacing) * 4 + iconSpacing) * 0.5f;
            ImGui.SetCursorPosX(startX);

            // Row 1: CRP, BSM, ARM, GSM
            DrawJobToggle(5, ref C.ShowCarpenter, "Carpenter", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(6, ref C.ShowBlacksmith, "Blacksmith", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(7, ref C.ShowArmorer, "Armorer", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(8, ref C.ShowGoldsmith, "Goldsmith", usingIceTheme);

            ImGui.SetCursorPosX(startX);

            // Row 2: LTW, WVR, ALC, CUL
            DrawJobToggle(9, ref C.ShowLeatherworker, "Leatherworker", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(10, ref C.ShowWeaver, "Weaver", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(11, ref C.ShowAlchemist, "Alchemist", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            DrawJobToggle(12, ref C.ShowCulinarian, "Culinarian", usingIceTheme);

            // Gatherers
            ImGui.Spacing();
            ImGui.Text("Gatherers:");
            ImGui.Spacing();

            ImGui.SetCursorPosX(startX);
            DrawJobToggle(4, ref C.ShowFisher, "Fisher", usingIceTheme);
            ImGui.SameLine(0, iconSpacing);
            bool tempDisable = false;

            DrawJobDisable(2, ref tempDisable, "Miner", usingIceTheme);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("These aren't supported yet, but will be in a future update!");
                ImGui.EndTooltip();
            }
            ImGui.SameLine(0, iconSpacing);
            DrawJobDisable(3, ref tempDisable, "Botanist", usingIceTheme);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("These aren't supported yet, but will be in a future update!");
                ImGui.EndTooltip();
            }

            #endregion

            #region Additional Filters
            // Additional Filters heading
            ImGui.Spacing();
            ImGui.Spacing();
            string AdditionalFilterText = "Additional Filters:";
            if (usingIceTheme)
            {
                int styleCount = ThemeHelper.PushHeadingTextStyle();
                ImGui.Text(AdditionalFilterText);
                ImGui.PopStyleColor(styleCount);
            }
            else
            {
                ImGui.Text(AdditionalFilterText);
            }
            ImGui.Separator();
            ImGui.Spacing();

            float FilterSize1 = 0;
            float FilterSize2 = 0;

            if (ImGui.BeginTable("###AdditionalFilterTags", 2))
            {
                ImGui.TableSetupColumn("###Text", ImGuiTableColumnFlags.WidthFixed, FilterSize1);
                ImGui.TableSetupColumn("###Input Boxes", ImGuiTableColumnFlags.WidthFixed, FilterSize2);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                // Level filter
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Level:");
                FilterSize1 = Math.Max(ImGui.CalcTextSize("Level:").X, FilterSize1);

                ImGui.TableNextColumn();
                var level = C.LevelFilter > 0 ? C.LevelFilter.ToString() : "";

                void LevelFilter()
                {
                    if (ImGui.InputText("###Level", ref level, 3))
                    {
                        C.LevelFilter = (int)(uint.TryParse(level, out var num) && num > 0 ? num : 0);
                        C.Save();
                    }
                }
                ImGui.SetNextItemWidth(60);
                if (usingIceTheme)
                {
                    int controlStyleCount = ThemeHelper.PushControlStyle();
                    LevelFilter();
                    ImGui.PopStyleColor(controlStyleCount);
                }
                else
                {
                    LevelFilter();
                }

                ImGui.TableNextRow();

                // Name filter
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Name:");
                FilterSize1 = Math.Max(ImGui.CalcTextSize("Name:").X, FilterSize1);

                ImGui.TableNextColumn();

                FilterSize2 = 100;
                void NameFilter()
                {
                    if (ImGui.InputText("###Name", ref C.NameFilter, 200))
                    {
                        C.NameFilter = C.NameFilter.Trim();
                        C.Save();
                    }
                }
                ImGui.SetNextItemWidth(FilterSize2);
                if (usingIceTheme)
                {
                    int controlStyleCount = ThemeHelper.PushControlStyle();
                    NameFilter();
                    ImGui.PopStyleColor(controlStyleCount);
                }
                else
                {
                    NameFilter();
                }

                ImGui.EndTable();
            }
            #endregion

            // End of left panel
            ImGui.EndChild();
            if (usingIceTheme)
            {
                ImGui.PopStyleColor(childBgStyleCount);
            }

            ImGui.NextColumn();

            // -----------------------------------------
            //  MIDDLE PANEL: Leve List
            // -----------------------------------------
            ImGui.SetColumnWidth(1, middlePanelWidth);

            int middlePanelChildBgCount = 0;
            if (usingIceTheme)
            {
                middlePanelChildBgCount = ThemeHelper.PushChildStyle();
            }
            ImGui.BeginChild("###LeveList", new Vector2(0, childHeight), true);

            int headerColors = 0;
            if (usingIceTheme)
            {
                headerColors = ThemeHelper.PushHeaderStyle();
            }

            // Draw each leve entry if it passes the filter
            foreach (var kdp in LeveDictionary)
            {
                if (FilterLeve(kdp.Key))
                {
                    var id = (int)kdp.Key;
                    ImGui.PushID(id);

                    // Show a small numeric "badge" for the leve's level
                    ImGui.Text($"{LeveDictionary[kdp.Key].Level}");
                    ImGui.SameLine(30);

                    // Main clickable row for the leve
                    if (ImGui.Selectable($"###Leve{id}", selectedLeve == id, ImGuiSelectableFlags.SpanAllColumns))
                        selectedLeve = id;

                    // Double-click to add leve to the WorkList
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (!C.workList.Any(e => e.LeveID == id))
                        {
                            C.workList.Add(new LeveEntry { LeveID = kdp.Key, InputValue = 1 });
                        }
                    }

                    ImGui.SetItemAllowOverlap();
                    ImGui.SameLine();

                    // Job Icon
                    if (LeveTypeDict[kdp.Value.JobAssignmentType].AssignmentIcon != null)
                    {
                        ImGui.Image(LeveTypeDict[kdp.Value.JobAssignmentType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(20, 20));
                        ImGui.SameLine(0, 4);
                    }

                    // Favorite icon
                    if (C.FavoriteLeves.Contains(kdp.Key))
                    {
                        var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
                        Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
                        Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);
                        ImGui.Image(starTex.ImGuiHandle, new Vector2(18, 18), uvMin, uvMax);
                        ImGui.SameLine(0, 4);
                    }

                    // Completion icon
                    if (IsComplete(kdp.Key))
                    {
                        var CompleteTexture = LeveStatusDict[1].GetWrapOrEmpty();
                        ImGui.Image(CompleteTexture.ImGuiHandle, new Vector2(18, 18));
                        ImGui.SameLine(0, 4);
                    }

                    // Then print the actual leve name
                    ImGui.TextWrapped($"{LeveDictionary[kdp.Key].LeveName}");
                    ImGui.PopID();
                }
            }

            if (headerColors > 0)
            {
                ImGui.PopStyleColor(headerColors);
            }
            ImGui.EndChild();
            if (usingIceTheme)
            {
                ImGui.PopStyleColor(middlePanelChildBgCount);
            }

            ImGui.NextColumn();

            // -----------------------------------------
            //  RIGHT PANEL: Selected Leve Details
            // -----------------------------------------
            int rightPanelChildBgCount = 0;
            if (usingIceTheme)
            {
                rightPanelChildBgCount = ThemeHelper.PushChildStyle();
            }
            ImGui.BeginChild("###LeveDetail", new Vector2(0, childHeight), true);

            var leve = (uint)selectedLeve;
            var questSheet = Svc.Data.GetExcelSheet<Quest>();

            if (LeveDictionary.ContainsKey(leve))
            {
                // Print the job icon (if any) + leve name + level
                if (usingIceTheme)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ThemeHelper.FrostWhite);
                }
                if (LeveTypeDict[LeveDictionary[leve].JobAssignmentType].AssignmentIcon != null)
                {
                    ImGui.Image(LeveTypeDict[LeveDictionary[leve].JobAssignmentType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(24, 24));
                    ImGui.SameLine(0, 8);
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"[{LeveDictionary[leve].Level}] {LeveDictionary[leve].LeveName}");
                ImGui.TextDisabled($"LeveID: {leve}");
                ImGui.Separator();
                if (usingIceTheme)
                {
                    ImGui.PopStyleColor();
                }

                // Rewards section
                string rewardText = "Rewards";
                if (usingIceTheme)
                {
                    int styleCount = ThemeHelper.PushHeadingTextStyle();
                    ImGui.Text(rewardText);
                    ImGui.PopStyleColor(styleCount);
                }
                else
                {
                    ImGui.Text(rewardText);
                }

                ImGui.BeginTable("reward_table", 2, ImGuiTableFlags.SizingFixedFit);
                ImGui.TableNextColumn();
                ImGui.Text("EXP Reward:");
                ImGui.TableNextColumn();
                ImGui.Text($"{LeveDictionary[leve].ExpReward:N0}");
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Gil Reward:");
                ImGui.TableNextColumn();
                ImGui.Text($"{LeveDictionary[leve].GilReward:N0} ± 5%");
                ImGui.EndTable();

                ImGui.Separator();

                // Location
                string locationText = "Location";
                if (usingIceTheme)
                {
                    int styleCount = ThemeHelper.PushHeadingTextStyle();
                    ImGui.Text(locationText);
                    ImGui.PopStyleColor(styleCount);
                }
                else
                {
                    ImGui.Text(locationText);
                }

                var vendorId = LeveDictionary[leve].LeveVendorID;
                var startZoneId = LeveNPCDict[vendorId].ZoneID;

                ImGui.BeginTable("location_table", 2, ImGuiTableFlags.SizingFixedFit);
                ImGui.TableNextColumn();
                ImGui.Text("Starting Zone:");
                ImGui.TableNextColumn();
                ImGui.Text($"{ZoneName(startZoneId)}");
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("NPC:");
                ImGui.TableNextColumn();

                int npcButtonColors = 0;
                if (usingIceTheme)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, ThemeHelper.DarkIceBlue);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(ThemeHelper.DarkIceBlue.X + 0.1f, ThemeHelper.DarkIceBlue.Y + 0.1f, ThemeHelper.DarkIceBlue.Z + 0.1f, 1.0f));
                    npcButtonColors = 2;
                }

                if (ImGui.Button($"{LeveDictionary[leve].LeveVendorName}###NPC"))
                {
                    var flagX = LeveNPCDict[vendorId].flagX;
                    var flagZ = LeveNPCDict[vendorId].flagZ;
                    SetFlagForNPC(startZoneId, flagX, flagZ);
                }
                if (npcButtonColors > 0)
                {
                    ImGui.PopStyleColor(npcButtonColors);
                }

                // If crafter/fisher job, show the turn-in NPC
                var JobAssignment = LeveDictionary[leve].JobAssignmentType;
                if (CraftFisherJobs.Contains(JobAssignment))
                {
                    uint turninNPCId = CraftDictionary[leve].LeveTurninVendorID;
                    string turninName = turninNPCId != 0 ? NPCName(turninNPCId) : "not valid";
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Turnin NPC:");
                    ImGui.TableNextColumn();

                    if (turninNPCId != 0)
                    {
                        int turninNpcButtonColors = 0;
                        if (usingIceTheme)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Button, ThemeHelper.DarkIceBlue);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(ThemeHelper.DarkIceBlue.X + 0.1f, ThemeHelper.DarkIceBlue.Y + 0.1f, ThemeHelper.DarkIceBlue.Z + 0.1f, 1.0f));
                            turninNpcButtonColors = 2;
                        }

                        if (ImGui.Button($"{turninName}###TurninNPC"))
                        {
                            var flagX = LeveNPCDict[turninNPCId].flagX;
                            var flagZ = LeveNPCDict[turninNPCId].flagZ;
                            var TurninZoneId = LeveNPCDict[turninNPCId].ZoneID;
                            SetFlagForNPC(TurninZoneId, flagX, flagZ);
                        }
                        if (turninNpcButtonColors > 0)
                        {
                            ImGui.PopStyleColor(turninNpcButtonColors);
                        }
                    }
                    else
                    {
                        ImGui.Text($"{turninName}");
                    }
                }

                ImGui.EndTable();

                ImGui.Separator();

                // Status
                if (usingIceTheme)
                {
                    int styleCount = ThemeHelper.PushHeadingTextStyle();
                    ImGui.Text("Status");
                    ImGui.PopStyleColor(styleCount);
                }
                else
                {
                    ImGui.Text("Status");
                }

                ImGui.BeginTable("status_table", 2, ImGuiTableFlags.SizingFixedFit);
                ImGui.TableNextColumn();
                ImGui.Text("Is Complete:");
                ImGui.TableNextColumn();
                if (IsComplete(leve))
                {
                    var CompleteTexture = LeveStatusDict[1].GetWrapOrEmpty();
                    ImGui.Image(CompleteTexture.ImGuiHandle, new Vector2(24, 24));
                }
                else
                {
                    var CompleteTexture = LeveStatusDict[2].GetWrapOrEmpty();
                    ImGui.Image(CompleteTexture.ImGuiHandle, new Vector2(24, 24));
                }

                // If the quest is started, indicate that
                if (IsStarted(leve))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("Quest Status:");
                    ImGui.TableNextColumn();
                    ImGui.TextColored(new Vector4(0.0f, 0.8f, 0.2f, 1.0f), "Accepted");
                }
                ImGui.EndTable();

                // If it's crafter or fisher, show required items
                if (CraftFisherJobs.Contains(JobAssignment))
                {
                    ImGui.Separator();
                    if (usingIceTheme)
                    {
                        int styleCount = ThemeHelper.PushHeadingTextStyle();
                        ImGui.Text("Required Items");
                        ImGui.PopStyleColor(styleCount);
                    }
                    else
                    {
                        ImGui.Text("Required Items");
                    }

                    if (usingIceTheme)
                    {
                        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.25f, 0.3f, 0.5f));
                    }
                    ImGui.BeginChild("###ItemInfo", new Vector2(ImGui.GetContentRegionAvail().X, 50), true);

                    ImGui.Image(CraftDictionary[leve].ItemIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(32, 32));
                    ImGui.SameLine(0, 10);
                    if (CraftDictionary[leve].RepeatAmount > 1)
                    {
                        var totalTurnin = CraftDictionary[leve].RepeatAmount;
                        ImGui.Text($"{CraftDictionary[leve].TurninAmount}({totalTurnin})x {CraftDictionary[leve].ItemName}");
                    }
                    else
                    {
                        ImGui.Text($"{CraftDictionary[leve].TurninAmount}x {CraftDictionary[leve].ItemName}");
                    }

                    ImGui.EndChild();
                    if (usingIceTheme)
                    {
                        ImGui.PopStyleColor();
                    }
                }

                // Favorites/WorkList buttons
                ImGui.Dummy(new Vector2(0, 10));
                ImGui.PushID((int)leve);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);

                int actionButtonColors = 0;
                if (usingIceTheme)
                {
                    actionButtonColors = ThemeHelper.PushButtonStyle();
                }

                // Favorite add/remove
                if (ImGui.Button(C.FavoriteLeves.Contains(leve) ? "Remove from Favorites" : "Add to Favorites",
                                 new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                {
                    if (C.FavoriteLeves.Contains(leve))
                    {
                        C.FavoriteLeves.Remove(leve);
                        C.Save();
                    }
                    else
                    {
                        C.FavoriteLeves.Add(leve);
                        C.Save();
                    }
                }

                ImGui.Spacing();

                // Checking if we have the quest unlocked
                var requiredQuestId = LeveNPCDict[vendorId].RequiredQuestId;
                bool levesQuestUnlocked = QuestChecker(requiredQuestId);

                // Gray out the add/remove from worklist if not unlocked
                using (ImRaii.Disabled(!levesQuestUnlocked))
                {
                    if (C.workList.Any(e => e.LeveID == leve))
                    {
                        if (ImGui.Button("Remove from WorkList", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                        {
                            C.workList.RemoveAll(e => e.LeveID == leve);
                            C.Save();
                        }
                    }
                    else
                    {
                        if (ImGui.Button("Add to WorkList", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                        {
                            C.workList.Add(new LeveEntry { LeveID = leve, InputValue = 1});
                            C.Save();
                        }
                    }
                }

                if (actionButtonColors > 0)
                {
                    ImGui.PopStyleColor(actionButtonColors);
                }
                ImGui.PopStyleVar();

                // If quest is locked, draw a red warning text
                if (!levesQuestUnlocked)
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, "⚠ Required quest not completed");
                    if (ImGui.IsItemHovered())
                    {
                        bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
                        bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
                        bool GridaniaNPC = IsMSQComplete(65665);  // Spirithold Broken

                        ImGui.BeginTooltip();
                        ImGui.Text("You need to complete the following quest(s) to be able to do this leve");
                        if (requiredQuestId == 0)
                        {
                            uint questId = 0;
                            if (LimsaLeveNPC)
                                questId = 66229;
                            else if (UlDahLeveNPC)
                                questId = 66223;
                            else if (GridaniaNPC)
                                questId = 65756;

                            var questName = questSheet.GetRow(questId).Name.ToString();
                            ImGui.Text(questName);
                        }
                        else if (requiredQuestId == 1)
                        {
                            uint questId1 = 0;
                            uint questId2 = 0;
                            if (LimsaLeveNPC)
                            {
                                questId1 = 66229;
                                questId2 = 65595;
                            }
                            else if (UlDahLeveNPC)
                            {
                                questId1 = 66223;
                                questId2 = 65594;
                            }
                            else if (GridaniaNPC)
                            {
                                questId1 = 65756;
                                questId2 = 65596;
                            }
                            var quest1Name = questSheet.GetRow(questId1).Name.ToString();
                            var quest2Name = questSheet.GetRow(questId2).Name.ToString();
                            if (!IsMSQComplete(questId1))
                            {
                                ImGui.Text($"  -> {quest1Name}");
                            }
                            if (!IsMSQComplete(questId2))
                            {
                                ImGui.Text($"  -> {quest2Name}");
                            }
                        }
                        else
                        {
                            var questName = questSheet.GetRow(requiredQuestId).Name.ToString();
                            ImGui.Text(questName);
                        }
                        ImGui.EndTooltip();
                    }
                }
                ImGui.PopID();
            }
            else
            {
                // If none is selected
                float centerY = ImGui.GetWindowHeight() * 0.4f;
                ImGui.SetCursorPosY(centerY);
                float textWidth = ImGui.CalcTextSize("No Leve Selected").X;
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() - textWidth) * 0.5f);
                if (usingIceTheme)
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.85f, 1.0f, 0.7f), "No Leve Selected");
                }
                else
                {
                    ImGui.TextDisabled("No Leve Selected");
                }
                ImGui.Spacing();
                ImGui.Spacing();
                string hintText = "Select a leve from the list to view details";
                float hintWidth = ImGui.CalcTextSize(hintText).X;
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() - hintWidth) * 0.5f);
                if (usingIceTheme)
                {
                    ImGui.TextColored(new Vector4(0.6f, 0.7f, 0.8f, 0.5f), hintText);
                }
                else
                {
                    ImGui.TextDisabled(hintText);
                }
            }

            ImGui.EndChild();
            if (usingIceTheme)
            {
                ImGui.PopStyleColor(rightPanelChildBgCount);
            }

            ImGui.Columns(1);

            // End theming at the end of the Draw method
            ThemeHelper.EndTheming(usingIceTheme, themingStyleCount);
        }

        /// <summary>
        /// Helper method to draw job toggle buttons for the crafters/gatherers.
        /// </summary>
        private void DrawJobToggle(uint row, ref bool state, string tooltip, bool usingIceTheme)
        {
            // If enabled, use original color icon, else grey texture
            ISharedImmediateTexture? icon = state ? LeveTypeDict[row].AssignmentIcon : GreyTexture[row];

            // Slight padding around the button
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));

            int styleCount = 1;
            int colorCount = 0;

            if (state)
            {
                // Indicate an enabled job filter with some highlight
                if (usingIceTheme)
                {
                    // Ice theme
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.15f, 0.25f, 0.38f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.Border, ThemeHelper.IceBlue);
                    colorCount = 2;
                }
                else
                {
                    // Dalamud theme
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.35f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.Border, ImGuiColors.ParsedGold);
                    colorCount = 2;
                }
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
                styleCount++;
            }
            else if (usingIceTheme)
            {
                // Disabled job with Ice theme
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.3f, 0.3f));
                colorCount = 2;
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.5f);
                styleCount++;
            }
            else
            {
                // Disabled job with Dalamud theme
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 0.1f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.4f, 0.4f, 0.4f, 0.5f));
                colorCount = 2;
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.5f);
                styleCount++;
            }

            // Rounded corners
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 2.0f);
            styleCount++;

            Vector2 size = new Vector2(26, 26);
            float zoomFactor = 0.25f; // 25% zoom-in
            float cropAmount = zoomFactor / 2; // Crop equally from all sides

            Vector2 uv0 = state ? new Vector2(0, 0) : new Vector2(cropAmount, cropAmount);
            Vector2 uv1 = state ? new Vector2(1, 1) : new Vector2(1 - cropAmount, 1 - cropAmount);

            if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, size, uv0, uv1))
            {
                state = !state;
                C.Save();
            }

            // Pop style variables and colors
            ImGui.PopStyleVar(styleCount);
            if (colorCount > 0)
            {
                ImGui.PopStyleColor(colorCount);
            }

            // Show tooltip on hover
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"{tooltip} Leves");
                ImGui.Text($"Showing: {state}");
                ImGui.EndTooltip();
            }
        }

        private void DrawJobDisable(uint row, ref bool state, string tooltip, bool usingIceTheme)
        {
            // If enabled, use original color icon, else grey texture
            ISharedImmediateTexture? icon = state ? LeveTypeDict[row].AssignmentIcon : GreyTexture[row];

            // Slight padding around the button
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));

            int styleCount = 1;
            int colorCount = 0;

            if (state)
            {
                // Indicate an enabled job filter with some highlight
                if (usingIceTheme)
                {
                    // Ice theme
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.15f, 0.25f, 0.38f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.Border, ThemeHelper.IceBlue);
                    colorCount = 2;
                }
                else
                {
                    // Dalamud theme
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.35f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.Border, ImGuiColors.ParsedGold);
                    colorCount = 2;
                }
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
                styleCount++;
            }
            else if (usingIceTheme)
            {
                // Disabled job with Ice theme
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.3f, 0.3f));
                colorCount = 2;
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.5f);
                styleCount++;
            }
            else
            {
                // Disabled job with Dalamud theme
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 0.1f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.4f, 0.4f, 0.4f, 0.5f));
                colorCount = 2;
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.5f);
                styleCount++;
            }

            // Rounded corners
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 2.0f);
            styleCount++;

            Vector2 size = new Vector2(26, 26);
            float zoomFactor = 0.25f; // 25% zoom-in
            float cropAmount = zoomFactor / 2; // Crop equally from all sides

            Vector2 uv0 = state ? new Vector2(0, 0) : new Vector2(cropAmount, cropAmount);
            Vector2 uv1 = state ? new Vector2(1, 1) : new Vector2(1 - cropAmount, 1 - cropAmount);

            if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, size, uv0, uv1))
            {
                state = false;
                C.Save();
            }

            // Pop style variables and colors
            ImGui.PopStyleVar(styleCount);
            if (colorCount > 0)
            {
                ImGui.PopStyleColor(colorCount);
            }

            // Show tooltip on hover
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"{tooltip} Leves");
                ImGui.Text($"Showing: {state}");
                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// The filter logic for whether a leve should be displayed in the middle panel.
        /// </summary>
        private bool FilterLeve(uint leveId)
        {
            var showLeve = true;
            var jobAssignmentType = LeveDictionary[leveId].JobAssignmentType;

            // If "Only Favorites" is on, ignore everything except favorite status
            if (C.OnlyFavorites)
            {
                return C.FavoriteLeves.Contains(leveId);
            }

            // Level filter
            if (C.LevelFilter > 0)
            {
                showLeve &= LeveDictionary[leveId].Level == C.LevelFilter;
            }

            // Completed / incomplete filter
            if (C.CompleteFilter == 1)
            {
                showLeve &= IsComplete(leveId);
            }
            else if (C.CompleteFilter == 2)
            {
                showLeve &= !IsComplete(leveId);
            }

            // Name filter
            if (!string.IsNullOrEmpty(C.NameFilter))
            {
                showLeve &= LeveDictionary[leveId].LeveName.Contains(C.NameFilter, StringComparison.OrdinalIgnoreCase);
            }

            // Filter out jobs that are not currently shown
            showLeve &= showLeve && !C.GetJobFilter().Contains(jobAssignmentType);

            // Update VisibleLeves collection for external references
            if (VisibleLeves.Contains(leveId) && !showLeve)
            {
                VisibleLeves.Remove(leveId);
            }
            else if (!VisibleLeves.Contains(leveId) && showLeve)
            {
                VisibleLeves.Add(leveId);
            }
            return showLeve;
        }


        /// Draws the header at the top of the window showing plugin name, allowances, etc.

        private void DrawSimpleHeader(bool usingIceTheme)
        {
            float fontScale = ImGui.GetIO().FontGlobalScale;
            float textLineHeight = ImGui.GetTextLineHeight();
            // Increase padding to ensure text is not cut off
            float headerPadding = textLineHeight * 1.5f;

            // Get the title bar background color based on the theme
            Vector4 titleBgColor = usingIceTheme ? ThemeHelper.DeepIceBlue : ImGui.GetStyle().Colors[(int)ImGuiCol.TitleBgActive];
            
            // Calculate the header size with extra height to ensure text is visible
            Vector2 headerSize = new Vector2(ImGui.GetWindowWidth(), textLineHeight + headerPadding * 2);

            // Draw the title bar background
            ImGui.GetWindowDrawList().AddRectFilled(
                ImGui.GetWindowPos(),
                ImGui.GetWindowPos() + headerSize,
                ImGui.ColorConvertFloat4ToU32(titleBgColor)
            );

            // Add allowances information centered in the header
            string allowancesText = $"Allowances: {Allowances}/100";
            string nextText = $"Next in: {NextAllowances:hh':'mm':'ss}";
            string sepText = " | ";
            
            // Calculate text dimensions
            float sepWidth = ImGui.CalcTextSize(sepText).X;
            float allowancesWidth = ImGui.CalcTextSize(allowancesText).X;
            float nextWidth = ImGui.CalcTextSize(nextText).X;
            
            float totalInfoWidth = allowancesWidth + sepWidth + nextWidth;
            
            // Calculate exact center positions for perfect alignment
            float windowWidth = ImGui.GetWindowWidth();
            float centerX = windowWidth / 2;
            float textStartX = centerX - (totalInfoWidth / 2);
            float centerY = headerPadding + (textLineHeight / 2);
            
            if (usingIceTheme)
            {
                ThemeHelper.PushHeadingTextStyle();
            }
            
            // Position and draw allowances text
            ImGui.SetCursorPos(new Vector2(textStartX, centerY));
            ImGui.Text(allowancesText);
            
            // Position and draw separator
            textStartX += allowancesWidth;
            ImGui.SetCursorPos(new Vector2(textStartX, centerY));
            ImGui.Text(sepText);
            
            // Position and draw next text
            textStartX += sepWidth;
            ImGui.SetCursorPos(new Vector2(textStartX, centerY));
            ImGui.Text(nextText);
            
            if (usingIceTheme)
            {
                ImGui.PopStyleColor();
            }

            // Add padding for the rest of the content
            ImGui.SetCursorPosY(headerSize.Y);
        }
    }
}
