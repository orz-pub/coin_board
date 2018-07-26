namespace Coin
{
    partial class FormMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
			this.listView = new System.Windows.Forms.ListView();
			this.columnHeaderKind = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderPrice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.remainSeconds = new System.Windows.Forms.Label();
			this.time = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.BackColor = System.Drawing.SystemColors.Control;
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderKind,
            this.columnHeaderPrice});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Enabled = false;
			this.listView.FullRowSelect = true;
			this.listView.GridLines = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(180, 205);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderKind
			// 
			this.columnHeaderKind.Text = "Currency";
			this.columnHeaderKind.Width = 65;
			// 
			// columnHeaderPrice
			// 
			this.columnHeaderPrice.Text = "Price(KRP)";
			this.columnHeaderPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeaderPrice.Width = 110;
			// 
			// remainSeconds
			// 
			this.remainSeconds.AutoSize = true;
			this.remainSeconds.Location = new System.Drawing.Point(70, 8);
			this.remainSeconds.Name = "remainSeconds";
			this.remainSeconds.Size = new System.Drawing.Size(12, 12);
			this.remainSeconds.TabIndex = 1;
			this.remainSeconds.Text = "s";
			// 
			// time
			// 
			this.time.AutoSize = true;
			this.time.Location = new System.Drawing.Point(10, 8);
			this.time.Name = "time";
			this.time.Size = new System.Drawing.Size(0, 12);
			this.time.TabIndex = 2;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(180, 205);
			this.Controls.Add(this.time);
			this.Controls.Add(this.remainSeconds);
			this.Controls.Add(this.listView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "FormMain";
			this.Text = "Board - Coinone";
			this.TopMost = true;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseUp);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderKind;
        private System.Windows.Forms.ColumnHeader columnHeaderPrice;
		private System.Windows.Forms.Label remainSeconds;
		private System.Windows.Forms.Label time;
	}
}

