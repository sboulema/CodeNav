using CodeNav.OutOfProc.Models;
using VisualBasicDocumentMapper = CodeNav.OutOfProc.Languages.VisualBasic.Mappers.DocumentMapper;

namespace CodeNav.Test;

[TestFixture]
internal class VisualBasicDocumentMapperTests
{
    [Test]
    public async Task MapsStatementMembersInInterfacesAndClasses()
    {
        const string source = """
            Namespace Example
                Public Interface IExample
                    Sub Execute(value As Integer)
                    Property Name As String
                    Event Changed As EventHandler
                End Interface

                Public MustInherit Class ExampleClass
                    Public MustOverride Sub Run()
                    Public Property Count As Integer
                    Public Event Updated As EventHandler
                End Class
            End Namespace
            """;

        var codeItems = await VisualBasicDocumentMapper.MapDocument(
            source,
            new CodeDocumentViewModel(),
            filePaths: [],
            cancellationToken: default);

        var namespaceItem = codeItems.Single() as CodeNamespaceItem;
        Assert.That(namespaceItem, Is.Not.Null);

        var interfaceItem = namespaceItem!.Members
            .OfType<CodeInterfaceItem>()
            .Single(item => item.Name == "IExample");
        var classItem = namespaceItem.Members
            .OfType<CodeClassItem>()
            .Single(item => item.Name == "ExampleClass");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(interfaceItem.Members.Select(item => item.Name),
                Is.EquivalentTo(["Execute", "Name", "Changed"]));
            Assert.That(classItem.Members.Select(item => item.Name),
                Is.EquivalentTo(["Run", "Count", "Updated"]));
            Assert.That(interfaceItem.Members.Select(item => item.Kind),
                Is.EquivalentTo([
                    CodeItemKindEnum.Method,
                    CodeItemKindEnum.Property,
                    CodeItemKindEnum.Event,
                ]));
        }
    }

    [Test]
    public async Task MapsClassMemberBlocksAndTheirDetails()
    {
        const string source = """
            Namespace Example
                Public Class Sample
                    Private _first, _second As Integer
                    Public Const Maximum As Integer = 10

                    Public Sub New(value As Integer)
                    End Sub

                    Public Function Calculate(value As Integer) As String
                        Return value.ToString()
                    End Function

                    Public Default Property Value(index As Integer) As Integer
                        Get
                            Return _first
                        End Get
                        Set(value As Integer)
                            _first = value
                        End Set
                    End Property

                    Public Custom Event Changed As EventHandler
                        AddHandler(value As EventHandler)
                        End AddHandler
                        RemoveHandler(value As EventHandler)
                        End RemoveHandler
                        RaiseEvent()
                        End RaiseEvent
                    End Event
                End Class
            End Namespace
            """;

        var classItem = GetNamespace(await Map(source)).Members
            .OfType<CodeClassItem>()
            .Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(classItem.Members.Select(item => item.Name), Is.EquivalentTo([
                "_first", "_second", "Maximum", "New", "Calculate", "Value", "Changed",
            ]));
            Assert.That(classItem.Members.Select(item => item.Kind), Is.EquivalentTo([
                CodeItemKindEnum.Variable,
                CodeItemKindEnum.Variable,
                CodeItemKindEnum.Constant,
                CodeItemKindEnum.Constructor,
                CodeItemKindEnum.Method,
                CodeItemKindEnum.Property,
                CodeItemKindEnum.Event,
            ]));
        }

        var calculate = classItem.Members
            .OfType<CodeFunctionItem>()
            .Single(item => item.Name == "Calculate");
        var property = classItem.Members
            .OfType<CodePropertyItem>()
            .Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(calculate.Parameters, Is.EqualTo("(value As Integer)"));
            Assert.That(calculate.ReturnType, Is.EqualTo("String"));
            Assert.That(property.Parameters, Is.EqualTo("(index As Integer) {Get,Set}"));
            Assert.That(property.ReturnType, Is.EqualTo("Integer"));
        }
    }

    [Test]
    public async Task MapsNamespacesAndTopLevelDeclarationKinds()
    {
        const string source = """
            Namespace Example.Outer
                Public Delegate Function Transformer(value As Integer) As String

                Public Enum Choice
                    First
                    Second
                End Enum

                Public Structure Point
                    Public X As Integer
                    Public Shared Operator +(left As Point, right As Point) As Point
                        Return left
                    End Operator
                End Structure

                Public Module Utilities
                    Public Declare Function GetTickCount Lib "kernel32" () As Integer
                End Module
            End Namespace
            """;

        var namespaceItem = GetNamespace(await Map(source));
        var enumItem = namespaceItem.Members
            .OfType<CodeClassItem>()
            .Single(item => item.Name == "Choice");
        var structureItem = namespaceItem.Members
            .OfType<CodeClassItem>()
            .Single(item => item.Name == "Point");
        var moduleItem = namespaceItem.Members
            .OfType<CodeClassItem>()
            .Single(item => item.Name == "Utilities");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(namespaceItem.Name, Is.EqualTo("Example.Outer"));
            Assert.That(namespaceItem.Members.Select(item => item.Kind), Is.EquivalentTo([
                CodeItemKindEnum.Delegate,
                CodeItemKindEnum.Enum,
                CodeItemKindEnum.Struct,
                CodeItemKindEnum.Class,
            ]));
            Assert.That(enumItem.Members.Select(item => item.Name),
                Is.EquivalentTo(["First", "Second"]));
            Assert.That(enumItem.Members.Select(item => item.Kind),
                Is.All.EqualTo(CodeItemKindEnum.EnumMember));
            Assert.That(structureItem.Members.Select(item => item.Name),
                Is.EquivalentTo(["X", "+"]));
            Assert.That(moduleItem.Members.Single().Name, Is.EqualTo("GetTickCount"));
            Assert.That(moduleItem.Members.Single().Kind, Is.EqualTo(CodeItemKindEnum.Method));
        }
    }

    [Test]
    public async Task MapsNestedRegionsAndPlacesMembersInTheInnermostRegion()
    {
        const string source = """
            Namespace Example
                Public Class Sample
            #Region "Outer"
            #Region "Inner"
                    Public Sub Execute()
                    End Sub
            #End Region
            #End Region
                End Class
            End Namespace
            """;

        var classItem = GetNamespace(await Map(source)).Members
            .OfType<CodeClassItem>()
            .Single();
        var outerRegion = classItem.Members.OfType<CodeRegionItem>().Single();
        var innerRegion = outerRegion.Members.OfType<CodeRegionItem>().Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outerRegion.Name, Is.EqualTo("Outer"));
            Assert.That(innerRegion.Name, Is.EqualTo("Inner"));
            Assert.That(innerRegion.Members.Single().Name, Is.EqualTo("Execute"));
            Assert.That(innerRegion.Members.Single().Kind, Is.EqualTo(CodeItemKindEnum.Method));
        }
    }

    [Test]
    public void OnlyMapsVisualBasicFilesWhenVisualBasicIsEnabled()
    {
        var mapper = new VisualBasicDocumentMapper();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mapper.CanMapDocument("Example.vb", new GlobalSettings
            {
                EnableVisualBasic = true,
            }), Is.True);
            Assert.That(mapper.CanMapDocument("Example.VB", new GlobalSettings
            {
                EnableVisualBasic = true,
            }), Is.True);
            Assert.That(mapper.CanMapDocument("Example.vb", new GlobalSettings()), Is.False);
            Assert.That(mapper.CanMapDocument("Example.cs", new GlobalSettings
            {
                EnableVisualBasic = true,
            }), Is.False);
        }
    }

    private static async Task<List<CodeItem>> Map(string source)
        => await VisualBasicDocumentMapper.MapDocument(
            source,
            new CodeDocumentViewModel(),
            filePaths: [],
            cancellationToken: default);

    private static CodeNamespaceItem GetNamespace(IEnumerable<CodeItem> codeItems)
    {
        var namespaceItem = codeItems.Single() as CodeNamespaceItem;
        Assert.That(namespaceItem, Is.Not.Null);

        return namespaceItem!;
    }
}
