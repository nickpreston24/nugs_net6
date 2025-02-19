using System.ServiceModel.Syndication;
using System.Xml;
using CodeMechanic.Diagnostics;
using CodeMechanic.Embeds;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace nugsnet6.Pages.RSSFeeds;

public class IndexModel : PageModel
{
    private readonly IEmbeddedResourceQuery embeddedResourceQuery;
    private readonly IDriver driver;

    private static int count = 0;

    private string[] feeds = { "https://www.theepochtimes.com/c-us-politics/feed" };

    public IndexModel(IEmbeddedResourceQuery embeddedResourceQuery, IDriver driver)
    {
        this.embeddedResourceQuery = embeddedResourceQuery;
        this.driver = driver;
    }

    public void OnGet()
    {
        // reset on refresh
        count = 0;
    }

    public async Task<IActionResult> OnGetRSSFeeds()
    {
        var url = "https://khalidabuhakmeh.com/feed.xml";
        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);

        var post = feed.Items.FirstOrDefault();

        post.Dump("post");

        return Content("<h1 class='text-3xl'>blah</h1>");
    }
}

public class RSSFeedList<T>
{
    private string json;
    private List<T> records = new List<T>();

    public RSSFeedList(
        string json =
            """
                    {
                        "BaseUri": null,
                        "Content": null,
                        "Copyright": null,
                        "Id": "https://khalidabuhakmeh.com/dotnet-maui-app-stopped-working-help",
                        "LastUpdatedTime": "0001-01-01T00:00:00+00:00",
                        "PublishDate": "2023-03-28T00:00:00+00:00",
                        "SourceFeed": null,
                        "Summary": {
                                Text: "blah"
                        },
                        "Title": {
                            "Text": ".NET MAUI App Stopped Working -- HELP!"
                        }
                    }
                """
    )
    {
        this.json = json;
        /// https://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm

        JObject search = JObject.Parse(json);

        // get JSON result objects into a list
        IList<JToken> results = search["records"]
            .Children() /*.Dump("children")*/
            ["fields"]
            /*.Dump("fields children")*/
            .ToList();

        // serialize JSON results into .NET objects
        IList<T> records = new List<T>();
        foreach (JToken result in results)
        {
            // JToken.ToObject is a helper method that uses JsonSerializer internally
            T instance = result.ToObject<T>();
            this.records.Add(instance);
        }
    }

    public void Deconstruct(out string raw_json, out List<T> records)
    {
        raw_json = this.json;
        records =
            this.records /*.Dump("returned records")*/
        ;
    }
}
