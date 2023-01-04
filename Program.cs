using System.Net.Http.Json;
using System.Xml.Serialization;

namespace Bandcamp;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var cookie = Environment.GetEnvironmentVariable("BANDCAMP_COOKIE");
        var username = Environment.GetEnvironmentVariable("BANDCAMP_USERNAME");
        var crumb = Environment.GetEnvironmentVariable("BANDCAMP_CRUMB");

        if (cookie is null || username is null || crumb is null)
        {
            Console.Error.WriteLine("Required environment variables are missing.");
            return 1;
        }

        using var handler = new HttpClientHandler { UseCookies = false };
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://bandcamp.com/") };
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var timeSeries = await GetTimeSeries();
        var total = 0m;

        await foreach (var purchase in GetPurchases(client, username, crumb))
        {
            var price = purchase.CalculatePrice();
            var exchangeRate = GetExchangeRate(timeSeries, purchase);
            var priceEuro = price / exchangeRate;
            total += priceEuro;

            Console.WriteLine($"{purchase.ItemTitle} ({price} {purchase.Currency} -> {priceEuro:0.##} EUR)");
        }

        Console.WriteLine();
        Console.WriteLine($"Total value of your Bandcamp collection: {total:0.##} EUR");

        return 0;
    }

    private static async IAsyncEnumerable<Purchase> GetPurchases(HttpClient client, string username, string crumb)
    {
        string lastToken = null;

        for (; ; )
        {
            var request = new Request
            {
                Username = username,
                Crumb = crumb,
                LastToken = lastToken,
                Platform = "mac"
            };

            using var response = await client.PostAsJsonAsync("api/orderhistory/1/get_items", request);
            var page = await response.Content.ReadFromJsonAsync<Page>();

            foreach (var purchase in page.Purchases)
            {
                yield return purchase;
            }

            lastToken = page.LastToken;

            if (lastToken == null)
            {
                break;
            }
        }
    }

    private static async Task<List<DailyRates>> GetTimeSeries()
    {
        var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        using var stream = await response.Content.ReadAsStreamAsync();

        var serializer = new XmlSerializer(typeof(HistoricalRates));
        var historicalRates = (HistoricalRates)serializer.Deserialize(stream);
        var timeSeries = historicalRates.TimeSeries.ToList();

        timeSeries.Sort();

        return timeSeries;
    }

    private static decimal GetExchangeRate(List<DailyRates> timeSeries, Purchase purchase)
    {
        if (purchase.Currency == "EUR")
        {
            return 1;
        }

        var date = DateTime.Parse(purchase.PaymentDate);
        var index = timeSeries.BinarySearch(new DailyRates { Date = date });

        if (index < 0)
        {
            index = ~index - 1;
        }

        var rates = timeSeries[index].Rates;
        var rate = rates.First(rate => rate.Currency == purchase.Currency);

        return rate.Value;
    }
}
