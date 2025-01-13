using System.Reflection;
using CodeMechanic.Diagnostics;
using CodeMechanic.Embeds;
using CodeMechanic.FileSystem;
using CodeMechanic.RazorHAT;
using CodeMechanic.RazorHAT.Services;
using CodeMechanic.Shargs;
using CodeMechanic.Types;
using Hydro.Configuration;
using nugsnet6;
using nugsnet6.Services.Sqlite;
using ILogger = Serilog.ILogger;

internal static class Program
{
    static async Task Main(string[] args)
    {
        DotEnv.Load();

        var arguments = new ArgsMap(args);
        if (arguments.HasFlag("--debug"))
        {
            // foreach (var arg in args)
            // {
            //     Console.WriteLine(arg);
            // }

            arguments.Dump(ignoreNulls: true);
        }

        // var logger = new LoggerConfiguration()
        //     .MinimumLevel.Information()
        //     .WriteTo.Console()
        //     .WriteTo.File("./nugs/nugs.log",
        //         rollingInterval: RollingInterval.Day,
        //         rollOnFileSizeLimit: true
        //     )
        //     .CreateLogger();

        // arguments.Dump();
        (bool run_as_web, bool run_as_cli) = arguments.GetRunModes();

        Console.WriteLine($"{nameof(run_as_web)} {run_as_web}");
        Console.WriteLine($"{nameof(run_as_cli)} {run_as_cli}");

        if (run_as_cli)
            await RunAsCli(arguments);

        if (run_as_web)
            RunAsWeb(args);
    }

    private static async Task RunAsCli(IArgsMap arguments)
    {
        Console.WriteLine(nameof(RunAsCli));
        var services = CreateServices(arguments);

        Application app = services.GetRequiredService<Application>();
        app.Run();
    }

    private static ServiceProvider CreateServices(IArgsMap arguments)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IArgsMap>(arguments)
            .AddSingleton<Application>()
            .BuildServiceProvider();

        return serviceProvider;
    }

    static void RunAsWeb(string[] args)
    {
        Console.WriteLine(nameof(RunAsWeb));
        var builder = WebApplication.CreateBuilder(args);

        // Read .env values for setting up services
        bool dev_mode =
            Environment.GetEnvironmentVariable("DEVMODE").ToBoolean();
        Console.WriteLine("Developer mode (all debugs enabled)? " + dev_mode);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var props_service = new PropertyCache();
        builder.Services.AddSingleton<ILocalLogger, LocalLoggerService>();
        builder.Services.AddSingleton<IPropertyCache>(props_service);
        // builder.Services.AddSingleton<IDriver, Neo4jDriver>();

        builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
        builder.Services.AddSingleton<ICsvService>(
            new CsvService(props_service, dev_mode));
        builder.Services
            .AddSingleton<ISqliteInsightsService, SqliteInsightsService>();

        builder.Services.AddScoped<IFakerService, FakerService>();
        builder.Services.AddScoped<IImageService, ImageService>();

        //  TODO: Fix the httpclient factory
        //  TODO: move to razorhat library
        // builder.Services.AddHttpClient<IndexModel>(client =>
        // {
        //     client.BaseAddress = new Uri(builder.Configuration["BaseUrl"]);
        // });
        // .AddPolicyHandler(GetRetryPolicy())
        // .AddPolicyHandler(GetCircuitBreakerPolicy());

        var main_assembly = Assembly.GetExecutingAssembly();
        builder.Services.AddSingleton<IEmbeddedResourceQuery>(
            new EmbeddedResourceService(
                new Assembly[] { main_assembly },
                debugMode: false
            ).CacheAllEmbeddedFileContents()
        );

        builder.Services.ConfigureAirtable();
        builder.Services.ConfigureNeo4j();
        builder.Services.AddHydro();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.MapRazorPages();
        app.UseExceptionHandler();

        app.MapControllerRoute(name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.UseHydro(builder.Environment);

        app.Run();
    }
}
 
public class Application
{
    private readonly ILogger _logger;

    public Application(ILogger logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        // _logger.Information("Hello, World!");
    }
}