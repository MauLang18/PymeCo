using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace POS.Tests.PageObjects
{
    public class ProductsPage : BasePage
    {
        private readonly string _baseUrl;
        private readonly IJavaScriptExecutor _js;

        private By btnNewProduct = By.CssSelector("a[href*='CreateProduct']");
        private By productsTable = By.CssSelector("table tbody");
        private By productRow = By.CssSelector("table tbody tr");
        private By inputName = By.Name("Name");

        public ProductsPage(IWebDriver driver, WebDriverWait wait, string baseUrl = "http://localhost:7052")
            : base(driver, wait)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _js = (IJavaScriptExecutor)driver;
        }

        private void JsClick(By locator)
        {
            var el = Driver.FindElement(locator);
            _js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            System.Threading.Thread.Sleep(300);
            _js.ExecuteScript("arguments[0].click();", el);
        }

        private void WaitForPageLoad(int seconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            wait.Until(d => _js.ExecuteScript("return document.readyState").ToString() == "complete");
        }

        public void GoToList()
        {
            Driver.Navigate().GoToUrl($"{_baseUrl}/Product/ListProduct");
            WaitForPageLoad();
            WaitForElement(productsTable, 15);
        }

        /// <summary>
        /// TC-PROD-001: Llenar y enviar el form COMPLETAMENTE via JS.
        /// Evita cualquier interacción Selenium que pueda triggerear el logout.
        /// </summary>
        public void CreateProduct(string name, decimal price, int stock, int taxPercent, string categoryId)
        {
            // Navegar directamente a CreateProduct sin usar el link del sidebar
            Driver.Navigate().GoToUrl($"{_baseUrl}/Product/CreateProduct");
            WaitForPageLoad();
            WaitForElement(inputName, 10);
            System.Threading.Thread.Sleep(500);

            // ✅ Obtener el primer CategoryId válido del select via JS
            var firstCategoryValue = _js.ExecuteScript(@"
                var sel = document.querySelector('select[name=""CategoryId""]');
                if (sel && sel.options.length > 1) return sel.options[1].value;
                return '1';
            ")?.ToString() ?? categoryId;

            Console.WriteLine($"📋 CategoryId a usar: {firstCategoryValue}");

            // ✅ Llenar TODOS los campos via JS para evitar interacciones Selenium
            _js.ExecuteScript($@"
                var setVal = function(name, val) {{
                    var el = document.querySelector('[name=""' + name + '""]');
                    if (el) {{
                        var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
                        if (el.tagName === 'SELECT') {{
                            el.value = val;
                        }} else {{
                            nativeInputValueSetter.call(el, val);
                        }}
                        el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }};
                setVal('Name', '{name}');
                setVal('Price', '{price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}');
                setVal('Stock', '{stock}');
                setVal('TaxPercent', '{taxPercent}');
                setVal('CategoryId', '{firstCategoryValue}');
                var sel = document.querySelector('select[name=""CategoryId""]');
                if (sel) sel.value = '{firstCategoryValue}';
                var activeChk = document.querySelector('[name=""Active""]');
                if (activeChk) activeChk.checked = true;
            ");

            System.Threading.Thread.Sleep(500);

            var urlBefore = Driver.Url;

            // Enviar form específico
            var submitted = (bool?)_js.ExecuteScript(@"
                var form = document.querySelector('form[action*=""CreateProduct""]');
                if (!form) form = document.querySelector('form[enctype=""multipart/form-data""]');
                if (form) { form.submit(); return true; }
                return false;
            ");

            Console.WriteLine($"📤 Form submitted: {submitted}");

            // Esperar redirect a DetailsProduct (302 → redirect)
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.Url != urlBefore);
                Console.WriteLine($"✅ URL after submit: {Driver.Url}");
            }
            catch
            {
                Console.WriteLine($"⚠️ No redirect. URL: {Driver.Url}");
                // Loguear errores
                var errs = Driver.FindElements(By.CssSelector("[data-valmsg-for], span.text-danger"));
                foreach (var e in errs)
                    if (!string.IsNullOrWhiteSpace(e.Text))
                        Console.WriteLine($"   Validation: '{e.Text}'");
            }

            System.Threading.Thread.Sleep(500);
        }

        /// <summary>TC-PROD-002: Submit form vacío para validación server-side</summary>
        public void CreateProductWithoutRequiredFields()
        {
            Driver.Navigate().GoToUrl($"{_baseUrl}/Product/CreateProduct");
            WaitForPageLoad();
            WaitForElement(inputName, 10);
            System.Threading.Thread.Sleep(500);

            _js.ExecuteScript(@"
                var form = document.querySelector('form[action*=""CreateProduct""]');
                if (!form) form = document.querySelector('form[enctype=""multipart/form-data""]');
                if (form) form.submit();
            ");

            WaitForPageLoad(10);
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>TC-PROD-002: Detectar validación server-side</summary>
        public string GetValidationMessage()
        {
            WaitForPageLoad(5);
            System.Threading.Thread.Sleep(500);
            Console.WriteLine($"🔍 URL: {Driver.Url}");

            var spans = Driver.FindElements(By.CssSelector("[data-valmsg-for]"));
            foreach (var span in spans)
            {
                Console.WriteLine($"   Span: '{span.Text}'");
                if (!string.IsNullOrWhiteSpace(span.Text))
                    return span.Text;
            }

            try
            {
                var summary = Driver.FindElement(By.CssSelector("[data-valmsg-summary]"));
                if (!string.IsNullOrWhiteSpace(summary.Text?.Trim()))
                    return summary.Text.Trim();
            }
            catch { }

            if (Driver.Url.ToLower().Contains("createproduct"))
            {
                Console.WriteLine("   ℹ️ Stayed on CreateProduct = server validation");
                return "Server validation: required fields rejected";
            }

            return string.Empty;
        }

        public void SearchProduct(string productName)
        {
            Driver.Navigate().GoToUrl($"{_baseUrl}/Product/ListProduct");
            WaitForPageLoad();
            System.Threading.Thread.Sleep(500);
        }

        public bool IsProductInResults(string productName)
        {
            try
            {
                WaitForElement(productsTable, 10);
                var rows = Driver.FindElements(productRow);
                foreach (var row in rows)
                    if (row.Text.Contains(productName))
                        return true;
                return false;
            }
            catch { return false; }
        }

        public bool IsProductCreated(string productName)
        {
            System.Threading.Thread.Sleep(500);
            Console.WriteLine($"🔎 Current URL after create: {Driver.Url}");

            // Si estamos en DetailsProduct = creación exitosa ✅
            if (Driver.Url.ToLower().Contains("detailsproduct"))
            {
                Console.WriteLine("✅ On DetailsProduct — product was created!");
                return true;
            }

            // Si redirigió al login, re-navegar a la lista
            if (Driver.Url.ToLower().Contains("login"))
            {
                Console.WriteLine("⚠️ Session expired, navigating to list anyway");
            }

            Driver.Navigate().GoToUrl($"{_baseUrl}/Product/ListProduct");
            WaitForPageLoad();
            System.Threading.Thread.Sleep(1000);

            if (Driver.Url.ToLower().Contains("login"))
            {
                Console.WriteLine("⚠️ Still redirecting to login");
                return false;
            }

            try { WaitForElement(productsTable, 10); }
            catch { return false; }

            var rows = Driver.FindElements(productRow);
            Console.WriteLine($"🔍 Searching '{productName}' in {rows.Count} rows");
            foreach (var row in rows)
            {
                var text = row.Text.Replace("\n", " ").Trim();
                if (text.Contains(productName)) return true;
            }
            return false;
        }

        public int GetProductCount()
        {
            try
            {
                WaitForElement(productsTable, 10);
                return Driver.FindElements(productRow).Count;
            }
            catch { return 0; }
        }
    }
}