using System.Net;
using POS.Tests.Seed;
using POS.Tests.TestInfrastructure;
using POS.Tests.Utilities;

namespace POS.Tests.Endpoints;

[TestClass]
public class ProductsEndpointsTests : BaseWebTest
{
    private static int _seedId;

    [ClassInitialize]
    public static void Setup(TestContext _)
    {
        _seedId = SeedHelper.EnsureOneProduct(Factory.Services);
    }

    [TestMethod]
    public async Task ListProduct_Returns_200()
    {
        var resp = await Client.GetAsync("/Product/ListProduct");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var html = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(html, "Product");
    }

    [TestMethod]
    public async Task DetailsProduct_Returns_200_When_Exists()
    {
        var resp = await Client.GetAsync($"/Product/DetailsProduct/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task DetailsProduct_Returns_404_When_NotFound()
    {
        var resp = await Client.GetAsync("/Product/DetailsProduct/999999");
        Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [TestMethod]
    public async Task CreateProduct_Post_Redirects_On_Success()
    {
        var (token, _) = await AntiforgeryHelper.GetTokenAsync(Client, "/Product/CreateProduct");

        var form = new Dictionary<string, string>
        {
            ["Name"] = "Test From MSTest",
            ["CategoryId"] = "2",
            ["Price"] = "100.50",
            ["TaxPercent"] = "13",
            ["Stock"] = "7",
            ["ImageUrl"] = "",
            ["Active"] = "true",
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync(
            "/Product/CreateProduct",
            new FormUrlEncodedContent(form)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(resp.Headers.Location?.ToString() ?? "", "/Product/DetailsProduct/");
    }

    [TestMethod]
    public async Task EditProduct_Get_Returns_200()
    {
        var resp = await Client.GetAsync($"/Product/EditProduct/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task EditProduct_Post_Redirects_On_Success()
    {
        var (token, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            $"/Product/EditProduct/{_seedId}"
        );

        var form = new Dictionary<string, string>
        {
            ["Name"] = "Updated From MSTest",
            ["CategoryId"] = "3",
            ["Price"] = "123.45",
            ["TaxPercent"] = "13",
            ["Stock"] = "25",
            ["ImageUrl"] = "",
            ["Active"] = "true",
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync(
            $"/Product/EditProduct/{_seedId}",
            new FormUrlEncodedContent(form)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(
            resp.Headers.Location?.ToString() ?? "",
            $"/Product/DetailsProduct/{_seedId}"
        );
    }

    [TestMethod]
    public async Task DeleteProduct_Get_Returns_200()
    {
        var resp = await Client.GetAsync($"/Product/DeleteProduct/{_seedId}");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task DeleteProduct_Post_Redirects_To_List()
    {
        // Create a product to delete
        var (tokenForCreate, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            "/Product/CreateProduct"
        );
        var createForm = new Dictionary<string, string>
        {
            ["Name"] = "To be deleted",
            ["CategoryId"] = "9",
            ["Price"] = "10",
            ["TaxPercent"] = "13",
            ["Stock"] = "1",
            ["ImageUrl"] = "",
            ["Active"] = "true",
            ["__RequestVerificationToken"] = tokenForCreate,
        };
        var createResp = await Client.PostAsync(
            "/Product/CreateProduct",
            new FormUrlEncodedContent(createForm)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, createResp.StatusCode);

        var location = createResp.Headers.Location?.ToString() ?? "";
        var idStr = location.Split('/').Last();
        Assert.IsTrue(int.TryParse(idStr, out var newId));

        // Delete it
        var (token, _) = await AntiforgeryHelper.GetTokenAsync(
            Client,
            $"/Product/DeleteProduct/{newId}"
        );
        var deleteForm = new Dictionary<string, string>
        {
            ["id"] = newId.ToString(),
            ["__RequestVerificationToken"] = token,
        };

        var resp = await Client.PostAsync(
            "/Product/DeleteProduct",
            new FormUrlEncodedContent(deleteForm)
        );
        Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
        StringAssert.Contains(resp.Headers.Location?.ToString() ?? "", "/Product/ListProduct");
    }
}
