using System.Diagnostics;
using CodeMechanic.Embeds;
using CodeMechanic.RazorHAT.Services;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.Driver;
using nugsnet6.Models;

namespace nugsnet6.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IEmbeddedResourceQuery embeddedResourceQuery;
    private readonly IDriver driver;

    private static int count = 0;
    private readonly ICsvService csv;
    private readonly IPartsService partService;

    public List<Part> PartsFromCsv { get; set; } = new();
    public List<Build> BuildsFromCsv { get; set; } = new();

    public IndexModel(
        IEmbeddedResourceQuery embeddedResourceQuery,
        ICsvService csvService,
        IPartsService part,
        IDriver driver
    )
    {
        this.embeddedResourceQuery = embeddedResourceQuery;
        this.driver = driver;
        csv = csvService;
        this.partService = part;
    }

    public void OnGet()
    {
        // reset on refresh
        count = 0;
        PartsFromCsv = GetPartsFromCsvFile("Experimental/Parts-Grid view.csv");
        BuildsFromCsv = GetBuildsFromCsvFile("Experimental/Builds-Grid view.csv");
    }

    private List<Build> GetBuildsFromCsvFile(string filepath)
    {
        return csv.Read<Build>(
                filepath,
                (csv) =>
                {
                    var record = new Build
                    {
                        Id = csv.GetField<string>("Id"),
                        Name = csv.GetField("Name"),
                        // Cost = csv.GetField("Cost")
                        //     .Replace("$", "").ToDouble(),
                        // Combo = csv.GetField("Combo")
                        // Cost = TypeExtensions.ToDouble(csv.GetField("Cost").ToString())
                    };
                    return record;
                }
            )
            .ToList();
    }

    public IActionResult OnGetSave()
    {
        Console.WriteLine("Saving to db ... ");
        return Partial("Alert", new AlertModel() { Message = "Success!" });
    }

    private List<Part> GetPartsFromCsvFile(string filepath)
    {
        return csv.Read<Part>(
                filepath,
                (csv) =>
                {
                    var record = new Part
                    {
                        Id = csv.GetField<string>("Id"),
                        Name = csv.GetField("Name"),
                        Cost = csv.GetField("Cost").Replace("$", "").ToDouble(),
                        Combo = csv.GetField("Combo"),
                        // Cost = TypeExtensions.ToDouble(csv.GetField("Cost").ToString())
                    };
                    return record;
                }
            )
            .ToList();
    }

    public async Task<IActionResult> OnGetQuery()
    {
        var failure = Content(
            $"""
                 <div class='alert alert-error'>
                     <p class='text-3xl text-warning text-sh'>
                         An Error Occurred...  But fret not! Our team of intelligent lab mice are on the job!
                     </p>
                 </div>
             """
        );

        string query = "..."; // This can be ANY SQL query.  In my case, I'm using cypher, because it's lovely.

        // Magically infers from the tract that the current method name is referring to 'Recommendations.cypher'
        query = await embeddedResourceQuery.GetQueryAsync<IndexModel>(new StackTrace());

        if (string.IsNullOrEmpty(query))
            return failure; // If for some reason, nothing comes back, alert the user with this div.

        // This can also be a template, if we want, but here's a fancy-schmancy use of the triple-double quotes to easily send back anything in C# directly to HTML/X:
        return Content(
            $"""
                 <div class='alert alert-primary'>
                     <p class='text-xl text-secondary text-sh'>
                     {query}                           
                     </p>
                 </div>
             """
        );
    }

    // public async Task<IActionResult> OnGetCreateParts()
    // {
    //     Console.WriteLine(nameof(OnGetCreateParts));
    //
    //     var fakeparts = CreateFakeParts(2);
    //
    //     // fakeparts.Dump();
    //     // Parts.Dump("all parts");
    //     fakeparts.Length.Dump("number of parts to create (pre sql)");
    //     int count =
    //             await partService.Create(fakeparts)
    //         // fakeparts.Length
    //         ;
    //     return Content($"<b>Created '{count}' parts</b>");
    // }

    private static readonly string[] part_names = new string[]
    {
        "DD 300 BLK PDW",
        "DD 556 NATO MK18",
        "MCMR-15 BCM 300 BLK Upper",
        "IWI Tavor X-95",
        "P90",
        "MCX Spear",
        "Honey Badger",
        "PPQ",
        "Sig Rattler 300 BLK",
        "JP Rifles .224 Valkyrie",
        "AWP",
    }.Shuffle();

    private static readonly string[] part_kinds = new string[] { "ar-15", "ar-10" }.Shuffle();

    private static readonly string[] part_manufacturers = new string[]
    {
        "Bravo Company",
        "Proof Research",
        "Aero Precision",
        "Faxon Firearms",
        "Smith & Wesson",
        "Dan Wesson",
        "Kahr Arms",
        "IWI",
        "Sig Sauer",
        "Walther",
        "Q LLC",
        "Remington",
        "Glock",
    }.Shuffle();

    private static readonly double[] part_costs = Enumerable
        .Range(5, 20)
        .Select(x => x * 500.00)
        .ToArray()
        .Shuffle();

    private Part[] CreateFakeParts(int count = 1)
    {
        var fakepart = Enumerable
            .Range(1, count)
            .Select(index => new Part
            {
                // Id = index.ToString(),
                Name = part_names.TakeFirstRandom(),
                Kind = part_kinds.TakeFirstRandom(),
                Manufacturer = part_manufacturers.TakeFirstRandom(),
                Cost = part_costs.TakeFirstRandom(),
            })
            .ToArray();
        return fakepart;
    }
}