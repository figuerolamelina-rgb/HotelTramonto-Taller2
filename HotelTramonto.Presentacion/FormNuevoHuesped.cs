
using System;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormNuevoHuesped : Form
    {
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormNuevoHuesped()
        {
            InitializeComponent();
            AplicarEstilo();
        }

        private void AplicarEstilo()
        {
            Text = "Nuevo huésped - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(540, 490);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            // Encabezado
            Panel panelTitulo = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 90),
                BackColor = marron
            };

            Label lblTitulo = new Label
            {
                Text = "REGISTRAR HUÉSPED",
                Location = new Point(25, 14),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Text = "Ingresá los datos personales del huésped",
                Location = new Point(28, 55),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 9.5F)
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);

            // Panel de datos
            Panel panelDatos = new Panel
            {
                Location = new Point(20, 105),
                Size = new Size(500, 305),
                BackColor = marfil,
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(panelDatos);

            Label[] etiquetas =
            {
                lblNombre, lblApellido, lblDNI,
                lblTelefono, lblEmail, lblNacionalidad
            };

            TextBox[] campos =
            {
                txtNombre, txtApellido, txtDNI,
                txtTelefono, txtEmail, txtNacionalidad
            };

            string[] nombres =
            {
                "Nombre", "Apellido", "DNI",
                "Teléfono", "Email", "Nacionalidad"
            };

            for (int i = 0; i < etiquetas.Length; i++)
            {
                etiquetas[i].Text = nombres[i];
                etiquetas[i].Location = new Point(25, 20 + i * 43);
                etiquetas[i].AutoSize = true;
                etiquetas[i].ForeColor = marron;
                etiquetas[i].Font =
                    new Font("Segoe UI", 10F, FontStyle.Bold);

                campos[i].Location = new Point(155, 16 + i * 43);
                campos[i].Size = new Size(315, 28);
                campos[i].BackColor = marfil;
                campos[i].ForeColor = marron;
                campos[i].BorderStyle = BorderStyle.FixedSingle;
                campos[i].Font = new Font("Segoe UI", 10F);

                panelDatos.Controls.Add(etiquetas[i]);
                panelDatos.Controls.Add(campos[i]);
            }

            // Botón guardar
            EstilizarBoton(btnGuardar, "Guardar huésped", dorado, marron);
            btnGuardar.Location = new Point(155, 430);
            btnGuardar.Size = new Size(175, 40);

            // Botón cancelar
            EstilizarBoton(btnCancelar, "Cancelar", marronClaro, Color.White);
            btnCancelar.Location = new Point(345, 430);
            btnCancelar.Size = new Size(120, 40);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void EstilizarBoton(
            Button boton, string texto, Color fondo, Color textoColor)
        {
            boton.Text = texto;
            boton.BackColor = fondo;
            boton.ForeColor = textoColor;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);
            boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                HuespedNegocios negocio = new HuespedNegocios();

                negocio.Insertar(
                    txtNombre.Text,
                    txtApellido.Text,
                    txtDNI.Text,
                    txtTelefono.Text,
                    txtEmail.Text,
                    txtNacionalidad.Text
                );

                MessageBox.Show(
                    "Huésped registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo registrar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
