using Autodesk.Revit.UI;

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
}
