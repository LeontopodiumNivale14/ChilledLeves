using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui.MainWindow;

internal class About
{
    private static string GetImageURL()
    {
        return Svc.PluginInterface.Manifest.IconUrl ?? "";
    }

    public static void Draw()
    {
        ImGuiEx.Text($"Explorer's Icebox - {Svc.PluginInterface.Manifest.AssemblyVersion}");
        ImGuiEx.Text($"Published and developed by Ice");

        ImGuiHelpers.ScaledDummy(10f);

        ImGuiEx.LineCentered("ExpIceboxAbout2", delegate
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(GetImageURL(), out var texture))
            {
                ImGui.Image(texture.ImGuiHandle, new(150f, 150f));
            }
        });
        ImGuiHelpers.ScaledDummy(10f);
        
        ImGui.TextWrapped("Join the Puni.sh Discord for support, questions, announcements.");
        ImGui.TextWrapped("If you need help, just ping me in #ffxiv-visland");
        if (ImGui.Button("Discord Link"))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://discord.gg/Zzrcc8kmvy",
                UseShellExecute = true
            });
        }
        ImGui.SameLine();
        if (ImGui.Button("Repository"))
        {
            ImGui.SetClipboardText("https://puni.sh/api/repository/ice");
            Notify.Success("Link copied to clipboard");
        }
        ImGui.SameLine();
        if (ImGui.Button("Source Code"))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Svc.PluginInterface.Manifest.RepoUrl,
                UseShellExecute = true
            });
        }
        ImGui.Text("Have a bug? Maybe a request? You can make a report here!");
        if (ImGui.Button("Source Code"))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/LeontopodiumNivale14/Explorers-Icebox/issues",
                UseShellExecute = true
            });
        }
        ImGui.Dummy(new(0, 10));
        ImGui.Text("Want to donate?");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red color
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.0f, 0.0f, 1.0f)); // Darker red when hovered
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.6f, 0.0f, 0.0f, 1.0f)); // Even darker red when clicked
        if (ImGui.Button("Ice's Kofi"))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://ko-fi.com/ice643269",
                UseShellExecute = true
            });
        }
        ImGui.PopStyleColor(3);

        ImGui.Text("Visland Routes that I use");
        ImGui.SameLine();
        if (ImGui.Button("Link to wiki page"))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/LeontopodiumNivale14/Explorers-Icebox/wiki/Visland-Routes:",
                UseShellExecute = true
            });
        }
    }
}
