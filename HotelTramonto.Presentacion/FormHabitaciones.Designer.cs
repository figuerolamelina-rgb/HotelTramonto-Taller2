namespace HotelTramonto.Presentacion
{
    partial class FormHabitaciones
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvHabitaciones = new DataGridView();
            btnNuevaHabitacion = new Button();
            btnModificarHabitacion = new Button();
            btnDesactivarHabitacion = new Button();
            btnReactivarHabitacion = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHabitaciones).BeginInit();
            SuspendLayout();
            // 
            // dgvHabitaciones
            // 
            dgvHabitaciones.AllowUserToAddRows = false;
            dgvHabitaciones.AllowUserToDeleteRows = false;
            dgvHabitaciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHabitaciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHabitaciones.Location = new Point(0, 143);
            dgvHabitaciones.Name = "dgvHabitaciones";
            dgvHabitaciones.ReadOnly = true;
            dgvHabitaciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHabitaciones.Size = new Size(800, 309);
            dgvHabitaciones.TabIndex = 0;
            dgvHabitaciones.SelectionChanged += dgvHabitaciones_SelectionChanged;
            // 
            // btnNuevaHabitacion
            // 
            btnNuevaHabitacion.Location = new Point(53, 23);
            btnNuevaHabitacion.Name = "btnNuevaHabitacion";
            btnNuevaHabitacion.Size = new Size(127, 33);
            btnNuevaHabitacion.TabIndex = 1;
            btnNuevaHabitacion.Text = "Nueva habitación";
            btnNuevaHabitacion.UseVisualStyleBackColor = true;
            btnNuevaHabitacion.Click += btnNuevaHabitacion_Click;
            // 
            // btnModificarHabitacion
            // 
            btnModificarHabitacion.Location = new Point(53, 62);
            btnModificarHabitacion.Name = "btnModificarHabitacion";
            btnModificarHabitacion.Size = new Size(139, 31);
            btnModificarHabitacion.TabIndex = 2;
            btnModificarHabitacion.Text = "Modificar habitación";
            btnModificarHabitacion.UseVisualStyleBackColor = true;
            btnModificarHabitacion.Click += btnModificarHabitacion_Click;
            // 
            // btnDesactivarHabitacion
            // 
            btnDesactivarHabitacion.Location = new Point(53, 99);
            btnDesactivarHabitacion.Name = "btnDesactivarHabitacion";
            btnDesactivarHabitacion.Size = new Size(130, 29);
            btnDesactivarHabitacion.TabIndex = 3;
            btnDesactivarHabitacion.Text = "Desactivar habitación";
            btnDesactivarHabitacion.UseVisualStyleBackColor = true;
            btnDesactivarHabitacion.Click += btnDesactivarHabitacion_Click;
            // 
            // btnReactivarHabitacion
            // 
            btnReactivarHabitacion.Location = new Point(230, 28);
            btnReactivarHabitacion.Name = "btnReactivarHabitacion";
            btnReactivarHabitacion.Size = new Size(143, 28);
            btnReactivarHabitacion.TabIndex = 4;
            btnReactivarHabitacion.Text = "Reactivar habitación";
            btnReactivarHabitacion.UseVisualStyleBackColor = true;
            btnReactivarHabitacion.Click += btnReactivarHabitacion_Click;
            // 
            // FormHabitaciones
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnReactivarHabitacion);
            Controls.Add(btnDesactivarHabitacion);
            Controls.Add(btnModificarHabitacion);
            Controls.Add(btnNuevaHabitacion);
            Controls.Add(dgvHabitaciones);
            Name = "FormHabitaciones";
            Text = "FormHabitaciones";
            ((System.ComponentModel.ISupportInitialize)dgvHabitaciones).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvHabitaciones;
        private Button btnNuevaHabitacion;
        private Button btnModificarHabitacion;
        private Button btnDesactivarHabitacion;
        private Button btnReactivarHabitacion;
    }
}