using DuckDB.EFCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DuckDB.EFCore.FunctionalTests;

public class ParquetTests
{
    [Fact]
    public void Simple_query_uses_read_parquet()
    {
        using var context = CreateContext();
        var sql = context.MyData.ToQueryString();

        Assert.Contains("read_parquet('data/*.parquet')", sql);
    }

    [Fact]
    public void Where_query_uses_read_parquet()
    {
        using var context = CreateContext();
        var sql = context.MyData.Where(x => x.Id > 10).ToQueryString();

        Assert.Contains("read_parquet('data/*.parquet')", sql);
        Assert.Contains("WHERE", sql);
    }

    [Fact]
    public void Join_query_uses_read_parquet()
    {
        using var context = CreateContext();
        var sql = context.MyData.Join(context.Others, x => x.Id, y => y.Id, (x, y) => x).ToQueryString();

        Assert.Contains("read_parquet('data/*.parquet')", sql);
    }

    [Fact]
    public void Write_throws_for_parquet_entity()
    {
        using var context = CreateContext();
        context.MyData.Add(new MyData { Id = 1 });

        Assert.ThrowsAny<Exception>(() => context.SaveChanges());
    }

    private static ParquetContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ParquetContext>()
            .UseDuckDB("DataSource=:memory:")
            .Options;

        return new ParquetContext(options);
    }

    private sealed class ParquetContext(DbContextOptions<ParquetContext> options) : DbContext(options)
    {
        public DbSet<MyData> MyData => Set<MyData>();
        public DbSet<OtherData> Others => Set<OtherData>();
    }

    [Parquet("data/*.parquet")]
    private sealed class MyData
    {
        public int Id { get; set; }
    }

    private sealed class OtherData
    {
        public int Id { get; set; }
    }
}
