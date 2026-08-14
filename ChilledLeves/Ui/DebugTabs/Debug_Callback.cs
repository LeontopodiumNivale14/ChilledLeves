using ChilledLeves.Utilities;
using Dalamud.Bindings.ImGui;
using Dalamud.Memory;
using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChilledLeves.Ui.DebugTabs;

// Cleanup vs original:
//  - AtkValue -> object parsing was duplicated (SetupDetour + CallbackDetour); pulled into one static helper.
//  - `callbacksSearch.Length < 0` search filter was dead code (always false) -> fixed to `== 0`.
//  - String parsing was inconsistent (a->String.ToString() vs Marshal.PtrToStringUTF8) -> unified on a->String.ToString().
//  - setupHooks.Add -> TryAdd so re-hooking an already-hooked addon name doesn't throw.
//  - DebugManager.ClickToCopyText replaced with a small local ImGui helper (no SimpleTweaks dependency).
//  - SimpleTweaks' HookWrapper<T>/IHookWrapper removed entirely; uses ECommons.EzHookManager.EzHook<T>.
//    EzHook auto-registers with EzHookCommon so it's disposed on plugin unload without us tracking every
//    field by hand, and its Disable() fully disposes the underlying hook (not just pauses it) while
//    Enable() lazily recreates -- which matches the "off means off" behavior the checkboxes here want,
//    instead of us manually doing hook?.Disable() + conditional recreate against raw Dalamud Hook<T>.
//  - Because EzHook.Disable() already disposes, DrawInstance's checkboxes no longer need the
//    `fireCallbackHook?.Disable()` guard before conditionally re-enabling -- Enable() handles both
//    "first creation" and "re-creation after a full dispose" itself.
public unsafe class AddonDebugTab
{
    public string Name => "Addon Logging";

    private static AddonDebugTab? instance;
    private static AddonDebugTab Instance => instance ??= new AddonDebugTab();

    private AddonDebugTab() { }

    public static void Draw() => Instance.DrawInstance();

    public static void DisposeInstance()
    {
        if (instance == null) return;
        foreach (var hook in instance.setupHooks.Values)
        {
            hook.EzHook.Disable();
        }
        instance.setupHooks.Clear();
        instance = null;
    }

    #region Shared AtkValue parsing

    private static (List<object> values, List<AtkValueType> types) ParseAtkValues(AtkValue* atkValues, int count)
    {
        var values = new List<object>(count);
        var types = new List<AtkValueType>(count);

        var a = atkValues;
        for (var i = 0; i < count; i++)
        {
            types.Add(a->Type);
            values.Add(a->Type switch
            {
                AtkValueType.Int => a->Int,
                AtkValueType.UInt => a->UInt,
                AtkValueType.Bool => a->Byte != 0,
                AtkValueType.ManagedString or AtkValueType.ConstString or AtkValueType.String => a->String.ToString(),
                _ => $"Unknown Type: {a->Type}",
            });
            a++;
        }

        return (values, types);
    }

    private static void ClickToCopyText(string text)
    {
        ImGui.Text(text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Click to copy");
            if (ImGui.IsItemClicked()) ImGui.SetClipboardText(text);
        }
    }

    private static void ClickToCopy(void* ptr) => ClickToCopyText($"{(ulong)ptr:X}");

    #endregion

    #region Callbacks (FireCallback hook)

    private delegate void* FireCallbackDelegate(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility);
    private EzHook<FireCallbackDelegate>? fireCallbackHook;
    private bool callbackLoggingEnabled;
    private string callbacksSearch = string.Empty;

    public class Callback
    {
        public required string AtkUnitBaseName;
        public required List<object> AtkValues;
        public required List<AtkValueType> AtkValueTypes;
        public required void* ReturnValue;
        public required AtkUnitBase* AtkUnitBase;
        public required byte UpdateVisibility;

        // Consecutive-repeat collapsing: bumped instead of inserting a new row when the same
        // addon fires again with identical values back-to-back. Reset to 1 on any new row.
        public int RepeatCount = 1;

        public bool MatchesForRepeat(string addonName, List<object> values, byte updateVisibility)
        {
            if (AtkUnitBaseName != addonName) return false;
            if (UpdateVisibility != updateVisibility) return false;
            if (AtkValues.Count != values.Count) return false;

            for (var i = 0; i < values.Count; i++)
            {
                if (!Equals(AtkValues[i], values[i])) return false;
            }

            return true;
        }
    }

    private readonly List<Callback> callbacks = new();
    private const int MaxCallbacks = 1000;

