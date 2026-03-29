using ChilledLeves.Scheduler.Handlers;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_DeepDungeonTeset
    {
        private static uint floorId = 0;
        private static uint hoardCount = 0;
        private static float timeRemaining = 0;

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

            TodoDirector();
        }

        private static unsafe void Test()
        {
            var ef = EventFramework.Instance();
            if (ef == null) return;

            var dd = ef->GetInstanceContentDeepDungeon();
            if (dd == null) return;

            floorId = dd->Floor;
            hoardCount = dd->HoardCount;
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
