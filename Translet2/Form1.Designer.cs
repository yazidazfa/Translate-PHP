
namespace Translet2
{
    partial class Form1
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
            this.btn_browse = new System.Windows.Forms.Button();
            this.btn_translate = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.btn_export = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.btn_copy1 = new System.Windows.Forms.Button();
            this.btn_copy2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_help = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(20, 60);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(125, 35);
            this.btn_browse.TabIndex = 0;
            this.btn_browse.Text = "Select File";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // btn_translate
            // 
            this.btn_translate.Location = new System.Drawing.Point(20, 101);
            this.btn_translate.Name = "btn_translate";
            this.btn_translate.Size = new System.Drawing.Size(125, 35);
            this.btn_translate.TabIndex = 1;
            this.btn_translate.Text = "Translate";
            this.btn_translate.UseVisualStyleBackColor = true;
            this.btn_translate.Click += new System.EventHandler(this.btn_translate_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Location = new System.Drawing.Point(20, 533);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(125, 35);
            this.btn_clear.TabIndex = 2;
            this.btn_clear.Text = "Reset";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // btn_export
            // 
            this.btn_export.Enabled = false;
            this.btn_export.Location = new System.Drawing.Point(20, 574);
            this.btn_export.Name = "btn_export";
            this.btn_export.Size = new System.Drawing.Size(125, 35);
            this.btn_export.TabIndex = 4;
            this.btn_export.Text = "Save";
            this.btn_export.UseVisualStyleBackColor = true;
            this.btn_export.Click += new System.EventHandler(this.btn_export_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(160, 60);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.richTextBox1.Size = new System.Drawing.Size(487, 549);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // richTextBox2
            // 
            this.richTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox2.Location = new System.Drawing.Point(655, 60);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.Size = new System.Drawing.Size(490, 549);
            this.richTextBox2.TabIndex = 7;
            this.richTextBox2.Text = "";
            // 
            // btn_copy1
            // 
            this.btn_copy1.Enabled = false;
            this.btn_copy1.Location = new System.Drawing.Point(160, 615);
            this.btn_copy1.Name = "btn_copy1";
            this.btn_copy1.Size = new System.Drawing.Size(150, 35);
            this.btn_copy1.TabIndex = 8;
            this.btn_copy1.Text = "Copy JSON";
            this.btn_copy1.UseVisualStyleBackColor = true;
            this.btn_copy1.Click += new System.EventHandler(this.btn_copy1_Click);
            // 
            // btn_copy2
            // 
            this.btn_copy2.Enabled = false;
            this.btn_copy2.Location = new System.Drawing.Point(655, 615);
            this.btn_copy2.Name = "btn_copy2";
            this.btn_copy2.Size = new System.Drawing.Size(150, 35);
            this.btn_copy2.TabIndex = 8;
            this.btn_copy2.Text = "Copy PHP";
            this.btn_copy2.UseVisualStyleBackColor = true;
            this.btn_copy2.Click += new System.EventHandler(this.btn_copy2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(475, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(306, 29);
            this.label1.TabIndex = 9;
            this.label1.Text = "xtUML JSON model to PHP";
            // 
            // btn_help
            // 
            this.btn_help.Location = new System.Drawing.Point(1045, 15);
            this.btn_help.Name = "btn_help";
            this.btn_help.Size = new System.Drawing.Size(100, 33);
            this.btn_help.TabIndex = 10;
            this.btn_help.Text = "Help";
            this.btn_help.UseVisualStyleBackColor = true;
            this.btn_help.Click += new System.EventHandler(this.btn_help_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(667, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(209, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Translated PHP appears here...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 681);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_help);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_copy2);
            this.Controls.Add(this.btn_copy1);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_export);
            this.Controls.Add(this.btn_clear);
            this.Controls.Add(this.btn_translate);
            this.Controls.Add(this.btn_browse);
            this.Name = "Form1";
            this.Text = "Translate to PHP";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Button btn_translate;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button btn_export;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button btn_copy1;
        private System.Windows.Forms.Button btn_copy2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.Label label2;
    }
}

