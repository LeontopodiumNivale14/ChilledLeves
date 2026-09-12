using ChilledLeves.Gui;
using ChilledLeves.Utilities;

namespace ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;

public static partial class SettingsUi
{
    private static readonly string AlertOption = "Alert Options";

    private static SettingEntry EnableSound = new()
    {
        Label = "Use In Game Sound Effect",
        Category = AlertOption,
        Keywords = new[] { "Sound", "Effects", "Sound Effect" },
        Draw = () =>
        {
            var v = C.PlaySound;
            if (ImGui.Checkbox("Use in game sound effects", ref v))
            {
                C.PlaySound = v;
                C.Save();
            }
        }
    };

    private static readonly Sounds[] soundValues = Enum.GetValues(typeof(Sounds)).Cast<Sounds>().ToArray();
    private static readonly string[] soundNames = Enum.GetValues(typeof(Sounds)).Cast<Sounds>().Select(s => s.ToName()).ToArray();

    private static SettingEntry SelectedSound = new()
    {
        Label = "Select Sound Effect",
        Category = AlertOption,
        Keywords = new[] { "Sound", "Effects", "Sound Effect" },
        Draw = () =>
        {
            var v = C.Sounds;
            int currentIndex = Array.IndexOf(soundValues, v);

            ImGui.SetNextItemWidth(200);
            if (ImGui.Combo("###Select Sound_LeveSE", ref currentIndex, soundNames, soundNames.Length))
            {
                var selectedSound = soundValues[currentIndex];

                C.Sounds = selectedSound; // Set the variable in C
                SoundAlert.PlaySoundEffect(selectedSound);
                C.Save();
            }
        }
    };

    private static SettingEntry GetChatNotification = new()
    {
        Category = AlertOption,
        Label = "Chat Notification",
        Keywords = new[] { "Chat", "Notification", "True/False", "Alert" },
        Draw = () =>
        {
            var v = C.SendChat;
            if (ImGui.Checkbox("Send Chat Notification", ref v))
            {
                C.SendChat = v;
                C.SaveDebounced();
            }
        }
    };

    private static SettingEntry ShowAlertWindow = new()
    {
        Category = AlertOption,
        Label = "Show Alert Window",
        Keywords = new[] { "Popup", "Notification", "Alert" },
        Draw = () =>
        {
            var v = C.ShowOverlayAlert;
            if (ImGui.Checkbox("Show Alert Window", ref v))
            {
                C.ShowOverlayAlert = v;
                C.Save();
            }
        }
    };

    private static SettingEntry LeveAlertAmount = new()
    {
        Category = AlertOption,
        Label = "Leve: Alert Amount",
        Keywords = new[] {"Alert", "Amount", "Notification"},
        Draw = () =>
        {
            var v = C.LeveAlertAmount;

            ImGui.Text($"Leve Notification Amount");
            ImGui.SameLine();
            ImGui_Ice.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set this to the amount of leves you want to be alerted for.\n" +
                    "Will alert you when your character is at or above this amount.");
            }
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("##Leve_AlertAmount", ref v, 0, 100))
            {
                C.LeveAlertAmount = v;
                C.SaveDebounced();
            }
        }
    };

}
