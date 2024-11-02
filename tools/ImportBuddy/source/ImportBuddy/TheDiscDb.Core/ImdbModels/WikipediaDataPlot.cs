namespace TheDiscDb.Imdb;

public class WikipediaDataPlot
{
    public string PlainText { get; set; }

    public string Html { get; set; }

    public WikipediaDataPlot()
    {
        PlainText = (Html = string.Empty);
    }
}
