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

    // Borrowed from cosmic plugin... 
    public static void Draw_XPBar(float current, float needed, string label = null, Vector2? size = null)
    {
        // If we want it to have a standard label above the bar. Not required but for small things it's nice to just have the option
        if (label != null)
        {
            ImGui.TextWrapped(label);
        }

        // Setting the dimensions of the custom bar/drawing it.
        // Usual stuff of drawlist being OP
        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        // Calculating the size of the bar and everything here.
        // If size is null, then it just defaults to the norm. Otherwise it uses whatever size we set (nice in case I want to use this for other things besides XP/Modify it a bit easier)
        var barStart = pos;
        var actualSize = size ?? new Vector2(ImGui.GetContentRegionAvail().X, 10);
        var barEnd = new Vector2(pos.X + actualSize.X, pos.Y + actualSize.Y);

        // Draw background (dark gray)
        drawList.AddRectFilled(barStart, barEnd, ImGui.GetColorU32(new Vector4(0.15f, 0.15f, 0.15f, 1f)));

        // Now comes the fun part, actually creating the filling (that sounds bad)

        // Defining the colors globaly here just cause they're used across the board
        var blueColor = new Vector4(0.2f, 0.6f, 1f, 1f);      // Blue #3399ff  - Fill Bar Part #1 (Left Side)
        var greenColor = new Vector4(0.6f, 1f, 0.8f, 1f);      // Green #99ffcc - Fill Bar Part #2 (Right Side)
        var brassColor = new Vector4(0.71f, 0.55f, 0.18f, 1f);  // Brass #b58d2e - Fill Bar Part #1 (Left Side)
        var goldColor = new Vector4(1f, 0.84f, 0f, 1f);        // Gold #ffd600  - Fill Bar Part #2 (Right Side)

        // Case 1: At or above cap when needed == max (show full gold) [Really only used when at max stage for that planet when a new one comes out)
        if (needed > 0 && current >= needed)
        {
            drawList.AddRectFilledMultiColor(
                barStart, barEnd,
                    ImGui.GetColorU32(brassColor),  // top-left
                    ImGui.GetColorU32(goldColor),   // top-right
                    ImGui.GetColorU32(goldColor),   // bottom-right
                    ImGui.GetColorU32(brassColor)   // bottom-left
                );
        }
        // Case 2: Normal progression (not overcapped)
        else if (current <= needed && needed > 0)
        {
            float fraction = Math.Clamp((float)current / needed, 0f, 1f);
            float filledWidth = actualSize.X * fraction;

            if (filledWidth > 0f)
            {
                var filledEnd = new Vector2(pos.X + filledWidth, pos.Y + actualSize.Y);
                drawList.AddRectFilledMultiColor(
                    barStart, filledEnd,
                    ImGui.GetColorU32(blueColor),  // top-left
                    ImGui.GetColorU32(greenColor), // top-right
                    ImGui.GetColorU32(greenColor), // bottom-right
                    ImGui.GetColorU32(blueColor)   // bottom-left
                );
            }
        }

        // Reset to captured pos before Dummy so manual cursor shifts (e.g. vertical centering)
        // don't cause the Dummy to double-advance. No-op for normal usage.
        ImGui.SetCursorScreenPos(pos);
        ImGui.Dummy(actualSize);
    }
}
