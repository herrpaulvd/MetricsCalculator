using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

string filename = args.Length < 1 ? Console.ReadLine() : args[0];

// Получим AST кода на C#
SyntaxTree ast = CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
// Это корневой элемент дерева
var mainNode = ast.GetRoot();
// Сначала посчитаем LOC
// для этого понадобится объект Line Span
var lineSpan = ast.GetLineSpan(mainNode.FullSpan);
// с помощью которого мы можем получить начальную и конечную строки
// => LOC = end - start + 1
int LOC = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line + 1;
// и вот ответ готов
Console.WriteLine($"Task3::LOC = {LOC}");

// Удобнее хранить все узлы дерева в списке
// чем потом для каждой операции свой DFS писать
List<SyntaxNode> nodes = new(); // собственно узлы AST
List<SyntaxTrivia> trivia = new(); // то, что обычно пропускается lexer'ом
void RecAddNodes(SyntaxNode node)
{
    nodes.Add(node);
    trivia.AddRange(node.GetLeadingTrivia());
    trivia.AddRange(node.GetTrailingTrivia());
    foreach (var child in node.ChildNodes())
        RecAddNodes(child);
}
RecAddNodes(mainNode);

// Получим количество комментариев
int cntComments = trivia
    // оставим только комментарии
    .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia))
    // логика парсера тут такова, что
    // комментарии в список выше попадут несколько раз
    // поэтому придётся сначала свести к удобному для distinct ключу
    .Select(t => t.SpanStart)
    // а потом distinct + count
    .Distinct().Count();
// Вычисляем и выводим comments / LOC
double commentLevel = (double)cntComments / LOC;
Console.WriteLine($"Task3::CommentLevel = {commentLevel}");

// Джилб
// Я посчитаю тут два варианта, благо не сложно
// 1) Только if'ы как это написано в оригинале
// 2) (1) + циклы + switch-case
// Под всеми операторами буду понимать всё,
// что данный парсер называет <...>Statement и <...>Expression
// кроме <...>LiteralExpression, которые представляют на самом деле операнды
// поскольку здесь не предусмотрели отдельные методы/флаги
// для всех Statement и Expression
// однако всё называется достаточно единообразно
// можно и нужно прибегнуть к магии рефлексии
var allSyntaxKindValues = Enum.GetValues(typeof(SyntaxKind)); // все значения enum'а
var allEnumOperatorValues =
    Enumerable.Range(0, allSyntaxKindValues.Length)
    // ибо объект Array не является IEnumerable
    .Select(i => (SyntaxKind)allSyntaxKindValues.GetValue(i))
    .Where(v =>
    {
        var name = Enum.GetName(v);
        return (name.EndsWith("Expression") && !name.EndsWith("LiteralExpression"))
            || name.EndsWith("Statement");
    }).ToArray();
int cntOperators = nodes.Count(node => allEnumOperatorValues.Any(v => node.IsKind(v)));
int ifCount = nodes.Count(node => node.IsKind(SyntaxKind.IfStatement)); // v1
int allCount = ifCount + nodes.Count(node =>
    node.IsKind(SyntaxKind.WhileStatement)
    || node.IsKind(SyntaxKind.DoStatement)
    || node.IsKind(SyntaxKind.ForEachStatement)
    || node.IsKind(SyntaxKind.ForEachVariableStatement)
    || node.IsKind(SyntaxKind.ForStatement)
    || node.IsKind(SyntaxKind.SwitchStatement)); // v2
double jilbJustIf = (double)ifCount / cntOperators;
double jilbAll = (double)allCount / cntOperators;
Console.WriteLine($"Task4::JilbIf = {jilbJustIf}");
Console.WriteLine($"Task4::JilbAll = {jilbAll}");

// Холстед
// Noptr мы уже почти посчитали (cntOperators)
// Noprnds это по-хорошему листья дерева
// но только некоторые
// ими однозначно являются все литералы
// => аналогичная коду выше магия с рефлексией
// и НЕКОТОРЫЕ идентификаторы
// Холстед усложняет подсчёт Noprnds тем
// что операндами считаются "имена процедур"
// в то время как парсер считает эти имена операндами
// операторов . и ()
// которые мы сами, таким образом, не можем считать операторами
// следовательно, из числа операторов нужно вычесть "."
// () мы вычитать не будем: каждому вызову соответствует некий метод
// поэтому их число есть число методов как операторов
int Noprtr = cntOperators - nodes.Count(node =>
    node.IsKind(SyntaxKind.SimpleMemberAccessExpression));
// найдём все операнды-литералы
var allEnumLiteralValues =
    Enumerable.Range(0, allSyntaxKindValues.Length)
    .Select(i => (SyntaxKind)allSyntaxKindValues.GetValue(i))
    .Where(v => Enum.GetName(v).EndsWith("LiteralExpression"))
    .ToArray();
var literals = nodes.Where(node => allEnumLiteralValues.Any(v => node.IsKind(v))).ToArray();
// проблема с идентификаторами заключается в том, что:
// 1) нужно отличать локальные переменные, параметры, поля и свойства,
// которые мы считаем операндами, от методов
// и включить сюда методы, которые не вызываются напрямую
// но передаются как ссылки
// и typeof операнды
// 2) поле/свойство является операндом, но оно может быть не листом
// но составным выражением типа Namespace1.Namespace2.Class1.Field
// 3) для подсчёта уникальных операций потом нужно будет различать методы
// пусть есть два объекта A a и B b
// от обоих вызывается метод f()
// является f одним методом (если оба класса его наследуют от C)
// или двумя разными методами?
// для этого придётся получить информацию от семантического анализатора
CSharpCompilation compilation = CSharpCompilation.Create(
    "TestedAsm", // служебное имя
    Enumerable.Repeat(ast, 1), // синтакические деревья (наше одно)
    // ссылка на mscorlib
    Enumerable.Repeat(MetadataReference.CreateFromFile(typeof(object).Assembly.Location), 1),
    // опции компиляции
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
    optimizationLevel: OptimizationLevel.Debug));
