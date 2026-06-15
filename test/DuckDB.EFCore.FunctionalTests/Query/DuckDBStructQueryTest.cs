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
            .Select(e => new ContactResponse(
                e.Contact.Name,
                new AddressResponse(e.Contact.Address.City)))
            .Single();

        projected.Should().BeEquivalentTo(new ContactResponse(
            "Ada",
            new AddressResponse("London")));

        AssertSql(
            """
            SELECT struct_extract(s."Contact", 'Name') AS "Name", struct_extract(struct_extract(s."Contact", 'Address'), 'City') AS "City"
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
                INSERT INTO "StructRows" ("Id", "Contact")
                VALUES (1, struct_pack(Name := 'Ada', Address := struct_pack(City := 'London', Zip := 12345)))
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
            });
        }
    }

    public sealed class StructRow
    {
        public int Id { get; set; }
        public ContactStruct Contact { get; set; }
    }

    public readonly record struct ContactStruct(string Name, AddressStruct Address);

    public readonly record struct AddressStruct(string City, int Zip);

    public sealed record ContactResponse(string Name, AddressResponse Address);

    public sealed record AddressResponse(string City);
}
