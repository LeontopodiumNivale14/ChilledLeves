using ChilledLeves.Scheduler.Handlers;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_DeepDungeonTeset
    {
        private static uint floorId = 0;
        private static uint hoardCount = 0;
        private static float timeRemaining = 0;
        private static float progress = 0;

        private static DeepDungeonPartyInfo partyInfo = new();

        internal static readonly string[] VfxPaths = 
        [
            "bg/ffxiv/fst_f1/common/vfx/eff/b0941trp1a_o.avfx",
            "bg/ffxiv/fst_f1/common/vfx/eff/b0941trp1b_o.avfx",
            "bg/ffxiv/fst_f1/common/vfx/eff/b0941trp1c_o.avfx",
            "bg/ffxiv/fst_f1/common/vfx/eff/b0941trp1d_o.avfx",
            "bg/ffxiv/fst_f1/common/vfx/eff/b0941trp1e_o.avfx",
            "bg/ex2/02_est_e3/common/vfx/eff/b0941trp1f_o.avfx",
            "bg/ex4/07_lak_l5/common/vfx/eff/b2640trp1g_o.avfx",
        ];

        private static string vxf = "bg/ex5/06_nvt_n6/common/vfx/eff/b3700trp1_m2.avfx";

        public static void Draw()
        {
            ImGui.InputText("Vfx", ref vxf);
            PictoManager.TestCustomVfx("Trap?", vxf, Player.Position, new(3, 7, 3));

            Test();
            UpdateTimer();

            ImGui.Text($"Floor ID: {floorId}");
            ImGui.Text($"Hoard Count {hoardCount}");
            ImGui.Text($"Time Remaining: {timeRemaining:N2}]");
            ImGui.Text($"Progress: {progress}");

            ImGui.Checkbox("Show Active Only", ref ShowOnlyActive);
            ImGui.Checkbox("Hide Non-Valid Squares", ref ShowNoneTiles);

            DDMapGrid();

            DDMap();
        }

        private static unsafe void Test()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var dd = ef->GetInstanceContentDeepDungeon();
            if (dd == null) return;

            floorId = dd->Floor;
            hoardCount = dd->HoardCount;
            progress = dd->PassageProgress;
            foreach (var member in dd->Party)
            {
                ImGui.Text($"ID: {member.EntityId} | Room: {member.RoomIndex + 1}");
            }

            foreach (var chest in dd->Chests)
            {
                ImGui.Text($"Chest Type: {chest.ChestType} | Room: {chest.RoomIndex + 1}");
            }
        }

        private static unsafe void DDItems()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var dd = ef->GetInstanceContentDeepDungeon();
            if (dd == null) return;

            if (ImGui.BeginTable("Deep Dungeon Items", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("");

                foreach (var item in dd->Items)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{item.ItemId}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{item.Count}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{item.IsUsable}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{item.IsActive}");
                }

                ImGui.EndTable();
            }
        }

        private static readonly Dictionary<RoomFlags, Vector4> TileUVs = new()
        {
            [RoomFlags.None] = new Vector4(0.010416667f, 0.010416667f, 0.23958333f, 0.23958333f),
            [RoomFlags.ConnectionN] = new Vector4(0.26041666f, 0.010416667f, 0.48958334f, 0.23958333f),
            [RoomFlags.ConnectionS] = new Vector4(0.5104167f, 0.010416667f, 0.7395833f, 0.23958333f),
            [RoomFlags.ConnectionN | RoomFlags.ConnectionS] = new Vector4(0.7604167f, 0.010416667f, 0.9895833f, 0.23958333f),
            [RoomFlags.ConnectionW] = new Vector4(0.010416667f, 0.26041666f, 0.23958333f, 0.48958334f),
            [RoomFlags.ConnectionW | RoomFlags.ConnectionN] = new Vector4(0.26041666f, 0.26041666f, 0.48958334f, 0.48958334f),
            [RoomFlags.ConnectionW | RoomFlags.ConnectionS] = new Vector4(0.5104167f, 0.26041666f, 0.7395833f, 0.48958334f),
            [RoomFlags.ConnectionN | RoomFlags.ConnectionW | RoomFlags.ConnectionS] = new Vector4(0.7604167f, 0.26041666f, 0.9895833f, 0.48958334f),
            [RoomFlags.ConnectionE] = new Vector4(0.010416667f, 0.5104167f, 0.23958333f, 0.7395833f),
            [RoomFlags.ConnectionN | RoomFlags.ConnectionE] = new Vector4(0.26041666f, 0.5104167f, 0.48958334f, 0.7395833f),
            [RoomFlags.ConnectionS | RoomFlags.ConnectionE] = new Vector4(0.5104167f, 0.5104167f, 0.7395833f, 0.7395833f),
            [RoomFlags.ConnectionN | RoomFlags.ConnectionE | RoomFlags.ConnectionS] = new Vector4(0.7604167f, 0.5104167f, 0.9895833f, 0.7395833f),
            [RoomFlags.ConnectionE | RoomFlags.ConnectionW] = new Vector4(0.010416667f, 0.7604167f, 0.23958333f, 0.9895833f),
            [RoomFlags.ConnectionW | RoomFlags.ConnectionN | RoomFlags.ConnectionE] = new Vector4(0.26041666f, 0.7604167f, 0.48958334f, 0.9895833f),
            [RoomFlags.ConnectionW | RoomFlags.ConnectionS | RoomFlags.ConnectionE] = new Vector4(0.5104167f, 0.7604167f, 0.7395833f, 0.9895833f),
            [RoomFlags.ConnectionN | RoomFlags.ConnectionE | RoomFlags.ConnectionW | RoomFlags.ConnectionS] = new Vector4(0.7604167f, 0.7604167f, 0.9895833f, 0.9895833f),
        };


        private static readonly Dictionary<int, Vector4> PassageUVs = new()
        {
            [0] = new Vector4(0f, 0f, 0.25f, 0.33333334f),
            [2] = new Vector4(0.25f, 0f, 0.5f, 0.33333334f),
            [3] = new Vector4(0.5f, 0f, 0.75f, 0.33333334f),
            [4] = new Vector4(0.75f, 0f, 1f, 0.33333334f),
            [5] = new Vector4(0f, 0.3f, 0.25f, 0.6666667f),
            [6] = new Vector4(0.25f, 0.3f, 0.5f, 0.6666667f),
            [7] = new Vector4(0.5f, 0.3f, 0.75f, 0.6666667f),
            [8] = new Vector4(0.75f, 0.3f, 1f, 0.6666667f),
            [9] = new Vector4(0f, 0.6f, 0.25f, 1f),
            [10] = new Vector4(0.25f, 0.6f, 0.5f, 1f),
            [11] = new Vector4(0.5f, 0.6f, 0.75f, 1f),
        };

        private static readonly Vector2 TileSize = new(64, 64);
        private static readonly Vector2 IconSize = new(TileSize.X / 2.5f, TileSize.Y / 2.5f);
        private static readonly float DotRadius = 4f;
        private static readonly Vector4 HomeUV = new(0.4848485f, 0.4848485f, 0.72727275f, 0.6857143f);

        private static readonly Dictionary<int, nint> ChestTextures = new(); // populated after wrap resolution

        private static bool ShowOnlyActive = true;
        private static bool ShowNoneTiles = true;

        private static unsafe void DDMapGrid()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var dd = ef->GetInstanceContentDeepDungeon();
            if (dd == null) return;

            const RoomFlags connectionMask = RoomFlags.ConnectionN | RoomFlags.ConnectionS
                                           | RoomFlags.ConnectionW | RoomFlags.ConnectionE;

            var roomTex = Svc.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Rooms_hr1.tex");
            var passageTex = Svc.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Key_hr1.tex");
            var returnTex = Svc.Texture.GetFromGame("ui/icon/060000/060905_hr1.tex");
            var homeTex = Svc.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_hr1.tex");

            var goldWrap = Svc.Texture.GetFromGame("ui/icon/060000/060913_hr1.tex").GetWrapOrEmpty();
            var silverWrap = Svc.Texture.GetFromGame("ui/icon/060000/060912_hr1.tex").GetWrapOrEmpty();
            var bronzeWrap = Svc.Texture.GetFromGame("ui/icon/060000/060911_hr1.tex").GetWrapOrEmpty();

            var roomWrap = roomTex.GetWrapOrEmpty();
            var passageWrap = passageTex.GetWrapOrEmpty();
            var returnWrap = returnTex.GetWrapOrEmpty();
            var homeWrap = homeTex.GetWrapOrEmpty();

            var passageProgress = dd->PassageProgress;

            void RenderTile(int index)
            {
                var room = dd->MapData[index];
                var flags = room & connectionMask;
                var isRevealed = room.HasFlag(RoomFlags.Revealed);
                var tint = flags != RoomFlags.None && !isRevealed
                                     ? new Vector4(1f, 1f, 1f, 0.35f)
                                     : Vector4.One;

                var cursorPos = ImGui.GetCursorScreenPos();

                // Base tile
                if (flags != RoomFlags.None && TileUVs.TryGetValue(flags, out var uv))
                    ImGui.Image(roomWrap.Handle, TileSize, new Vector2(uv.X, uv.Y), new Vector2(uv.Z, uv.W), tint);
                else if (flags == RoomFlags.None && ShowNoneTiles && TileUVs.TryGetValue(RoomFlags.None, out var noneUv))
                    ImGui.Image(roomWrap.Handle, TileSize, new Vector2(noneUv.X, noneUv.Y), new Vector2(noneUv.Z, noneUv.W), tint);
                else
                    ImGui.Dummy(TileSize);

                var drawList = ImGui.GetWindowDrawList();
                var padding = new Vector2(5f, 6f);

                // Left column: Return, Passage, Home
                var leftPos = cursorPos + padding;
                void DrawLeftOverlay(IDalamudTextureWrap texture, Vector2 uv0, Vector2 uv1)
                {
                    drawList.AddImage(texture.Handle, leftPos, leftPos + IconSize, uv0, uv1);
                    leftPos.Y += IconSize.Y;
                }

                if (room.HasFlag(RoomFlags.Return))
                    DrawLeftOverlay(returnWrap, Vector2.Zero, Vector2.One);

                if (room.HasFlag(RoomFlags.Passage) && PassageUVs.TryGetValue(passageProgress, out var passUv))
                    DrawLeftOverlay(passageWrap, new Vector2(passUv.X, passUv.Y), new Vector2(passUv.Z, passUv.W));

                if (room.HasFlag(RoomFlags.Home))
                    DrawLeftOverlay(homeWrap, new Vector2(HomeUV.X, HomeUV.Y), new Vector2(HomeUV.Z, HomeUV.W));

                // Right column: highest value chest only
                var bestChest = -1;
                foreach (var chest in dd->Chests)
                {
                    if (chest.RoomIndex != index) continue;
                    var type = (int)chest.ChestType;
                    if (type > bestChest) bestChest = type;
                }

                if (bestChest > 0)
                {
                    var chestWrap = bestChest switch
                    {
                        3 => goldWrap,
                        2 => silverWrap,
                        _ => bronzeWrap
                    };
                    var rightPos = cursorPos + new Vector2(TileSize.X - IconSize.X - padding.X, padding.Y);
                    drawList.AddImage(chestWrap.Handle, rightPos, rightPos + IconSize, Vector2.Zero, Vector2.One);
                }

                // Center: party member dots clustered
                var members = new List<uint>();
                foreach (var member in dd->Party)
                    if (member.RoomIndex == index)
                        members.Add(member.EntityId);

                if (members.Count > 0)
                {
                    var center = cursorPos + TileSize / 2f;
                    var dotColor = ImGui.GetColorU32(new Vector4(0.2f, 0.5f, 1f, 1f));
                    var spacing = DotRadius * 2.5f;

                    // Offset dots around center so they cluster together
                    var offsets = members.Count switch
                    {
                        1 => new[] { Vector2.Zero },
                        2 => new[] { new Vector2(-spacing / 2f, 0), new Vector2(spacing / 2f, 0) },
                        3 => new[] { new Vector2(-spacing, spacing / 2f), new Vector2(0, -spacing / 2f), new Vector2(spacing, spacing / 2f) },
                        _ => new[] { new Vector2(-spacing / 2f, -spacing / 2f), new Vector2(spacing / 2f, -spacing / 2f),
                         new Vector2(-spacing / 2f,  spacing / 2f), new Vector2(spacing / 2f,  spacing / 2f) }
                    };

                    for (var d = 0; d < members.Count; d++)
                        drawList.AddCircleFilled(center + offsets[d], DotRadius, dotColor);
                }
            }

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

            if (ShowOnlyActive)
            {
                var activeRows = Enumerable.Range(0, 5)
                    .Where(row => Enumerable.Range(0, 5).Any(col => dd->MapData[row * 5 + col] != RoomFlags.None))
                    .ToHashSet();
                var activeCols = Enumerable.Range(0, 5)
                    .Where(col => Enumerable.Range(0, 5).Any(row => dd->MapData[row * 5 + col] != RoomFlags.None))
                    .ToHashSet();

                for (var row = 0; row < 5; row++)
                {
                    if (!activeRows.Contains(row)) continue;
                    var rowStarted = false;

                    for (var col = 0; col < 5; col++)
                    {
                        if (!activeCols.Contains(col)) continue;
                        if (rowStarted) ImGui.SameLine(0, 0);
                        rowStarted = true;
                        RenderTile(row * 5 + col);
                    }
                }
            }
            else
            {
                for (var i = 0; i < dd->MapData.Length; i++)
                {
                    if (i % 5 != 0) ImGui.SameLine(0, 0);
                    RenderTile(i);
                }
            }

            ImGui.PopStyleVar();
        }

        private static unsafe void DDMap()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var dd = ef->GetInstanceContentDeepDungeon();
            if (dd == null) return;

            var flagNames = Enum.GetValues<RoomFlags>()
                                .Where(f => f != RoomFlags.None)
                                .ToArray();

            if (ImGui.BeginTable("Deep Dungeon Map", flagNames.Length + 1,
                    ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // Header row
                ImGui.TableSetupColumn("Room");
                foreach (var flag in flagNames)
                    ImGui.TableSetupColumn(flag.ToString());
                ImGui.TableHeadersRow();

                for (var i = 0; i < dd->MapData.Length; i++)
                {
                    var room = dd->MapData[i];

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"Room {i + 1}");

                    foreach (var flag in flagNames)
                    {
                        ImGui.TableNextColumn();
                        if (room.HasFlag(flag))
                        {
                            var center = ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetColumnWidth() / 2f, ImGui.GetTextLineHeight() / 2f);
                            ImGui.GetWindowDrawList().AddCircleFilled(center, 5f, ImGui.GetColorU32(ImGuiCol.CheckMark));
                        }
                        ImGui.Dummy(new Vector2(ImGui.GetColumnWidth(), ImGui.GetTextLineHeight()));
                    }
                }

                ImGui.EndTable();
            }
        }

        private static unsafe void UpdateTimer()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var cd = ef->GetContentDirector();
            if (cd == null) return;

            timeRemaining = cd->ContentTimeLeft;
        }

        private static unsafe void TodoDirector()
        {
            var c = UIState.Instance()->MassivePcContentTodo.Director;
            if (c != null)
            {


                for (int i = 0; i < c->MassivePcContentTodos.Length; i++)
                {
                    var todo = c->MassivePcContentTodos[i];
                    for (int i1 = 0; i1 < todo.Count; i1++)
                    {
                        var t = todo[i1];
                        if (t.Enabled)
                        {
                            ImGuiEx.Text($"{i} - {i1} - {t.EndTimestamp - Framework.GetServerTime()}");
                        }
                    }
                }
            }
        }
    }
}
