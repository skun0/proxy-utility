using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Threading;

class Program
{
    static readonly HttpClient httpClient = new HttpClient();
    static readonly string[] urls =
    {
        "https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&proxy_format=ipport&format=text&timeout=20000"
        // poor quality free proxies, not all will be valid.
    };

    static async Task ProxyScraper()
    {
        Console.Clear();
        Console.WriteLine("Scraping proxies..");

        var proxies = new List<string>();
        foreach (var url in urls)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    foreach (var line in text.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            proxies.Add(line.Trim());
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to get proxies from {url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting {url}: {ex.Message}");
            }
        }

        File.WriteAllLines("proxies.txt", proxies);
        Console.WriteLine($"Scraped {proxies.Count} proxies and saved to proxies.txt");
        Console.WriteLine("Press enter to return to menu..");
        Console.ReadLine();
    }

    static async Task ProxyChecker()
    {
        Console.Clear();
        if (!File.Exists("proxies.txt"))
        {
            Console.WriteLine("No proxies.txt file found.");
            Console.WriteLine("Press enter to return to menu..");
            Console.ReadLine();
            return;
        }

        var proxyList = new List<string>(File.ReadAllLines("proxies.txt"));
        Console.WriteLine($"Checking {proxyList.Count} proxies...");

        var validProxies = new List<string>();
        int checkedCount = 0;
        object locker = new object();

        await Task.WhenAll(proxyList.ConvertAll(proxy => Task.Run(async () =>
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy),
                    UseProxy = true
                };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                var resp = await client.GetAsync("https://api.myip.com/");
                lock (locker)
                {
                    checkedCount++;
                    if (resp.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[VALID] {proxy}");
                        validProxies.Add(proxy);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[INVALID] {proxy}");
                    }
                    Console.ResetColor();
                }
            }
            catch
            {
                lock (locker)
                {
                    checkedCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[INVALID] {proxy}");
                    Console.ResetColor();
                }
            }
        })));

        File.WriteAllLines("valid_proxies.txt", validProxies);
        Console.WriteLine($"\n{validProxies.Count} valid proxies saved to valid_proxies.txt");
        Console.WriteLine($"{proxyList.Count - validProxies.Count} proxies were invalid.");
        Console.WriteLine("Press enter to return to menu..");
        Console.ReadLine();
    }

    static async Task Main()
    {
        while (true)
        {
            Console.Title = "x.skuno";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            string logo = @"
        ██████╗ ██████╗  ██████╗ ██╗  ██╗██╗   ██╗    ███████╗ ██████╗██████╗  █████╗ ██████╗ ███████╗██████╗ 
        ██╔══██╗██╔══██╗██╔═══██╗╚██╗██╔╝╚██╗ ██╔╝    ██╔════╝██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔════╝██╔══██╗
        ██████╔╝██████╔╝██║   ██║ ╚███╔╝  ╚████╔╝     ███████╗██║     ██████╔╝███████║██████╔╝█████╗  ██████╔╝
        ██╔═══╝ ██╔══██╗██║   ██║ ██╔██╗   ╚██╔╝      ╚════██║██║     ██╔══██╗██╔══██║██╔═══╝ ██╔══╝  ██╔══██╗
        ██║     ██║  ██║╚██████╔╝██╔╝ ██╗   ██║       ███████║╚██████╗██║  ██║██║  ██║██║     ███████╗██║  ██║
        ╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝       ╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚══════╝╚═╝  ╚═╝
                                                                                                      ";
            Console.WriteLine(logo);
            Console.WriteLine("                  1 - Proxy Scraper                    2 - Proxy Checker                    3 - Exit");
            Console.ResetColor();
            Console.Write("\n\n:");
            

            string choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1":
                    await ProxyScraper();
                    break;
                case "2":
                    await ProxyChecker();
                    break;
                case "3":
                    Console.Clear();
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor= ConsoleColor.White;
                    Console.WriteLine("Advertisement: This script has been made by https://github.com/skun0. Press any key to exit..");
                    Console.ReadKey();
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    Console.ReadLine();
                    break;
            }
        }
    }
}
