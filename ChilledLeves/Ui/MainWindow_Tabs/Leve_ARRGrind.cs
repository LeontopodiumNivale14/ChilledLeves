using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_ARRGrind
    {

        // TODO IDEAS:
        // Playlist mode where you can set which NPC to run till X thing
        // Would allow for making sure all leves are completed at each vendor as ex.
        // Or would allow for leveling till X at vendor A, then leveling till Y at vendor B
        // Still need to curate a list of npcs for this though that this will work with. . . 
        public static void Draw()
        {
            using (var child = ImRaii.Child("ARR Grind: NPC Mode", default, true))
            {
                if (!child.Success)
                    return;

                var selectedNpc = C.ARR_NpcId;
                var selectedJob = C.ARR_SelectedJob;

                // Need to make the height of this like I did with the gathering ui one
                using (var ARR_NpcWindow = ImRaii.Child("ARR: Npc Info", default, true))
                {
                    if (!ARR_NpcWindow.Success)
                        return;
                }

                // actually show the npc details here if it's valid. Need to check that
                using (var ARR_LeveInfo = ImRaii.Child("ARR: Leve Info", default, true))
                {
                    if (!ARR_LeveInfo.Success)
                        return;

                    // Write the table here at this point
                    // Need to write out the priority system like I did originally (that way users can set the top of the list as the highest prio)
                }
            }
        }
    }
}
