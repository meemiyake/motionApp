using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MotionApp
{
    public partial class Form1 : Form
    {
        // Azure SQL Server接続文字列（テスト用）
        // 実際の値に置き換えてください
        private readonly string connectionString = "Server=tcp:fasol-zaiseki.database.windows.net,1433;Initial Catalog=motion;Persist Security Info=False;User ID=fasol;Password=Zaiseki@pp;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private Label labelLoading;

        public Form1()
        {
            InitializeComponent();
            InitializeLoadingLabel();
            InitializeDataGridViewEvents();
        }

        private void InitializeLoadingLabel()
        {
            // ロード中表示用のLabelを作成
            labelLoading = new Label();
            labelLoading.Text = "ロード中です...";
            labelLoading.Font = new Font("MS UI Gothic", 14, FontStyle.Bold);
            labelLoading.ForeColor = Color.Blue;
            labelLoading.AutoSize = true;
            labelLoading.BackColor = Color.White;
            labelLoading.Visible = false;
            
            // フォームに追加
            this.Controls.Add(labelLoading);
            labelLoading.BringToFront();
            
            // 位置を中央に設定（フォームのサイズが変わったときも対応）
            this.Resize += (s, e) => CenterLoadingLabel();
            CenterLoadingLabel();
        }

        private void InitializeDataGridViewEvents()
        {
            // 行の色を変更するためのイベントを追加
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].IsNewRow)
                return;

            // zaiseki_status列のインデックスを取得
            if (dataGridView1.Columns.Contains("zaiseki_status"))
            {
                var zaisekiStatusCell = dataGridView1.Rows[e.RowIndex].Cells["zaiseki_status"];
                
                if (zaisekiStatusCell.Value != null && zaisekiStatusCell.Value != DBNull.Value)
                {
                    string status = zaisekiStatusCell.Value.ToString();
                    Color rowColor = Color.White; // デフォルトは白
                    
                    // 在席状況に応じて色を設定
                    switch (status)
                    {
                        case "在席":
                            rowColor = Color.LightBlue;
                            break;
                        case "帰宅":
                            rowColor = Color.LightGray;
                            break;
                        case "在宅勤務":
                            rowColor = Color.LightGreen;
                            break;
                        case "出張":
                            rowColor = Color.LightCoral;
                            break;
                    }
                    
                    // 行全体の背景色を設定
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = rowColor;
                }
            }
        }

        private void CenterLoadingLabel()
        {
            labelLoading.Left = (this.ClientSize.Width - labelLoading.Width) / 2;
            labelLoading.Top = (this.ClientSize.Height - labelLoading.Height) / 2;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // フォーム読み込み時にデータを表示
            await LoadUserStatusDataAsync();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // ボタンクリック時にデータを更新
            await LoadUserStatusDataAsync();
        }

        private async Task LoadUserStatusDataAsync()
        {
            try
            {
                // ロード中表示を表示
                ShowLoading(true);
                
                // データベースからJOINしたデータを取得してDataGridViewに表示
                DataTable userData = await GetUserStatusDataAsync();
                
                // DataGridViewにデータをバインド
                dataGridView1.DataSource = userData;
                
                // 列の表示設定
                ConfigureDataGridViewColumns();
            }
            catch (Exception ex)
            {
                // エラーの場合はエラーメッセージを表示
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // ロード中表示を非表示
                ShowLoading(false);
            }
        }

        private void ShowLoading(bool show)
        {
            labelLoading.Visible = show;
            dataGridView1.Enabled = !show;
            button1.Enabled = !show;
            
            if (show)
            {
                CenterLoadingLabel();
                labelLoading.BringToFront();
            }
            
            // UIを即座に更新
            Application.DoEvents();
        }

        private void ConfigureDataGridViewColumns()
        {
            // すべての列を非表示にする
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.Visible = false;
            }
            
            // display_user_nameとzaiseki_statusとnoteを表示する
            if (dataGridView1.Columns.Contains("display_user_name"))
            {
                dataGridView1.Columns["display_user_name"].Visible = true;
                dataGridView1.Columns["display_user_name"].HeaderText = "ユーザー名";
                dataGridView1.Columns["display_user_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["display_user_name"].ReadOnly = true;
            }
            
            if (dataGridView1.Columns.Contains("zaiseki_status"))
            {
                // 既存の列を削除してComboBoxColumnに置き換える
                int columnIndex = dataGridView1.Columns["zaiseki_status"].Index;
                dataGridView1.Columns.Remove("zaiseki_status");
                
                // ComboBoxColumnを作成
                DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn();
                comboColumn.Name = "zaiseki_status";
                comboColumn.DataPropertyName = "zaiseki_status";
                comboColumn.HeaderText = "在席状況";
                comboColumn.Items.AddRange(new string[] { "在席", "帰宅", "在宅勤務", "出張" });
                comboColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                comboColumn.FlatStyle = FlatStyle.Flat; // ComboBoxの背景色を反映させるため
                
                // 元の位置に挿入
                dataGridView1.Columns.Insert(columnIndex, comboColumn);
            }
            
            if (dataGridView1.Columns.Contains("note"))
            {
                dataGridView1.Columns["note"].Visible = true;
                dataGridView1.Columns["note"].HeaderText = "備考";
                dataGridView1.Columns["note"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private async Task<DataTable> GetUserStatusDataAsync()
        {
            DataTable dataTable = new DataTable();
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // m_user、t_status_period、t_zaisekiをJOIN
                string query = @"
                    SELECT 
                        u.user_id,
                        u.user_name,
                        u.user_name_e,
                        u.employee_number,
                        u.display_user_name,
                        z.zaiseki_status,
                        z.sort_id,
                        s.project_code,
                        s.note,
                        s.updated_at
                    FROM m_user u
                    LEFT JOIN t_status_period s ON u.user_id = s.user_id
                    LEFT JOIN t_zaiseki z ON u.user_id = z.user_id AND z.is_enable = 1
                    ORDER BY z.sort_id, u.user_id";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataTable));
                    }
                }
            }
            
            return dataTable;
        }
    }
}
