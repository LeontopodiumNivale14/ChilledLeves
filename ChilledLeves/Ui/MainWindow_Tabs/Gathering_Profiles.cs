using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.GatheringHelper;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using ECommons.Hooks;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Gathering_Profiles
    {
        public static void Draw()
        {
            using (var table = ImRaii.Table("Gathering Profile Editor", 2, ImGuiTableFlags.SizingFixedFit))
            {
                if (!table.Success)
                    return;

                ImGui.TableSetupColumn("Profile Names");
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
                using (ImRaii.Disabled(SelectedProfile == 0))
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

        private static void ProfileEditor()
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

                    float maxButtonWidth = 0f;
                    foreach (var setting in gatherProfile.GatheringBuffs)
                    {
                        var key = setting.Key;
                        var actionInfo = Gather_Util.gathActionDict[key];
                        if (actionInfo.ClassAction.TryGetValue(selectedJob, out var jobInfo))
                        {
                            var textSize = ImGui.CalcTextSize(jobInfo.Name);
                            var iconHeight = ImGui.GetFrameHeight() - 4; // match your button's sizing logic
                            var estimatedWidth = 4 + iconHeight + 4 + textSize.X + 4;
                            maxButtonWidth = MathF.Max(maxButtonWidth, estimatedWidth);
                        }
                    }

                    using (var buffChild = ImRaii.Child("Buff Selection Child", new(maxButtonWidth + 20, default), true))
                    {
                        if (buffChild.Success)
                        {
                            foreach (var setting in gatherProfile.GatheringBuffs)
                            {
                                var key = setting.Key;
                                var enabled = setting.Value.Enabled;

                                var actionInfo = Gather_Util.gathActionDict[key];
                                if (actionInfo.ClassAction.TryGetValue(selectedJob, out var jobInfo))
                                {
                                    if (ImGui_Ice.ImageButtonWithText(jobInfo.IconId, $"{jobInfo.Name}", $"{jobInfo.Name}_{jobInfo.ActionId}", enabled))
                                    {
                                        selectedBuff = key;
                                    }
                                }
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
