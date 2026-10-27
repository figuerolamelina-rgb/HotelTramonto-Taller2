namespace HotelTramonto.Presentacion
{
    partial class FormReservas
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
            lblTitulo = new Label();
            dgvReservas = new DataGridView();
            btnCancelarReserva = new Button();
            btnActualizar = new Button();
            btnCerrar = new Button();
            cmbEstado = new ComboBox();
            btnFiltrarEstado = new Button();
            dtpFechaDesde = new DateTimePicker();
            dtpFechaHasta = new DateTimePicker();
            btnFiltrarFechas = new Button();
            lblFechaDesde = new Label();
            lblFechaHasta = new Label();
            btnConsultarSaldo = new Button();
            btnCheckIn = new Button();
            btnCheckOut = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvReservas).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(334, 24);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(108, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Gestión de reservas";
            // 
            // dgvReservas
            // 
            dgvReservas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReservas.Location = new Point(42, 183);
            dgvReservas.Name = "dgvReservas";
            dgvReservas.Size = new Size(725, 183);
            dgvReservas.TabIndex = 1;
            // 
            // btnCancelarReserva
            // 
            btnCancelarReserva.Location = new Point(252, 383);
            btnCancelarReserva.Name = "btnCancelarReserva";
            btnCancelarReserva.Size = new Size(124, 23);
            btnCancelarReserva.TabIndex = 2;
            btnCancelarReserva.Text = "Cancelar reserva";
            btnCancelarReserva.UseVisualStyleBackColor = true;
            btnCancelarReserva.Click += btnCancelarReserva_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.Location = new Point(421, 383);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(75, 23);
            btnActualizar.TabIndex = 3;
            btnActualizar.Text = "Actualizar";
            btnActualizar.UseVisualStyleBackColor = true;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Location = new Point(535, 383);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(75, 23);
            btnCerrar.TabIndex = 4;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = true;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // cmbEstado
            // 
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.FormattingEnabled = true;
            cmbEstado.Location = new Point(646, 56);
            cmbEstado.Name = "cmbEstado";
            cmbEstado.Size = new Size(121, 23);
            cmbEstado.TabIndex = 5;
            // 
            // btnFiltrarEstado
            // 
            btnFiltrarEstado.Location = new Point(646, 99);
            btnFiltrarEstado.Name = "btnFiltrarEstado";
            btnFiltrarEstado.Size = new Size(129, 23);
            btnFiltrarEstado.TabIndex = 6;
            btnFiltrarEstado.Text = "Filtrar por estado";
            btnFiltrarEstado.UseVisualStyleBackColor = true;
            btnFiltrarEstado.Click += btnFiltrarEstado_Click;
            // 
            // dtpFechaDesde
            // 
            dtpFechaDesde.Location = new Point(381, 56);
            dtpFechaDesde.Name = "dtpFechaDesde";
            dtpFechaDesde.Size = new Size(229, 23);
            dtpFechaDesde.TabIndex = 7;
            // 
            // dtpFechaHasta
            // 
            dtpFechaHasta.Location = new Point(381, 99);
            dtpFechaHasta.Name = "dtpFechaHasta";
            dtpFechaHasta.Size = new Size(229, 23);
            dtpFechaHasta.TabIndex = 8;
            // 
            // btnFiltrarFechas
            // 
            btnFiltrarFechas.Location = new Point(474, 139);
            btnFiltrarFechas.Name = "btnFiltrarFechas";
            btnFiltrarFechas.Size = new Size(110, 23);
            btnFiltrarFechas.TabIndex = 9;
            btnFiltrarFechas.Text = "Filtrar por fechas";
            btnFiltrarFechas.UseVisualStyleBackColor = true;
            btnFiltrarFechas.Click += btnFiltrarFechas_Click;
            // 
            // lblFechaDesde
            // 
            lblFechaDesde.AutoSize = true;
            lblFechaDesde.Location = new Point(334, 62);
            lblFechaDesde.Name = "lblFechaDesde";
            lblFechaDesde.Size = new Size(42, 15);
            lblFechaDesde.TabIndex = 10;
            lblFechaDesde.Text = "Desde:";
            // 
            // lblFechaHasta
            // 
            lblFechaHasta.AutoSize = true;
            lblFechaHasta.Location = new Point(336, 103);
            lblFechaHasta.Name = "lblFechaHasta";
            lblFechaHasta.Size = new Size(40, 15);
            lblFechaHasta.TabIndex = 11;
            lblFechaHasta.Text = "Hasta:";
            // 
            // btnConsultarSaldo
            // 
            btnConsultarSaldo.Location = new Point(344, 137);
            btnConsultarSaldo.Name = "btnConsultarSaldo";
            btnConsultarSaldo.Size = new Size(124, 25);
            btnConsultarSaldo.TabIndex = 12;
            btnConsultarSaldo.Text = "Consultar saldo";
            btnConsultarSaldo.UseVisualStyleBackColor = true;
            btnConsultarSaldo.Click += btnConsultarSaldo_Click;
            // 
            // btnCheckIn
            // 
            btnCheckIn.Location = new Point(115, 139);
            btnCheckIn.Name = "btnCheckIn";
            btnCheckIn.Size = new Size(151, 23);
            btnCheckIn.TabIndex = 13;
            btnCheckIn.Text = "Registrar Check-in";
            btnCheckIn.UseVisualStyleBackColor = true;
            btnCheckIn.Click += btnCheckIn_Click;
            // 
            // btnCheckOut
            // 
            btnCheckOut.Location = new Point(115, 110);
            btnCheckOut.Name = "btnCheckOut";
            btnCheckOut.Size = new Size(151, 23);
            btnCheckOut.TabIndex = 14;
            btnCheckOut.Text = "Registrar Check-out";
            btnCheckOut.UseVisualStyleBackColor = true;
            btnCheckOut.Click += btnCheckOut_Click;
            // 
            // FormReservas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCheckOut);
            Controls.Add(btnCheckIn);
            Controls.Add(btnConsultarSaldo);
            Controls.Add(lblFechaHasta);
            Controls.Add(lblFechaDesde);
            Controls.Add(btnFiltrarFechas);
            Controls.Add(dtpFechaHasta);
            Controls.Add(dtpFechaDesde);
            Controls.Add(btnFiltrarEstado);
            Controls.Add(cmbEstado);
            Controls.Add(btnCerrar);
            Controls.Add(btnActualizar);
            Controls.Add(btnCancelarReserva);
            Controls.Add(dgvReservas);
            Controls.Add(lblTitulo);
            Name = "FormReservas";
            Text = "FormReservas";
            ((System.ComponentModel.ISupportInitialize)dgvReservas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private DataGridView dgvReservas;
        private Button btnCancelarReserva;
        private Button btnActualizar;
        private Button btnCerrar;
        private ComboBox cmbEstado;
        private Button btnFiltrarEstado;
        private DateTimePicker dtpFechaDesde;
        private DateTimePicker dtpFechaHasta;
        private Button btnFiltrarFechas;
        private Label lblFechaDesde;
        private Label lblFechaHasta;
        private Button btnConsultarSaldo;
        private Button btnCheckIn;
        private Button btnCheckOut;
    }
}