using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.NameUtility;
using Dalamud.Interface.Utility.Raii;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class User_Info
    {
        private static bool _anonymizeNames = false;
        private static bool _showHidden = false;

        private static ImGuiEx.RealtimeDragDrop<ulong> CharacterDrop = new("CharacterOrder", (id) => id.ToString());

        public static void Draw()
        {
            var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

            using (var child = ImRaii.Child("Child: User Info", default, true))
            {
                var characterData = C.CharacterInfo;

                if (characterData.Count() == 0)
                {
                    ImGui.Text($"No characters currently exist here");
                }

                if (ImGui.Checkbox("Anonymize names (for screenshots)", ref _anonymizeNames))
                {
                    // Fresh names each time it's switched on, so old sessions don't leak a
                    // consistent fake identity across unrelated screenshots.
                    if (_anonymizeNames)
                        NameAnonymizer.ClearCache();
                }

                if (_anonymizeNames)
                {
                    ImGui.SameLine();
                    ImGui_Ice.Icon(FontAwesomeIcon.QuestionCircle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Replaces character names below with randomized placeholder names.\n" +
                            "Only affects what's shown here - your real data is untouched.");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine();

                ImGui.Checkbox("Show Hidden Characters", ref _showHidden);

                foreach (var order in characterData)
                {
                    if (!C.Character_Order.Contains(order.Key))
                        C.Character_Order.Add(order.Key);

                    C.SaveDebounced();
                }

                CharacterDrop.Begin();

                using (var table = ImRaii.Table("Character Info", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("##ReOrder");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("World");
                    ImGui.TableSetupColumn("Leve Count [Estimate]");
                    ImGui.TableSetupColumn("##Bar");
                    ImGui.TableSetupColumn("Last Updated");
                    ImGui.TableSetupColumn("Show Alert");
                    ImGui.TableSetupColumn("Hide");


                    ImGui.TableHeadersRow();

                    for (int i = 0; i < C.Character_Order.Count(); i++)
                    {
                        var id = C.Character_Order[i];

                        if (characterData.TryGetValue(id, out var character))
                        {
                            if (character.Blacklisted && !_showHidden)
                                continue;

                            var value = character;

                            ImGui.PushID($"{id}_{value.Name}");

                            ImGui.TableNextRow();
                            CharacterDrop.NextRow();
                            CharacterDrop.SetRowColor(id);

                            ImGui.TableSetColumnIndex(0);
                            CharacterDrop.DrawButtonDummy(id, C.Character_Order, i);

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();

                            if (_anonymizeNames)
                            {
                                var displayName = NameAnonymizer.GetFakeName($"{value.World}_{value.Name}");
                                ImGui.Text(displayName);

                                // Small inline reroll button in case a generated name looks awkward
                                // or too close to the real one.
                                ImGui.SameLine();
                                if (ImGuiEx.IconButton(FontAwesomeIcon.Dice))
                                {
                                    NameAnonymizer.Reroll($"{value.World}_{value.Name}");
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text("Reroll this character's placeholder name");
                                    ImGui.EndTooltip();
                                }
                            }
                            else
                            {
                                ImGui.Text($"{value.Name}");
                            }

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(_anonymizeNames ? "***" : $"{value.World}");

                            ImGui.TableNextColumn();
                            var estimatedLeves = Utils.EstimateCurrentAllowance(value);
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{estimatedLeves} / 100");
                            if (estimatedLeves != value.LastKnownAllowance)
                            {
                                ImGui.SameLine();
                                ImGui_Ice.Icon(FontAwesomeIcon.QuestionCircle);
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text("This is a route estimate of what your leves should be right now.\n" +
                                        "Let me know if the math doesn't math properly\n" +
                                        $"Last logged in leve count: {value.LastKnownAllowance}");
                                    ImGui.EndTooltip();
                                }
                            }

                            ImGui.TableNextColumn();
                            var height = ImGui.GetFrameHeight();
                            ImGui_Ice.Draw_XPBar(estimatedLeves, 100, size: new(200, height));

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{ToRelativeTimeString(value.Time_LastObserved)}");

                            ImGui.TableNextColumn();
                            var showAlert = value.AllowNotification;
                            if (ImGui.Checkbox("##AlertWhenAmount", ref showAlert))
                            {
                                value.AllowNotification = showAlert;
                                C.Save();
                            }

                            ImGui.TableNextColumn();
                            bool shiftHeld = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
                            bool cntrlHeld = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);

                            using (ImRaii.Disabled(!(shiftHeld && cntrlHeld)))
                            {
                                bool hide = value.Blacklisted;
                                if (ImGui.Checkbox("##HideCharacter", ref hide))
                                {
                                    value.Blacklisted = hide;
                                    C.Save();
                                }
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("Will hide this character from being shown in the table.\n" +
                                    "Hold Shift + Control to press");
                                ImGui.EndTooltip();
                            }

                            ImGui.PopID();
                        }
                    }
                }

                CharacterDrop.End();
            }
        }

        private static string ToRelativeTimeString(DateTime timestampUtc)
        {
            if (timestampUtc == DateTime.MinValue)
                return "Never";

            var elapsed = DateTime.UtcNow - timestampUtc;

            if (elapsed.TotalSeconds < 60)
                return "Just now";
            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes}m ago";
            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours}h ago";

            int days = (int)elapsed.TotalDays;
            int hours = elapsed.Hours; // remainder hours after whole days, 0-23

            return hours > 0 ? $"{days}d {hours}h ago" : $"{days}d ago";
        }
    }
}