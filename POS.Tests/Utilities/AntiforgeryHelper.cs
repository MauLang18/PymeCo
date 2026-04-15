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
            baseClient.BaseAddress = new Uri("http://localhost");

        var resp = await baseClient.GetAsync(getFormUrl);

        // ✅ No usar EnsureSuccessStatusCode() — acepta 200 y también 302 (redirect)
        if (!resp.IsSuccessStatusCode && resp.StatusCode != HttpStatusCode.Redirect)
            throw new InvalidOperationException(
                $"Error al obtener formulario '{getFormUrl}': {(int)resp.StatusCode} {resp.StatusCode}");

        // Si hubo redirect al login, el auth bypass no está funcionando
        if (resp.StatusCode == HttpStatusCode.Redirect)
        {
            var location = resp.Headers.Location?.ToString() ?? "";
            if (location.ToLower().Contains("login"))
                throw new InvalidOperationException(
                    $"El endpoint '{getFormUrl}' redirigió al login. " +
                    "Verificar que TestApplicationFactory tiene el auth bypass configurado correctamente.");
        }

        var html = await resp.Content.ReadAsStringAsync();
        var match = TokenRegex.Match(html);

        if (!match.Success)
            throw new InvalidOperationException(
                $"Antiforgery token no encontrado en '{getFormUrl}'. " +
                "Asegurarse que la vista tiene @Html.AntiForgeryToken().");

        return (match.Groups[1].Value, new CookieContainer());
    }
}