using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace POS.Tests.Helpers
{
    public class DriverManager : IDisposable
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        public IWebDriver Driver => _driver;
        public WebDriverWait Wait => _wait;

        private const string TestEmail = "admin@pos.com";
        private const string TestPassword = "Admin123!";

        public DriverManager(string baseUrl, int implicitWait = 10)
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--ignore-certificate-errors");   // ✅ aceptar cert SSL de localhost
            options.AddArgument("--allow-insecure-localhost");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            _driver.Navigate().GoToUrl(baseUrl);
            Login(baseUrl);
        }

        private void Login(string baseUrl)
        {
            try
            {
                Console.WriteLine($"🌐 URL inicial: {_driver.Url}");

                // Esperar campo Email
                _wait.Until(d => {
                    try { return d.FindElement(By.Name("Email")) != null; }
                    catch { return false; }
                });

                Console.WriteLine($"✅ Formulario de login encontrado en: {_driver.Url}");

                _driver.FindElement(By.Name("Email")).SendKeys(TestEmail);
                _driver.FindElement(By.Name("Password")).SendKeys(TestPassword);
                _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                // Esperar que desaparezca la URL de login
                _wait.Until(d => !d.Url.ToLower().Contains("login"));

                Console.WriteLine($"✅ Login exitoso. URL: {_driver.Url}");
            }
            catch (Exception ex)
            {
                // ✅ Ahora lanza excepción en vez de continuar silenciosamente
                var msg = $"❌ Login falló en '{_driver.Url}'. Error: {ex.Message}";
                Console.WriteLine(msg);
                throw new InvalidOperationException(msg, ex);
            }
        }

        public void TakeScreenshot(string fileName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
                Directory.CreateDirectory(folderPath);
                var path = Path.Combine(folderPath, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                screenshot.SaveAsFile(path);
                Console.WriteLine($"📸 Screenshot: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error screenshot: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}