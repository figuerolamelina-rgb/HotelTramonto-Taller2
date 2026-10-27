namespace HotelTramonto.Presentacion
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnProbarConexion = new Button();
            btnListarRoles = new Button();
            lblTitulo = new Label();
            dgvUsuarios = new DataGridView();
            btnNuevoUsuario = new Button();
            btnModificarUsuario = new Button();
            btnDesactivarUsuario = new Button();
            btnReactivarUsuario = new Button();
            btnCerrarSesion = new Button();
            btnHabitaciones = new Button();
            btnReservas = new Button();
            btnHuespedes = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            SuspendLayout();
            // 
            // btnProbarConexion
            // 
            btnProbarConexion.Location = new Point(249, 38);
            btnProbarConexion.Margin = new Padding(3, 2, 3, 2);
            btnProbarConexion.Name = "btnProbarConexion";
            btnProbarConexion.Size = new Size(82, 22);
            btnProbarConexion.TabIndex = 0;
            btnProbarConexion.Text = "Probar conexión";
            btnProbarConexion.UseVisualStyleBackColor = true;
            btnProbarConexion.Click += button1_Click;
            // 
            // btnListarRoles
            // 
            btnListarRoles.Location = new Point(355, 37);
            btnListarRoles.Name = "btnListarRoles";
            btnListarRoles.Size = new Size(75, 23);
            btnListarRoles.TabIndex = 1;
            btnListarRoles.Text = "Listar roles";
            btnListarRoles.UseVisualStyleBackColor = true;
            btnListarRoles.Click += button1_Click_1;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitulo.Location = new Point(158, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(344, 25);
            lblTitulo.TabIndex = 2;
            lblTitulo.Text = "HOTEL TRAMONTO - GESTIÓN";
            lblTitulo.Click += label1_Click;
            // 
            // dgvUsuarios
            // 
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;
            dgvUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsuarios.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsuarios.Location = new Point(23, 136);
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.Size = new Size(740, 284);
            dgvUsuarios.TabIndex = 3;
            // 
            // btnNuevoUsuario
            // 
            btnNuevoUsuario.Location = new Point(32, 79);
            btnNuevoUsuario.Name = "btnNuevoUsuario";
            btnNuevoUsuario.Size = new Size(120, 30);
            btnNuevoUsuario.TabIndex = 4;
            btnNuevoUsuario.Text = "Nuevo usuario";
            btnNuevoUsuario.UseVisualStyleBackColor = true;
            btnNuevoUsuario.Click += btnNuevoUsuario_Click;
            // 
            // btnModificarUsuario
            // 
            btnModificarUsuario.Location = new Point(158, 79);
            btnModificarUsuario.Name = "btnModificarUsuario";
            btnModificarUsuario.Size = new Size(140, 30);
            btnModificarUsuario.TabIndex = 5;
            btnModificarUsuario.Text = "Modificar usuario";
            btnModificarUsuario.UseVisualStyleBackColor = true;
            btnModificarUsuario.Click += btnModificarUsuario_Click;
            // 
            // btnDesactivarUsuario
            // 
            btnDesactivarUsuario.Location = new Point(304, 79);
            btnDesactivarUsuario.Name = "btnDesactivarUsuario";
            btnDesactivarUsuario.Size = new Size(140, 30);
            btnDesactivarUsuario.TabIndex = 6;
            btnDesactivarUsuario.Text = "Desactivar usuario";
            btnDesactivarUsuario.UseVisualStyleBackColor = true;
            btnDesactivarUsuario.Click += btnDesactivarUsuario_Click;
            // 
            // btnReactivarUsuario
            // 
            btnReactivarUsuario.Location = new Point(450, 79);
            btnReactivarUsuario.Name = "btnReactivarUsuario";
            btnReactivarUsuario.Size = new Size(140, 30);
            btnReactivarUsuario.TabIndex = 7;
            btnReactivarUsuario.Text = "Reactivar usuario";
            btnReactivarUsuario.UseVisualStyleBackColor = true;
            btnReactivarUsuario.Click += btnReactivarUsuario_Click;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.Location = new Point(459, 37);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Size = new Size(116, 23);
            btnCerrarSesion.TabIndex = 8;
            btnCerrarSesion.Text = "Cerrar sesión";
            btnCerrarSesion.UseVisualStyleBackColor = true;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // btnHabitaciones
            // 
            btnHabitaciones.Location = new Point(599, 37);
            btnHabitaciones.Name = "btnHabitaciones";
            btnHabitaciones.Size = new Size(98, 23);
            btnHabitaciones.TabIndex = 9;
            btnHabitaciones.Text = "Habitaciones";
            btnHabitaciones.UseVisualStyleBackColor = true;
            btnHabitaciones.Click += btnHabitaciones_Click;
            // 
            // btnReservas
            // 
            btnReservas.Location = new Point(613, 66);
            btnReservas.Name = "btnReservas";
            btnReservas.Size = new Size(75, 23);
            btnReservas.TabIndex = 10;
            btnReservas.Text = "Reservas";
            btnReservas.UseVisualStyleBackColor = true;
            btnReservas.Click += btnReservas_Click;
            // 
            // btnHuespedes
            // 
            btnHuespedes.Location = new Point(613, 95);
            btnHuespedes.Name = "btnHuespedes";
            btnHuespedes.RightToLeft = RightToLeft.No;
            btnHuespedes.Size = new Size(75, 23);
            btnHuespedes.TabIndex = 11;
            btnHuespedes.Text = "Huéspedes";
            btnHuespedes.UseVisualStyleBackColor = true;
            btnHuespedes.Click += btnHuespedes_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(btnHuespedes);
            Controls.Add(btnReservas);
            Controls.Add(btnHabitaciones);
            Controls.Add(btnCerrarSesion);
            Controls.Add(btnDesactivarUsuario);
            Controls.Add(btnModificarUsuario);
            Controls.Add(btnNuevoUsuario);
            Controls.Add(btnReactivarUsuario);
            Controls.Add(dgvUsuarios);
            Controls.Add(lblTitulo);
            Controls.Add(btnListarRoles);
            Controls.Add(btnProbarConexion);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnProbarConexion;
        private Button btnListarRoles;
        private Label lblTitulo;
        private DataGridView dgvUsuarios;
        private Button btnNuevoUsuario;
        private Button btnModificarUsuario;
        private Button btnDesactivarUsuario;
        private Button btnReactivarUsuario;
        private Button btnCerrarSesion;
        private Button btnHabitaciones;
        private Button btnReservas;
        private Button btnHuespedes;
    }
}
