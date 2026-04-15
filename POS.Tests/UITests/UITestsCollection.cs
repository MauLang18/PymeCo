using Xunit;

/// <summary>
/// Deshabilita el paralelismo para los UITests de Selenium.
/// Sin esto, xUnit corre los 3 tests al mismo tiempo y los drivers
/// de Chrome se pisan entre sí causando "invalid session id".
/// </summary>
[CollectionDefinition("UITests", DisableParallelization = true)]
public class UITestsCollection { }