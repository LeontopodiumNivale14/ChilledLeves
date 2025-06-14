using ChilledLeves.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui
{
    internal class AlertWindow : Window
    {
        public AlertWindow() : base("Notification Alert | Chilled Leves ###ChilledLevesNotifPopup", ImGuiWindowFlags.AlwaysAutoResize)
        {
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            ImGui.Text($"HEY! You've hit the threshold for the alert on this character for leves! \n" +
                       $"This is just a nice little pop-up saying \" Hey, you should really turn these in before you cap out\"");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Text($"Current Leve Amount: {Utils.Allowances} / 100");
            if (ImGui.Button($"Hide Popup"))
            {
                SoundAlert.refreshOverlay = false;
                P.alertUi.IsOpen = false;
            }
        }
    }
}
