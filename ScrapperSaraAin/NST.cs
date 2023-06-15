using Microsoft.Playwright;
using ScrapperSaraAin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ScrapperSaraAin
{
    public static class NST
    {
        public static async Task<List<string>> GetLink()
        {
            //DateTime startTime = DateTime.Now;
            //DateTime targetTime = startTime.AddHours();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();

            List<string> newsLinks = new List<string>();
            List<int> missedPage = new List<int>();

            string url = "https://www.nst.com.my/news/politics?page=";
            //int maxTesting = 100;
            int count = 0;
            bool hasContent = true;
            //&& count < maxTesting //for testing
            
            while (hasContent)
            {
                //if (DateTime.Now > targetTime)
                //{
                //    Console.WriteLine(":,D");
                //    //break;
                //}

                string theURL = url+ count;
                Console.Write($"\rCurrently at {theURL}.");
                await page.GotoAsync(theURL);

                Thread.Sleep(12000);//wait for the page to load

                var content = await page.QuerySelectorAsync("div.article-listing");
                bool isEmpty = await page.EvaluateAsync<bool>
                    (@"(element) => { return element.textContent.trim() === '';}", content);   //check if div content is empty or not

                if (count<817 || !isEmpty)
                {
                    if (isEmpty)
                    {
                        missedPage.Add(count);
                    }
                    //Find all<a> elements on the page
                    var links = await page.QuerySelectorAllAsync("a");
                    // Iterate over the links and extract their href attributes
                    foreach (var link in links)
                    {
                        string href = await link.GetAttributeAsync("href");
                        
                        if (href.StartsWith("/news/politics/20"))
                        {
                            newsLinks.Add(href);
                            
                        }
                        if (href.StartsWith("https://www.nst.com.my/news/politics?page="))
                        {
                            break;//stop iterate
                        }
                    }
                    Console.Write($" {newsLinks.Count} collected. Supposed to have {(count + 1) * 20}");
                }
                else
                {
                    hasContent = false;
                    Console.WriteLine("\nThere are no more news links to get.");
                    break;
                }
                count++;

            }
            // Close the current page
            await page.CloseAsync();
            // Close the entire browser
            await browser.CloseAsync();
            // Dispose of the browser
            await browser.DisposeAsync();

            
            return newsLinks;
        }
        public static async Task<List<string>> CSVtoList (string filePath)
        {
            List<string> csvData = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    csvData.Add(line);
                }
            }
            csvData.RemoveAt(0);
            csvData = csvData.Distinct().ToList();
            return csvData;
        }
        public static async Task<List<NSTStream>> GetArticles(List<String> newsLinks)
        {
            DateTime startTime = DateTime.Now;
            DateTime targetTime = startTime.AddHours(2);

            List<NSTStream> newsList = new List<NSTStream>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();

            //863,1809,2274,3194,4252,4616,4974,5077,6303,6727,7118,7817,9067,10155,11365
            int countProgress = 11365;

            Console.WriteLine("\n");

            for(int i = countProgress; i < newsLinks.Count; i++)
            {
                if (DateTime.Now > targetTime)
                {
                    Console.WriteLine("\n:,D");//newsLinks EXPORT AS CSV
                    break;
                }

                Console.Write($"\rStatus: {(countProgress + 1) * 100 / newsLinks.Count}%");
                await page.GotoAsync("https://www.nst.com.my" + newsLinks[i]);
                Thread.Sleep(5000);

                // Find the element for headline, date, and article"
                var pageTitleElement = await page.QuerySelectorAsync("h1");
                // Get the text content of the h1 element
                var headline = await pageTitleElement.TextContentAsync();

                var elementHandle = await page.QuerySelectorAsync("div.page-article");

                // Extract the text content of the element
                var textContent = await elementHandle.EvaluateAsync<string>("(element) => element.textContent");

                // Get the date
                Regex regex = new Regex(@"\b(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\s+@\s+\d{1,2}:\d{2}(am|pm)\b", RegexOptions.IgnoreCase);
                Match match = regex.Match(textContent);
                string dateStr = "";

                if (match.Success)
                {
                    dateStr = match.Value;
                }

                string[] parts = dateStr.Split('@');
                DateTime date = DateTime.Parse(parts[0].Trim());

                // Get the article
                var pageContent = await page.QuerySelectorAsync("div.dable-content-wrapper");
                // Get <p> only
                var paragraphs = await pageContent.QuerySelectorAllAsync("p");
                string article = "";

                foreach (var p in paragraphs)
                {
                    article = article + await p.InnerTextAsync();
                }

                newsList.Add(new NSTStream()
                {
                    date = date,
                    headline = headline,
                    article = article
                });
                countProgress++;
            }
            // Close the current page
            await page.CloseAsync();
            // Close the entire browser
            await browser.CloseAsync();
            // Dispose of the browser
            await browser.DisposeAsync();
            return newsList;
        }
    }
}
//List<string> financialWords = new List<string>()
//{
//    "Stocks","Market","Investors","Earnings","Revenue","Profit","Shares",
//    "Economy","Growth","Financial","Report","Analysts","Bonds","Trading",
//    "Acquisitions","Merger","IPO","Federal Reserve","Interest Rates","Inflation",
//    "Economic Indicators","Global Markets","Commodities","Tech Stocks","Banking",
//    "Exchange rate","Currency pair","Forex","USD/MYR","Foreign exchange",
//    "Dollar","Ringgit","Central bank","Monetary policy","Interest rates","Economic indicators",
//    "Inflation","Trade balance","Export","Import","Economic growth","Fiscal policy",
//    "Reserve bank","Market volatility","Investor sentiment","Risk appetite","Safe haven","Currency intervention",
//    "Monetary easing","Capital flows"
//};
//List<string> newsLinks = new List<string>();
//List<NSTStream> news = new List<NSTStream>();
//List<int> missedPage = new List<int>();

