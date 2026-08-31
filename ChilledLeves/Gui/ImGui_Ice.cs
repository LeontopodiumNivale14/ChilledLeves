using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;

namespace ChilledLeves.Gui;

public static class ImGui_Ice
{
    // Typically I JUST need the icon + string in the sameline,
    // so this works for me to throw the text -> Icon
    public static void Icon(FontAwesomeIcon icon, string? s = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        if (s != null) 
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(s);
        }
    }

    public static void Icon(FontAwesomeIcon icon, Vector4 color, string? s = null)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(color)))
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                ImGui.TextUnformatted(icon.ToIconString());
            }
        }
        if (s != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(s);
        }
    }

    // Version of Ecommons, but in a format that I work with more
    public static void IconWithTooltip(FontAwesomeIcon icon, string? tooltip = null, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }

    // ReSharper version that gives me the gameIcon. Allows for a cleaner image and less... blurry
    // Minor tradeoff. Still debating on how much is worth
    public static bool ImageButtonWithText(uint iconId, string label, string id, float padding = 4f, float sidePadding = 4f)
    {
        var iconHeight = (int)MathF.Round(ImGui.GetFrameHeight() - sidePadding);
        if (GameIcons.TryGetScaledIcon(iconId, iconHeight, out var texture))
            return ImageButtonWithText(texture, label, id, new Vector2(iconHeight, iconHeight), padding, sidePadding);

        return ImGui.Button($"{label}##{id}");
    }

    public static bool ImageButtonWithText(IDalamudTextureWrap texture, string label, string id, Vector2 imageSize, float padding = 4f, float sidePadding = 4f)
    {
        var frameHeight = ImGui.GetFrameHeight();
        var textSize = ImGui.CalcTextSize(label);

        var iconHeight = frameHeight - sidePadding;
        var aspect = imageSize.X / imageSize.Y;
        var scaledImage = new Vector2(iconHeight * aspect, iconHeight);

        var buttonSize = new Vector2(
            sidePadding + scaledImage.X + padding + textSize.X + sidePadding,
            frameHeight
        );

        var pos = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.InvisibleButton($"##{id}", buttonSize);

        bool hovered = ImGui.IsItemHovered();
        bool active = ImGui.IsItemActive();

        var drawList = ImGui.GetWindowDrawList();
        var rounding = ImGui.GetStyle().FrameRounding;

        uint bgColor = active ? ImGui.GetColorU32(ImGuiCol.ButtonActive) :
                       hovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) :
                                 ImGui.GetColorU32(ImGuiCol.Button);
        drawList.AddRectFilled(pos, pos + buttonSize, bgColor, rounding);
        drawList.AddRect(pos, pos + buttonSize, ImGui.GetColorU32(ImGuiCol.Border), rounding);

        var imagePos = new Vector2(
            pos.X + sidePadding,
            pos.Y + (frameHeight - scaledImage.Y) / 2f
        );
        drawList.AddImage(texture.Handle, imagePos, imagePos + scaledImage);

        var textPos = new Vector2(
            imagePos.X + scaledImage.X + padding,
            pos.Y + (frameHeight - textSize.Y) / 2f
        );
        drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);

        return clicked;
    }

    public static bool ImageButtonWithText(uint iconId, string label, string id, bool isOn, float padding = 4f, float sidePadding = 4f)
    {
        var iconHeight = (int)MathF.Round(ImGui.GetFrameHeight() - sidePadding);
        if (!GameIcons.TryGetScaledIcon(iconId, iconHeight, out var texture))
            return ImGui.Button($"{label}##{id}");

        return ImageButtonWithText(texture, label, id, new Vector2(iconHeight, iconHeight), isOn, padding, sidePadding);
    }

    public static bool ImageButtonWithText(IDalamudTextureWrap texture, string label, string id, Vector2 imageSize, bool isOn, float padding = 4f, float sidePadding = 4f)
    {
        var frameHeight = ImGui.GetFrameHeight();
        var textSize = ImGui.CalcTextSize(label);

        var iconHeight = frameHeight - sidePadding;
        var aspect = imageSize.X / imageSize.Y;
        var scaledImage = new Vector2(iconHeight * aspect, iconHeight);

        var buttonSize = new Vector2(
            sidePadding + scaledImage.X + padding + textSize.X + sidePadding,
            frameHeight
        );

        var pos = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.InvisibleButton($"##{id}", buttonSize);

        bool hovered = ImGui.IsItemHovered();
        bool active = ImGui.IsItemActive();

        var drawList = ImGui.GetWindowDrawList();
        var rounding = ImGui.GetStyle().FrameRounding;

        uint bgColor = active ? ImGui.GetColorU32(ImGuiCol.ButtonActive) :
                       hovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) :
                                 ImGui.GetColorU32(ImGuiCol.Button);
        drawList.AddRectFilled(pos, pos + buttonSize, bgColor, rounding);

        // Border color carries the on/off state
        uint borderColor = isOn
            ? ImGui.GetColorU32(new Vector4(0.35f, 0.85f, 0.4f, 1f))   // "on" green — swap for your theme
            : ImGui.GetColorU32(ImGuiCol.Border);                       // default border when off
        drawList.AddRect(pos, pos + buttonSize, borderColor, rounding, ImDrawFlags.None, isOn ? 1.5f : 1f);

        // Icon tint reinforces the state — dim/desaturate when off
        uint imageTint = isOn
            ? ImGui.GetColorU32(Vector4.One)
            : ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.6f, 0.7f));

        var imagePos = new Vector2(
            pos.X + sidePadding,
            pos.Y + (frameHeight - scaledImage.Y) / 2f
        );
        drawList.AddImage(texture.Handle, imagePos, imagePos + scaledImage, Vector2.Zero, Vector2.One, imageTint);

        var textPos = new Vector2(
            imagePos.X + scaledImage.X + padding,
            pos.Y + (frameHeight - textSize.Y) / 2f
        );
        drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);

        return clicked;
    }
}
