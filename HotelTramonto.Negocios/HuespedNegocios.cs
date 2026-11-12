
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class HuespedNegocios
    {
        private readonly HuespedDatos huespedDatos = new HuespedDatos();

        public void Insertar(
            string nombre,
            string apellido,
            string dni,
            string? telefono,
            string? email,
            string nacionalidad)
        {
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
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "El huésped seleccionado no es válido.");

            huespedDatos.Desactivar(idHuesped);
        }

        public DataTable Listar()
        {
            return huespedDatos.Listar();
        }

        public DataTable BuscarPorDNI(string dni)
        {
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
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "El huésped seleccionado no es válido.");

            huespedDatos.Reactivar(idHuesped);
        }

    }

}