//using var playwright = await Playwright.CreateAsync();
//await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
//var page = await browser.NewPageAsync();

//string url = "https://www.nst.com.my/business?page=";
////int maxTesting = 100;
//int count = 883;
//bool hasContent = true;

//while (hasContent)
//{
//    string theURL = url + count;
//    Console.Write($"\rCurrently at {theURL}.");
//    await page.GotoAsync(theURL);

//    Thread.Sleep(10000);//wait for the page to load

//    var content = await page.QuerySelectorAsync("div.article-listing");
//    bool isEmpty = await page.EvaluateAsync<bool>
//        (@"(element) => { return element.textContent.trim() === '';}", content);   //check if div content is empty or not

//    if (count < 1541 || !isEmpty)
//    {
//        if (isEmpty)
//        {
//            missedPage.Add(count);
//        }
//        //Find all<a> elements on the page
//        var articles = await page.QuerySelectorAllAsync("div.article-listing div.article-teaser:not(.d-block)");
//        // Iterate over the links and extract their href attributes
//        foreach (var article in articles)
//        {
//            string html = await article.InnerHTMLAsync();

//            await page.SetContentAsync(html);

//            var title = await page.EvaluateAsync<string>("document.querySelector('h6.field-title').textContent");
//            var teaser = await page.EvaluateAsync<string>("document.querySelector('div.d-block.article-teaser').textContent");

//            string headline = title + " " + teaser;

//            bool containsFinancialWord = financialWords
//                .Select(word => word.ToLower())
//                .Any(word => headline.ToLower().Trim().Contains(word));

//            string href = await page.EvaluateAsync<string>("document.querySelector('a.d-flex.article.listing.mb-3.pb-3').href");

//            if (href.StartsWith("https://www.nst.com.my/business/20") && containsFinancialWord == true)
//            {
//                newsLinks.Add(href);

//                string date = await page.EvaluateAsync<string>("document.querySelector('span.created-ago').textContent");
//                DateTime dateTime = DateTime.Now;
//                if (date.EndsWith("ago"))
//                {
//                    string timeValue = date.Replace(" ago", string.Empty);
//                    if (timeValue.Contains("hour"))
//                    {
//                        timeValue = RemoveTimeUnit(timeValue, "hour");
//                        if (TryParseTimeValue(timeValue, out int hours))
//                        {
//                            dateTime = DateTime.Now.AddHours(-hours);
//                        }
//                    }
//                    else if (timeValue.Contains("minute"))
//                    {
//                        timeValue = RemoveTimeUnit(timeValue, "minute");
//                        if (TryParseTimeValue(timeValue, out int minutes))
//                        {
//                            dateTime = DateTime.Now.AddMinutes(-minutes);
//                        }
//                    }
//                }
//                else
//                {
//                    dateTime = DateTime.ParseExact(date, "MMM d, yyyy @ h:mmtt", CultureInfo.InvariantCulture);
//                }

//                news.Add(new NSTStream()
//                {
//                    date = dateTime,
//                    headline = title,
//                    article = teaser,
//                    link = href
//                });
//            }
//        }
//        Console.Write($" {newsLinks.Count} collected.");
//    }
//    else
//    {
//        hasContent = false;
//        Console.WriteLine("\nThere are no more news links to get.");
//        break;
//    }
//    count++;

//}
//Console.ReadLine();
//string RemoveTimeUnit(string timeValue, string timeUnit)
//{
//    timeValue = timeValue.Replace($" {timeUnit}", string.Empty);
//    timeValue = new string(timeValue.Where(char.IsDigit).ToArray());
//    return timeValue;
//}

//// Helper method to parse the time value into an integer
//bool TryParseTimeValue(string timeValue, out int result)
//{
//    return int.TryParse(timeValue, out result);
//}
