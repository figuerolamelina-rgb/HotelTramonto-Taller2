namespace HotelTramonto.Presentacion
{
    partial class FormLogin
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
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblContrasena = new Label();
            txtContrasena = new TextBox();
            btnIngresar = new Button();
            btnSalir = new Button();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(309, 78);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(112, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "HOTEL TRAMONTO";
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(270, 113);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(108, 15);
            lblEmail.TabIndex = 1;
            lblEmail.Text = "Correo electrónico:";
            lblEmail.Click += label2_Click;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(270, 131);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(214, 23);
            txtEmail.TabIndex = 2;
            // 
            // lblContrasena
            // 
            lblContrasena.AutoSize = true;
            lblContrasena.Location = new Point(270, 168);
            lblContrasena.Name = "lblContrasena";
            lblContrasena.Size = new Size(70, 15);
            lblContrasena.TabIndex = 3;
            lblContrasena.Text = "Contraseña:";
            // 
            // txtContrasena
            // 
            txtContrasena.Location = new Point(270, 195);
            txtContrasena.Name = "txtContrasena";
            txtContrasena.Size = new Size(214, 23);
            txtContrasena.TabIndex = 4;
            txtContrasena.UseSystemPasswordChar = true;
            // 
            // btnIngresar
            // 
            btnIngresar.Location = new Point(311, 239);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(110, 27);
            btnIngresar.TabIndex = 5;
            btnIngresar.Text = "Ingresar";
            btnIngresar.UseVisualStyleBackColor = true;
            btnIngresar.Click += btnIngresar_Click;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(329, 288);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(75, 23);
            btnSalir.TabIndex = 6;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSalir);
            Controls.Add(btnIngresar);
            Controls.Add(txtContrasena);
            Controls.Add(lblContrasena);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(lblTitulo);
            Name = "FormLogin";
            Text = "FormLogin";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblContrasena;
        private TextBox txtContrasena;
        private Button btnIngresar;
        private Button btnSalir;
    }
}