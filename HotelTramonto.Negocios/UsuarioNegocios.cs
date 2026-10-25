
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class UsuarioNegocios
    {
        private readonly UsuarioDatos usuarioDatos;

        public UsuarioNegocios()
        {
            usuarioDatos = new UsuarioDatos();
        }

        public DataTable Listar()
        {
            return usuarioDatos.Listar();
        }

        public DataTable Listar(string rol)
        {
            Permisos.ExigirAdministrador(rol);

            return usuarioDatos.Listar();
        }

        public DataTable BuscarPorDNI(string dni)
        {
            if (!Validaciones.DNIValido(dni))
            {
                throw new ArgumentException(
                    "El DNI debe contener entre 7 y 15 dígitos.");
            }

            return usuarioDatos.BuscarPorDNI(dni.Trim());
        }

        public void Insertar(
    int idRol,
    string nombre,
    string apellido,
    string dni,
    string email,
    string contrasena,
    string? telefono,
    string rolUsuario)
        {
            Permisos.ExigirAdministrador(rolUsuario);

            ValidarDatos(
                idRol, nombre, apellido, dni, email, contrasena);

            string contrasenaHash = PasswordService.Hash(contrasena);

            usuarioDatos.Insertar(
                idRol,
                nombre.Trim(),
                apellido.Trim(),
                dni.Trim(),
                email.Trim(),
                contrasenaHash,
                telefono);
        }

        public void Modificar(
            int idUsuario,
            int idRol,
            string nombre,
            string apellido,
            string dni,
            string email,
            string? telefono,
            string rolUsuario)
        {
            Permisos.ExigirAdministrador(rolUsuario);

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            ValidarDatos(
                idRol, nombre, apellido, dni, email, null);

            usuarioDatos.Modificar(
                idUsuario,
                idRol,
                nombre.Trim(),
                apellido.Trim(),
                dni.Trim(),
                email.Trim(),
                telefono);
        }


        public void Desactivar(int idUsuario, string rolUsuario)
        {
            Permisos.ExigirAdministrador(rolUsuario);

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            usuarioDatos.Desactivar(idUsuario);
        }


        public void Reactivar(int idUsuario, string rolUsuario)
        {
            Permisos.ExigirAdministrador(rolUsuario);

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            usuarioDatos.Reactivar(idUsuario);
        }



        public void CambiarContrasena(
     int idUsuario,
     string contrasena)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            if (!Validaciones.TextoObligatorio(contrasena))
                throw new ArgumentException(
                    "La contraseña es obligatoria.");

            string contrasenaHash = PasswordService.Hash(contrasena);

            usuarioDatos.CambiarContrasena(
                idUsuario, contrasenaHash);
        }

        private static void ValidarDatos(
            int idRol,
            string nombre,
            string apellido,
            string dni,
            string email,
            string? contrasena)
        {
            if (idRol <= 0)
                throw new ArgumentException("Debe seleccionar un rol válido.");

            if (!Validaciones.TextoObligatorio(nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (!Validaciones.TextoObligatorio(apellido))
                throw new ArgumentException("El apellido es obligatorio.");

            if (!Validaciones.DNIValido(dni))
                throw new ArgumentException(
                    "El DNI debe contener entre 7 y 15 dígitos.");

            if (!Validaciones.EmailValido(email))
                throw new ArgumentException(
                    "Debe ingresar un email válido.");

            if (contrasena != null
                && !Validaciones.TextoObligatorio(contrasena))
            {
                throw new ArgumentException(
                    "La contraseña es obligatoria.");
            }
        }
        public DataRow? Autenticar(string email, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("La contraseña es obligatoria.");

            DataTable usuarios = usuarioDatos.BuscarPorEmail(email.Trim());

            if (usuarios.Rows.Count == 0)
                return null;

            DataRow usuario = usuarios.Rows[0];

            if (!Convert.ToBoolean(usuario["activo"]))
                return null;

            string hash = Convert.ToString(usuario["contrasena"]) ?? string.Empty;

            if (!PasswordService.Verificar(hash, contrasena))
                return null;

            return usuario;
        }
    }
}
