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

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // データベースから件数を取得
                int userCount = await GetUserCountAsync();
                
                // メッセージボックスで表示
                MessageBox.Show($"m_userテーブルの件数: {userCount}件", "データベース件数", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // エラーの場合はエラーメッセージを表示
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> GetUserCountAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                string query = "SELECT COUNT(*) FROM m_user";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}
