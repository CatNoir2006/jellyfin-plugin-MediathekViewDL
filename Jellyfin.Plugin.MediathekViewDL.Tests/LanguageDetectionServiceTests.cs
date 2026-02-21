using Xunit;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class LanguageDetectionServiceTests
{
    private readonly LanguageDetectionService _service;

    public LanguageDetectionServiceTests()
    {
        _service = new LanguageDetectionService();
    }

    [Theory]
    // Standard Titles (No explicit language)
    [InlineData("Folge 168: Lehrer sind auch nur Menschen (S11/E04)", "deu", "Folge 168: Lehrer sind auch nur Menschen (S11/E04)")]
    [InlineData("Folge 178: Im Auftrag des Teufels (S12/E02)", "deu", "Folge 178: Im Auftrag des Teufels (S12/E02)")]
    [InlineData("Magie außer Kontrolle (S02/E18)", "deu", "Magie außer Kontrolle (S02/E18)")]
    [InlineData("Wo ist er? (S02/E06)", "deu", "Wo ist er? (S02/E06)")]
    [InlineData("Folge 6: Die Bande (S02/E06)", "deu", "Folge 6: Die Bande (S02/E06)")]
    [InlineData("Folge 5: Die Lawine (S02/E05)", "deu", "Folge 5: Die Lawine (S02/E05)")]
    [InlineData("Heinz (S01/E08)", "deu", "Heinz (S01/E08)")]
    [InlineData("Durrieux (S01/E07)", "deu", "Durrieux (S01/E07)")]
    [InlineData("Prudence (S01/E06)", "deu", "Prudence (S01/E06)")]
    [InlineData("Gwen (S01/E05)", "deu", "Gwen (S01/E05)")]
    [InlineData("Quentin (S01/E04)", "deu", "Quentin (S01/E04)")]

    // Explicit Language
    [InlineData("Am Limit (S02/E03) (Englisch)", "eng", "Am Limit (S02/E03)")]
    [InlineData("Heinz (S01/E08) (Französisch)", "fra", "Heinz (S01/E08)")]
    [InlineData("Durrieux (S01/E07) (Französisch)", "fra", "Durrieux (S01/E07)")]
    [InlineData("Prudence (S01/E06) (Französisch)", "fra", "Prudence (S01/E06)")]
    [InlineData("Gwen (S01/E05) (Französisch)", "fra", "Gwen (S01/E05)")]
    [InlineData("Quentin (S01/E04) (Französisch)", "fra", "Quentin (S01/E04)")]
    [InlineData("S02E14 - Opfer.eng.strm", "eng", "S02E14 - Opfer.strm")]
    [InlineData("S02E04 - Krieger und Hüter.eng.strm", "eng", "S02E04 - Krieger und Hüter.strm")]


    // Absolute Numbering (No explicit language)
    [InlineData("Nix für die Katz · 13.06.13 | Folge 1135", "deu", "Nix für die Katz · 13.06.13 | Folge 1135")]
    [InlineData("98. Gefangene von Avalon", "deu", "98. Gefangene von Avalon")]
    [InlineData("94. Iwein der Schreckliche", "deu", "94. Iwein der Schreckliche")]
    [InlineData("3. Anziehungskräfte", "deu", "3. Anziehungskräfte")]
    [InlineData("Folge 52: Akte Waschbär", "deu", "Folge 52: Akte Waschbär")]

    // Feature Tags (keep only 5)
    [InlineData("Alles im Einklang (157) - Audiodeskription", "deu", "Alles im Einklang (157) - Audiodeskription")]
    [InlineData("Der Erlkönig (152) - Audiodeskription", "deu", "Der Erlkönig (152) - Audiodeskription")]
    [InlineData("Gnadenbrot (253) (Audiodeskription)", "deu", "Gnadenbrot (253) (Audiodeskription)")]
    [InlineData("Magie außer Kontrolle (S02/E18) (Audiodeskription)", "deu", "Magie außer Kontrolle (S02/E18) (Audiodeskription)")]
    [InlineData("Wo ist er? (S02/E06) (Audiodeskription)", "deu", "Wo ist er? (S02/E06) (Audiodeskription)")]
    [InlineData("Folge 6: Die Bande (S02/E06) (Audiodeskription)", "deu", "Folge 6: Die Bande (S02/E06) (Audiodeskription)")]

    // Titles without numbers or features
    [InlineData("Folge 5: Die Lawine (S02/E05) (Audiodeskription)", "deu", "Folge 5: Die Lawine (S02/E05) (Audiodeskription)")]
    [InlineData("Arrietty und die wundersame Welt der Borger", "deu", "Arrietty und die wundersame Welt der Borger")]
    [InlineData("u.a. Deutschlandtag der Jungen Union (JU) mit Friedrich Merz (CDU, Bundeskanzler)", "deu", "u.a. Deutschlandtag der Jungen Union (JU) mit Friedrich Merz (CDU, Bundeskanzler)")]
    [InlineData("Gartenreise: Vielfältige Gartenkunst in Dänemark", "deu", "Gartenreise: Vielfältige Gartenkunst in Dänemark")]
    [InlineData("Knochenjob Neuschwanstein: 24 Stunden hinter den Kulissen des Märchenschlosses", "deu", "Knochenjob Neuschwanstein: 24 Stunden hinter den Kulissen des Märchenschlosses")]
    [InlineData("Hladun/Lazarenko vollenden Märchen – Mol/Sörum mit 5. EM-Titel", "deu", "Hladun/Lazarenko vollenden Märchen – Mol/Sörum mit 5. EM-Titel")]



    // Additional Language Tests (OV/OmU)
    [InlineData("Folge 6: Die Bande (S02/E06) (Originalversion)", "und", "Folge 6: Die Bande (S02/E06)")]
    [InlineData("Folge 5: Die Lawine (S02/E05) (Originalversion)", "und", "Folge 5: Die Lawine (S02/E05)")]
    [InlineData("Film Title (OV)", "und", "Film Title")]
    [InlineData("Another Movie (OmU)", "und", "Another Movie")]

    public void DetectLanguage_ShouldDetectAndClean(string inputTitle, string expectedLanguage, string expectedCleanedTitle)
    {
        // Act
        var result = _service.DetectLanguage(inputTitle);

        // Assert
        Assert.Equal(expectedLanguage, result.LanguageCode);
        Assert.Equal(expectedCleanedTitle, result.CleanedTitle);
    }
}
