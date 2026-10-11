using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class UsuarioDatos
    {
        public void Insertar(
            int idRol,
            string nombre,
            string apellido,
            string dni,
            string? email,
            string contrasena,
            string? telefono)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_rol", idRol);
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@dni", dni);
            comando.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);
            comando.Parameters.AddWithValue("@contrasena", contrasena);
            comando.Parameters.AddWithValue("@telefono",
                string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Modificar(
            int idUsuario,
            int idRol,
            string nombre,
            string apellido,
            string dni,
            string? email,
            string? telefono)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_usuario", idUsuario);
            comando.Parameters.AddWithValue("@id_rol", idRol);
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@dni", dni);
            comando.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);
            comando.Parameters.AddWithValue("@telefono",
                string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Desactivar(int idUsuario)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_Desactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_usuario", idUsuario);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable BuscarPorDNI(string dni)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_BuscarPorDNI", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@dni", dni);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public void CambiarContrasena(int idUsuario, string contrasena)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Usuario_CambiarContrasena", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_usuario", idUsuario);
            comando.Parameters.AddWithValue("@contrasena", contrasena);

            conexion.Open();
            comando.ExecuteNonQuery();
        }
    }
}