    private void* CallbackDetour(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility)
    {
        List<object> values;
        List<AtkValueType> types;
        try
        {
            (values, types) = ParseAtkValues(atkValues, valueCount);
        }
        catch
        {
            return fireCallbackHook!.Original(atkUnitBase, valueCount, atkValues, updateVisibility);
        }

        var ret = fireCallbackHook!.Original(atkUnitBase, valueCount, atkValues, updateVisibility);

        try
        {
            var addonName = atkUnitBase->NameString;
            var head = callbacks.Count > 0 ? callbacks[0] : null;

            if (head != null && head.MatchesForRepeat(addonName, values, updateVisibility))
            {
                // Same addon, same values, fired again immediately -> collapse into the existing
                // row instead of inserting a new one. ReturnValue is refreshed to the latest call.
                head.RepeatCount++;
                head.ReturnValue = ret;
            }
            else
            {
                callbacks.Insert(0, new Callback
                {
                    AtkUnitBaseName = addonName,
                    AtkUnitBase = atkUnitBase,
                    UpdateVisibility = updateVisibility,
                    ReturnValue = ret,
                    AtkValues = values,
                    AtkValueTypes = types,
                });

                if (callbacks.Count > MaxCallbacks)
                {
                    callbacks.RemoveRange(MaxCallbacks, callbacks.Count - MaxCallbacks);
                }
            }
        }
        catch
        {
            // best-effort logging, never let this break the real callback
        }

        return ret;
    }

    private void DrawCallbacksTab()
    {
        if (ImGui.Checkbox("Enable Logging", ref callbackLoggingEnabled))
        {
            if (callbackLoggingEnabled)
            {
                // EzHook lazily (re)creates on Enable() -- no need to Disable() first like raw Hook<T>.
                fireCallbackHook ??= new EzHook<FireCallbackDelegate>("E8 ?? ?? ?? ?? 0F B6 E8 8B 44 24 20", CallbackDetour, autoEnable: false);
                fireCallbackHook.Enable();
            }
            else
            {
                // EzHook.Disable() fully disposes the underlying hook, not just pauses it.
                fireCallbackHook?.Disable();
            }
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.InputText("##searchCallbacks", ref callbacksSearch, 50);

        ImGui.SameLine();
        if (ImGui.Button("Clear")) callbacks.Clear();

        ImGui.Separator();

        if (!ImGui.BeginTable("callbacksTable", 5, ImGuiTableFlags.RowBg)) return;

        ImGui.TableSetupColumn("Addon", ImGuiTableColumnFlags.WidthFixed, 150);
        ImGui.TableSetupColumn("Values", ImGuiTableColumnFlags.WidthFixed, 300);
        ImGui.TableSetupColumn("Update Visibility", ImGuiTableColumnFlags.WidthFixed, 150);
        ImGui.TableSetupColumn("Return", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableSetupColumn("Repeats", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableHeadersRow();

        var filtered = callbacksSearch.Length == 0
            ? callbacks
            : callbacks.Where(cb => cb.AtkUnitBaseName.Contains(callbacksSearch, StringComparison.OrdinalIgnoreCase));

        foreach (var cb in filtered)
        {
            ImGui.TableNextColumn();
            ImGui.Text(cb.AtkUnitBaseName);

            ImGui.TableNextColumn();
            for (var i = 0; i < cb.AtkValues.Count; i++)
            {
                ImGui.Text($"{i} [{cb.AtkValueTypes[i]}]:   {cb.AtkValues[i]}");
            }

            ImGui.TableNextColumn();
            ImGui.Text(cb.UpdateVisibility.ToString());

            ImGui.TableNextColumn();
            ClickToCopy(cb.ReturnValue);

            ImGui.TableNextColumn();
            if (cb.RepeatCount > 1) ImGui.Text($"x{cb.RepeatCount}");
        }

        ImGui.EndTable();
    }

    #endregion

    #region Setups (per-addon OnSetup hooks)

    public delegate void* OnSetupDelegate(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues);

    public class SetupCall
    {
        public required string AtkUnitBaseName;
        public required List<object> AtkValues;
        public required List<AtkValueType> AtkValueTypes;
        public required void* ReturnValue;
        public required AtkUnitBase* AtkUnitBase;
    }

    public class SetupHook
    {
        // EzHook here uses the (nint address, T detour, bool autoEnable) constructor -- this is a
        // dynamically-discovered vtable address per AtkUnitBase, so it can't use EzHookAttribute's
        // reflection-based static field init (that's for known signatures on known fields).
        public readonly EzHook<OnSetupDelegate> EzHook;
        public readonly List<SetupCall> Calls = new();
    }

    private readonly Dictionary<string, SetupHook> setupHooks = new();

    #endregion

    private void DrawInstance()
    {
        DrawCallbacksTab();
    }
}