using ChilledLeves.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui
{
    internal class WorkListUi : Window
    {
        public WorkListUi() : 
            base($"Worklist [ChilledLeves] {P.GetType().Assembly.GetName().Version} ###WorkListChilledLeves")
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
            WorklistMode();
        }

        #region

        public static void WorklistMode()
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

            string artisan = "Copy for Artisan";
            var CopyButton = artisan.Length;

            ImGui.SetNextItemWidth(CopyButton);
            if (ImGui.Button(artisan))
            {
                string importString = string.Empty;

                importString = "Items : \n";

                foreach (var kdp in CrafterLeves)
                {
                    var leveID = kdp.Key;
                    var itemId = kdp.Value.ItemID;
                    var itemName = kdp.Value.ItemName;
                    var itemAmount = kdp.Value.TurninAmount;
                    var jobType = kdp.Value.JobAssignmentType;

                    if (!C.workList.Any(x => x.LeveID == leveID))
                    {
                        continue;
                    }
                    if (!CraftFisherJobs.Contains(jobType))
                    {
                        continue;
                    }


                    var WorklistInput = C.workList.Where(x => x.LeveID == leveID).FirstOrDefault();
                    var InputAmount = WorklistInput.InputValue;
                    var AmountNeeded = InputAmount * itemAmount;

                    string temp = $"{AmountNeeded}x {itemName}\n";

                    importString += temp;
                }

                ImGui.SetClipboardText($"{importString}");
            }
            ImGuiEx.HelpMarker("This lets you copy your worklist -> a format to import into Artisan \n" +
                               "You can import it by going to \"Crafting List\" -> \"Teamcraft List Import\" \n" +
                               "MAKE SURE TO PUT THIS IN FINAL ITEMS");

            float col0Width = 0f;
            float col1Width = 0f;
            float col2Width = 0f;
            float col3Width = 0f;
            float col4Width = 0f;
            float col5Width = 0f;
            float col6Width = 0f;

            float totalWidth = col0Width + col1Width + col2Width + col3Width + col4Width + col5Width + col6Width;

            if (ImGui.BeginTable($"Workshop List", 7, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Reorderable))
            {
                ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, col0Width);
                ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, col1Width);
                ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, col2Width);
                ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, col3Width);
                ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, col4Width);
                ImGui.TableSetupColumn("Have?###CrafterCompleteCheck", ImGuiTableColumnFlags.WidthFixed, col5Width);
                ImGui.TableSetupColumn("Remove###CrafterLevesRemoveWorkList");

                ImGui.TableHeadersRow();

                foreach (var kdp in CrafterLeves)
                {
                    var leveID = kdp.Key;
                    var leveLevel = kdp.Value.Level;
                    var leveName = kdp.Value.LeveName;
                    var jobAssignment = kdp.Value.JobAssignmentType;
                    var jobIcon = LeveTypeDict[jobAssignment].AssignmentIcon;

                    var ItemImage = kdp.Value.ItemIcon.GetWrapOrEmpty();
                    var itemName = kdp.Value.ItemName;
                    var itemNeed = kdp.Value.TurninAmount;
                    var itemId = kdp.Value.ItemID;

                    if (!C.workList.Any(x => x.LeveID == leveID))
                    {
                        continue;
                    }

                    ImGui.TableNextRow();

                    // Colomn 0 | Level Column
                    ImGui.PushID((int)leveID);
                    ImGui.TableSetColumnIndex(0);
                    CenterText($"{leveLevel}");
                    col0Width = Math.Max(col0Width, "Level".Length);

                    // Column 1 | JobIcon + Leve Name
                    ImGui.TableNextColumn();
                    ImGui.Image(jobIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(25, 25));
                    ImGui.SameLine(0, 5);
                    ImGui.AlignTextToFramePadding();
                    CenterTextInHeight($"{leveName}");
                    col1Width = Math.Max(col1Width, leveName.Length + 10);

                    // Column 2 | Amount to Run
                    ImGui.TableNextColumn();
                    var WorklistInput = C.workList.Where(x => x.LeveID == leveID).FirstOrDefault();
                    var input = WorklistInput.InputValue;

                    float availableWidth = ImGui.GetContentRegionAvail().X;  // Get remaining space in the column
                    ImGui.SetNextItemWidth(availableWidth);  // Set the slider width
                    ImGui.SliderInt("###RunAmountSlider", ref input, 1, 100);
                    if (input < 1)
                        input = 1;
                    else if (input > 100)
                        input = 100;
                    if (WorklistInput.InputValue != input)
                    {
                        WorklistInput.InputValue = input;
                        C.Save();
                    }
                    col2Width = 75;

                    // Column 3 | Item Turnin
                    ImGui.TableNextColumn();
                    ImGui.Image(ItemImage.ImGuiHandle, new Vector2(25, 25));
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Image(ItemImage.ImGuiHandle, new Vector2(50, 50));
                        ImGui.EndTooltip();
                    }
                    ImGui.SameLine(0, 5);
                    ImGui.AlignTextToFramePadding();
                    CenterTextInHeight($"{itemName}");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        ImGui.SetClipboardText(itemName);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Left click to copy item to clipboard");
                        ImGui.EndTooltip();
                    }
                    col3Width = Math.Max(col3Width, itemName.Length + 15);

                    // Column 4 | Need
                    ImGui.TableNextColumn();
                    int needAmount = WorklistInput.InputValue * itemNeed;
                    if (needAmount < 0)
                        needAmount = 0;
                    CenterText(needAmount.ToString());
                    col4Width = Math.Max(col4Width, "Need".Length);

                    // Column 5 | Have
                    ImGui.TableNextColumn();
                    int CurrentAmount = GetItemCount((int)itemId);
                    bool hasEnough = needAmount <= CurrentAmount;
                    if (needAmount != 0)
                    {
                        if (hasEnough)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, EColor.Green);
                        }
                        else if (!hasEnough)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, EColor.Red);
                        }
                        CenterText(CurrentAmount.ToString());
                        ImGui.PopStyleColor();
                    }
                    col5Width = Math.Max(col5Width, "Have".Length);

                    // Column 6 | Remove Leve from worklist
                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Remove From LeveList"))
                    {
                        C.workList.Remove(WorklistInput);
                    }
                    col6Width = 75;

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

        #endregion
    }
}
