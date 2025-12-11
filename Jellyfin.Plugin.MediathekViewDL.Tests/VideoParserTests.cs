using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class VideoParserTests
{
    private readonly Mock<ILogger<VideoParser>> _mockLogger;
    private readonly Mock<LanguageDetectionService> _mockLanguageDetectionService;
    private readonly VideoParser _videoParser;

    public VideoParserTests()
    {
        _mockLogger = new Mock<ILogger<VideoParser>>();
        // LanguageDetectionService has a parameterless constructor
        _mockLanguageDetectionService = new Mock<LanguageDetectionService>();

        _videoParser = new VideoParser(_mockLogger.Object, _mockLanguageDetectionService.Object);

        // Setup default behavior for the language detection mock
        _mockLanguageDetectionService.Setup(s => s.DetectLanguage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string title, string _) =>
            {
                var cleaned = title.Replace("(Englisch)", "").Trim();
                return new LanguageDetectionResult { LanguageCode = "de", CleanedTitle = cleaned };
            });
    }

    [Theory]
    [InlineData("Am Limit (S02/E03)", "Am Limit", 2, 3, null)]
    [InlineData("Am Limit (S02/E03) (Englisch)", "Am Limit", 2, 3, null)]
    [InlineData("Folge 168: Lehrer sind auch nur Menschen (S11/E04)", "Lehrer sind auch nur Menschen", 11, 4, null)]
    [InlineData("Folge 178: Im Auftrag des Teufels (S12/E02)", "Im Auftrag des Teufels", 12, 2, null)]
    [InlineData("Folge 8: Score Null (S02/E08)", "Score Null", 2, 8, null)]
    [InlineData("Folge 37: Schatten der Vergangenheit (S04/E13)", "Schatten der Vergangenheit", 4, 13, null)]
    [InlineData("Leo und die explodierenden Melonen (S01/E31)", "Leo und die explodierenden Melonen", 1, 31, null)]
    [InlineData("Leo und der Drachenflieger (S01/E28)", "Leo und der Drachenflieger", 1, 28, null)]
    [InlineData("Heinz (S01/E08)", "Heinz", 1, 8, null)]
    [InlineData("Magie außer Kontrolle (S02/E18)", "Magie außer Kontrolle", 2, 18, null)]
    [InlineData("Wo ist er? (S02/E06)", "Wo ist er?", 2, 6, null)]
    [InlineData("Schulprojekt: Mord (S01/E01)", "Schulprojekt: Mord", 1, 1, null)]
    [InlineData("Von der Metzgerei zum Märchenschloss – (Staffel 9, Folge 3)", "Von der Metzgerei zum Märchenschloss", 9, 3, null)]
    [InlineData("Märchenprinz (S13/E04)", "Märchenprinz", 13, 4, null)]
    [InlineData("Nutella: Das grüne Märchen von Ferrero - Greenwashed? (S2025/E02)", "Nutella: Das grüne Märchen von Ferrero - Greenwashed?", 2025, 2, null)]
    // Additional supported formats (X-Notation, Verbose, etc.)
    [InlineData("My Episode 1x05", "My Episode", 1, 5, null)]
    [InlineData("My Episode 1X05", "My Episode", 1, 5, null)]
    [InlineData("My Episode S01 E05", "My Episode", 1, 5, null)]
    [InlineData("My Episode s01 e05", "My Episode", 1, 5, null)]
    [InlineData("My Episode Staffel 1, Folge 5", "My Episode", 1, 5, null)]
    [InlineData("My Episode (Staffel 1, Folge 5)", "My Episode", 1, 5, null)]
    [InlineData("S03_E01 - Abwärts", "Abwärts", 3, 1, null)]
    public void ParseVideoInfo_ShouldParseNormalNumbering(string title, string expectedEpisodeTitle, int expectedSeason, int expectedEpisode, string? subscriptionName)
    {
        // Act
        var result = _videoParser.ParseVideoInfo(subscriptionName, title);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsShow);
        Assert.Equal(expectedSeason, result.SeasonNumber);
        Assert.Equal(expectedEpisode, result.EpisodeNumber);
        Assert.Equal(expectedEpisodeTitle, result.Title, ignoreCase: true);
    }

    [Theory]
    [InlineData("Nix für die Katz · 13.06.13 | Folge 1135", null, 1135, "Nix für die Katz")]
    [InlineData("98. Gefangene von Avalon", null, 98, "Gefangene von Avalon")]
    [InlineData("94. Iwein der Schreckliche", null, 94, "Iwein der Schreckliche")]
    [InlineData("3. Anziehungskräfte", null, 3, "Anziehungskräfte")]
    [InlineData("Folge 52: Akte Waschbär", null, 52, "Akte Waschbär")]
    [InlineData("Alles im Einklang (157)", null, 157, "Alles im Einklang")]
    [InlineData("Der Erlkönig (152)", null, 152, "Der Erlkönig")]
    [InlineData("Gnadenbrot (253)", null, 253, "Gnadenbrot")]
    [InlineData("34. Ungewohnte Zaubertricks (Hörfassung)", null, 34, "Ungewohnte Zaubertricks")]
    public void ParseVideoInfo_ShouldParseAbsoluteNumbering(string title, string? subscriptionName, int expectedAbsolute, string expectedEpisodeTitle)
    {
        // Act
        var result = _videoParser.ParseVideoInfo(subscriptionName, title);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsShow);
        Assert.Null(result.SeasonNumber);
        Assert.Null(result.EpisodeNumber);
        Assert.Equal(expectedAbsolute, result.AbsoluteEpisodeNumber);
        Assert.Equal(expectedEpisodeTitle, result.Title, ignoreCase: true);
    }

    [Theory]
    [InlineData("Alles im Einklang (157) - Audiodeskription", null, true, false, "Alles im Einklang")]
    [InlineData("Der Erlkönig (152) - Audiodeskription", null, true, false, "Der Erlkönig")]
    [InlineData("Gnadenbrot (253) (Audiodeskription)", null, true, false, "Gnadenbrot")]
    [InlineData("Magie außer Kontrolle (S02/E18) (Audiodeskription)", null, true, false, "Magie außer Kontrolle")]
    [InlineData("Wo ist er? (S02/E06) (Audiodeskription)", null, true, false, "Wo ist er?")]
    [InlineData("34. Ungewohnte Zaubertricks (Hörfassung)", null, true, false, "Ungewohnte Zaubertricks")]
    [InlineData("Hans im Glück (Audiodeskription)", null, true, false, "Hans im Glück")]
    [InlineData("Vom Fischer und seiner Frau (Gebärdensprache)", null, false, true, "Vom Fischer und seiner Frau")]
    [InlineData("König Drosselbart (Audiodeskription)", null, true, false, "König Drosselbart")]
    [InlineData("Helene, die wahre Braut (Gebärdensprache)", null, false, true, "Helene, die wahre Braut")]
    [InlineData("Der Schweinehirt (Audiodeskription)", null, true, false, "Der Schweinehirt")]
    [InlineData("Das Märchen von der silbernen Brücke (Audiodeskription)", null, true, false, "Das Märchen von der silbernen Brücke")]
    [InlineData("Von der Metzgerei zum Märchenschloss – (Staffel 9, Folge 3) (Audiodeskription)", null, true, false, "Von der Metzgerei zum Märchenschloss")]
    [InlineData("Märchenprinz (S13/E04) (Audiodeskription)", null, true, false, "Märchenprinz")]
    [InlineData("Das Märchen vom Frosch und der goldenen Kugel (Hörfassung)", null, true, false, "Das Märchen vom Frosch und der goldenen Kugel")]
    // Titles without numbers or features but need cleaning/subscription removal
    [InlineData("Arrietty und die wundersame Welt der Borger", null, false, false, "Arrietty und die wundersame Welt der Borger")]
    [InlineData("u.a. Deutschlandtag der Jungen Union (JU) mit Friedrich Merz (CDU, Bundeskanzler)", null, false, false, "Deutschlandtag der Jungen Union (JU) mit Friedrich Merz (CDU, Bundeskanzler)")]
    [InlineData("Gartenreise: Vielfältige Gartenkunst in Dänemark", null, false, false, "Gartenreise: Vielfältige Gartenkunst in Dänemark")]
    [InlineData("Knochenjob Neuschwanstein: 24 Stunden hinter den Kulissen des Märchenschlosses", null, false, false, "Knochenjob Neuschwanstein: 24 Stunden hinter den Kulissen des Märchenschlosses")]
    [InlineData("Hladun/Lazarenko vollenden Märchen – Mol/Sörum mit 5. EM-Titel", null, false, false, "Hladun/Lazarenko vollenden Märchen – Mol/Sörum mit 5. EM-Titel")]
    public void ParseVideoInfo_ShouldDetectFeaturesAndCleanTitle(string title, string? subscriptionName, bool hasAd, bool hasGs, string expectedEpisodeTitle)
    {
        // Act
        var result = _videoParser.ParseVideoInfo(subscriptionName, title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hasAd, result.HasAudiodescription);
        Assert.Equal(hasGs, result.HasSignLanguage);
        Assert.Equal(expectedEpisodeTitle, result.Title, ignoreCase: true);
    }

    // This test calls the private method indirectly via the public ParseVideoInfo
    // to ensure the tag cleaning happens as expected within the overall flow.

    [Theory]
    [InlineData("Cooler Film (Trailer)", true, false, "Cooler Film (Trailer)")]
    [InlineData("Trailer: Neuer Film", true, false, "Trailer: Neuer Film")]
    [InlineData("Spannendes Interview", false, true, "Spannendes Interview")]
    [InlineData("Interview: Ein Star packt aus", false, true, "Interview: Ein Star packt aus")]
    public void ParseVideoInfo_ShouldDetectTypeAndKeepTag(string title, bool isTrailer, bool isInterview, string expectedTitle)
    {
        // Act
        var result = _videoParser.ParseVideoInfo(null, title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(isTrailer, result.IsTrailer);
        Assert.Equal(isInterview, result.IsInterview);
        Assert.Equal(expectedTitle, result.Title, ignoreCase: true);
    }
}
