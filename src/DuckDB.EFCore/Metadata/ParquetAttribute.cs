namespace DuckDB.EFCore.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ParquetAttribute : Attribute
{
    public ParquetAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
