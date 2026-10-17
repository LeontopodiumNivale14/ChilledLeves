using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal class Window_CraftDetails : Window
    {
        public Window_CraftDetails() : base($"Chilled Leves: Item Info ###ChilledLeves_ItemInfo")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new(200, 200)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        private static bool IncludeCrystals = false;
        private static bool SubtractInventory = false;
        private static bool HighlightComplete = false;

        public override void Draw()
        {
            using var colors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.Header, Theme_Colors.HeaderBg) : default;
            if (C.UseIceTheme)
            {
                colors.Push(ImGuiCol.HeaderHovered, Theme_Colors.HeaderHovered);
                colors.Push(ImGuiCol.HeaderActive, Theme_Colors.HeaderActive);
                colors.Push(ImGuiCol.Button, Theme_Colors.ButtonBg);
                colors.Push(ImGuiCol.ButtonHovered, Theme_Colors.ButtonHovered);
                colors.Push(ImGuiCol.ButtonActive, Theme_Colors.ButtonActive);
                colors.Push(ImGuiCol.WindowBg, Theme_Colors.DarkSlate);
                colors.Push(ImGuiCol.FrameBg, Theme_Colors.FrameBg);
                colors.Push(ImGuiCol.FrameBgHovered, Theme_Colors.FrameBgHovered);
                colors.Push(ImGuiCol.FrameBgActive, Theme_Colors.FrameBgActive);
                colors.Push(ImGuiCol.CheckMark, Theme_Colors.IceBlue);
                colors.Push(ImGuiCol.Text, Theme_Colors.FrostWhite);
            }

            using var style = C.UseIceTheme ? ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 4.0f) : default;

            ImGui_Ice.IconToggle(FontAwesomeIcon.Gem, "Include Gems", ref IncludeCrystals, "Show Crystals");
            ImGui.SameLine();
            ImGui_Ice.IconToggle(FontAwesomeIcon.Calculator, "Calculate Inventory", ref SubtractInventory, "Subtract what you already own from the totals. Affects the Need column, pre-crafts, ingredients and exports. Press Calculate to apply.");
            ImGui.SameLine();
            ImGui_Ice.IconToggle(FontAwesomeIcon.Highlighter, "Highlight Complete", ref HighlightComplete, "Highlight Inventory");

            if (ImGui.Button("Calculate Items"))
            {
                Utils.Calculate(IncludeCrystals, SubtractInventory);
            }

            using var tabBar = ImRaii.TabBar("ChilledLeves: Item Tab Bar");
            if (!tabBar.Success)
                return;

            DrawAllTab();
            DrawMainCraftsTab();
            DrawPreCraftsTab();
            DrawIngredientsTab();
        }

        private static void DrawAllTab()
        {
            using var tab = ImRaii.TabItem("All Items");
            if (!tab.Success)
                return;

            using var child = ImRaii.Child("All Item Child", default, true);
            if (!child.Success)
                return;

            MainTable();
            PreCraftTable();
            IngredientTable();
        }

        private static void DrawMainCraftsTab()
        {
            using var tab = ImRaii.TabItem("Main Crafts");
            if (!tab.Success)
                return;

            using var child = ImRaii.Child("Main Craft Child", default, true);
            if (!child.Success)
                return;

            MainTable();
        }

        private static void DrawPreCraftsTab()
        {
            using var tab = ImRaii.TabItem("Pre Crafts");
            if (!tab.Success)
                return;

            using var child = ImRaii.Child("Pre Craft Child", default, true);
            if (!child.Success)
                return;

            PreCraftTable();
        }

        private static void DrawIngredientsTab()
        {
            using var tab = ImRaii.TabItem("Ingredients");
            if (!tab.Success)
                return;

            using var child = ImRaii.Child("Ingredients Child", default, true);
            if (!child.Success)
                return;

            IngredientTable();
        }

        private static void MainTable()
        {
            if (ImGui.Button("Copy Main Item List"))
            {
                Utils.ExportFinalCrafts();
            }

            DrawItemTable("Main Craft Items", Utils.ArtisanFinalCrafts, showCrafts: true);
        }

        private static void PreCraftTable()
        {
            if (ImGui.Button("Copy Pre-Craft List"))
            {
                Utils.ExportPreCrafts();
            }

            DrawItemTable("Pre Craft Items", Utils.ArtisanPreCrafts, showCrafts: true);
        }

        private static void IngredientTable()
        {
            if (ImGui.Button("Export to Gather & Chill"))
            {
                var amountExported = Utils.ExportToGather(IncludeCrystals);
                if (amountExported > 0)
                {
                    Notify.Success($"Amount of items exported to Gather & Chill: {amountExported}");
                }
            }

            DrawItemTable("Raw Material Items", Utils.RawMaterials, showCrafts: false);
        }

        private static void DrawItemTable(string id, IEnumerable<KeyValuePair<uint, int>> items, bool showCrafts)
        {
            var showInventory = Utils.UsedInventory;

            var columnCount = 3;
            if (showInventory)
                columnCount += 2;
            if (showCrafts)
                columnCount += 1;

            using var table = ImRaii.Table(id, columnCount, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            if (!table.Success)
                return;

            ImGui.TableSetupColumn("Icon");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Amount");
            if (showInventory)
            {
                ImGui.TableSetupColumn("Have");
                ImGui.TableSetupColumn("Need");
            }
            if (showCrafts)
                ImGui.TableSetupColumn("Crafts");
            ImGui.TableHeadersRow();

            var completeColor = ImGui.GetColorU32(new Vector4(0.2f, 0.7f, 0.3f, 0.35f));

            foreach (var item in items)
            {
                if (!ExcelHelper.Sheet_Item.TryGetRow(item.Key, out var itemInfo))
                    continue;

                var owned = Utils.GetItemCount(item.Key);
                var isComplete = HighlightComplete && owned >= item.Value;

                ImGui.TableNextRow();

                // Must come after TableNextRow so it applies to this row.
                if (isComplete)
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, completeColor);

                ImGui.TableSetColumnIndex(0);
                GameIcons.DrawInline(itemInfo.Icon, false);

                ImGui.TableNextColumn();
                ImGui.Text(itemInfo.Name.ToString());
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"{item.Key}");
                    ImGui.EndTooltip();
                }

                ImGui.TableNextColumn();
                ImGui.Text($"{item.Value:N0}");

                if (showInventory)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{owned:N0}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{Utils.GetNeeded(item.Key, item.Value):N0}");
                }

                if (showCrafts)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{Utils.GetCraftCount(item.Key, item.Value):N0}");
                }
            }
        }
    }
}