
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class HuespedNegocios
    {
        private readonly string? rolOperacion;
        public HuespedNegocios(string? rol = null) { rolOperacion = rol; }
        private readonly HuespedDatos huespedDatos = new HuespedDatos();

        public DataTable HistorialReservas(int idHuesped, string rol)
        {
            ExigirPermisoHistorial(rol);
            if (idHuesped <= 0)
                throw new ArgumentOutOfRangeException(nameof(idHuesped),
                    "Seleccioná un huésped válido.");
            return huespedDatos.HistorialReservas(idHuesped);
        }

        public static void ExigirPermisoHistorial(string rol)
        {
            string normalizado = rol?.Trim().ToUpperInvariant() ?? "";
            if (normalizado != "ADMINISTRADOR" && normalizado != "RECEPCIONISTA")
                throw new UnauthorizedAccessException("No tiene permisos para consultar el historial.");
        }

        public void Insertar(
            string nombre,
            string apellido,
            string dni,
            string? telefono,
            string? email,
            string nacionalidad)
        {
            PermisosOperacion.Exigir(rolOperacion);
            ValidarHuesped(nombre, apellido, dni, email, nacionalidad);

            huespedDatos.Insertar(
                nombre.Trim(),
                apellido.Trim(),
                dni.Trim(),
                telefono,
                email,
                nacionalidad.Trim());
        }

        public void Modificar(
            int idHuesped,
            string nombre,
            string apellido,
            string dni,
            string? telefono,
            string? email,
            string nacionalidad)
        {
            PermisosOperacion.Exigir(rolOperacion);
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "El huésped seleccionado no es válido.");

            ValidarHuesped(nombre, apellido, dni, email, nacionalidad);

            huespedDatos.Modificar(
                idHuesped,
                nombre.Trim(),
                apellido.Trim(),
                dni.Trim(),
                telefono,
                email,
                nacionalidad.Trim());
        }

        public void Desactivar(int idHuesped)
        {
            PermisosOperacion.Exigir(rolOperacion);
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "El huésped seleccionado no es válido.");

            huespedDatos.Desactivar(idHuesped);
        }

        public DataTable Listar()
        {
            PermisosOperacion.Exigir(rolOperacion);
            return huespedDatos.Listar();
        }

        public DataTable BuscarPorDNI(string dni)
        {
            PermisosOperacion.Exigir(rolOperacion);
            if (!Validaciones.DNIValido(dni))
                throw new ArgumentException(
                    "El DNI debe contener entre 7 y 15 dígitos.");

            return huespedDatos.BuscarPorDNI(dni.Trim());
        }

        private static void ValidarHuesped(
            string nombre,
            string apellido,
            string dni,
            string? email,
            string nacionalidad)
        {
            if (!Validaciones.HuespedValido(
                nombre, apellido, dni, email, nacionalidad))
            {
                throw new ArgumentException(
                    "Verifique los datos: nombre, apellido, DNI, email y nacionalidad.");
            }
        }


        public void Reactivar(int idHuesped)
        {
            PermisosOperacion.Exigir(rolOperacion);
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "El huésped seleccionado no es válido.");

            huespedDatos.Reactivar(idHuesped);
        }

        public DataTable ObtenerHistorial(int idHuesped)
        {
            var historial = HistorialReservas(idHuesped, rolOperacion!);
            var reservas = new ReservaNegocios(rolOperacion).ListarPorHuesped(idHuesped);
            return DetalleHistorialHuesped.Completar(historial, reservas);
        }


    }

}
