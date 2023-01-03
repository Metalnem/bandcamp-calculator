using System.Xml.Serialization;

namespace Bandcamp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rates = await GetDailyRates();
    }

    private static async Task<List<DailyRates>> GetDailyRates()
    {
        var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        using var client = new HttpClient();
        using var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();

        var serializer = new XmlSerializer(typeof(HistoricalRates));
        var historicalRates = (HistoricalRates)serializer.Deserialize(stream);
        var dailyRates = historicalRates.DailyRates.ToList();

        dailyRates.Sort();

        return dailyRates;
    }
}
