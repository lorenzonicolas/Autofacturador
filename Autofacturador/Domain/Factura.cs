using NUnit.Framework;

namespace Autofacturador
{
    public class Factura(TipoDeFactura tipoFactura)
    {
        public string Codigo { get; set; } = TestContext.Parameters[$"{tipoFactura}_codigo"];
        public int CantidadHoras { get; set; } = int.Parse(TestContext.Parameters[$"{tipoFactura}_cantidad_horas"]);
        public int CantidadItems { get; set; } = int.Parse(TestContext.Parameters[$"{tipoFactura}_cantidad_items"]);
        public int PrecioPorHora { get; set; } = int.Parse(TestContext.Parameters[$"{tipoFactura}_precio_hora"]);
        public string Descripcion { get; set; } = TestContext.Parameters[$"{tipoFactura}_descripcion"];
        public TipoDeFactura Tipo { get; set; } = tipoFactura;
    }
}