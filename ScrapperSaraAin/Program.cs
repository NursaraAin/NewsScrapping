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

await TEScrapping.GetTENews();

