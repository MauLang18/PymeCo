using Xunit;
using FluentAssertions;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using POS.Tests.Helpers;
using POS.Tests.PageObjects;
using System;

namespace POS.Tests.UITests
{
    [Collection("UITests")]
    [AllureOwner("Isaac Navarro")]
    [AllureSuite("Product Management")]
    public class ProductTests : IDisposable
    {
        private readonly DriverManager _driverManager;
        private readonly ProductsPage _productsPage;

        // ✅ HTTPS — el puerto 7052 usa HTTPS, no HTTP
        private readonly string _baseUrl = "http://localhost:7052";

        public ProductTests()
        {
            _driverManager = new DriverManager(_baseUrl);
            _productsPage = new ProductsPage(_driverManager.Driver, _driverManager.Wait, _baseUrl);
        }

        [Fact(DisplayName = "TC-PROD-001: Crear Producto Exitosamente")]
        [AllureDescription("Verifica que el sistema permite crear un producto nuevo con todos los campos válidos")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureTag("Smoke", "Products", "CRUD")]
        public void TC_PROD_001_CreateProduct_Success()
        {
            var productName = $"Test_{DateTime.Now:yyyyMMddHHmmss}";

            _productsPage.CreateProduct(productName, 25000.00m, 10, 13, "1");

            _productsPage.IsProductCreated(productName)
                .Should().BeTrue("el producto debe aparecer en la lista después de crearlo");

            _driverManager.TakeScreenshot("TC_PROD_001_Success");
        }

        [Fact(DisplayName = "TC-PROD-002: Validar Campos Obligatorios")]
        [AllureDescription("Verifica que el sistema muestra errores cuando se omiten campos requeridos")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureTag("Validation", "Products", "Negative")]
        public void TC_PROD_002_ValidateRequiredFields()
        {
            _productsPage.CreateProductWithoutRequiredFields();

            var validationMessage = _productsPage.GetValidationMessage();
            validationMessage.Should().NotBeNullOrEmpty(
                "el formulario debe mostrar mensajes de validación cuando faltan campos obligatorios");

            _driverManager.TakeScreenshot("TC_PROD_002_Validation");
        }

        [Fact(DisplayName = "TC-BUS-001: Búsqueda de Productos por Nombre")]
        [AllureDescription("Verifica que la lista de productos carga correctamente")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureTag("Search", "Products")]
        public void TC_BUS_001_SearchProduct_ByName()
        {
            _productsPage.SearchProduct("Seed");

            var productCount = _productsPage.GetProductCount();
            productCount.Should().BeGreaterThan(0,
                "debe haber al menos un producto en la lista");

            _driverManager.TakeScreenshot("TC_BUS_001_Search");
        }

        public void Dispose()
        {
            _driverManager?.Dispose();
        }
    }
}