var sem = compilation.GetSemanticModel(ast, true);
// получим все операции с т.з. семантического анализатора
var semops = nodes.Select(node => sem.GetOperation(node)).Where(op => op is not null).ToArray();
// и все интересующие нас объекты
// каждый в том количестве, в котором он выступает в роли операндов
// 1) методы, которые именно вызываются
var invokedMethods = semops
    .Select(op => (op as IInvocationOperation)?.TargetMethod)
    .Where(m => m is not null).ToArray();
// 2) все методы, на которые просто берутся ссылки
var referencedMethods = semops
    .Select(op => (op as IMethodReferenceOperation)?.Method)
    .Where(m => m is not null).ToArray();
// 3) параметры методов
var parameters = semops
    .Select(op => (op as IParameterReferenceOperation)?.Parameter)
    .Where(p => p is not null).ToArray();
// 4) локальные переменные
var locals = semops
    .Select(op => ((op as ILocalReferenceOperation)?.Local))
    .Where(v => v is not null).ToArray();
// 5) поля
var fields = semops
    .Select(op => (op as IFieldReferenceOperation)?.Field)
    .Where(f => f is not null).ToArray();
// 6) свойства
var properties = semops
    .Select(op => (op as IPropertyReferenceOperation)?.Property)
    .Where(p => p is not null).ToArray();
// 7) typeof
var referencedTypes = semops
    .Select(op => (op as ITypeOfOperation)?.TypeOperand)
    .Where(t => t is not null).ToArray();
// 8) ещё нужно this учесть
var these = nodes.Where(node => node.IsKind(SyntaxKind.ThisExpression)).ToArray();
// добавлять к операторам число вызовов методов не нужно
// т.к. мы уже учли их с помощью ()
// а к числу операндов добавим всё остальное
int Noprnd = literals.Length
    + referencedMethods.Length
    + parameters.Length
    + locals.Length
    + fields.Length
    + properties.Length
    + referencedTypes.Length
    + these.Length;

// теперь уникальные
// число уникальных операторов есть сумма двух составляющих
int NUOprtr =
    // 1) число всех обычных операторов, хотя бы раз задействованных
    allEnumOperatorValues.Count(v => nodes.Any(node => node.IsKind(v)))
    // 2) число всех методов, хотя бы раз вызванных
    + invokedMethods
    // для различения методов нужен ключ
    // пусть им будет полная их сигнатура
    .Select(m => m.ToDisplayString())
    .Distinct().Count();
// число уникальных операндов складывается из:
int NUOprnd =
    // 1) литералов, ключ - их строковое представление
    literals.Select(l => l.ToString())
    .Distinct().Count()
    // 2) ссылок на методы, ключ - сигнатура
    + referencedMethods.Select(m => m.ToDisplayString())
    .Distinct().Count()
    // 3) параметры, ключ - (имя, имя метода)
    + parameters.Select(p => (p.Name, p.ContainingSymbol.ToDisplayString()))
    .Distinct().Count()
    // 4) локальные переменные - аналогично + самое раннее из вхождений
    // поскольку мб две переменные в одном методе с одним именем в разных {}
    + locals.Select(l => (l.Name, l.ContainingSymbol.ToString(),
        l.DeclaringSyntaxReferences.Select(r => r.Span.Start).Min()))
    .Distinct().Count()
    // 5) поля, ключ - сигнатура
    + fields.Select(f => f.ToDisplayString())
    .Distinct().Count()
    // 6) свойства - аналогично
    + properties.Select(p => p.ToDisplayString())
    .Distinct().Count()
    // 7) typeof по имени типа
    + referencedTypes.Select(t => t.ToDisplayString())
    .Distinct().Count()
    // 8) this по содержащему их методу
    + these.Select(node =>
    {
        while (node is not null && !node.IsKind(SyntaxKind.MethodDeclaration))
            node = node.Parent;
        if (node is null)
            return -1;
        return node.SpanStart;
    }).Distinct().Count();

// теперь можно считать и выводить метрики
Console.WriteLine($"Task5::Noprtr = {Noprtr}");
Console.WriteLine($"Task5::Noprnd = {Noprnd}");
Console.WriteLine($"Task5::NUOprtr = {NUOprtr}");
Console.WriteLine($"Task5::NUOprnd = {NUOprnd}");
int HPVoc = NUOprtr + NUOprnd;
Console.WriteLine($"Task5::HPVoc = {HPVoc}");
int HPLen = Noprtr + Noprnd;
Console.WriteLine($"Task5::HPLen = {HPLen}");
double HPVol = HPLen * Math.Log2(HPVoc);
Console.WriteLine($"Task5::HPVol = {HPVol}");
double HDiff = NUOprtr / 2.0 * Noprnd / NUOprnd;
Console.WriteLine($"Task5::HDiff = {HDiff}");
double HEff = HDiff * HPVol;
Console.WriteLine($"Task5::HEff = {HEff}");

