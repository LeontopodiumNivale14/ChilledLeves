using ChilledLeves.Utilities.LogInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_Logs
    {
        private static LogTableInfo.LogTable? LogTable;

        public static void Draw()
        {
            LogTable ??= new LogTableInfo.LogTable();

            if (ImGui.Button("copy Logs"))
            {
                IceLogging.LogSystem.CopyToClipboard();
            }
            LogTable.Reload();
            LogTable.Draw();
        }
    }
}
