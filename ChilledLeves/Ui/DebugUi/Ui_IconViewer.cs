using ChilledLeves.Utilities.LeveUtilities;
using ChilledLeves.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.DebugUi
{
    internal class Ui_IconViewer
    {
        public static void Draw()
        {
            if (ImGui.BeginTable("Icon Viewer Info", 4, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("JobId");
                ImGui.TableSetupColumn("Enabled Icon");
                ImGui.TableSetupColumn("DisabledIcon");

                ImGui.TableHeadersRow();

                foreach (var job in LeveInfo.JobInfo)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{job.Value.Name}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{job.Key}");

                    ImGui.TableNextColumn();
                    ImGui.Image(job.Value.ColorIcon.GetWrapOrEmpty().Handle, new Vector2(26, 26));

                    ImGui.TableNextColumn();
                    Vector2 size = new Vector2(26, 26);
                    float zoomFactor = 0.30f; // 25% zoom-in
                    float cropAmount = zoomFactor / 2; // Crop equally from all sides

                    Vector2 uv0 = new Vector2(cropAmount, cropAmount);
                    Vector2 uv1 = new Vector2(1 - cropAmount, 1 - cropAmount);
                    ImGui.Image(job.Value.GreyIcon.Handle, new Vector2(26, 26));
                }

                ImGui.EndTable();
            }
        }
    }
}
