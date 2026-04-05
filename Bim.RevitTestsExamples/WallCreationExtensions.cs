using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TUnit.Core.Executors;

#if NET
#else
using System.Runtime.InteropServices;
#endif

namespace Bim.RevitTestsExamples;

public class WallCreationExtensionsTests : RevitApiTest
{
    private static Document _document = null!;

    [Before(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Setup()
    {
        _document = Application.NewProjectDocument(UnitSystem.Metric);
    }

    [After(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Cleanup()
    {
        _document.Close(false);
    }

    [Test]
    public async Task CreateWall_ValidInput_WallIsCreated()
    {
        Wall wall;

        using Transaction transaction = new Transaction(_document, "Create Wall");
        
        transaction.Start();

        wall = _document.CreateWall(
            new XYZ(0, 0, 0),
            new XYZ(10, 0, 0));

        transaction.Commit();
        

        await Assert.That(wall).IsNotNull();
    }

    [Test]
    public async Task CreateWall_WallHasValidId()
    {
        Wall wall;

        using Transaction transaction = new(_document, "Create Wall");
        
        transaction.Start();

        wall = _document.CreateWall(
            new XYZ(0, 0, 0),
            new XYZ(10, 0, 0));

        transaction.Commit();
        

        await Assert.That(wall.Id.IntegerValue).IsGreaterThan(0);
    }
}

public static class WallCreationExtensions
{
    /// <summary>
    /// Creates a simple wall between two points.
    /// </summary>
    public static Wall CreateWall(
        this Document document,
        XYZ start,
        XYZ end,
        double height = 3000)
    {
        var line = Line.CreateBound(start, end);

        var level = new FilteredElementCollector(document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .First();

        var wall = Wall.Create(
            document,
            line,
            level.Id,
            false);

        return wall;
    }
}

