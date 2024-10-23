namespace antena_tester
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            button_connect = new Button();
            button_stop = new Button();
            button_start = new Button();
            textBox_log = new TextBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            button_fopen = new Button();
            textBox_path = new TextBox();
            zedGraph1 = new ZedGraph.ZedGraphControl();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 2);
            tableLayoutPanel1.Controls.Add(textBox_log, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 0);
            tableLayoutPanel1.Controls.Add(zedGraph1, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100.000008F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            tableLayoutPanel1.Size = new Size(743, 450);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 5;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(button_connect, 0, 0);
            tableLayoutPanel2.Controls.Add(button_stop, 3, 0);
            tableLayoutPanel2.Controls.Add(button_start, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 183);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tableLayoutPanel2.Size = new Size(737, 64);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // button_connect
            // 
            button_connect.Dock = DockStyle.Fill;
            button_connect.Location = new Point(3, 3);
            button_connect.Name = "button_connect";
            button_connect.Size = new Size(64, 64);
            button_connect.TabIndex = 4;
            button_connect.Text = "Connect";
            button_connect.UseVisualStyleBackColor = true;
            button_connect.Click += button_connect_Click;
            // 
            // button_stop
            // 
            button_stop.Dock = DockStyle.Fill;
            button_stop.Location = new Point(143, 3);
            button_stop.Name = "button_stop";
            button_stop.Size = new Size(64, 64);
            button_stop.TabIndex = 2;
            button_stop.Text = "Stop";
            button_stop.UseVisualStyleBackColor = true;
            button_stop.Click += button_stop_Click;
            // 
            // button_start
            // 
            button_start.Dock = DockStyle.Fill;
            button_start.Location = new Point(73, 3);
            button_start.Name = "button_start";
            button_start.Size = new Size(64, 64);
            button_start.TabIndex = 0;
            button_start.Text = "Start";
            button_start.UseVisualStyleBackColor = true;
            button_start.Click += button_start_Click;
            // 
            // textBox_log
            // 
            textBox_log.Dock = DockStyle.Fill;
            textBox_log.Location = new Point(3, 53);
            textBox_log.Multiline = true;
            textBox_log.Name = "textBox_log";
            textBox_log.ReadOnly = true;
            textBox_log.ScrollBars = ScrollBars.Vertical;
            textBox_log.Size = new Size(737, 124);
            textBox_log.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel3.Controls.Add(button_fopen, 0, 0);
            tableLayoutPanel3.Controls.Add(textBox_path, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(737, 44);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // button_fopen
            // 
            button_fopen.Dock = DockStyle.Fill;
            button_fopen.Location = new Point(670, 3);
            button_fopen.Name = "button_fopen";
            button_fopen.Size = new Size(64, 38);
            button_fopen.TabIndex = 3;
            button_fopen.Text = "Open";
            button_fopen.UseVisualStyleBackColor = true;
            button_fopen.Click += button1_fopen_Click;
            // 
            // textBox_path
            // 
            textBox_path.Dock = DockStyle.Fill;
            textBox_path.Location = new Point(3, 3);
            textBox_path.Multiline = true;
            textBox_path.Name = "textBox_path";
            textBox_path.Size = new Size(661, 38);
            textBox_path.TabIndex = 2;
            // 
            // zedGraph1
            // 
            zedGraph1.Dock = DockStyle.Fill;
            zedGraph1.Location = new Point(4, 253);
            zedGraph1.Margin = new Padding(4, 3, 4, 3);
            zedGraph1.Name = "zedGraph1";
            zedGraph1.ScrollGrace = 0D;
            zedGraph1.ScrollMaxX = 0D;
            zedGraph1.ScrollMaxY = 0D;
            zedGraph1.ScrollMaxY2 = 0D;
            zedGraph1.ScrollMinX = 0D;
            zedGraph1.ScrollMinY = 0D;
            zedGraph1.ScrollMinY2 = 0D;
            zedGraph1.Size = new Size(735, 194);
            zedGraph1.TabIndex = 3;
            zedGraph1.UseExtendedPrintDialog = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(743, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Form1";
            Text = "antena tester betta";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_start;
        private Button button_stop;
        private TextBox textBox_log;
        private TableLayoutPanel tableLayoutPanel3;
        private Button button_fopen;
        private TextBox textBox_path;
        private ZedGraph.ZedGraphControl zedGraph1;
        private Button button_connect;
    }
}