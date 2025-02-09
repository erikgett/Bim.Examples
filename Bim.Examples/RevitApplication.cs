using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Bim.Examples
{
    public class RevitApplication
        : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            int a = 4;
            int b = 5;
            int c = a + b;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
            => Result.Succeeded;
    }

    public static class Extensions
    {
        public static long GetValue(this ElementId elementId)
        {
            #if R17 || R18 || R19 || R20 || R21 || R22 || R23
            return elementId.IntegerValue;
            #else
            return elementId.Value;
            #endif
        }
    }
}
