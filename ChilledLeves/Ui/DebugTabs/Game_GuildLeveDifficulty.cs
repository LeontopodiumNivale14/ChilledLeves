using ChilledLeves.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_GuildLeveDifficulty
    {
        private static int levelSlider = 0;

        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<GuildLeveDifficulty>(out var glDifficulty) && glDifficulty.IsAddonReady)
            {
                if (ImGui.Button("Set level"))
                {
                    glDifficulty.SliderLevel(glDifficulty, levelSlider);
                }
                ImGui.SetNextItemWidth(200);
                ImGui.SliderInt("Level", ref levelSlider, 0, 5);
                if (ImGui.Button("Confirm"))
                {
                    glDifficulty.Yes();
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    glDifficulty.No();
                }
            }
        }
    }
}
