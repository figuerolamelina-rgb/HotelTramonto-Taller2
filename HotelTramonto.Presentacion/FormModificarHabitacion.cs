
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormModificarHabitacion : Form
    {
        private readonly int idHabitacion;
        private readonly string rolUsuario;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private bool EsAdministrador =>
            string.Equals(
                rolUsuario,
                "ADMINISTRADOR",
                StringComparison.OrdinalIgnoreCase);

        // Constructor original para mantener compatibilidad.
        public FormModificarHabitacion(int idHabitacion)
            : this(idHabitacion, string.Empty)
        {
        }

        // Constructor que recibe el rol desde FormHabitaciones.
        public FormModificarHabitacion(
            int idHabitacion,
            string rolUsuario)
        {
            InitializeComponent();

            this.idHabitacion = idHabitacion;
            this.rolUsuario =
                rolUsuario?.Trim().ToUpperInvariant()
                ?? string.Empty;

            AplicarEstilo();

            // Solo el administrador puede guardar modificaciones.
            btnGuardarCambios.Enabled = EsAdministrador;

            ConfigurarCombos();
            CargarDatosHabitacion();
        }

        private void AplicarEstilo()
        {
            Text = "Modificar habitación - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(540, 490);
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            Panel panelTitulo = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 90),
                BackColor = marron
            };

            Label lblEncabezado = new Label
            {
                Text = "MODIFICAR HABITACIÓN",
                Location = new Point(25, 14),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font(
                    "Segoe UI", 19F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Text = "Actualizá los datos de la habitación",
                Location = new Point(28, 55),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 9.5F)
            };

            panelTitulo.Controls.Add(lblEncabezado);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);

            // Ocultar el título original del Designer.
            lblTitulo.Visible = false;

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
                lblTipo,
                lblPiso,
                lblNumero,
                lblEstado
            };

            string[] textos =
            {
                "Tipo de habitación",
                "Piso",
                "Número",
                "Estado"
            };

            for (int i = 0; i < etiquetas.Length; i++)
            {
                etiquetas[i].Text = textos[i];
                etiquetas[i].Location =
                    new Point(25, 30 + i * 58);
                etiquetas[i].AutoSize = true;
                etiquetas[i].ForeColor = marron;
                etiquetas[i].Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold);

                panelDatos.Controls.Add(etiquetas[i]);
            }

            Control[] campos =
            {
                cmbTipoHabitacion,
                txtPiso,
                txtNumero,
                cmbEstado
            };

            for (int i = 0; i < campos.Length; i++)
            {
                campos[i].Location =
                    new Point(190, 25 + i * 58);
                campos[i].Size = new Size(275, 30);
                campos[i].Font = new Font("Segoe UI", 10F);
                campos[i].ForeColor = marron;
                campos[i].BackColor = marfil;

                panelDatos.Controls.Add(campos[i]);
            }

            txtPiso.BorderStyle = BorderStyle.FixedSingle;
            txtNumero.BorderStyle = BorderStyle.FixedSingle;

            cmbTipoHabitacion.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbEstado.DropDownStyle =
                ComboBoxStyle.DropDownList;

            EstilizarBoton(
                btnGuardarCambios,
                "Guardar cambios",
                dorado,
                marron);

            btnGuardarCambios.Location =
                new Point(190, 430);
            btnGuardarCambios.Size =
                new Size(175, 40);

            EstilizarBoton(
                btnCancelar,
                "Cancelar",
                marronClaro,
                Color.White);

            btnCancelar.Location = new Point(380, 430);
            btnCancelar.Size = new Size(120, 40);

            btnCancelar.Click -= btnCancelar_Click;
            btnCancelar.Click += btnCancelar_Click;

            AcceptButton = btnGuardarCambios;
            CancelButton = btnCancelar;
        }

        private void EstilizarBoton(
            Button boton,
            string texto,
            Color fondo,
            Color textoColor)
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

            boton.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);

            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false)
        {
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition =
                    FormStartPosition.CenterParent;
                mensaje.FormBorderStyle =
                    FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(380, 165);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(380, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(350, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font(
                        "Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(340, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(135, 115),
                    Size = new Size(110, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };

                btnAceptar.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }

        private void ConfigurarCombos()
        {
            try
            {
                TipoHabitacionNegocios negocio =
                    new TipoHabitacionNegocios();

                DataTable tipos = negocio.Listar();

                cmbTipoHabitacion.DataSource = tipos;
                cmbTipoHabitacion.DisplayMember = "nombre";
                cmbTipoHabitacion.ValueMember =
                    "id_tipoHabitacion";
                cmbTipoHabitacion.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                cmbEstado.Items.Clear();
                cmbEstado.Items.Add("DISPONIBLE");
                cmbEstado.Items.Add("OCUPADA");
                cmbEstado.Items.Add("MANTENIMIENTO");
                cmbEstado.DropDownStyle =
                    ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar los tipos",
                    "No se pudieron cargar los tipos de habitación.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }

        private void CargarDatosHabitacion()
        {
            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                DataTable resultado =
                    negocio.BuscarPorId(idHabitacion);

                if (resultado.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Habitación no encontrada",
                        "No se encontró la habitación seleccionada.",
                        true);

                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                DataRow habitacion = resultado.Rows[0];

                cmbTipoHabitacion.SelectedValue =
                    Convert.ToInt32(
                        habitacion["tipo_habitacion"]);

                txtPiso.Text =
                    habitacion["piso"] == DBNull.Value
                        ? ""
                        : habitacion["piso"].ToString();

                txtNumero.Text =
                    habitacion["numero"].ToString();

                cmbEstado.SelectedItem =
                    habitacion["estado"].ToString();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar la habitación",
                    "No se pudieron cargar los datos de la habitación.\n\nDetalle: "
                    + ex.Message,
                    true);

                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnGuardarCambios_Click(
            object sender,
            EventArgs e)
        {
            // Validar permiso antes de modificar.
            if (!EsAdministrador)
            {
                MostrarMensaje(
                    "Acceso denegado",
                    "Solo el administrador puede modificar habitaciones.",
                    true);
                return;
            }

            if (cmbTipoHabitacion.SelectedValue == null)
            {
                MostrarMensaje(
                    "Tipo de habitación requerido",
                    "Seleccioná un tipo de habitación para continuar.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MostrarMensaje(
                    "Número requerido",
                    "Ingresá el número de la habitación.");

                txtNumero.Focus();
                return;
            }

            if (cmbEstado.SelectedItem == null)
            {
                MostrarMensaje(
                    "Estado requerido",
                    "Seleccioná un estado para la habitación.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPiso.Text))
            {
                MostrarMensaje(
                    "Piso requerido",
                    "Ingresá el piso de la habitación para continuar.");

                txtPiso.Focus();
                return;
            }

            if (!int.TryParse(
                    txtPiso.Text.Trim(),
                    out int pisoIngresado)
                || pisoIngresado < 0)
            {
                MostrarMensaje(
                    "Piso inválido",
                    "Ingresá un número de piso válido, igual o mayor que cero.");

                txtPiso.Focus();
                return;
            }

            int tipoHabitacion =
                Convert.ToInt32(
                    cmbTipoHabitacion.SelectedValue);

            int? piso = pisoIngresado;

            string numero = txtNumero.Text.Trim();
            string estado =
                cmbEstado.SelectedItem.ToString()!;

            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                negocio.Modificar(
                    idHabitacion,
                    tipoHabitacion,
                    piso,
                    numero,
                    estado,
                    rolUsuario);

                MostrarMensaje(
                    "¡Cambios guardados!",
                    "La habitación se modificó correctamente.");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al modificar",
                    "No se pudo modificar la habitación.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }

        private void btnCancelar_Click(
            object sender,
            EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
