using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CodeMechanic.Extensions;
using CodeMechanic.RazorPages;
using System.Text;
using Neo4j.Driver;
using nugsnet6.Models;

namespace nugsnet6.Pages.Builder;

public class IndexModel : HighSpeedPageModel
{
    private static string table_name = "Builds";
    private static AirtableSearch _search = new AirtableSearch();
    private static List<Build> builds_found = new List<Build>();
    public List<Build> BuildsFound => builds_found;

    public IndexModel(
        IEmbeddedResourceQuery embeddedResourceQuery
        , IDriver driver
        , IAirtableRepo repo
    ) 
        : base(embeddedResourceQuery, driver, repo)
    {
    }

    public void OnGet()
    {
    }

    public void OnPatchSetTableName(string next_table_name = "Loadouts") => table_name = next_table_name;

    public async Task<IActionResult> OnGetSearchBuilds(string Name = "")
    {  
        Name.Dump("name searched");
        // Pop.Dump("Pop?");
        table_name.Dump("new table name");

        builds_found = await airtable_repo
         .SearchRecords<Build>(_search
            .With(s=>
            {   
                s.table_name = "Builds";
                s.maxRecords = 100;
                // s.pageSize = 20;
                // s.offset = "30";
                s.filterByFormula = $"(FIND(\"{Name}\", {{Name}}))";
            })
        );

        string html = new StringBuilder()
            .AppendEach(
                builds_found, build => 
        $"""
            <tr>
                <th>
                    <label>
                        <input type="checkbox" class="checkbox" />
                    </label>
                </th>
                <th class='text-primary'>{build.Name}</th>
                <td class='text-accent'>${build.Total_Cost.ToString()}</td>
                <td class='text-secondary'>{build.Reasoning}</td>
            </tr>
        """).ToString();
        return Content(html);
        // return Partial("_BuildsTable", builds_found);
    }

    public async Task<IActionResult> OnPostFavoriteBuild()
    {  
        return Content("<p class='alert alert-primary text-accent'>He's a chicken!</p>");

    }

    public async Task<IActionResult> OnGetRecommendedBarrels()
    {       
        Console.WriteLine("@ Hello:>> ");
        Debug.WriteLine("@ Hello:>> ");
        
        var failure = Content(
            $"<div class='alert alert-error'><p class='text-xl text-warning text-sh'>An Error Occurred...  But fret not! Our team of intelligent lab mice are on the job!</p></div>");

        string query = "...";

        string resource = new StackTrace().GetCurrentResourcePath();
        if(embeddedResourceQuery == null) 
            return failure;

        await using Stream stream = embeddedResourceQuery.Read<IndexModel>(resource);

        query = await stream.ReadAllLinesFromStreamAsync();

        var records = await SearchNeo4J<Build>(query, new {});

        return Content("""
            <div class='alert alert-success shadow-lg'>
                <span>Confirmed!</span>
            </div>
        """);
    }

}


