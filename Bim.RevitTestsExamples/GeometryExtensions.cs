using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bim.RevitTestsExamples;

public class GeometryExtensionsTests : RevitApiTest
{
    [Test]
    [Arguments(0, 0, 0, 10, 0, 0, 0, 5, 0, 10, 5, 0)]
    [Arguments(0, 0, 0, 0, 10, 0, 5, 0, 0, 5, 10, 0)]
    public async Task IsParallel_Lines_ReturnsTrue(
        double x1, double y1, double z1,
        double x2, double y2, double z2,
        double x3, double y3, double z3,
        double x4, double y4, double z4)
    {
        var line1 = Line.CreateBound(new XYZ(x1, y1, z1), new XYZ(x2, y2, z2));
        var line2 = Line.CreateBound(new XYZ(x3, y3, z3), new XYZ(x4, y4, z4));

        var result = line1.IsParallelTo(line2);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsPerpendicular_TwoPerpendicularLines_ReturnsTrue()
    {
        var line1 = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0));
        var line2 = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 10, 0));

        var result = line1.IsPerpendicularTo(line2);

        await Assert.That(result).IsTrue();
    }

    [Test]
    [MethodDataSource(nameof(ParallelCases))]
    public async Task IsParallel_Lines_ReturnsTrueWithDataSource(
        LineTestDto dto1,
        LineTestDto dto2)
    {
        var line1 = dto1.ToLine();
        var line2 = dto2.ToLine();

        var result = line1.IsParallelTo(line2);

        await Assert.That(result).IsTrue();
    }

    public static IEnumerable<(LineTestDto, LineTestDto)> ParallelCases()
    {
        yield return (
            new LineTestDto(
                "Horizontal line pair",
                new (0, 0, 0),
                new (10, 0, 0)
            ),
            new LineTestDto(
                "Offset horizontal line",
                new (0, 5, 0),
                new (10, 5, 0)
            )
        );

        yield return (
            new LineTestDto(
                "Vertical line pair",
                new (0, 0, 0),
                new (0, 10, 0)
            ),
            new LineTestDto(
                "Offset vertical line",
                new (5, 0, 0),
                new (5, 10, 0)
            )
        );
    }
}

public static class GeometryExtensions
{
    private const double Tolerance = 1e-6;

    /// <summary>
    /// Checks if two curves are parallel.
    /// </summary>
    public static bool IsParallelTo(this Curve first, Curve second)
    {
        var dir1 = (first.GetEndPoint(1) - first.GetEndPoint(0)).Normalize();
        var dir2 = (second.GetEndPoint(1) - second.GetEndPoint(0)).Normalize();

        return dir1.CrossProduct(dir2).GetLength() < Tolerance;
    }

    /// <summary>
    /// Checks if two curves are perpendicular.
    /// </summary>
    public static bool IsPerpendicularTo(this Curve first, Curve second)
    {
        var dir1 = (first.GetEndPoint(1) - first.GetEndPoint(0)).Normalize();
        var dir2 = (second.GetEndPoint(1) - second.GetEndPoint(0)).Normalize();

        return Math.Abs(dir1.DotProduct(dir2)) < Tolerance;
    }
}

public class LineTestDto(string description, XYZDto startPoint, XYZDto endPoint)
{
    public string Description = description;
    public XYZDto StartPoint = startPoint;
    public XYZDto EndPoint = endPoint;

    public override string ToString() => Description;
}

public static class LineTestDtoExtensions
{ 
    public static Line ToLine(this LineTestDto lineDto) =>
        Line.CreateBound(lineDto.StartPoint.ToXYZ(), lineDto.EndPoint.ToXYZ());
}


public class XYZDto(double x, double y, double z)
{
    public double X = x;
    public double Y = y;
    public double Z = z;

    public override string ToString() => $"XYZ({X},{Y},{Z})";
}

public static class XYZDtoDtoExtensions
{
    public static XYZ ToXYZ(this XYZDto xyzDto) =>
        new (xyzDto.X, xyzDto.Y, xyzDto.Z);
}
