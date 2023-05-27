using HtmlAgilityPack;
using Microsoft.Playwright;
using Microsoft.Playwright.Core;
using Newtonsoft.Json;
using ScrapperSaraAin;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Excel = Microsoft.Office.Interop.Excel;

//List<string> newslinks = await NST.GetLink();

//string filePath = "C:\\Users\\nursa\\OneDrive - Universiti Malaya\\Documents\\newsLinks_NST_170523.csv";
//List<string> csvData = await NST.CSVtoList(filePath);

//List<NSTStream> streams = await NST.GetArticles(csvData);

//await TEScrapping.GetTENews();

List<string> financialWords = new List<string>()
{
    "Stocks","Market","Investors","Earnings","Revenue","Profit","Shares",
    "Economy","Growth","Financial","Report","Analysts","Bonds","Trading",
    "Acquisitions","Merger","IPO","Federal Reserve","Interest Rates","Inflation",
    "Economic Indicators","Global Markets","Commodities","Tech Stocks","Banking",
    "Exchange rate","Currency pair","Forex","USD/MYR","Foreign exchange",
    "Dollar","Ringgit","Central bank","Monetary policy","Interest rates","Economic indicators",
    "Inflation","Trade balance","Export","Import","Economic growth","Fiscal policy",
    "Reserve bank","Market volatility","Investor sentiment","Risk appetite","Safe haven","Currency intervention",
    "Monetary easing","Capital flows"
};
List<string> newsLinks = new List<string>();
List<NSTStream> news = new List<NSTStream>();
List<int> missedPage = new List<int>();

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
var page = await browser.NewPageAsync();

string url = "https://www.nst.com.my/business?page=";
//int maxTesting = 100;
int count = 0;
bool hasContent = true;

while (hasContent)
{
    string theURL = url + count;
    Console.Write($"\rCurrently at {theURL}.");
    await page.GotoAsync(theURL);

    Thread.Sleep(10000);//wait for the page to load

    var content = await page.QuerySelectorAsync("div.article-listing");
    bool isEmpty = await page.EvaluateAsync<bool>
        (@"(element) => { return element.textContent.trim() === '';}", content);   //check if div content is empty or not

    if (count < 1539 || !isEmpty)
    {
        if (isEmpty)
        {
            missedPage.Add(count);
        }
        //Find all<a> elements on the page
        var articles = await page.QuerySelectorAllAsync("div.article-listing div.article-teaser:not(.d-block)");
        // Iterate over the links and extract their href attributes
        foreach (var article in articles)
        {
            string html = await article.InnerHTMLAsync();

            await page.SetContentAsync(html);

            var title = await page.EvaluateAsync<string>("document.querySelector('h6.field-title').textContent");
            var teaser = await page.EvaluateAsync<string>("document.querySelector('div.d-block.article-teaser').textContent");

            string headline = title+ " " + teaser;

            bool containsFinancialWord = financialWords
                .Select(word => word.ToLower())
                .Any(word => headline.ToLower().Trim().Contains(word));

            string href = await page.EvaluateAsync<string>("document.querySelector('a.d-flex.article.listing.mb-3.pb-3').href");

            if (href.StartsWith("https://www.nst.com.my/business/20") && containsFinancialWord == true)
            {
                newsLinks.Add(href);

                string date = await page.EvaluateAsync<string>("document.querySelector('span.created-ago').textContent");
                DateTime dateTime = DateTime.Now;
                if (date.EndsWith("ago"))
                {
                    string timeValue = date.Replace(" ago", string.Empty);
                    if (timeValue.Contains("hour"))
                    {
                        timeValue = RemoveTimeUnit(timeValue, "hour");
                        if (TryParseTimeValue(timeValue, out int hours))
                        {
                            dateTime = DateTime.Now.AddHours(-hours);
                        }
                    }
                    else if (timeValue.Contains("minute"))
                    {
                        timeValue = RemoveTimeUnit(timeValue, "minute");
                        if (TryParseTimeValue(timeValue, out int minutes))
                        {
                            dateTime = DateTime.Now.AddMinutes(-minutes);
                        }
                    }
                }
                else
                {
                    dateTime = DateTime.ParseExact(date, "MMM d, yyyy @ h:mmtt", CultureInfo.InvariantCulture);
                }

                news.Add(new NSTStream()
                {
                    date = dateTime,
                    headline = title,
                    article = teaser,
                    link = href
                });
            }
        }
        Console.Write($" {newsLinks.Count} collected.");
    }
    else
    {
        hasContent = false;
        Console.WriteLine("\nThere are no more news links to get.");
        break;
    }
    count++;

}
Console.ReadLine();
string RemoveTimeUnit(string timeValue, string timeUnit)
{
    timeValue = timeValue.Replace($" {timeUnit}", string.Empty);
    timeValue = new string(timeValue.Where(char.IsDigit).ToArray());
    return timeValue;
}

// Helper method to parse the time value into an integer
bool TryParseTimeValue(string timeValue, out int result)
{
    return int.TryParse(timeValue, out result);
}