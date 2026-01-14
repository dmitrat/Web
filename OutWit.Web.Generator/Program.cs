using CommandLine;
using OutWit.Web.Generator.Commands;

namespace OutWit.Web.Generator;

/// <summary>
/// Entry point for the OutWit.Web content generator CLI.
/// </summary>
internal static class Program
{
    #region Functions

    public static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<GeneratorOptions>(args)
            .MapResult(
                async (GeneratorOptions options) => await RunGeneratorAsync(options),
                _ => Task.FromResult(1));
    }

    private static async Task<int> RunGeneratorAsync(GeneratorOptions options)
    {
        Console.WriteLine("OutWit.Web Content Generator");
        Console.WriteLine($"  Site Path: {options.SitePath}");
        Console.WriteLine($"  Output Path: {options.GetOutputPath()}");
        Console.WriteLine();

        var config = new GeneratorConfig
        {
            SitePath = options.SitePath,
            OutputPath = options.GetOutputPath(),
            SiteUrl = options.SiteUrl,
            GenerateSitemap = !options.SkipSitemap,
            GenerateSearchIndex = !options.SkipSearch,
            GenerateRssFeed = !options.SkipRss,
            GenerateStaticPages = !options.SkipStatic,
            GenerateOgImages = options.GenerateOgImages,
            ForceOgImages = options.ForceOgImages,
            SearchContentMaxLength = options.SearchContentMaxLength,
            HostingProvider = options.HostingProvider
        };

        var generator = new ContentGenerator(config);
        await generator.GenerateAllAsync();
        
        return 0;
    }

    #endregion
}
