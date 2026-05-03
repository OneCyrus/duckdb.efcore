using DuckDB.EFCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace DuckDB.EFCore.Metadata.Conventions;

public sealed class DuckDBParquetConvention : IEntityTypeAddedConvention
{
    public void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var parquetAttribute = entityTypeBuilder.Metadata.ClrType?.GetCustomAttributes(typeof(ParquetAttribute), true)
            .OfType<ParquetAttribute>()
            .FirstOrDefault();

        if (parquetAttribute is null)
        {
            return;
        }

        entityTypeBuilder.HasAnnotation(DuckDBAnnotationNames.ParquetPath, parquetAttribute.Path);
        entityTypeBuilder.ToView(null);
        entityTypeBuilder.Metadata.SetIsTableExcludedFromMigrations(true);
    }
}
