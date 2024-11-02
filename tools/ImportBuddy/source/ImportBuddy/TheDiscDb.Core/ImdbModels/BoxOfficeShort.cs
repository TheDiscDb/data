namespace TheDiscDb.Imdb;

public record BoxOfficeShort
{
    public string Budget { get; set; } = string.Empty;
    public string OpeningWeekendUSA { get; set; } = string.Empty;
    public string GrossUSA { get; set; } = string.Empty;
    public string CumulativeWorldwideGross { get; set; } = string.Empty;
}
