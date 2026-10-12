
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotelTramonto.Negocios;
using System;

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

        [TestMethod]
        public void DNIValido_ConSoloNumeros_RetornaTrue()
        {
            bool resultado = Validaciones.DNIValido("12345678");
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void DNIValido_ConLetras_RetornaFalse()
        {
            bool resultado = Validaciones.DNIValido("1234AB78");
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void EmailValido_ConFormatoCorrecto_RetornaTrue()
        {
            bool resultado = Validaciones.EmailValido("persona@gmail.com");
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void EmailValido_SinArroba_RetornaFalse()
        {
            bool resultado = Validaciones.EmailValido("personagmail.com");
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void MontoValido_ConMontoPositivo_RetornaTrue()
        {
            bool resultado = Validaciones.MontoValido(15000m);
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void MontoValido_ConCero_RetornaFalse()
        {
            bool resultado = Validaciones.MontoValido(0m);
            Assert.IsFalse(resultado);
        }
    }
}
