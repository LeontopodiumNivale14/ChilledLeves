using Dalamud.Interface.Colors;
using ECommons.Automation;
using ECommons.ChatMethods;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace ChilledLeves.Ui.MainWindow;

internal class CraftingLeves
{
    private static float maxWidth = 0.0f;
    private static float maxItemWidth = 0.0f;
    private static float maxLocationWidth = 0.0f;
    private static uint CurrentClass = 0;
    private static string LocationSelected = "All";

    public static void Draw()
    {
        if (ImGuiEx.TreeNode(ImGuiColors.DalamudWhite, "Leve Filters"))
        {
            ImGui.Text("Test");

            // Radio Buttons for me to to check and see the filters are being applied properly
            for (int i = 0; i < CraftingClass.Count; i++)
            {
                ImGui.RadioButton($"##Active {i}", CraftingClassActive[i]);
                if (i < CraftingClass.Count - 1)
                    ImGui.SameLine(0, 2);
            }

            // Actual buttons for each kind of crafting leve
            for (int i = 0; i < CraftingClass.Count; i++)
            {
                uint craftingClass = CraftingClass[i];

                if (i == 0)
                {
                    AllLeves(new Vector2(40, 40));
                    if (ImGui.IsItemClicked())
                    {
                        CurrentClass = (uint)i;
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("All Crafters");
                        ImGui.EndTooltip();
                    }
                }
                else if (i != 0)
                {
                    uint ClassSheet = CraftingClass[i];
                    ImGui.SameLine(0, 10);
                    LeveJobIcons(ClassSheet, new Vector2(40, 40));
                    string className = Svc.Data.GetExcelSheet<LeveAssignmentType>().GetRow(ClassSheet).Name.ToString();
                    if (ImGui.IsItemClicked())
                    {
                        CurrentClass = CraftingClass[i];
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"{Svc.Data.GetExcelSheet<LeveAssignmentType>().GetRow(ClassSheet).Name.ToString()}");
                        ImGui.EndTooltip();
                    }

                }
            }

            for (int i = 0; i < AllLocations.Count; i++)
            {
                ImGui.Text($"Location: {AllLocations[i]}");
                ImGui.SameLine(0, 5);
                ImGui.Text($"Bool: {LocationsActive[i]}");
            }

            ImGui.SetNextItemWidth(maxLocationWidth + 40);
            if (ImGui.BeginCombo("##Mode Select Combo", LocationSelected))
            {
                for (int i = 0; i < AllLocations.Count; i++)
                {
                    var option = AllLocations[i];

                    // Render the selectable option
                    if (ImGui.Selectable(option, option == LocationSelected))
                    {
                        LocationSelected = option;
                    }

                    // Set focus to the selected item for better UX
                    if (option == LocationSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }

                    // Display tooltip if this option is hovered
                }
                ImGui.EndCombo();
            }

            ImGui.TreePop();
        }

        for (var i = 0; i < CraftingClass.Count; i++)
        {
            CraftingClassActive[i] = CraftingClass[i] == CurrentClass;
        }

        for (var i = 0; i < AllLocations.Count; i++)
        {
            LocationsActive[i] = AllLocations[i] == LocationSelected;
        }

