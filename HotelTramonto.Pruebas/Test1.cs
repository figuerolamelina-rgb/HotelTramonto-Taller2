
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas
{
    [TestClass]
    public class Test1
    {
        [TestMethod]
        public void TextoObligatorio_ConTextoValido_RetornaTrue()
        {
            bool resultado = Validaciones.TextoObligatorio("Iara");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void TextoObligatorio_ConTextoVacio_RetornaFalse()
        {
            bool resultado = Validaciones.TextoObligatorio("   ");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void FechasReservaValidas_SalidaPosterior_RetornaTrue()
        {
            DateTime entrada = new DateTime(2026, 11, 10);
            DateTime salida = new DateTime(2026, 11, 15);

            bool resultado = Validaciones.FechasReservaValidas(entrada, salida);

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void FechasReservaValidas_SalidaAnterior_RetornaFalse()
        {
            DateTime entrada = new DateTime(2026, 11, 15);
            DateTime salida = new DateTime(2026, 11, 10);

            bool resultado = Validaciones.FechasReservaValidas(entrada, salida);

            Assert.IsFalse(resultado);
        }
    }
}
