using System.Reflection;
using CodeMechanic.Diagnostics;
using CodeMechanic.Neo4j;
using CodeMechanic.Reflection;
using Neo4j.Driver;
using nugsnet6.Models;

namespace nugsnet6.Services;

public class BuilderService : IBuilderService
{
    private readonly IDriver driver;

    public BuilderService(IDriver neodriver)
    {
        driver = neodriver;
    }

    public async Task<int> StartBuild()
    {
        string query = """

                       """;

        return 0;
    }

    public async Task<int> SeedPartTypes(int limit = 100)
    {
        var all_types = LowerPartType.GetAll<LowerPartType>();
        string query = """
                           MERGE ...
                       """;
        // TODO: run query.

        return 0;
    }

    public async Task<List<T>> GetAll<T>(
        string query,
        object parameters,
        Func<IRecord, T> mapper = null,
        bool debug_mode = false
    )
        where T : class, new()
    {
        if (mapper == null)
            mapper = delegate(IRecord record)
            {
                if (debug_mode)
                {
                    record.Values.Dump("record values");
                }

                return record.MapToV2<T>(label: typeof(T).Name);
            };

        var collection = new List<T>();

        if (debug_mode)
            parameters.Dump(nameof(parameters));

        // if (parameters == null || string.IsNullOrWhiteSpace(query))
        //     return collection;

        await using var session = driver.AsyncSession();

        try
        {
            var results = await session.ExecuteReadAsync(async tx =>
            {
                var result = await tx.RunAsync(query, parameters);
                return await result.ToListAsync(mapper);
            });

            // if (debug_mode)
            //     results.Dump("search results");

            return results;
        }
        // Capture any errors along with the query and data for traceability
        catch (Neo4jException ex)
        {
            Console.WriteLine($"{query} - {ex}");
            throw;
        }
        finally
        {
            session.CloseAsync();
        }

        // Console.WriteLine(nameof(GetAll));
        // if (mapper == null)
        //     mapper = delegate(IRecord record) { return record.MapTo<T>(); };
        //
        // var collection = new List<T>();
        //
        // if (query.IsEmpty())
        //     throw new ArgumentNullException(nameof(query) + " cannot be empty!");
        //
        // await using var session = driver.AsyncSession();
        //
        // try
        // {
        //     Console.WriteLine("reading ...");
        //     var results = await session.ExecuteReadAsync(async tx =>
        //     {
        //         var result = await tx.RunAsync(query, parameters);
        //         result.Dump("results");
        //         return await result.ToListAsync<T>(mapper);
        //     });
        //
        //     return results;
        // }
        //
        // // Capture any errors along with the query and data for traceability
        // catch (Neo4jException ex)
        // {
        //     Console.WriteLine($"{query} - {ex}");
        //     throw;
        //     // return collection;
        // }
        // finally
        // {
        //     session.CloseAsync();
        // }
    }

    public Task<List<Build>> Search(Build search)
    {
        throw new NotImplementedException();
    }

    public Task<Build> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> Create(params Build[] model)
    {
        throw new NotImplementedException();
    }

    public Task Update(int id, Build model)
    {
        throw new NotImplementedException();
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCount()
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> FindTables()
    {
        throw new NotImplementedException();
    }

    private async Task<IList<T>> BulkCreateNodes<T>(
        string query = "",
        object parameters = null,
        Func<IRecord, T> mapper = null
    )
        where T : class, new()
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        await using var session = driver.AsyncSession();

        try
        {
            var results = await session.ExecuteWriteAsync(async tx =>
            {
                // var _ = await tx.RunAsync(batch_command, null);
                var result = await tx.RunAsync(query, parameters);
                return await result.ToListAsync<T>(record => record.MapTo<T>());
            });

            return results;
        }
        // Capture any errors along with the query and data for traceability
        catch (Neo4jException ex)
        {
            Console.WriteLine($"{query} - {ex}");
            throw;
        }
        finally
        {
            session.CloseAsync();
        }
    }
}

//
// public class RifleBuilder
// {
//     public static RifleBuilder Start() => new RifleBuilder();
//
//     public BuildDto Complete() => MyBuild;
//
//     public RifleBuilder AddLowerParts(params Part[] parts)
//     {
//         var lower_types = new string[]
//         {
//             "Lower", "Lower Receiver", "LPK", "Lower Parts Kit", "Grip", "Trigger", "Stock", "Buttstock", "Buffer",
//             "Buffer Spring", "Castle Nut", "Takedown Pin", "Pivot Pin", "Hammer Pin", "Buffer Tube"
//         };
//
//         var valid_parts = parts.Where(part => lower_types.Contains(part.Type));
//
//         return this;
//     }
//
//     public RifleBuilder AddUpperParts(params Part[] parts)
//     {
//         var upper_types = new string[]
//         {
//             "Barrel", "Gas Tube", "Barrel Nut", "Handguard", "Foregrip", "Upper", "Upper Receiver", "BCG",
//             "Bolt Carrier Group", "Charging Handle", "Rail"
//         };
//
//         var valid_parts = parts.Where(part => upper_types.Contains(part.Type));
//
//         return this;
//     }
//
//     public RifleBuilder AddAccessories(params Part[] parts)
//     {
//         var accessories = new string[] { "Scope" };
//         var valid_parts = parts.Where(part => accessories.Contains(part.Type));
//
//         return this;
//     }
//
//     protected BuildDto MyBuild { get; set; } = new();
//
//     protected List<Part> MyParts { get; set; } = new();
// }
//
// public sealed class BuildDto
// {
//     private Build dbBuild;
//     private List<Part> dbParts;
// }

public static class UpdatedNeo4JExtensions
{
    /// <summary>
    /// PropertyCache stores the properties we wish to use again so we only have to run Reflection once per property.
    /// </summary>
    private static readonly IDictionary<Type, ICollection<PropertyInfo>> _propertyCache =
        new Dictionary<Type, ICollection<PropertyInfo>>();

    public static T MapToV2<T>(
        this IRecord record,
        string label = "",
        List<PropertyInfo> props = null
    )
        where T : class, new()
    {
        if (props == null)
            props = new List<PropertyInfo>();

        var type = typeof(T);

        // if no label provided, use the type's lowercased name
        label = string.IsNullOrEmpty(label) ? type.Name.ToLowerInvariant() : label;

        // if no props passed as an override, get them from the cache
        var properties = props.Count > 0 ? props : _propertyCache.TryGetProperties<T>(true);

        // Neo4j nodes require labels
        if (!record.Keys.Contains(label))
            return new T();

        var node = record[label].As<INode>();

        if (properties.Count == 0)
        {
            return new T();
        }

        var obj = new T();

        foreach (var prop in properties ?? Enumerable.Empty<PropertyInfo>())
        {
            string name =
                    prop.Name /*.Dump("key")*/
                ;
            // var value = node.Properties[name].Dump("value");
            node.Properties.TryGetValue(name, out var value);

            var next_value = CreateSafeValue(value, prop);

            prop.SetValue(
                obj,
                next_value /*.Dump("value")*/
                ,
                null
            );
        }

        return obj;
    }

    private static object CreateSafeValue(object value, PropertyInfo prop)
    {
        Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        object safeValue = value == null ? null : Convert.ChangeType(value, propType);

        return safeValue;
    }
}