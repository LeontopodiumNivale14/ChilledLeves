using ChilledLeves.Utilities.LeveData;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files
{
    internal class Config_Migrate
    {
        public static void UpdateConfig()
        {
            foreach (var leve in LeveInfo.Leve_SheetInfo)
            {
                if (!C.LeveList.ContainsKey(leve.Key))
                    C.LeveList[leve.Key] = 0;

                C.Save();
            }

            if (C.ConfigVersion == 1)
            {
                foreach (var leve in C.workList)
                {
                    C.LeveList[leve.LeveID] = leve.InputValue;
                }
                C.Save();
            }
        }
    }
}
