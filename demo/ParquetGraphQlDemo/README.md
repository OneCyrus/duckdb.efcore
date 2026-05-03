# Parquet + DuckDB.EFCore + HotChocolate demo

This demo project shows how to:

1. Generate demo parquet data files at startup.
2. Query multiple parquet files through `DuckDB.EFCore`.
3. Use an in-memory DuckDB connection (`Data Source=:memory:`).
4. Expose the data through a HotChocolate GraphQL endpoint.

## Run

```bash
dotnet run --project demo/ParquetGraphQlDemo/ParquetGraphQlDemo.csproj
```

Then open:

- `http://localhost:5000/graphql` (or the port printed by ASP.NET Core)

## Example query

```graphql
query {
  customers(order: { id: ASC }) {
    id
    name
    email
    orders(order: { id: ASC }) {
      id
      orderedOn
      totalAmount
      lines(order: { id: ASC }) {
        id
        productName
        quantity
        lineTotal
      }
    }
  }
}
```
