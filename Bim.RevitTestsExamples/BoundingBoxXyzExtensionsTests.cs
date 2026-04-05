using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using System.Threading.Tasks;

namespace Bim.RevitTestsExamples;

// Тесты для метода BoundingBox.Contains наследуемся от класса RevitApiTest, чтобы была возможность исользовать RevitAPi
// Проверяем, правильно ли BoundingBox определяет,
// находится точка внутри него или нет. Ниже что стоит почитать
// https://medium.com/@thomhurst/per-test-isolation-in-asp-net-core-a-tunit-aspnetcore-guide-ce09f7d4a05f
// https://tunit.dev
public class BoundingBoxXyzExtensionsTests : RevitApiTest
{
    [Test]
    public async Task Contains_PointInsideBox_ReturnsTrue()
    {
        // тесты пишем по следующей логике Arrange → Act → Assert (Подготовка → Действие → Проверка)

        // Act
        // Создаем BoundingBox (трехмерную коробку)
        // Min — нижняя точка коробки
        // Max — верхняя точка коробки
        var boundingBox = new BoundingBoxXYZ
        {
            Min = new XYZ(0, 0, 0),
            Max = new XYZ(10, 10, 10)
        };

        // Создаем точку, которая явно находится внутри коробки
        // Она лежит между Min и Max по всем координатам
        var pointInside = new XYZ(5, 5, 5);

        // Act
        bool isContains = boundingBox.Contains(pointInside);

        // Assert
        // Проверяем, что метод Contains вернет true,
        // то есть точка действительно находится внутри BoundingBox

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

// класс из вашего проекта которые тестируем
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
