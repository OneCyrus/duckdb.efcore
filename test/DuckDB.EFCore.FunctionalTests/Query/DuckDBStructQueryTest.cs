using AwesomeAssertions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class DuckDBStructQueryTest : IClassFixture<DuckDBStructQueryTest.DuckDBStructQueryFixture>
{
    public DuckDBStructQueryTest(DuckDBStructQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    private DuckDBStructQueryFixture Fixture { get; }

    [ConditionalFact]
    public void Projects_top_level_and_nested_duckdb_struct_members()
    {
        using var context = CreateContext();

        var projected = context.StructRows
            .OrderBy(e => e.Id)
            .Select(e => new
            {
                PrimaryName = e.Contact.Name,
                BackupName = e.BackupContact.Name,
                PrimaryCity = e.Contact.Address.City
            })
            .Single();

        projected.PrimaryName.Should().Be("Ada");
        projected.BackupName.Should().Be("Grace");
        projected.PrimaryCity.Should().Be("London");

        AssertSql(
            """
            SELECT struct_extract(s."Contact", 'Name') AS "PrimaryName", struct_extract(s."BackupContact", 'Name') AS "BackupName", struct_extract(struct_extract(s."Contact", 'Address'), 'City') AS "PrimaryCity"
            FROM "StructRows" AS s
            ORDER BY s."Id"
            """);
    }

    private StructQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public sealed class DuckDBStructQueryFixture : SharedStoreFixtureBase<StructQueryContext>, ITestSqlLoggerFactory
    {
        protected override string StoreName => "DuckDBStructQueryTest";
        protected override ITestStoreFactory TestStoreFactory => DuckDBTestStoreFactory.Instance;
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(StructQueryContext context)
        {
            context.Database.ExecuteSqlRaw(
                """
                INSERT INTO "StructRows" ("Id", "Contact", "BackupContact")
                VALUES (
                    1,
                    struct_pack(Name := 'Ada', Address := struct_pack(City := 'London', Zip := 12345)),
                    struct_pack(Name := 'Grace', Address := struct_pack(City := 'Arlington', Zip := 22207)))
                """);

            return Task.CompletedTask;
        }
    }

    public sealed class StructQueryContext(DbContextOptions<StructQueryContext> options) : DbContext(options)
    {
        public DbSet<StructRow> StructRows => Set<StructRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StructRow>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Contact)
                    .HasColumnType("STRUCT(Name VARCHAR, Address STRUCT(City VARCHAR, Zip INTEGER))");
                b.Property(e => e.BackupContact)
                    .HasColumnType("STRUCT(Name VARCHAR, Address STRUCT(City VARCHAR, Zip INTEGER))");
            });
        }
    }

    public sealed class StructRow
    {
        public int Id { get; set; }
        public ContactStruct Contact { get; set; }
        public ContactStruct BackupContact { get; set; }
    }

    public readonly record struct ContactStruct(string Name, AddressStruct Address);

    public readonly record struct AddressStruct(string City, int Zip);
}
