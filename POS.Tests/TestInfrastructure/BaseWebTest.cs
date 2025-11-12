using Microsoft.AspNetCore.Mvc.Testing;

namespace POS.Tests.TestInfrastructure;

[TestClass]
public abstract class BaseWebTest
{
    protected static TestApplicationFactory Factory = default!;
    protected static HttpClient Client = default!;

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext _)
    {
        Factory = new TestApplicationFactory();
        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}
