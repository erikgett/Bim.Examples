using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using System;
using System.Reflection;

#if NET
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif
using System.Threading.Tasks;
using TUnit.Core.Executors;

namespace Bim.RevitTestsExamples;

public class BuildInExtensionsTests : RevitApiTest
{
    private static Document _document = null!;

    [Before(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Setup()
    {
        // тут можете указать любой путь для тестовой модели
        // \\sb-sharegp\Bim2.0\5. Скрипты\999. BIM-отдел\RevitAutomation\Тестовые модели для разработки автоматизации 
        // МОДЕЛЬ ОБЯЗАТЕЛЬНО СОХРАНЯТЬ ТУТ, учитываем что в рамках тестов сохранять модель тогда
        // нельзя тк это может изменить поведение тестов, создание пустого проекта лучшее решение если это возможно
        //_document = Application.NewProjectDocument(UnitSystem.Metric);
        _document = Application.OpenDocumentFile(@"\\sb-sharegp\Bim2.0\5. Скрипты\999. BIM-отдел\RevitAutomation\Тестовые модели для разработки автоматизации\Плагины Revit\3_АР\Отделка ЛК\Тест_Отделка ЛК.rvt");
    }

    [After(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Cleanup()
    {
        _document.Close(false);
    }

    [Test]
    public async Task ToCategory_ValidBuiltInCategory_ReturnsCategory()
    {
        var category = BuiltInCategory.OST_Walls.ToCategory(_document);

        await Assert.That(category).IsNotNull();
    }

    [Test]
    public async Task ToCategory_ValidBuiltInCategory_ReturnsCorrectName()
    {
        var category = BuiltInCategory.OST_Walls.ToCategory(_document);

        await Assert.That(category.Name).IsNotNull().And.IsNotEmpty();
    }
}


public static class CategoryExtensions
{
    private static readonly Assembly CategoryAssembly = System.Reflection.Assembly.GetAssembly(typeof(Category))!;
    private static readonly Type ADocumentType = CategoryAssembly.GetType("ADocument")!;
    private static readonly Type ElementIdType = CategoryAssembly.GetType("ElementId")!;
    private static readonly MethodInfo GetADocumentMethod = typeof(Document).GetMethod("getADocument", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    private static readonly ConstructorInfo CategoryConstructor = typeof(Category).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, [ADocumentType.MakePointerType(), ElementIdType.MakePointerType()], null)!;

    /// <param name="builtInCategory">The source category</param>
    extension(BuiltInCategory builtInCategory)
    {
        /// <summary>
        /// Converts a BuiltInCategory into a Revit Category object.
        /// </summary>
        /// <param name="document">The Revit Document associated with the category conversion.</param>
        /// <returns>A Category object corresponding to the specified BuiltInCategory.</returns>
        /// <remarks>This method performs low-level operation to instantiate a Category object.</remarks>
        public
#if NET
            unsafe
#endif
            Category ToCategory(Document document)
        {
#if REVIT2025_OR_GREATER
            var elementId = (long)builtInCategory;
#else
            var elementId = (int)builtInCategory;
#endif
#if NET
            var aDocument = GetADocumentMethod.Invoke(document, null);
            var category = (Category)CategoryConstructor.Invoke([aDocument, (nint)Unsafe.AsPointer(ref elementId)]);

            return category;
#else
            var aDocument = GetADocumentMethod.Invoke(document, null);

            var handle = GCHandle.Alloc(elementId, GCHandleType.Pinned);
            var category = (Category)CategoryConstructor.Invoke([aDocument, handle.AddrOfPinnedObject()]);
            handle.Free();

            return category;
#endif
        }
        public ElementId ToElementId()
        {
            return new ElementId(builtInCategory);
        }
    }
}
