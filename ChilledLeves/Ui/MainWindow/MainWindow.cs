using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Data.Parsing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChilledLeves.Ui.MainWindow;

internal class MainWindow : Window
{
    public MainWindow() :
        base($"Chilled LevesTableOld {P.GetType().Assembly.GetName().Version} ###ChilledLevesMainWindow")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(2000, 2000)
        };
        P.windowSystem.AddWindow(this);
        AllowPinning = false;

        PopulateDictionary();
    }

    public void Dispose() { }

    private static bool DefaultBool = false;

    public override void Draw()
    {
        if (Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), SilverStarImage).TryGetWrap(out var texture, out var _))
        {
            DrawButtonStar(texture, "Test", ref DefaultBool, false);
        }
    }

    private void DrawFilterPanel()
    {
        DrawButton(LeveTypeDict[5].AssignmentIcon, $"{LeveTypeDict[5].LeveClassType} Leves", ref C.LeveFilter.ShowCarpenter, true);
        DrawButton(LeveTypeDict[6].AssignmentIcon, $"{LeveTypeDict[6].LeveClassType} Leves", ref C.LeveFilter.ShowBlacksmith, true);
        DrawButton(LeveTypeDict[7].AssignmentIcon, $"{LeveTypeDict[7].LeveClassType} Leves", ref C.LeveFilter.ShowArmorer, true);
        DrawButton(LeveTypeDict[8].AssignmentIcon, $"{LeveTypeDict[8].LeveClassType} Leves", ref C.LeveFilter.ShowGoldsmith, true);
        DrawButton(LeveTypeDict[9].AssignmentIcon, $"{LeveTypeDict[9].LeveClassType} Leves", ref C.LeveFilter.ShowLeatherworker, true);
        DrawButton(LeveTypeDict[10].AssignmentIcon, $"{LeveTypeDict[10].LeveClassType} Leves", ref C.LeveFilter.ShowWeaver, true);
        DrawButton(LeveTypeDict[11].AssignmentIcon, $"{LeveTypeDict[11].LeveClassType} Leves", ref C.LeveFilter.ShowAlchemist, true);
    }

    private void DrawButton(ISharedImmediateTexture? icon, string tooltip, ref bool state, bool sameLine)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, new Vector2(24)))
        {
            state = !state;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(tooltip);
        if (sameLine)
            ImGui.SameLine();
    }

    private void DrawButtonStar(IDalamudTextureWrap? icon, string tooltip, ref bool state, bool sameLine)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        if (ImGui.ImageButton(icon.ImGuiHandle, new Vector2(24)))
        {
            state = !state;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(tooltip);
        if (sameLine)
            ImGui.SameLine();
    }

    private void DrawLeves()
    {
        /// var leves = 
    }

    private bool FilterLeve(uint row)
    {
        var showLeve = true;

        if (C.LeveFilter.LevelFilter > 0)
        {
            // showLeve = LeveDict[row].
        }

        return showLeve;
    }
}
