using DuckDB.EFCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

var parquetRoot = System.IO.Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(parquetRoot);

await DemoDataSeeder.SeedAsync(parquetRoot);

builder.Services.AddPooledDbContextFactory<DemoDbContext>((_, options) =>
{
    options.UseDuckDB("Data Source=:memory:");
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .RegisterDbContextFactory<DemoDbContext>()
    .AddSorting();

var app = builder.Build();

app.MapGraphQL();
app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
