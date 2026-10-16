
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotelTramonto.Negocios;
using System;

namespace HotelTramonto.Pruebas
{
    [TestClass]
    public class Test1
    {
        // 1. Texto obligatorio con texto válido
        [TestMethod]
        public void TextoObligatorio_ConTextoValido_RetornaTrue()
        {
            bool resultado = Validaciones.TextoObligatorio("Iara");
            Assert.IsTrue(resultado);
        }

        // 2. Texto obligatorio con texto vacío
        [TestMethod]
        public void TextoObligatorio_ConTextoVacio_RetornaFalse()
        {
            bool resultado = Validaciones.TextoObligatorio("   ");
            Assert.IsFalse(resultado);
        }

        // 3. Fecha de salida posterior a la entrada
        [TestMethod]
        public void FechasReservaValidas_SalidaPosterior_RetornaTrue()
        {
            DateTime entrada = new DateTime(2026, 11, 10);
            DateTime salida = new DateTime(2026, 11, 15);

            bool resultado = Validaciones.FechasReservaValidas(entrada, salida);
            Assert.IsTrue(resultado);
        }

        // 4. Fecha de salida anterior a la entrada
        [TestMethod]
        public void FechasReservaValidas_SalidaAnterior_RetornaFalse()
        {
            DateTime entrada = new DateTime(2026, 11, 15);
            DateTime salida = new DateTime(2026, 11, 10);

            bool resultado = Validaciones.FechasReservaValidas(entrada, salida);
            Assert.IsFalse(resultado);
        }

        // 5. DNI compuesto solo por números
        [TestMethod]
        public void DNIValido_ConSoloNumeros_RetornaTrue()
        {
            bool resultado = Validaciones.DNIValido("12345678");
            Assert.IsTrue(resultado);
        }

        // 6. DNI con letras
        [TestMethod]
        public void DNIValido_ConLetras_RetornaFalse()
        {
            bool resultado = Validaciones.DNIValido("1234AB78");
            Assert.IsFalse(resultado);
        }

        // 7. Correo con formato correcto
        [TestMethod]
        public void EmailValido_ConFormatoCorrecto_RetornaTrue()
        {
            bool resultado = Validaciones.EmailValido("persona@gmail.com");
            Assert.IsTrue(resultado);
        }

        // 8. Correo sin arroba
        [TestMethod]
        public void EmailValido_SinArroba_RetornaFalse()
        {
            bool resultado = Validaciones.EmailValido("personagmail.com");
            Assert.IsFalse(resultado);
        }

        // 9. Monto positivo
        [TestMethod]
        public void MontoValido_ConMontoPositivo_RetornaTrue()
        {
            bool resultado = Validaciones.MontoValido(15000m);
            Assert.IsTrue(resultado);
        }

        // 10. Monto igual a cero
        [TestMethod]
        public void MontoValido_ConCero_RetornaFalse()
        {
            bool resultado = Validaciones.MontoValido(0m);
            Assert.IsFalse(resultado);
        }

        // 11. DNI con menos de siete dígitos
        [TestMethod]
        public void DNIValido_ConMenosDeSieteDigitos_RetornaFalse()
        {
            bool resultado = Validaciones.DNIValido("123456");
            Assert.IsFalse(resultado);
        }

        // 12. DNI con más de quince dígitos
        [TestMethod]
        public void DNIValido_ConMasDeQuinceDigitos_RetornaFalse()
        {
            bool resultado = Validaciones.DNIValido("1234567890123456");
            Assert.IsFalse(resultado);
        }

        // 13. Correo vacío
        [TestMethod]
        public void EmailValido_ConTextoVacio_RetornaFalse()
        {
            bool resultado = Validaciones.EmailValido("");
            Assert.IsFalse(resultado);
        }

        // 14. Correo sin dominio
        [TestMethod]
        public void EmailValido_SinDominioValido_RetornaFalse()
        {
            bool resultado = Validaciones.EmailValido("persona@");
            Assert.IsFalse(resultado);
        }

        // 15. Monto negativo
        [TestMethod]
        public void MontoValido_ConMontoNegativo_RetornaFalse()
        {
            bool resultado = Validaciones.MontoValido(-100m);
            Assert.IsFalse(resultado);
        }

        // 16. Texto nulo
        [TestMethod]
        public void TextoObligatorio_ConTextoNulo_RetornaFalse()
        {
            bool resultado = Validaciones.TextoObligatorio(null);
            Assert.IsFalse(resultado);
        }

        // 17. Huésped con todos los datos correctos
        [TestMethod]
        public void HuespedValido_ConDatosCorrectos_RetornaTrue()
        {
            bool resultado = Validaciones.HuespedValido(
                "Ana", "Gómez", "12345678", "ana@gmail.com", "Argentina");

            Assert.IsTrue(resultado);
        }

        // 18. Huésped sin nombre
        [TestMethod]
        public void HuespedValido_SinNombre_RetornaFalse()
        {
            bool resultado = Validaciones.HuespedValido(
                "", "Gómez", "12345678", "ana@gmail.com", "Argentina");

            Assert.IsFalse(resultado);
        }

        // 19. Huésped con DNI inválido
        [TestMethod]
        public void HuespedValido_ConDNIInvalido_RetornaFalse()
        {
            bool resultado = Validaciones.HuespedValido(
                "Ana", "Gómez", "1234AB78", "ana@gmail.com", "Argentina");

            Assert.IsFalse(resultado);
        }
        // 20. Huésped sin email
        [TestMethod]
        public void HuespedValido_SinEmail_RetornaTrue()
        {
            bool resultado = Validaciones.HuespedValido(
                "Ana", "Gómez", "12345678", null, "Argentina");

            Assert.IsTrue(resultado);
        }

    }
}
