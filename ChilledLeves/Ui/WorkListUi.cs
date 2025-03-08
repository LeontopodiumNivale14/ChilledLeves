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

        private static Dictionary<uint, CrafterDataDict> WorklistDict = new Dictionary<uint, CrafterDataDict>();

        public override void Draw()
        {
            foreach (var key in C.workList)
            {
                uint leveId = key.LeveID;

                if (!WorklistDict.ContainsKey(leveId))
                {
                    WorklistDict.Add(leveId, new CrafterDataDict()
                    {
                        Amount = key.ItemAmount,
                        JobAssignmentType = CrafterLeves[leveId].JobAssignmentType,
                        Level = CrafterLeves[leveId].Level,
                        LeveName = CrafterLeves[leveId].LeveName,
                        ItemName = CrafterLeves[leveId].ItemName,
                        ItemID = CrafterLeves[leveId].ItemID,
                        TurninAmount = CrafterLeves[leveId].TurninAmount,
                    });
                }
            }

            foreach (var entry in WorklistDict)
            {
                bool exist = C.workList.Any(e => e.LeveID == entry.Key);
                if (!exist)
                    WorklistDict.Remove(entry.Key);
            }

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

            float col0Width = 0f;
            float col1Width = 0f;
            float col2Width = 0f;
            float col3Width = 0f;
            float col4Width = 0f;
            float col5Width = 0f;
            float col6Width = 0f;

            if (ImGui.BeginTable("Workshop List", 7, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders))
            {
                // All the worklist leve columns
                ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, col0Width);
                ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, col1Width);
                ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, col2Width);
                ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, col3Width);
                ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, col4Width);
                ImGui.TableSetupColumn("Have?###CrafterCompleteCheck", ImGuiTableColumnFlags.WidthFixed, col5Width);
                ImGui.TableSetupColumn("Remove###CrafterLevesRemoveWorkList", ImGuiTableColumnFlags.WidthFixed, col6Width);

                // Making sure that the header row exist...
                ImGui.TableHeadersRow();

                foreach (var leve in WorklistDict)
                {
                    uint leveId = leve.Key;
                    uint level = leve.Value.Level;
                    uint jobId = leve.Value.JobAssignmentType;
                    var jobIcon = LeveTypeDict[jobId].AssignmentIcon;
                    string leveName = leve.Value.LeveName;
                    string itemName = leve.Value.ItemName;
                    uint itemId = leve.Value.ItemID;
                    int itemAmount = leve.Value.Amount;
                    int turninAmount = leve.Value.TurninAmount;

                    ImGui.TableNextRow();

                    ImGui.PushID((int)leveId);
                    ImGui.TableSetColumnIndex(0);
                    CenterText($"{level}");
                    col0Width = Math.Max(col0Width, "Level".Length);

                    ImGui.TableNextColumn();
                    ImGui.Image(jobIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(25, 25));
                    ImGui.SameLine(0, 5);
                    ImGui.AlignTextToFramePadding();
                    CenterTextInHeight($"{leveName}");
                    col1Width = Math.Max(col1Width, leveName.Length);

                    ImGui.TableNextColumn();
                    var inputValue = 0;
                    foreach (var kdp in C.workList)
                    {
                        if (kdp.LeveID == leveId)
                        {
                            var input = kdp.InputValue > 0 ? kdp.InputValue.ToString() : "0";
                            CenterInputTextInHeight("###Input Run Amount", ref input, 3);
                            if (uint.TryParse(input, out var num) && num > 0 && num <= 100)
                            {
                                kdp.InputValue = (int)num;
                                inputValue = kdp.InputValue;
                                C.Save();
                            }
                        }
                    }
                    col2Width = 75;

                    ImGui.TableNextColumn();
                    CenterTextInHeight(itemName);
                    col3Width = Math.Max(col3Width, itemName.Length);

                    ImGui.TableNextColumn();
                    int amountNeeded = inputValue * turninAmount;
                    if (amountNeeded > 0)
                    {
                        CenterText(amountNeeded.ToString());
                    }
                    else if (amountNeeded < 0)
                    {
                        amountNeeded = 0;
                        CenterText(amountNeeded.ToString());
                    }
                    col4Width = Math.Max(col4Width, "Need".Length);

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
                    col5Width = Math.Max(col5Width, "Have".Length);

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Remove From LeveList"))
                    {
                        foreach (var entry in C.workList)
                        {
                            if (entry.LeveID == leveId)
                            {
                                C.workList.Remove(entry);
                                break;
                            }
                        }
                    }
                    col6Width = 75;

                    ImGui.PopID();
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
    }
}
