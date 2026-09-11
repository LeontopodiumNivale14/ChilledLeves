using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class User_Logs
    {
        private static LogTableInfo.LogTable? LogTable;

        public static void Draw()
        {
            using (var child = ImRaii.Child("Main Window: User Logs", new(-1, -1), true))
            {
                if (!child.Success)
                    return;

                if (ImGui.Button("copy Logs"))
                {
                    IceLogging.LogSystem.CopyToClipboard();
                }

                using (var logTable = ImRaii.Child("Log Details Window", new(-1, -1), false))
                {
                    if (!logTable.Success)
                        return;

                    LogTable ??= new LogTableInfo.LogTable();

                    LogTable.Reload();
                    LogTable.Draw();
                }
            }
        }
    }
}
