using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.NameUtility;
using Dalamud.Interface.Utility.Raii;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class User_Info
    {
        private static bool _anonymizeNames = false;

        public static void Draw()
        {
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

                using (var table = ImRaii.Table("Character Info", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("World");
                    ImGui.TableSetupColumn("Leve Count [Estimate]");
                    ImGui.TableSetupColumn("##Bar");
                    ImGui.TableSetupColumn("Last Updated");
                    ImGui.TableSetupColumn("Show Alert");
                    ImGui.TableSetupColumn("Hide Character?");


                    ImGui.TableHeadersRow();

                    foreach (var character in characterData)
                    {
                        var id = character.Key;
                        var value = character.Value;

                        ImGui.PushID($"{id}_{value.Name}");

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
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
                            if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt))
                            {

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