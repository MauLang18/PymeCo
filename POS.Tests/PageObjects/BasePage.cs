using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace POS.Tests.PageObjects
{
    /// <summary>
    /// Clase base con métodos comunes para todas las páginas
    /// </summary>
    public class BasePage
    {
        protected IWebDriver Driver;
        protected WebDriverWait Wait;

        public BasePage(IWebDriver driver, WebDriverWait wait)
        {
            Driver = driver;
            Wait = wait;
        }

        protected IWebElement FindElement(By locator)
        {
            Wait.Until(drv => drv.FindElement(locator));
            return Driver.FindElement(locator);
        }

        protected void Click(By locator)
        {
            FindElement(locator).Click();
        }

        protected void SendKeys(By locator, string text)
        {
            var element = FindElement(locator);
            element.Clear();
            element.SendKeys(text);
        }

        protected string GetText(By locator)
        {
            return FindElement(locator).Text;
        }

        protected bool IsElementDisplayed(By locator)
        {
            try
            {
                return FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected void WaitForElement(By locator, int seconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            wait.Until(drv => drv.FindElement(locator).Displayed);
        }
    }
}