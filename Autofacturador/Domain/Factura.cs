using NUnit.Framework;

namespace BrowserAutomation
{
    public class Factura
    {
        public string Codigo { get; set; }
        public int CantidadHoras { get; set; }
        public int CantidadItems { get; set; }
        public int PrecioPorHora { get; set; }
        public string Descripcion { get; set; }
        public TipoDeFactura Tipo { get; set; }

        public Factura(TipoDeFactura tipoFactura)
        {
            this.PrecioPorHora = int.Parse(TestContext.Parameters[$"{tipoFactura}_precio_hora"]);
            this.CantidadHoras = int.Parse(TestContext.Parameters[$"{tipoFactura}_cantidad_horas"]);
            this.Descripcion = TestContext.Parameters[$"{tipoFactura}_descripcion"];
            this.Codigo = TestContext.Parameters[$"{tipoFactura}_codigo"];
            this.CantidadItems = int.Parse(TestContext.Parameters[$"{tipoFactura}_cantidad_items"]);
            this.Tipo = tipoFactura;
        }
    }
}