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

        if (ImGui.BeginTable($"Crafting Workshop List", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            // Columns for the crafters
            ImGui.TableSetupColumn("Level###CrafterLevels", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Leve Name###CrafterLeveNames", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Run Amount###CrafterRunAmounts", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Item Turnin###CrafterTurninItems", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Need###CrafterAmountNecessary", ImGuiTableColumnFlags.WidthFixed, 50);

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
                ImGui.TableNextRow();

                ImGui.PushID((int)leveID);
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{level}");

                ImGui.TableNextColumn();
                ImGui.Image(jobIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(25, 25));
                ImGui.SameLine(0, 5);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{LeveName}");

                ImGui.TableNextColumn();
                var input = kdp.InputValue > 0 ? kdp.InputValue.ToString() : "0";
                ImGui.SetNextItemWidth(75);
                if (ImGui.InputText("###Level", ref input, 3))
                {
                    kdp.InputValue = (int)(uint.TryParse(input, out var num) && num > 0 && num <= 100 ? num : 0);
                    C.Save();
                }

                ImGui.TableNextColumn();
                ImGui.Text(ItemName);

                ImGui.TableNextColumn();
                int amountNeeded = kdp.InputValue * CrafterLeves[leveID].TurninAmount;
                ImGui.Text(amountNeeded.ToString());

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
}
