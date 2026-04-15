using System.Net;
using POS.Tests.Seed;
using POS.Tests.TestInfrastructure;
using POS.Tests.Utilities;

namespace POS.Tests.Endpoints;

[TestClass]
public class ClientsEndpointsTests : BaseWebTest
{
    private static int _seedId;

    [ClassInitialize]
    public static void Setup(TestContext _)
    {
        _seedId = SeedHelper.EnsureOneClient(Factory.Services);
    }

    private static IServiceProvider TestWeb() => Factory.Services;

    [TestMethod]
    public async Task ListClient_Returns_200()
    {
        var resp = await Client.GetAsync("/Client/ListClient");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task DetailsClient_Returns_200_When_Exists()
    {
        var resp = await Client.GetAsync($"/Client/DetailsClient/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task DetailsClient_Returns_404_When_NotFound()
    {
        var resp = await Client.GetAsync("/Client/DetailsClient/999999");
        Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [TestMethod]
    public async Task CreateClient_Post_Redirects_On_Success()
    {
        // ✅ NationalId único por timestamp para evitar conflictos entre runs
        var uniqueSuffix = DateTime.Now.ToString("MMddHHmmss");

        var (token, _) = await AntiforgeryHelper.GetTokenAsync(Client, "/Client/CreateClient");

        var form = new Dictionary<string, string>
        {
            ["Name"] = $"Client Test {uniqueSuffix}",
            ["NationalId"] = $"2{uniqueSuffix}",   // ✅ único cada vez
            ["Email"] = $"client{uniqueSuffix}@test.com",
            ["Phone"] = "2222-2222",
            ["Address"] = "Test Address",
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync("/Client/CreateClient", new FormUrlEncodedContent(form));
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(resp.Headers.Location?.ToString() ?? "", "/Client/DetailsClient/");
    }

    [TestMethod]
    public async Task EditClient_Get_Returns_200()
    {
        var resp = await Client.GetAsync($"/Client/EditClient/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task EditClient_Post_Redirects_On_Success()
    {
        // ✅ NationalId único para evitar conflicto con el seed u otros runs
        var uniqueSuffix = DateTime.Now.ToString("MMddHHmmss");

        var (token, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            $"/Client/EditClient/{_seedId}"
        );

        var form = new Dictionary<string, string>
        {
            ["Name"] = $"Updated Client {uniqueSuffix}",
            ["NationalId"] = $"3{uniqueSuffix}",   // ✅ único cada vez
            ["Email"] = $"updated{uniqueSuffix}@test.com",
            ["Phone"] = "1234-5678",
            ["Address"] = "Updated Address",
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync(
            $"/Client/EditClient/{_seedId}",
            new FormUrlEncodedContent(form)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(
            resp.Headers.Location?.ToString() ?? "",
            $"/Client/DetailsClient/{_seedId}"
        );
    }

    [TestMethod]
    public async Task DeleteClient_Get_Returns_200()
    {
        var resp = await Client.GetAsync($"/Client/DeleteClient/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task DeleteClient_Post_Redirects_To_List()
    {
        // ✅ Crear un cliente temporal con datos únicos para luego borrarlo
        var uniqueSuffix = DateTime.Now.ToString("MMddHHmmss");

        var (tokenForCreate, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            "/Client/CreateClient"
        );
        var createForm = new Dictionary<string, string>
        {
            ["Name"] = $"Client To Delete {uniqueSuffix}",
            ["NationalId"] = $"4{uniqueSuffix}",   // ✅ único cada vez
            ["Email"] = $"del{uniqueSuffix}@test.com",
            ["Phone"] = "9999-9999",
            ["Address"] = "Somewhere",
            ["__RequestVerificationToken"] = tokenForCreate,
        };
        var createResp = await Client.PostAsync(
            "/Client/CreateClient",
            new FormUrlEncodedContent(createForm)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, createResp.StatusCode);

        var location = createResp.Headers.Location?.ToString() ?? "";
        var idStr = location.Split('/').Last();
        Assert.IsTrue(int.TryParse(idStr, out var newId));

        var (token, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            $"/Client/DeleteClient/{newId}"
        );
        var deleteForm = new Dictionary<string, string>
        {
            ["id"] = newId.ToString(),
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync(
            "/Client/DeleteClient",
            new FormUrlEncodedContent(deleteForm)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(resp.Headers.Location?.ToString() ?? "", "/Client/ListClient");
    }
}