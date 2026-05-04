using ChilledLeves.Resources;
using ChilledLeves.Utilities;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using InteropGenerator.Runtime;
using Pictomancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Scheduler.Handlers;

// TO ANYONE WONDERING WTF THIS IS.
// This is a universal way of me being able to add things to picto drawing without having to add it constantly. 
// This will run on framework thread -> just load whatever it's suppose to load
// Which, is nice cause it means that I can add things from different windows here and run it all under the same code

internal static class PictoManager
{
    private static string Tag = "PictoManager";

    // Storage for draw commands - initialize inline to avoid any timing issues
    private static readonly List<Action<PctDrawList>> drawCommands = new();
    private static readonly object lockObject = new();

    // Add a draw command to be executed this frame
    public static void AddDrawCommand(Action<PctDrawList> drawAction)
    {
        if (drawAction == null) return;

        lock (lockObject)
        {
            drawCommands.Add(drawAction);
        }
    }

    // Main draw function - call this every frame
    public static void DrawPicto()
    {
        try
        {
            using (var pictoDraw = PctService.Draw())
            {
                if (pictoDraw == null)
                    return;

                lock (lockObject)
                {
                    // Execute all queued draw commands
                    foreach (var command in drawCommands)
                    {
                        try
                        {
                            command(pictoDraw);
                        }
                        catch (Exception ex)
                        {
                            IceLogging.Error($"Error executing draw command: {ex}", Tag);
                        }
                    }

                    // Clear the queue for next frame
                    drawCommands.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            IceLogging.Error($"Error in DrawPicto: {ex}", Tag);
        }
    }

    // Helper methods for common draws
    public static void DrawArrow(Vector3 targetPos, uint fillColor, uint outlineColor, bool scaleWithDistance = true)
    {
        AddDrawCommand(pictoDraw =>
        {
            Picto_TriangleRotate(pictoDraw, targetPos, fillColor, outlineColor, scaleWithDistance);
        });
    }

    public static void DrawNodeCircle(Vector3 targetPos, Vector4 color)
    {
        AddDrawCommand(pictoDraw =>
        {
            var adaptedColor = Utils.ToUintABGR(color);
            pictoDraw.AddCircleFilled(targetPos, 3, adaptedColor);
        });
    }

    public static void DrawVfxCircle(string id, Vector3 origin, Vector4 color)
    {
        AddDrawCommand(pictoDraw =>
        {
            PctService.VfxRenderer.AddCircle(id, origin, 3, color);
            // PictoService.VfxRenderer.AddOmen($"{id}", "gl_circle1609_o0g", origin, new(3, 1, 3), color:color);
        });
    }

    public static void TestVfxCircle(string id, Vector3 origin, Vector4 color, string vfx)
    {
        // color = new Vector4(0, 0, 1, 0); // what color shows?
        AddDrawCommand(pictoDraw =>
        {
            PctService.VfxRenderer.AddOmen($"{id}", vfx, origin, new(3, 7, 3), color: color);
        });
    }

    public static void TestCustomVfx(string id, string path, Vector3 origin, Vector3? scale = null, float rotation = 0, Vector4? color = null)
    {
        AddDrawCommand(pictoDraw =>
        {
            PctService.VfxRenderer.AddCustom(id, path, origin, scale, rotation, color);
        });
    }

    public static void TestVfxCircle(string id, Vector3 origin, bool isSelected, string vfx)
    {
        Vector4 color = isSelected ? C.Picto_SelectedGatherBase : C.Picto_GatherBase;


        AddDrawCommand(pictoDraw =>
        {
            PctService.VfxRenderer.AddOmen($"{id}", vfx, origin, new(3, 7, 3), color: color);
        });
    }

    public static void DrawGatheringFan(GatheringNode location, bool isSelected)
    {
        var fanColor_Gather = isSelected ? C.Picto_SelectedFan : C.Picto_GatherFanColor;
        var fanColor_Flight = isSelected ? C.Picto_SelectedFan : C.Picto_FlightFanColor;

        var gatherFan = location.Gathering_FanInfo;
        var flightFan = location.Flight_FanInfo;

        Vector3 gatherFanPos = new(location.Position.X, location.Position.Y + gatherFan.Fan_Height, location.Position.Z);

        float pictoStart = (gatherFan.Fan_StartAngle + 180f) % 360f;
        float pictoEnd = (gatherFan.Fan_EndAngle + 180f) % 360f;

        if (pictoStart > pictoEnd)
        {
            AddDrawCommand(pictoDraw =>
            {
                pictoDraw.AddFanFilled(
                    gatherFanPos,
                    gatherFan.Fan_DistanceMin,
                    gatherFan.Fan_DistanceMax,
                    DegreesToRadians(pictoStart),
                    DegreesToRadians(360),
                    Utils.ToUintABGR(fanColor_Gather));

                pictoDraw.AddFanFilled(
                    gatherFanPos,
                    gatherFan.Fan_DistanceMin,
                    gatherFan.Fan_DistanceMax,
                    DegreesToRadians(0),
                    DegreesToRadians(pictoEnd),
                    Utils.ToUintABGR(fanColor_Gather));
            });
        }
        else
        {
            AddDrawCommand(pictoDraw =>
            {
                pictoDraw.AddFanFilled(
                    gatherFanPos,
                    gatherFan.Fan_DistanceMin,
                    gatherFan.Fan_DistanceMax,
                    DegreesToRadians(pictoStart),
                    DegreesToRadians(pictoEnd),
                    Utils.ToUintABGR(fanColor_Gather));
            });
        }

        float flight_PictoStart = (flightFan.Fan_StartAngle + 180f) % 360f;
        float flight_PictoEnd = (flightFan.Fan_EndAngle + 180f) % 360f;
        Vector3 flight_FanPos = new(location.Position.X, location.Position.Y + flightFan.Fan_Height, location.Position.Z);

        if (flight_PictoStart > flight_PictoEnd)
        {
            AddDrawCommand(pictoDraw =>
            {
                pictoDraw.AddFanFilled(
                    flight_FanPos,
                    flightFan.Fan_DistanceMin,
                    flightFan.Fan_DistanceMax,
                    DegreesToRadians(flight_PictoStart),
                    DegreesToRadians(360),
                    Utils.ToUintABGR(fanColor_Flight));

                pictoDraw.AddFanFilled(
                    flight_FanPos,
                    flightFan.Fan_DistanceMin,
                    flightFan.Fan_DistanceMax,
                    DegreesToRadians(0),
                    DegreesToRadians(flight_PictoEnd),
                    Utils.ToUintABGR(fanColor_Flight));
            });
        }
        else
        {
            AddDrawCommand(pictoDraw =>
            {
                pictoDraw.AddFanFilled(
                    flight_FanPos,
                    flightFan.Fan_DistanceMin,
                    flightFan.Fan_DistanceMax,
                    DegreesToRadians(flight_PictoStart),
                    DegreesToRadians(flight_PictoEnd),
                    Utils.ToUintABGR(fanColor_Flight));
            });
        }
    }

    /// <summary>
    /// Fancy ass fucking triangle that might not be used? But also, it's neat so I'm keeping it. <br></br>
    /// Esentially just picto drawing a custom triangle in the 3d space
    /// </summary>
    /// <param name="pictoDraw"></param>
    /// <param name="pos"></param>
    /// <param name="fillColor"></param>
    /// <param name="outlineColor"></param>
    /// <param name="scaleWithDistance"></param>
    public static void Picto_TriangleRotate(PctDrawList pictoDraw, Vector3 pos, uint fillColor, uint outlineColor, bool scaleWithDistance = true)
    {
        var player = Player.Position;

        // Position the arrow at a fixed world position (arrow TIP will be at this position)
        var arrowPosition = pos;

        // Calculate direction from arrow to player (in XZ plane)
        var toPlayer = new Vector2(
            player.X - arrowPosition.X,
            player.Z - arrowPosition.Z
        );

        // Calculate the angle to rotate - NEGATE to fix rotation direction
        float angle = -MathF.Atan2(toPlayer.X, toPlayer.Y);

        // Calculate scale based on distance to maintain constant screen size
        float scale = 1.0f;
        if (scaleWithDistance)
        {
            float distance = Vector3.Distance(player, arrowPosition);
            // Scale proportionally to distance to maintain constant screen size
            // Clamp between min and max to prevent it from getting too small or too large
            scale = Math.Clamp(distance / 5f, 0.5f, 5.0f); // Min 0.5x, Max 5x
        }

        // Arrow dimensions (scaled)
        float shaftTopWidth = 0.4f * scale;
        float shaftBottomWidth = 0.25f * scale;
        float shaftHeight = 1.5f * scale;
        float headWidth = 0.8f * scale;
        float headHeight = 1.0f * scale;

        // Total arrow height
        float totalHeight = shaftHeight + headHeight;

        // Helper function to rotate a point around the arrow's center position
        Vector3 RotatePoint(float x, float y, float z)
        {
            // Rotate around Y axis
            float rotatedX = x * MathF.Cos(angle) - z * MathF.Sin(angle);
            float rotatedZ = x * MathF.Sin(angle) + z * MathF.Cos(angle);

            return new Vector3(
                arrowPosition.X + rotatedX,
                arrowPosition.Y + y,
                arrowPosition.Z + rotatedZ
            );
        }

        // Define arrow shape in local space (tip is at origin, arrow extends upward)
        var shaftTopLeft = RotatePoint(-shaftTopWidth, totalHeight, 0);
        var shaftTopRight = RotatePoint(shaftTopWidth, totalHeight, 0);
        var shaftBottomLeft = RotatePoint(-shaftBottomWidth, headHeight, 0);
        var shaftBottomRight = RotatePoint(shaftBottomWidth, headHeight, 0);

        var headLeft = RotatePoint(-headWidth, headHeight, 0);
        var headRight = RotatePoint(headWidth, headHeight, 0);
        var arrowTip = RotatePoint(0, 0, 0);

        float lineHalfWidth = 0.05f * scale; // Scale line width too

        // FILL the shapes first (so outlines draw on top)
        // Fill shaft trapezoid
        pictoDraw.AddQuadFilled(shaftTopLeft, shaftTopRight, shaftBottomRight, shaftBottomLeft, fillColor);

        // Fill arrowhead triangle
        pictoDraw.AddTriangleFilled(headLeft, headRight, arrowTip, fillColor);

        // THEN draw the outlines
        // Draw shaft trapezoid outline
        pictoDraw.AddLine(shaftTopLeft, shaftTopRight, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(shaftTopRight, shaftBottomRight, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(shaftBottomRight, shaftBottomLeft, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(shaftBottomLeft, shaftTopLeft, lineHalfWidth, outlineColor, 2.0f);

        // Draw arrowhead outline
        pictoDraw.AddLine(shaftBottomLeft, headLeft, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(shaftBottomRight, headRight, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(headLeft, headRight, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(headLeft, arrowTip, lineHalfWidth, outlineColor, 2.0f);
        pictoDraw.AddLine(headRight, arrowTip, lineHalfWidth, outlineColor, 2.0f);
    }

    public static void DrawIcon(ImTextureID textureHandle, Vector3 worldPos, Vector2? size = null)
    {
        AddDrawCommand(_ =>
        {
            var iconSize = size ?? new Vector2(24, 24);
            // if (!PictoService.GameGui.WorldToScreen(worldPos, out var screenPos)) 
                return;

            // var topLeft = screenPos - iconSize / 2;
            // ImGui.GetForegroundDrawList().AddImage(textureHandle, topLeft, topLeft + iconSize);
        });
    }

    public static void DrawIcon(ImTextureID textureHandle, Vector3 worldPos, Vector2? size = null, float opacity = 1f, float? scaleByDistance = null)
    {
        AddDrawCommand(_ =>
        {
            // if (!PictoService.GameGui.WorldToScreen(worldPos, out var screenPos)) 
                return;

            var iconSize = size ?? new Vector2(24, 24);

            // Scale based on distance if a max distance is provided
            if (scaleByDistance != null)
            {
                float distance = Vector3.Distance(Player.Position, worldPos);
                // Normalize distance: 0 = right next to it (scale 1x), maxDist = far away (scale 4x)
                float t = Math.Clamp(distance / scaleByDistance.Value, 0f, 1f);
                float scaleFactor = 1f + (4f - 1f) * t;
                iconSize *= scaleFactor;
            }

            // Convert opacity 0-1 to alpha byte in RGBA
            uint alpha = (uint)(Math.Clamp(opacity, 0f, 1f) * 255) << 24;
            uint tintColor = alpha | 0x00FFFFFF; // full white RGB, variable alpha

            // var topLeft = screenPos - iconSize / 2;
            // ImGui.GetForegroundDrawList().AddImage(textureHandle, topLeft, topLeft + iconSize, Vector2.Zero, Vector2.One, tintColor);
        });
    }

    public static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }
}
