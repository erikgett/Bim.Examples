using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using System.Threading.Tasks;

namespace Bim.RevitTestsExamples;

// Tests for the BoundingBox.Contains method. Inherit from RevitApiTest to be able to use the Revit API.
// Verify whether BoundingBox correctly determines if a point is inside it or not. See also:
// https://medium.com/@thomhurst/per-test-isolation-in-asp-net-core-a-tunit-aspnetcore-guide-ce09f7d4a05f
// https://tunit.dev
public class BoundingBoxXyzExtensionsTests : RevitApiTest
{
    [Test]
    public async Task Contains_PointInsideBox_ReturnsTrue()
    {
        // Tests are written following the Arrange → Act → Assert pattern.

        // Act
        // Create a BoundingBox (3D box).
        // Min — the lower corner of the box.
        // Max — the upper corner of the box.
        var boundingBox = new BoundingBoxXYZ
        {
            Min = new XYZ(0, 0, 0),
            Max = new XYZ(10, 10, 10)
        };

        // Create a point that is clearly inside the box.
        // It lies between Min and Max on all coordinates.
        var pointInside = new XYZ(5, 5, 5);

        // Act
        bool isContains = boundingBox.Contains(pointInside);

        // Assert
        // Verify that the Contains method returns true,
        // i.e. the point is indeed inside the BoundingBox.

        await Assert.That(isContains).IsTrue();
    }

    [Test]
    public async Task Contains_PointOutsideBox_ReturnsFalse()
    {
        var boundingBox = new BoundingBoxXYZ
        {
            Min = new XYZ(0, 0, 0),
            Max = new XYZ(10, 10, 10)
        };

        var pointOutside = new XYZ(15, 15, 15);

        await Assert.That(boundingBox.Contains(pointOutside)).IsFalse();
    }
}

// Class from your project that we are testing
public static class BoundingBoxExtension
{
    private const double Tolerance = 1e-9;

    extension(BoundingBoxXYZ box)
    {
        public bool Contains(XYZ point)
        {
            return Contains(box, point, false);
        }

        public bool Contains(XYZ point, bool strict)
        {
            if (!box.Transform.IsIdentity)
            {
                point = box.Transform.Inverse.OfPoint(point);
            }

            var insideX = strict
                ? point.X > box.Min.X + Tolerance && point.X < box.Max.X - Tolerance
                : point.X >= box.Min.X - Tolerance && point.X <= box.Max.X + Tolerance;

            var insideY = strict
                ? point.Y > box.Min.Y + Tolerance && point.Y < box.Max.Y - Tolerance
                : point.Y >= box.Min.Y - Tolerance && point.Y <= box.Max.Y + Tolerance;

            var insideZ = strict
                ? point.Z > box.Min.Z + Tolerance && point.Z < box.Max.Z - Tolerance
                : point.Z >= box.Min.Z - Tolerance && point.Z <= box.Max.Z + Tolerance;

            return insideX && insideY && insideZ;
        }
    }
}
