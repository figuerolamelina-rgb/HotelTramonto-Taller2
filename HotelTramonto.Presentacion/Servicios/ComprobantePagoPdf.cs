
using System;
using System.Data;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelTramonto.Presentacion.Servicios
{
    internal class ComprobantePagoPdf
    {
        public static void Generar(DataRow pago, string rutaArchivo)
        {
            if (pago == null)
                throw new ArgumentNullException(nameof(pago));

            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("Debe indicar dónde guardar el PDF.");

            HotelTramonto.Negocios.ComprobantePagoNegocios.Validar(pago);
            QuestPDF.Settings.License = LicenseType.Community;

            CultureInfo cultura = new CultureInfo("es-AR");

            // Colores institucionales del hotel
            string marron = "#4A3027";
            string dorado = "#C6A15B";
            string crema = "#F5EFE5";
            string gris = "#766C65";

            // Datos del pago
            int idPago = Convert.ToInt32(pago["id_pago"]);
            int idReserva = Convert.ToInt32(pago["id_reserva"]);
            decimal monto = Convert.ToDecimal(pago["monto"]);

            string nombreHuesped =
                $"{pago["nombre_huesped"]} {pago["apellido_huesped"]}";

            string dni = Convert.ToString(pago["dni"]) ?? "";

            string nombreUsuario =
                $"{pago["nombre_usuario"]} {pago["apellido_usuario"]}";

            string metodoPago =
                Convert.ToString(pago["metodo_pago"]) ?? "";

            string tipoPago =
                Convert.ToString(pago["tipo_pago"]) ?? "";

            DateTime fechaPago =
                Convert.ToDateTime(pago["fecha_pago"]);

            string concepto = tipoPago switch
            {
                "PAGO_INICIAL" => "Pago inicial de alojamiento",
                "SALDO_ALOJAMIENTO" => "Saldo de alojamiento",
                "SERVICIOS_ADICIONALES" => "Servicios adicionales",
                _ => tipoPago
            };

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x =>
                        x.FontSize(11)
                         .FontColor(marron));

                    // ENCABEZADO
                    page.Header().Column(header =>
                    {
                        header.Item()
                            .Background(marron)
                            .Padding(22)
                            .Column(col =>
                            {
                                col.Item()
                                    .Text("HOTEL TRAMONTO")
                                    .FontSize(25)
                                    .Bold()
                                    .FontColor(dorado);

                                col.Item()
                                    .PaddingTop(5)
                                    .Text("COMPROBANTE DE PAGO")
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor(Colors.White);
                            });

                        header.Item()
                            .Background(dorado)
                            .Height(4);
                    });

                    // CONTENIDO
                    page.Content()
                        .PaddingTop(30)
                        .Column(column =>
                        {
                            column.Spacing(18);

                            // Número y fecha del comprobante
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item()
                                        .Text("COMPROBANTE N.º")
                                        .FontSize(9)
                                        .FontColor(gris);

                                    col.Item()
                                        .Text(idPago.ToString("D6"))
                                        .FontSize(17)
                                        .Bold();
                                });

                                row.RelativeItem()
                                    .AlignRight()
                                    .Column(col =>
                                    {
                                        col.Item()
                                        .AlignRight()
                                        .Text("FECHA DE EMISIÓN DEL PAGO")
                                        .FontSize(9)
                                        .FontColor(gris);

                                        col.Item()
                                        .AlignRight()
                                        .Text(fechaPago.ToString(
                                            "dd/MM/yyyy HH:mm", cultura))
                                        .FontSize(11)
                                        .SemiBold();
                                    });
                            });

                            // Datos de la reserva
                            column.Item()
                                .Background(crema)
                                .Padding(15)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("RESERVA N.º")
                                        .SemiBold();

                                    row.RelativeItem()
                                        .AlignRight()
                                        .Text(idReserva.ToString())
                                        .Bold();
                                });

                            // Datos del huésped
                            column.Item()
                                .PaddingTop(8)
                                .Text("DATOS DEL HUÉSPED")
                                .FontSize(13)
                                .Bold()
                                .FontColor(marron);

                            column.Item()
                                .LineHorizontal(1)
                                .LineColor(dorado);

                            column.Item().Column(col =>
                            {
                                col.Spacing(8);

                                col.Item()
                                    .Text($"Nombre y apellido: {nombreHuesped}");

                                col.Item()
                                    .Text($"DNI: {dni}");
                            });

                            // Detalle del pago
                            column.Item()
                                .PaddingTop(8)
                                .Text("DETALLE DEL PAGO")
                                .FontSize(13)
                                .Bold()
                                .FontColor(marron);

                            column.Item()
                                .LineHorizontal(1)
                                .LineColor(dorado);

                            column.Item().Column(col =>
                            {
                                col.Spacing(8);

                                col.Item()
                                    .Text($"Concepto: {concepto}");

                                col.Item()
                                    .Text($"Método de pago: {metodoPago}");

                                col.Item()
                                    .Text("Estado: APROBADO")
                                    .FontColor("#38724A")
                                    .SemiBold();
                            });

                            // Importe abonado
                            column.Item()
                                .PaddingTop(12)
                                .Background(crema)
                                .Border(1)
                                .BorderColor(dorado)
                                .Padding(22)
                                .Column(col =>
                                {
                                    col.Spacing(8);

                                    col.Item()
                                        .Text("IMPORTE ABONADO")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor(marron);

                                    col.Item()
                                        .Text(monto.ToString("C2", cultura))
                                        .FontSize(27)
                                        .Bold()
                                        .FontColor(marron);
                                });

                            // Usuario que registró el pago
                            column.Item()
                                .PaddingTop(15)
                                .Text($"Registrado por: {nombreUsuario}")
                                .FontSize(10)
                                .FontColor(gris);

                            // Aclaración
                            column.Item()
                                .PaddingTop(25)
                                .AlignCenter()
                                .Text("Comprobante interno de pago. No válido como factura.")
                                .FontSize(9)
                                .Italic()
                                .FontColor(gris);
                        });

                    // PIE DE PÁGINA
                    page.Footer().Column(footer =>
                    {
                        footer.Item()
                            .LineHorizontal(1)
                            .LineColor(dorado);

                        footer.Item()
                            .PaddingTop(10)
                            .AlignCenter()
                            .Text("HOTEL TRAMONTO | Empedrado, Corrientes")
                            .FontSize(9)
                            .FontColor(gris);
                    });
                });
            }).GeneratePdf(rutaArchivo);
        }
    }
}
