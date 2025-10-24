using ChilledLeves.Utilities.LeveUtilities;
using ChilledLeves.Utilities.UtilityClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
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

                    var Size = new Vector2(26, 26);
                    ImGui.TableNextColumn();
                    Svc.Texture.TryGetFromGameIcon(job.Value.ColorIconId, out var colorIcon);
                    ImGui.Image(colorIcon.GetWrapOrEmpty().Handle, Size);

                    ImGui.TableNextColumn();

                    var location = job.Value.GreyPath;
                    var greyIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), location).GetWrapOrEmpty();
                    ImGui.Image(greyIcon.Handle, new Vector2(26, 26));
                }

                ImGui.EndTable();
            }
        }
    }
}