        if (ImGui.BeginTable("All Items", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg, (new (maxItemWidth + maxItemWidth + 300, 25))))
        {
            ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, maxWidth);
            ImGui.TableSetupColumn("Run Amount", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Turnin Item", ImGuiTableColumnFlags.WidthFixed, maxItemWidth);
            ImGui.TableSetupColumn("Need", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Have", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, 100);

            ImGui.TableHeadersRow();

            foreach (var iLeves in Leves.CrafterLeves)
            {
                uint leve = iLeves.LeveID;

                // Info Block 1
                var leveJobType = Svc.Data.GetExcelSheet<Leve>().GetRow(leve).LeveAssignmentType.Value.RowId;
                if (!CraftingClass.Contains(0))
                {
                    CraftingClass.Add(0);
                    CraftingClassActive.Add(true);
                }
                else if (!CraftingClass.Contains(leveJobType))
                {
                    CraftingClass.Add(leveJobType);
                    CraftingClassActive.Add(false);
                }
                string leveName = Svc.Data.GetExcelSheet<Leve>().GetRow(leve).Name.ToString();
                float textWidth = ImGui.CalcTextSize(leveName).X + 30.0f; // Add padding
                maxWidth = Math.Max(maxWidth, textWidth);

                // Column 3
                int amount = iLeves.Amount; // amount to run

                // Column 4
                // QuestID/DataID to look up in the crafting sheet
                uint questID = Svc.Data.GetExcelSheet<Leve>().GetRow(leve).DataId.RowId;
                uint itemID = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Item[0].RowId;
                int repeatAmount = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Repeats.ToInt();
                int necessaryAmount = 0;

                // Column ??? 
                var startingCity = Svc.Data.GetExcelSheet<Leve>().GetRow(leve).Town.RowId;
                string zoneName = Svc.Data.GetExcelSheet<Town>().GetRow(startingCity).Name.ToString();
                if (!AllLocations.Contains("All"))
                {
                    AllLocations.Add("All");
                    LocationsActive.Add(true);
                    maxLocationWidth = Math.Max(maxLocationWidth, ImGui.CalcTextSize("All").X);
                }
                else if (!AllLocations.Contains(zoneName))
                {
                    AllLocations.Add(zoneName);
                    LocationsActive.Add(false);
                    maxLocationWidth = Math.Max(maxLocationWidth, ImGui.CalcTextSize(zoneName).X);
                }


                int town = (int)Svc.Data.GetExcelSheet<Leve>().GetRow(leve).Town.RowId;

                if (IsRowEnabled(leveJobType, CraftingClassActive[0]) && IsLocationEnabled(zoneName, LocationsActive[0]))
                {
                    ImGui.TableNextRow();
                    // Leve Name
                    ImGui.TableSetColumnIndex(0);
                    LeveJobIcons(leveJobType);
                    ImGui.SameLine(0, 10);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(leveName);

                    // Run Amount
                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt($"##AmountToRun-Leve{leve}", ref amount))
                    {
                        if (amount < 0)
                            amount = 0;
                        else if (amount > 100)
                            amount = 100;
                        iLeves.Amount = amount;
                    }

                    // Turnin Item
                    ImGui.TableSetColumnIndex(2);

                    ItemIconLookup(itemID);
                    string itemName = Svc.Data.GetExcelSheet<Item>().GetRow(itemID).Name.ToString();
                    ImGui.SameLine(0, 10);
                    ImGui.Text(itemName);
                    float textWidth2 = ImGui.CalcTextSize(itemName).X + 40.0f; // Add padding
                    maxItemWidth = Math.Max(maxItemWidth, textWidth2);

                    // Need this many items
                    ImGui.TableSetColumnIndex(3);
                    for (int x = 0; x < 3; x++)
                    {
                        if (itemID != 0)
                        {
                            necessaryAmount = necessaryAmount + Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).ItemCount[x].ToInt();
                        }
                    }

                    if (repeatAmount != 0)
                        necessaryAmount = necessaryAmount * (repeatAmount + 1);
                    ImGui.Text($"{necessaryAmount}");

                    // Have this many items
                    ImGui.TableSetColumnIndex(4);
                    string currentItemCount = GetItemCount((int)itemID).ToString();
                    if (GetItemCount((int)itemID) != necessaryAmount)
                    {
                        ImGui.TextColored(ImGuiColors.DalamudRed, $"{currentItemCount}");
                    }
                    else
                    {
                        ImGui.Text($"{currentItemCount}");
                    }

                    // Location of Leve Person
                    ImGui.TableSetColumnIndex(5);
                    ImGui.Text($"{zoneName}");
                }
            }
        }

        ImGui.EndTable();
    }
}
