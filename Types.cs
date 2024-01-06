using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Bandcamp;

[XmlRoot("Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
public class HistoricalRates
{
    [XmlArray("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    [XmlArrayItem("Cube")]
    public DailyRates[] TimeSeries { get; set; }
}

public class DailyRates : IComparable<DailyRates>
{
    [XmlAttribute("time")]
    public DateTime Date { get; set; }

    [XmlElement("Cube")]
    public Rate[] Rates { get; set; }

    public int CompareTo(DailyRates other)
    {
        return Date.CompareTo(other.Date);
    }
}

public class Rate
{
    [XmlAttribute("currency")]
    public string Currency { get; set; }

    [XmlAttribute("rate")]
    public decimal Value { get; set; }

    public override string ToString()
    {
        return $"{Value} {Currency}";
    }
}

public class Request
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    [JsonPropertyName("crumb")]
    public string Crumb { get; set; }

    [JsonPropertyName("last_token")]
    public string LastToken { get; set; }
}

public class Page
{
    [JsonPropertyName("items")]
    public List<Purchase> Purchases { get; set; }

    [JsonPropertyName("last_token")]
    public string LastToken { get; set; }
}

public class Purchase
{
    [JsonPropertyName("item_title")]
    public string ItemTitle { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("tax")]
    public decimal? Tax { get; set; }

    [JsonPropertyName("payment_date")]
    public string PaymentDate { get; set; }

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; }

    [JsonPropertyName("band_enabled")]
    public int? BandEnabled { get; set; }

    [JsonPropertyName("killed")]
    public int? Killed { get; set; }

    public decimal CalculatePrice()
    {
        return UnitPrice + (Tax ?? 0);
    }
}
