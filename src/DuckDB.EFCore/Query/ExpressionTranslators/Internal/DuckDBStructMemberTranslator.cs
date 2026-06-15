using DuckDB.EFCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace DuckDB.EFCore.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new EF Core release.
/// </summary>
public class DuckDBStructMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new EF Core release.
    /// </summary>
    public DuckDBStructMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(SqlExpression? instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance?.TypeMapping is not DuckDBStructTypeMapping)
        {
            return null;
        }

        if (member.DeclaringType is not null
            && !member.DeclaringType.IsAssignableFrom(instance.Type))
        {
            return null;
        }

        var returnTypeMapping = _typeMappingSource.FindMapping(returnType)
            ?? (IsStructLikeClrType(returnType) ? new DuckDBStructTypeMapping(returnType, "STRUCT") : null);

        return _sqlExpressionFactory.Function(
            "struct_extract",
            [instance, _sqlExpressionFactory.Constant(member.Name)],
            nullable: true,
            argumentsPropagateNullability: [true, false],
            returnType,
            returnTypeMapping);
    }

    private static bool IsStructLikeClrType(Type type)
        => type != typeof(string)
           && !type.IsArray
           && !type.IsPrimitive
           && !type.IsEnum
           && type.GetRuntimeProperties().Any(p => p.GetMethod is not null && p.GetIndexParameters().Length == 0);
}
