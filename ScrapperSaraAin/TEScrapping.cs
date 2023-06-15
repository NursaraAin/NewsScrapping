using Microsoft.Playwright;
using HtmlAgilityPack;
using ScrapperSaraAin;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ScrapperSaraAin
{
    public static class TEScrapping
    {
        public static async Task GetTENews()
        {
            //await MalaysiaNews.GetMalaysiaNews();
            HttpClient httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            string theStreamLink = "https://tradingeconomics.com/ws/stream.ashx?c=*country&start=*startInt&size=*sizeInt";

            List<TEStreamJson> listStreams = new List<TEStreamJson>();

            int itemSizePerPage = 100;

            string theURLToIterate = theStreamLink
                .Replace("*country", "united states")
                .Replace("*sizeInt", itemSizePerPage.ToString());

            string contentBody = "Not Blank";
            int multiplier = 0;

            //httpClient.DefaultRequestHeaders.Add("Cookie", "ASP.NET_SessionId=b2ib4lgl4dflvbb12kqn3jr2");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.68");

            DateTime datelimit = new DateTime(2013,1,1);//date limit

            bool conditionMet = false;

            while (contentBody != "" && !conditionMet)
            {
                string theURL = theURLToIterate
                    .Replace("*startInt", (multiplier * itemSizePerPage).ToString());


                Console.Write($"\rThe StartLine is {multiplier * itemSizePerPage}");

                HttpResponseMessage responseMessage = await httpClient.GetAsync(theURL);

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    contentBody = await responseMessage.Content.ReadAsStringAsync();

                    try
                    {
                        if (contentBody != "")
                        {
                            List<TEStreamJson> range = JsonConvert.DeserializeObject<List<TEStreamJson>>(contentBody);
                            foreach (TEStreamJson item in range)
                            {
                                if (item.date < datelimit)
                                {
                                    Console.WriteLine("\nSuccessfully Collected");
                                    conditionMet = true;
                                    break;
                                }
                                else
                                {
                                    listStreams.Add(item);
                                }
                            }
                            //listStreams.AddRange(range);
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else if (responseMessage.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                {
                    Console.WriteLine($"\nTE blocked access or Maximum amount reached. Multiplier is currently at {multiplier}");
                    conditionMet = true;
                    break;
                }
                multiplier++;
                Thread.Sleep(8000);
            }

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbooks workbooks = xlApp.Workbooks;
            Excel.Workbook workbook = workbooks.Add();
            Excel.Worksheet worksheet = workbook.Sheets[1];

            PropertyInfo[] propertyInfos = typeof(TEStreamJson).GetProperties();

            char letter = 'A';
            //int number = 1;

            Console.WriteLine("\nWriting to Excel...");
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                worksheet.Range[letter + "1"].Value = propertyInfo.Name;
                letter++;
            }
            for (int j = 0; j < listStreams.Count; j++)
            {
                letter = 'A';
                TEStreamJson data = listStreams[j];
                for (int i = 0; i < propertyInfos.Length; i++)
                {
                    string loc = letter + (j + 2).ToString();
                    worksheet.Range[loc].Value = propertyInfos[i].GetValue(data);
                    letter++;
                }
                double progress = ((j + 1) * 100 / listStreams.Count);
                Console.Write($"\r{progress}%");
            }

            xlApp.Visible = true;
        }

        //HttpClient httpClient = new HttpClient()
        //{
        //    Timeout = TimeSpan.FromSeconds(20)
        //};
        //using var pw = await Playwright.CreateAsync();
        //await using var browser = await pw.Chromium.LaunchAsync(options: new BrowserTypeLaunchOptions
        //    {
        //        Headless = false
        //    });

        ////var browser = await pw.Chromium.ConnectOverCDPAsync("http://localhost:9222");


        //var page = await browser.NewPageAsync();
        //await page.GotoAsync("https://www.nst.com.my/news/politics?page=0");

        //var title = await page.TitleAsync();
        //Console.WriteLine(title);

    }

}
