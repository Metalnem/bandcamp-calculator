using System.Xml.Serialization;

namespace Bandcamp;

[XmlRoot("Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
public class HistoricalRates
{
    [XmlArray("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    [XmlArrayItem("Cube")]
    public DailyRates[] DailyRates { get; set; }
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
