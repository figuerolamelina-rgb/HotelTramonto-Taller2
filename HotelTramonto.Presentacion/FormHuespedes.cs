using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormHuespedes : Form
    {
        public FormHuespedes()
        {
            InitializeComponent();
            AplicarEstilo();
            CargarHuespedes();
        }


        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private void AplicarEstilo()
        {
            Text = "Gestión de huéspedes - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);

            // Título
            Label lblTitulo = new Label
            {
                Name = "lblTituloHuespedes",
                Text = "GESTIÓN DE HUÉSPEDES",
                Location = new Point(25, 18),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Name = "lblSubtituloHuespedes",
                Text = "Administración de huéspedes registrados",
                Location = new Point(28, 60),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 10F)
            };

            Panel panelTitulo = new Panel
            {
                Name = "panelTituloHuespedes",
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = marron
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);
            panelTitulo.BringToFront();

            // Botones: primera fila
            EstilizarBoton(btnNuevoHuesped, "＋ Nuevo huésped", dorado, marron);
            EstilizarBoton(btnModificarHuesped, "Modificar huésped", marronClaro, Color.White);
            EstilizarBoton(btnDesactivarHuesped, "Desactivar huésped", marronClaro, Color.White);

            // Segunda fila
            EstilizarBoton(btnBuscarDNI, "Buscar por DNI", marronClaro, Color.White);
            EstilizarBoton(btnActualizar, "Actualizar listado", marronClaro, Color.White);
            EstilizarBoton(btnReactivarHuesped, "Reactivar huésped", marronClaro, Color.White);

            // Tabla
            dgvHuespedes.BackgroundColor = marfil;
            dgvHuespedes.BorderStyle = BorderStyle.None;
            dgvHuespedes.GridColor = Color.FromArgb(222, 211, 194);
            dgvHuespedes.EnableHeadersVisualStyles = false;
            dgvHuespedes.ColumnHeadersHeight = 42;
            dgvHuespedes.RowHeadersVisible = false;
            dgvHuespedes.Font = new Font("Segoe UI", 10F);
            dgvHuespedes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHuespedes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHuespedes.MultiSelect = false;
            dgvHuespedes.ReadOnly = true;
            dgvHuespedes.AllowUserToAddRows = false;
            dgvHuespedes.AllowUserToDeleteRows = false;

            dgvHuespedes.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHuespedes.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            dgvHuespedes.DefaultCellStyle.BackColor = marfil;
            dgvHuespedes.DefaultCellStyle.ForeColor = marron;
            dgvHuespedes.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 211, 184);
            dgvHuespedes.DefaultCellStyle.SelectionForeColor = marron;
            dgvHuespedes.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            dgvHuespedes.AlternatingRowsDefaultCellStyle.BackColor = crema;

            // Reacomodar automáticamente al cambiar el tamaño
            Resize += (s, e) => AcomodarControles();
            AcomodarControles();
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
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        private void AcomodarControles()
        {
            int margen = 25;
            int anchoDisponible = ClientSize.Width - margen * 2;
            int anchoBoton = Math.Max(120, (anchoDisponible - 24) / 3);
            int altoBoton = 38;
            int separacion = 12;
            int inicioY = 120;

            // Primera fila
            btnNuevoHuesped.SetBounds(
                margen, inicioY, anchoBoton, altoBoton);

            btnModificarHuesped.SetBounds(
                margen + anchoBoton + separacion,
                inicioY, anchoBoton, altoBoton);

            btnDesactivarHuesped.SetBounds(
                margen + (anchoBoton + separacion) * 2,
                inicioY, anchoBoton, altoBoton);

            // Segunda fila
            int segundaFilaY = inicioY + altoBoton + 10;

            btnBuscarDNI.SetBounds(
                margen, segundaFilaY, anchoBoton, altoBoton);

            btnActualizar.SetBounds(
                margen + anchoBoton + separacion,
                segundaFilaY, anchoBoton, altoBoton);

            btnReactivarHuesped.SetBounds(
                margen + (anchoBoton + separacion) * 2,
                segundaFilaY, anchoBoton, altoBoton);

            // Tabla debajo de los botones
            int tablaY = segundaFilaY + altoBoton + 18;
            dgvHuespedes.SetBounds(
                margen,
                tablaY,
                anchoDisponible,
                Math.Max(150, ClientSize.Height - tablaY - 25));

            dgvHuespedes.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
        }



        private void CargarHuespedes()
        {
            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                dgvHuespedes.DataSource = negocio.Listar();

                dgvHuespedes.ReadOnly = true;
                dgvHuespedes.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvHuespedes.MultiSelect = false;
                dgvHuespedes.AllowUserToAddRows = false;
                dgvHuespedes.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                // Ocultar el identificador interno
                if (dgvHuespedes.Columns.Contains("id_huesped"))
                {
                    dgvHuespedes.Columns["id_huesped"].Visible = false;
                }

                // Mostrar el estado con texto, sin alterar el valor booleano
                if (dgvHuespedes.Columns.Contains("activo"))
                {
                    dgvHuespedes.Columns["activo"].HeaderText = "Estado";

                    string nombreColumnaEstado = "estadoVisual";

                    if (!dgvHuespedes.Columns.Contains(nombreColumnaEstado))
                    {
                        DataGridViewTextBoxColumn columnaEstado =
                            new DataGridViewTextBoxColumn
                            {
                                Name = nombreColumnaEstado,
                                HeaderText = "Estado",
                                ReadOnly = true,
                                SortMode = DataGridViewColumnSortMode.NotSortable
                            };

                        dgvHuespedes.Columns.Add(columnaEstado);
                    }

                    foreach (DataGridViewRow fila in dgvHuespedes.Rows)
                    {
                        object valor = fila.Cells["activo"].Value;

                        if (valor != null && valor != DBNull.Value)
                        {
                            bool activo = Convert.ToBoolean(valor);

                            fila.Cells[nombreColumnaEstado].Value =
                                activo ? "Activo" : "Inactivo";
                        }
                        else
                        {
                            fila.Cells[nombreColumnaEstado].Value = "Sin datos";
                        }
                    }

                    // Ocultar la columna técnica original.
                    // La lógica de los botones sigue usando el valor booleano.
                    dgvHuespedes.Columns["activo"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los huéspedes: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarHuespedes();
        }


        private void btnNuevoHuesped_Click(object sender, EventArgs e)
        {
            using (FormNuevoHuesped formulario = new FormNuevoHuesped())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHuespedes();
                }
            }
        }


        private void btnModificarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para modificar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                using (FormModificarHuesped formulario =
                    new FormModificarHuesped(idHuesped))
                {
                    if (formulario.ShowDialog() == DialogResult.OK)
                    {
                        CargarHuespedes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al modificar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnDesactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para desactivar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (!activo)
            {
                MessageBox.Show(
                    "El huésped ya está desactivado.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de que desea desactivar al huésped seleccionado?",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Desactivar(idHuesped);

                MessageBox.Show(
                    "Huésped desactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo desactivar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnReactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para reactivar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (activo)
            {
                MessageBox.Show(
                    "El huésped seleccionado ya está activo.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Desea reactivar al huésped seleccionado?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Reactivar(idHuesped);

                MessageBox.Show(
                    "Huésped reactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnBuscarDNI_Click(object sender, EventArgs e)
        {
            string dni = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el DNI del huésped:",
                "Buscar huésped por DNI");

            if (string.IsNullOrWhiteSpace(dni))
                return;

            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                DataTable resultado = negocio.BuscarPorDNI(dni.Trim());

                dgvHuespedes.DataSource = resultado;

                if (resultado.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontró ningún huésped con ese DNI.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al buscar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnBuscarDNI_Click_1(object sender, EventArgs e)
        {
            string dni = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el DNI del huésped:",
                "Buscar huésped por DNI");

            if (string.IsNullOrWhiteSpace(dni))
                return;

            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                DataTable resultado = negocio.BuscarPorDNI(dni.Trim());

                dgvHuespedes.DataSource = resultado;

                if (resultado.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontró ningún huésped con ese DNI.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al buscar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
