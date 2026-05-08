using DuckDB.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var parquetRoot = System.IO.Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(parquetRoot);

// Ensure parquet files exist
await DemoDataSeeder.SeedAsync(parquetRoot);

builder.Services.AddPooledDbContextFactory<DemoDbContext>((_, options) =>
{
    options
        .UseDuckDB("Data Source=:memory:")
        .UseLazyLoadingProxies();
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
