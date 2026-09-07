using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Utilities.GatheringHelper;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using System.Collections.Generic;
using static ChilledLeves.Config_Files.Config;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Gathering_Profiles
    {
        public static void Draw()
        {
            using (var table = ImRaii.Table("Gathering Profile Editor", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable))
            {
                if (!table.Success)
                    return;

                ImGui.TableSetupColumn("Profile Names", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Profile Editor", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                ProfileSelector();

                ImGui.TableNextColumn();
                ProfileEditor();
            }
        }

        private static int SelectedProfile = 0;

        private static void ProfileSelector()
        {
            using (var child = ImRaii.Child("Gather: Profile Selection", default, true))
            {
                if (!child.Success)
                    return;

                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "Add new profile"))
                {
                    C.AddNewGatheringProfile();
                }
                ImGui.SameLine();
                using (ImRaii.Disabled(SelectedProfile < 5))
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Remove Current Profile"))
                    {
                        if (C.FindGatherProfile(SelectedProfile) is { } profile)
                        {
                            C.GatherProfiles.Remove(profile);
                            C.SaveDebounced();
                        }
                    }
                }
                bool shiftHeld = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
                bool cntrlHeld = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);

                ImGui.SameLine();
                using (ImRaii.Disabled(!(shiftHeld && cntrlHeld)))
                {
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.GroupArrowsRotate, "Restore"))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            var profile = C.FindGatherProfile(i);
                            if (profile != null)
                            {
                                C.GatherProfiles.Remove(profile);
                            }
                        }

                        C.GatherProfiles.InsertRange(0, new List<GatherProfile>
                        {
                            new(Gather_Util.DefaultProfile),
                            new(Gather_Util.Type_Search),
                            new(Gather_Util.Type_Procure),
                            new(Gather_Util.Type_Search_Procure),
                            new(Gather_Util.Type_Execute),
                        });
                    }
                }
                foreach (var profile in C.GatherProfiles)
                    {
                        using (var pushId = ImRaii.PushId($"{profile.ProfileId}_{profile.Name}"))
                        {
                            string name = $"[{profile.ProfileId}] - {profile.Name}";
                            bool isSlected = profile.ProfileId == SelectedProfile;

                            if (ImGui.Selectable(name, isSlected))
                            {
                                SelectedProfile = profile.ProfileId;
                            }
                        }
                    }
            }

        }

        private static bool EditName = false;
        private static int NameEditId = -1;
        private static string profileName = "";

        private static Job selectedJob = Job.MIN;
        private static Gather_Enums selectedBuff = Gather_Enums.BoonIncrease_1;

        private static int? DraggedBuffIndex = null;

        private static unsafe void ProfileEditor()
        {
            using (var child = ImRaii.Child("Gather: Profile Editor", default, true))
            {
                if (!child.Success)
                    return;

                if (NameEditId != -1 && SelectedProfile != NameEditId)
                {
                    EditName = false;
                    NameEditId = -1;
                    profileName = "";
                }

                if (C.FindGatherProfile(SelectedProfile) is { } gatherProfile)
                {
                    if (EditName)
                    {
                        if (ImGui.Button("Save"))
                        {
                            gatherProfile.Name = profileName;
                            C.Save();

                            NameEditId = -1;
                            profileName = "";
                            EditName = false;
                        }
                        ImGui.SameLine();
                        ImGui.InputText($"Profile Name", ref profileName);
                    }
                    else
                    {
                        if (ImGuiEx.IconButton(FontAwesomeIcon.PencilAlt, "Edit Profile Name"))
                        {
                            NameEditId = SelectedProfile;
                            profileName = gatherProfile.Name;
                            EditName = true;
                        }

                        ImGui.SameLine();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"Profile Name: {gatherProfile.Name}");
                    }

                    if (ImGui.RadioButton("MIN", selectedJob == Job.MIN))
                    {
                        selectedJob = Job.MIN;
                    }
                    ImGui.SameLine();
                    if (ImGui.RadioButton("BTN", selectedJob == Job.BTN))
                    {
                        selectedJob = Job.BTN;
                    }

                    string ruleName(GatheringRule rule)
                    {
                        return rule switch
                        {
                            GatheringRule.Search => "Search",
                            GatheringRule.Procurance => "Procurance",
                            GatheringRule.Search_Procurance => "Search & Procurance",
                            GatheringRule.Execution => "Execution",
                            _ => "???"
                        };
                    }

                    ImGui.Text("Apply to these gathering mission types...");

                    var ruleCount = 0;
                    foreach (var rule in C.RuleProfiles)
                    {
                        if (ruleCount > 0)
                            ImGui.SameLine();

                        bool isSelected = rule.Value == SelectedProfile;
                        if (ImGui.Checkbox($"{ruleName(rule.Key)}", ref isSelected))
                        {
                            if (isSelected)
                                C.RuleProfiles[rule.Key] = SelectedProfile;
                            else
                                C.RuleProfiles[rule.Key] = 0;

                            C.SaveDebounced();
                        }
                        ruleCount += 1;
                    }

                    float maxButtonWidth = 0f;
                    foreach (var setting in gatherProfile.GatheringBuffs)
                    {
                        var key = setting.Key;
                        var actionInfo = Gather_Util.gathActionDict[key];
                        if (actionInfo.ClassAction.TryGetValue(selectedJob, out var jobInfo))
                        {
                            var textSize = ImGui.CalcTextSize(jobInfo.Name);
                            var iconHeight = ImGui.GetFrameHeight() - 4;
                            var estimatedWidth = 4 + iconHeight + 4 + textSize.X + 4;
                            maxButtonWidth = MathF.Max(maxButtonWidth, estimatedWidth);
                        }
                    }

                    using (var buffChild = ImRaii.Child("Buff Selection Child", new(maxButtonWidth + 50, default), true))
                    {
                        if (buffChild.Success)
                        {
                            var priority = gatherProfile.BuffPriority;

                            ImGui.Text("Highest Priority");
                            ImGui.SameLine();
                            ImGui_Ice.Icon(FontAwesomeIcon.QuestionCircle);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("Drag->Drop the buttons in the preferred order you would like them to be\n" +
                                    "Buffs here will be applied before others if possible");
                            }
                            ImGui.Separator();

                            for (int i = 0; i < priority.Count; i++)
                            {
                                var key = priority[i];

                                if (!gatherProfile.GatheringBuffs.TryGetValue(key, out var buffSetting))
                                    continue;

                                var actionInfo = Gather_Util.gathActionDict[key];
                                if (!actionInfo.ClassAction.TryGetValue(selectedJob, out var jobInfo))
                                    continue;

                                var enabled = buffSetting.Enabled;
                                var label = $"{jobInfo.Name}_{jobInfo.ActionId}";

                                if (ImGui_Ice.ImageButtonWithText(jobInfo.IconId, $"{jobInfo.Name}", label, enabled))
                                {
                                    selectedBuff = key;
                                }

                                if (ImGui.BeginDragDropSource())
                                {
                                    DraggedBuffIndex = i;
                                    ImGui.SetDragDropPayload("BUFF_PRIORITY_REORDER", ReadOnlySpan<byte>.Empty, ImGuiCond.Once);
                                    ImGui.Text(jobInfo.Name);
                                    ImGui.EndDragDropSource();
                                }

                                if (ImGui.BeginDragDropTarget())
                                {
                                    var payload = ImGui.AcceptDragDropPayload("BUFF_PRIORITY_REORDER");
                                    if (payload.Handle != null && DraggedBuffIndex is int sourceIndex && sourceIndex != i)
                                    {
                                        var moved = priority[sourceIndex];
                                        priority.RemoveAt(sourceIndex);
                                        priority.Insert(i, moved);
                                        C.SaveDebounced();
                                        DraggedBuffIndex = null;
                                    }
                                    ImGui.EndDragDropTarget();
                                }
                            }

                            ImGui.Separator();
                            ImGui.Text("Lowest Priority");
                            ImGui.SameLine();
                            ImGui_Ice.Icon(FontAwesomeIcon.QuestionCircle);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("Drag->Drop the buttons in the preferred order you would like them to be\n" +
                                    "Buffs toward here will be used last if possible");
                            }
                        }
                    }

                    ImGui.SameLine();
                    using (var buffDetails = ImRaii.Child("Buff Details Selection", default, true))
                    {
                        if (buffDetails.Success)
                        {
                            var settings = gatherProfile.GatheringBuffs[selectedBuff];
                            var buffInfo = Gather_Util.gathActionDict[selectedBuff];
                            var actionInfo = buffInfo.ClassAction[selectedJob];

                            ImGui_Ice.ImageButtonWithText(actionInfo.IconId, $"{actionInfo.Name}", $"{actionInfo.Name}_{actionInfo.ActionId}");
                            bool enabled = settings.Enabled;
                            if (ImGui.Checkbox("Enable", ref enabled))
                            {
                                settings.Enabled = enabled;
                                C.Save();
                            }

                            uint minGp = settings.GP_Min;
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.InputUInt("Minimum GP", ref minGp, stepFast: 10))
                            {
                                settings.GP_Min = minGp;
                                C.SaveDebounced();
                            }

                            uint maxUse = settings.MaxUse;
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.InputUInt("Max use on node", ref maxUse))
                            {
                                settings.MaxUse = maxUse;
                                C.SaveDebounced();
                            }

                            uint minDurability = settings.Durability_MinUse;
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.SliderUInt("Minimum Durability", ref minDurability, 0, 10))
                            {
                                settings.Durability_MinUse = minDurability;
                                C.SaveDebounced();
                            }

                            if (selectedBuff == Gather_Enums.BYII)
                            {
                                uint minItems = settings.MinItems;
                                if (ImGui.InputUInt("Minimum items for BYII", ref minItems))
                                {
                                    settings.MinItems = minItems;
                                    C.SaveDebounced();
                                }
                            }
                        }
                    }
                }
                else
                {
                    ImGui.Text($"No valid profile selected");
                }
            }
        }
    }
}
