using BrowserAutomation;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Autofacturador
{
    [TestFixture]
    public partial class Tests
    {
        private string MONOTRIBUTO_URL;
        private int MAX_FACTURA_SIN_DNI;
        private string CUIT;
        private string ACCESS;
        private ChromeDriver driver;
        private DOMUtils utils;

        [SetUp]
        public void Setup()
        {
            Assert.That(TestContext.Parameters.Count > 0);
            
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            //Creates the ChomeDriver object, Executes tests on Google Chrome
            driver = new ChromeDriver(path + @"\drivers\");
            this.utils = new DOMUtils(driver);
            
            MONOTRIBUTO_URL = GetConfigKey("MONOTRIBUTO_URL");
            MAX_FACTURA_SIN_DNI = int.Parse(GetConfigKey("MAX_FACTURA_SIN_DNI"));
            CUIT = GetConfigKey("CUIT");
            ACCESS = GetConfigKey("ACCESS");
        }

        [Test]
        // [TestCase("06/05/2024", TipoDeFactura.ClasesParticulares)]
        // [TestCase("07/05/2024", TipoDeFactura.Asesoria_SeguridadWeb)]
        // [TestCase("08/05/2024", TipoDeFactura.ClasesParticulares)]
        // [TestCase("09/05/2024", TipoDeFactura.Asesoria_SeguridadWeb)]
        // [TestCase("10/05/2024", TipoDeFactura.ClasesParticulares)]
        
        public void GenerarFactura(string fechaComprobante, TipoDeFactura tipoFactura)
        {            
            if (!DateTime.TryParseExact(fechaComprobante, "d", new CultureInfo("es-ES"), DateTimeStyles.AssumeLocal, out _))
            {
                throw new Exception("Fecha invalida para la factura");
            }

            var factura = new Factura(tipoFactura);

            LOGIN();
            Paso_0_ClickGenerarComprobante();
            Paso_1_Fecha(fechaComprobante);
            Paso_2_ConsumidorFinal();
            Paso_3_DetallesDeLaFactura(factura);
            Paso_4_ValidarFactura(factura);

            // ATENCION - ESTO GENERA LA FACTURA
            Thread.Sleep(1500);
            utils.clickOnElement("btngenerar");
            driver.SwitchTo().Alert().Accept();

            // Assert que se creo bien
            Thread.Sleep(1500);
            var confirmText = driver.FindElement(By.CssSelector("#botones_comprobante")).Text;
            Assert.That(confirmText.Contains("Comprobante Generado", StringComparison.OrdinalIgnoreCase));
            utils.clickOnElementByXPath("//input[@value = 'Menú Principal']");
        }

        /// <summary>
        /// Paso 4 de 4: Validar factura de clases particulares
        /// </summary>
        /// <param name="cantItems"></param>
        /// <param name="factura"></param>
        private void Paso_4_ValidarFactura(Factura factura)
        {
            var domicilio = driver.FindElement(By.XPath("//html/body/div[2]/form/div[2]/table/tbody/tr[3]/td/table/tbody/tr[6]/td")).Text;
            Assert.That(domicilio.Contains(GetConfigKey("DOMICILIO"), StringComparison.OrdinalIgnoreCase));

            var concepto = driver.FindElement(By.XPath("//html/body/div[2]/form/div[2]/table/tbody/tr[3]/td/table/tbody/tr[7]/td")).Text;
            Assert.That("servicios".Equals(concepto, StringComparison.OrdinalIgnoreCase));

            var condicion2 = driver.FindElement(By.XPath("//html/body/div[2]/form/div[2]/table/tbody/tr[5]/td/table/tbody/tr[4]/td")).Text;
            Assert.That("consumidor final".Equals(condicion2, StringComparison.OrdinalIgnoreCase));

            var total = driver.FindElement(
                By.XPath("//html/body/div[2]/form/div[2]/table/tbody/tr[7]/td/table[2]/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td/b"))
                .Text;
            var totalAsDecimal = Convert.ToDecimal(total, new CultureInfo("es-AR"));
            Assert.That(totalAsDecimal.Equals(factura.PrecioPorHora * factura.CantidadHoras * factura.CantidadItems));
            Assert.That(totalAsDecimal < MAX_FACTURA_SIN_DNI);
        }

        /// <summary>
        /// Paso 3 de 4: Completar detalles de factura - Clases Particulares
        /// </summary>
        /// <param name="factura"></param>
        private void Paso_3_DetallesDeLaFactura(Factura factura)
        {
            for (int i = 0; i < factura.CantidadItems; i++)
            {
                FillDataRow(factura, i+2);
                
                if(i+1 < factura.CantidadItems)
                    utils.clickOnElementByXPath("//input[@value = 'Agregar línea descripción']");
            }

            utils.clickOnElementByXPath("//input[@value = 'Continuar >']");
        }

        /// <summary>
        /// Paso 2 de 4: Consumidor final y forma de pago al contado
        /// </summary>
        private void Paso_2_ConsumidorFinal()
        {
            utils.clickOnSelectElement("idivareceptor", " Consumidor Final");
            var labelText = driver.FindElement(By.XPath("//label[@for = 'formadepago1']")).Text;
            Assert.That(labelText.Equals("contado", StringComparison.OrdinalIgnoreCase));
            utils.clickOnElement("formadepago1");
            utils.clickOnElementByXPath("//input[@value = 'Continuar >']");
        }

        /// <summary>
        /// Paso 1 de 4 - Datos de emision: Concepto servicios y fecha de comprobante
        /// </summary>
        /// <param name="fechaComprobante"></param>
        private void Paso_1_Fecha(string fechaComprobante)
        {
            utils.clickOnSelectElement("idconcepto", " Servicios");
            utils.enterText("fsd", fechaComprobante);
            utils.enterText("fsh", fechaComprobante);
            utils.clickOnElementByXPath("//input[@value = 'Continuar >']");
        }

        /// <summary>
        /// Paso 0: click en generar comprobante
        /// </summary>
        private void Paso_0_ClickGenerarComprobante()
        {
            // Click en mi nombre
            utils.clickOnElementByXPath("/html/body/div[2]/form/table/tbody/tr[4]/td/input[2]");

            // A veces aparece este pop up
            if(utils.elementVisible(By.Id("novolveramostrar")))
            {
                utils.clickOnElement("novolveramostrar");
            }

            // Click en Generar comprobante
            utils.clickOnElement("btn_gen_cmp");

            // A veces aparece este pop up
            if(utils.elementVisible(By.Id("novolveramostrar")))
            {
                utils.clickOnElement("novolveramostrar");
            }

            // Select punto de venta
            utils.clickOnSelectElement("puntodeventa", GetConfigKey("PUNTO_DE_VENTA"));

            Thread.Sleep(500);
            var selectedTipoDeComprobante = new SelectElement(driver.FindElement(By.Id("universocomprobante"))).SelectedOption.Text;
            Assert.That(selectedTipoDeComprobante.Equals("factura c", StringComparison.OrdinalIgnoreCase));
            utils.clickOnElementByXPath("//input[@value = 'Continuar >']");
        }

        /// <summary>
        /// Paso inicial: Login en AFIP
        /// </summary>
        private void LOGIN()
        {
            driver.Navigate().GoToUrl(MONOTRIBUTO_URL);
            utils.clickOnElement("aIngresarDeNuevo");
            utils.enterText("F1:username", CUIT);
            utils.clickOnElement("F1:btnSiguiente");
            utils.enterText("F1:password", ACCESS);
            utils.clickOnElement("F1:btnIngresar");

            // go to Emitir Facturas
            utils.clickOnElementByXPath("//button[contains(text(), 'Emitir Factura')]");

            Thread.Sleep(1000);
            driver.SwitchTo().Window(driver.WindowHandles[1]);
            Thread.Sleep(1000);

            if(!utils.elementVisible(By.Id("encabezado_logo_afip")))
            {
                var isInLogginAgain = utils.elementVisible(By.XPath("//h4[contains(text(), 'Ingresar con Clave Fiscal')]"));

                if(isInLogginAgain)
                {
                    utils.enterText("F1:username", GetConfigKey("CUIT"));
                    utils.clickOnElement("F1:btnSiguiente");
                    utils.enterText("F1:password", GetConfigKey("ACCESS"));
                    utils.clickOnElement("F1:btnIngresar");

                    Thread.Sleep(1000);
                }
                else
                {
                    throw new Exception("Something went wrong");
                }
            }
        }

        private void FillDataRow(Factura factura, int selector = 2)
        {
            var baseXPath = "//html/body/div[2]/form/div[1]/div/table/tbody";

            utils.enterTextByXPath($"{baseXPath}/tr[{selector}]/td[1]/input[1]", factura.Codigo);
            utils.enterTextByXPath($"{baseXPath}/tr[{selector}]/td[2]/textarea", factura.Descripcion);
            utils.enterTextByXPath($"{baseXPath}/tr[{selector}]/td[3]/input", factura.CantidadHoras);
            utils.clickOnSelectElementByXPath($"{baseXPath}/tr[{selector}]/td[4]/select", "unidades");
            utils.enterTextByXPath($"{baseXPath}/tr[{selector}]/td[5]/input", factura.PrecioPorHora);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Closes the browser
            //driver.Quit();
        }

        public static string GetConfigKey (string key)
        {
            return TestContext.Parameters[key.ToUpper()] ?? throw new Exception($"Key not found in config: {key}");
        }
    }    
}