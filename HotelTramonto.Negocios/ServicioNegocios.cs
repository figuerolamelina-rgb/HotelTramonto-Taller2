
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class ServicioNegocios
    {
        private readonly ServicioDatos datos = new ServicioDatos();

        private void ValidarAdministrador(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol) ||
                rol.Trim().ToUpperInvariant() != "ADMINISTRADOR")
            {
                throw new UnauthorizedAccessException(
                    "No tenés permisos para gestionar los servicios.");
            }
        }

        private void ValidarDatos(
            string nombre,
            decimal precio,
            string categoria)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "Ingresá el nombre del servicio.");

            if (nombre.Trim().Length > 50)
                throw new ArgumentException(
                    "El nombre no puede superar los 50 caracteres.");

            if (precio <= 0)
                throw new ArgumentException(
                    "El precio debe ser mayor que cero.");

            if (precio > 99999999.99m)
                throw new ArgumentException(
                    "El precio supera el máximo permitido.");

            string[] categorias =
            {
                "Almuerzo",
                "Merienda",
                "Cena",
                "Tragos",
                "Pesca guiada",
                "Incluido",
                "Otros"
            };

            if (Array.IndexOf(categorias, categoria) < 0)
                throw new ArgumentException(
                    "Seleccioná una categoría válida.");
        }

        public void Insertar(
            string nombre,
            string? descripcion,
            decimal precio,
            string categoria,
            string rol)
        {
            ValidarAdministrador(rol);
            ValidarDatos(nombre, precio, categoria);

            if (descripcion?.Length > 150)
                throw new ArgumentException(
                    "La descripción no puede superar los 150 caracteres.");

            datos.Insertar(
                nombre.Trim(),
                descripcion,
                precio,
                categoria);
        }

        public void Modificar(
            int idServicio,
            string nombre,
            string? descripcion,
            decimal precio,
            string categoria,
            string rol)
        {
            ValidarAdministrador(rol);

            if (idServicio <= 0)
                throw new ArgumentException(
                    "El servicio seleccionado no es válido.");

            ValidarDatos(nombre, precio, categoria);

            if (descripcion?.Length > 150)
                throw new ArgumentException(
                    "La descripción no puede superar los 150 caracteres.");

            datos.Modificar(
                idServicio,
                nombre.Trim(),
                descripcion,
                precio,
                categoria);
        }

        public void Desactivar(int idServicio, string rol)
        {
            ValidarAdministrador(rol);

            if (idServicio <= 0)
                throw new ArgumentException(
                    "El servicio seleccionado no es válido.");

            datos.Desactivar(idServicio);
        }

        public DataTable Listar(string rol)
        {
            ValidarAdministrador(rol);

            return datos.Listar();
        }


        public DataTable ListarInactivos(string rol)
        {
            ValidarAdministrador(rol);

            return datos.ListarInactivos();
        }

        public void Reactivar(int idServicio, string rol)
        {
            ValidarAdministrador(rol);

            if (idServicio <= 0)
                throw new ArgumentException(
                    "El servicio seleccionado no es válido.");

            datos.Reactivar(idServicio);
        }

        public DataTable ListarParaConsumo()
        {
            DataTable servicios = datos.Listar();

            // Los servicios incluidos en el alojamiento
            // no deben aparecer como consumos adicionales.
            DataView vista = new DataView(servicios);

            vista.RowFilter = "categoria <> 'Incluido'";

            return vista.ToTable();
        }

    }
}
