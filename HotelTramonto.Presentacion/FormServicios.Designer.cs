
namespace HotelTramonto.Presentacion
{
    partial class FormServicios
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblNombre = new Label();
            lblDescripcion = new Label();
            lblCategoria = new Label();
            lblPrecio = new Label();

            txtNombre = new TextBox();
            txtDescripcion = new TextBox();

            cmbCategoria = new ComboBox();
            cmbFiltroCategoria = new ComboBox();

            nudPrecio = new NumericUpDown();

            dgvServicios = new DataGridView();

            btnNuevo = new Button();
            btnGuardar = new Button();
            btnModificar = new Button();
            btnDesactivar = new Button();
            btnLimpiar = new Button();
            btnVerInactivos = new Button();
            lblFiltro = new Label();

            ((System.ComponentModel.ISupportInitialize)nudPrecio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvServicios).BeginInit();

            SuspendLayout();

            // lblNombre
            lblNombre.AutoSize = true;
            lblNombre.Location = new Point(35, 85);
            lblNombre.Name = "lblNombre";
            lblNombre.Text = "Nombre del servicio";

            // txtNombre
            txtNombre.Location = new Point(35, 110);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(260, 27);
            txtNombre.MaxLength = 50;

            // lblCategoria
            lblCategoria.AutoSize = true;
            lblCategoria.Location = new Point(320, 85);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.Text = "Categoría";

            // cmbCategoria
            cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoria.Location = new Point(320, 110);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(210, 28);

            // lblPrecio
            lblPrecio.AutoSize = true;
            lblPrecio.Location = new Point(555, 85);
            lblPrecio.Name = "lblPrecio";
            lblPrecio.Text = "Precio ($)";

            // nudPrecio
            nudPrecio.DecimalPlaces = 2;
            nudPrecio.Maximum = 99999999;
            nudPrecio.Minimum = 0;
            nudPrecio.ThousandsSeparator = true;
            nudPrecio.Location = new Point(555, 110);
            nudPrecio.Name = "nudPrecio";
            nudPrecio.Size = new Size(170, 27);

            // lblDescripcion
            lblDescripcion.AutoSize = true;
            lblDescripcion.Location = new Point(755, 85);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Text = "Descripción";

            // txtDescripcion
            txtDescripcion.Location = new Point(755, 110);
            txtDescripcion.Name = "txtDescripcion";
            txtDescripcion.Size = new Size(300, 27);
            txtDescripcion.MaxLength = 150;

            // Botón Nuevo
            btnNuevo.Location = new Point(35, 165);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(145, 40);
            btnNuevo.Text = "＋ Nuevo";
            btnNuevo.Click += btnNuevo_Click;

            // Botón Guardar
            btnGuardar.Location = new Point(195, 165);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(145, 40);
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;

            // Botón Modificar
            btnModificar.Location = new Point(355, 165);
            btnModificar.Name = "btnModificar";
            btnModificar.Size = new Size(145, 40);
            btnModificar.Text = "Modificar";
            btnModificar.Click += btnModificar_Click;

            // Botón Desactivar
            btnDesactivar.Location = new Point(515, 165);
            btnDesactivar.Name = "btnDesactivar";
            btnDesactivar.Size = new Size(145, 40);
            btnDesactivar.Text = "Desactivar";
            btnDesactivar.Click += btnDesactivar_Click;

            // Botón Limpiar
            btnLimpiar.Location = new Point(675, 165);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(145, 40);
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.Click += btnLimpiar_Click;

            // Botón Ver inactivos
            btnVerInactivos.Location = new Point(835, 165);
            btnVerInactivos.Name = "btnVerInactivos";
            btnVerInactivos.Size = new Size(145, 40);
            btnVerInactivos.TabIndex = 11;
            btnVerInactivos.Text = "Ver inactivos";
            btnVerInactivos.UseVisualStyleBackColor = true;
            btnVerInactivos.Click += btnVerInactivos_Click;
            
            // Filtro
            lblFiltro.AutoSize = true;
            lblFiltro.Location = new Point(35, 235);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Text = "Filtrar por categoría:";

            cmbFiltroCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroCategoria.Location = new Point(180, 230);
            cmbFiltroCategoria.Name = "cmbFiltroCategoria";
            cmbFiltroCategoria.Size = new Size(220, 28);
            cmbFiltroCategoria.SelectedIndexChanged += cmbFiltroCategoria_SelectedIndexChanged;

            // Tabla
            dgvServicios.AllowUserToAddRows = false;
            dgvServicios.AllowUserToDeleteRows = false;
            dgvServicios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvServicios.ColumnHeadersHeight = 42;
            dgvServicios.Location = new Point(35, 280);
            dgvServicios.Name = "dgvServicios";
            dgvServicios.ReadOnly = true;
            dgvServicios.RowHeadersVisible = false;
            dgvServicios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvServicios.MultiSelect = false;
            dgvServicios.Size = new Size(1020, 350);
            dgvServicios.TabIndex = 10;
            dgvServicios.SelectionChanged += dgvServicios_SelectionChanged;

            // FormServicios
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 670);
            MinimumSize = new Size(1000, 650);
            Name = "FormServicios";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hotel Tramonto | Gestión de servicios";

            Controls.Add(lblNombre);
            Controls.Add(txtNombre);
            Controls.Add(lblCategoria);
            Controls.Add(cmbCategoria);
            Controls.Add(lblPrecio);
            Controls.Add(nudPrecio);
            Controls.Add(lblDescripcion);
            Controls.Add(txtDescripcion);

            Controls.Add(btnNuevo);
            Controls.Add(btnGuardar);
            Controls.Add(btnModificar);
            Controls.Add(btnDesactivar);
            Controls.Add(btnLimpiar);
            Controls.Add(btnVerInactivos);

            Controls.Add(lblFiltro);
            Controls.Add(cmbFiltroCategoria);
            Controls.Add(dgvServicios);

            ((System.ComponentModel.ISupportInitialize)nudPrecio).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvServicios).EndInit();

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNombre;
        private Label lblDescripcion;
        private Label lblCategoria;
        private Label lblPrecio;
        private Label lblFiltro;

        private TextBox txtNombre;
        private TextBox txtDescripcion;

        private ComboBox cmbCategoria;
        private ComboBox cmbFiltroCategoria;

        private NumericUpDown nudPrecio;

        private DataGridView dgvServicios;

        private Button btnNuevo;
        private Button btnGuardar;
        private Button btnModificar;
        private Button btnDesactivar;
        private Button btnLimpiar;
        private Button btnVerInactivos;
    }
}
