using System.Net;
using System.Text.RegularExpressions;

namespace POS.Tests.Utilities;

public static class AntiforgeryHelper
{
    private static readonly Regex TokenRegex = new(
        @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public static async Task<(string token, CookieContainer cookies)> GetTokenAsync(
        HttpClient baseClient,
        string getFormUrl
    )
    {
        if (baseClient.BaseAddress is null)
        {
            baseClient.BaseAddress = new Uri("http://localhost");
        }

        var resp = await baseClient.GetAsync(getFormUrl);
        resp.EnsureSuccessStatusCode();

        var html = await resp.Content.ReadAsStringAsync();
        var match = TokenRegex.Match(html);
        if (!match.Success)
            throw new InvalidOperationException("Antiforgery token not found.");

        return (match.Groups[1].Value, new CookieContainer());
    }
}
