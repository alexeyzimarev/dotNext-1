using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DotNext.Runtime.CompilerServices;

internal sealed class ClosureAnalyzer : ExpressionVisitor
{
    [SuppressMessage("Performance", "CA1805", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/5750")]
    private static readonly UserDataSlot<bool> ClosureVariableSlot = new();

    private readonly ICollection<ParameterExpression> locals;
    internal readonly Dictionary<ParameterExpression, ParameterExpression> Closures;

    internal ClosureAnalyzer(Dictionary<ParameterExpression, MemberExpression?> variables)
    {
        locals = variables.Keys;
        Closures = new(variables.Count, variables.Comparer);
    }

    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(Expression? node)
    {
        if (node is ParameterExpression p && locals.Contains(p))
        {
            // replace local with closure variable
            var closure = Expression.Variable(typeof(StrongBox<>).MakeGenericType(p.Type));
            p.GetUserData().Set(ClosureVariableSlot, true);
            Closures.Add(p, closure);
            return Expression.Field(closure, nameof(StrongBox<int>.Value));
        }

        return base.Visit(node);
    }

    internal static bool IsClosure(ParameterExpression p)
        => p.GetUserData().TryGet(ClosureVariableSlot, out var result) | result;
}