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
        // Azure SQL Server�ڑ�������i�e�X�g�p�j
        // ���ۂ̒l�ɒu�������Ă�������
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
            // ���[�h���\���p��Label���쐬
            labelLoading = new Label();
            labelLoading.Text = "���[�h���ł�...";
            labelLoading.Font = new Font("MS UI Gothic", 14, FontStyle.Bold);
            labelLoading.ForeColor = Color.Blue;
            labelLoading.AutoSize = true;
            labelLoading.BackColor = Color.White;
            labelLoading.Visible = false;
            
            // �t�H�[���ɒǉ�
            this.Controls.Add(labelLoading);
            labelLoading.BringToFront();
            
            // �ʒu�𒆉��ɐݒ�i�t�H�[���̃T�C�Y���ς�����Ƃ����Ή��j
            this.Resize += (s, e) => CenterLoadingLabel();
            CenterLoadingLabel();
        }

        private void InitializeDataGridViewEvents()
        {
            // �s�̐F��ύX���邽�߂̃C�x���g��ǉ�
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].IsNewRow)
                return;

            // zaiseki_status��̃C���f�b�N�X���擾
            if (dataGridView1.Columns.Contains("zaiseki_status"))
            {
                var zaisekiStatusCell = dataGridView1.Rows[e.RowIndex].Cells["zaiseki_status"];
                
                if (zaisekiStatusCell.Value != null && zaisekiStatusCell.Value != DBNull.Value)
                {
                    string status = zaisekiStatusCell.Value.ToString();
                    Color rowColor = Color.White; // �f�t�H���g�͔�
                    
                    // �ݐȏ󋵂ɉ����ĐF��ݒ�
                    switch (status)
                    {
                        case "�ݐ�":
                            rowColor = Color.LightBlue;
                            break;
                        case "�A��":
                            rowColor = Color.LightGray;
                            break;
                        case "�ݑ�Ζ�":
                            rowColor = Color.LightGreen;
                            break;
                        case "�o��":
                            rowColor = Color.LightCoral;
                            break;
                    }
                    
                    // �s�S�̂̔w�i�F��ݒ�
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
            // �t�H�[���ǂݍ��ݎ��Ƀf�[�^��\��
            await LoadUserStatusDataAsync();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // �{�^���N���b�N���Ƀf�[�^���X�V
            await LoadUserStatusDataAsync();
        }

        private async Task LoadUserStatusDataAsync()
        {
            try
            {
                // ���[�h���\����\��
                ShowLoading(true);
                
                // �f�[�^�x�[�X����JOIN�����f�[�^���擾����DataGridView�ɕ\��
                DataTable userData = await GetUserStatusDataAsync();
                
                // DataGridView�Ƀf�[�^���o�C���h
                dataGridView1.DataSource = userData;
                
                // ��̕\���ݒ�
                ConfigureDataGridViewColumns();
            }
            catch (Exception ex)
            {
                // �G���[�̏ꍇ�̓G���[���b�Z�[�W��\��
                MessageBox.Show($"�G���[���������܂���: {ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // ���[�h���\�����\��
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
            
            // UI�𑦍��ɍX�V
            Application.DoEvents();
        }

        private void ConfigureDataGridViewColumns()
        {
            // ���ׂĂ̗���\���ɂ���
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.Visible = false;
            }
            
            // display_user_name��zaiseki_status��note��\������
            if (dataGridView1.Columns.Contains("display_user_name"))
            {
                dataGridView1.Columns["display_user_name"].Visible = true;
                dataGridView1.Columns["display_user_name"].HeaderText = "���[�U�[��";
                dataGridView1.Columns["display_user_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["display_user_name"].ReadOnly = true;
            }
            
            if (dataGridView1.Columns.Contains("zaiseki_status"))
            {
                // �����̗���폜����ComboBoxColumn�ɒu��������
                int columnIndex = dataGridView1.Columns["zaiseki_status"].Index;
                dataGridView1.Columns.Remove("zaiseki_status");
                
                // ComboBoxColumn���쐬
                DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn();
                comboColumn.Name = "zaiseki_status";
                comboColumn.DataPropertyName = "zaiseki_status";
                comboColumn.HeaderText = "�ݐȏ�";
                comboColumn.Items.AddRange(new string[] { "�ݐ�", "�A��", "�ݑ�Ζ�", "�o��" });
                comboColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                comboColumn.FlatStyle = FlatStyle.Flat; // ComboBox�̔w�i�F�𔽉f�����邽��
                
                // ���̈ʒu�ɑ}��
                dataGridView1.Columns.Insert(columnIndex, comboColumn);
            }
            
            if (dataGridView1.Columns.Contains("note"))
            {
                dataGridView1.Columns["note"].Visible = true;
                dataGridView1.Columns["note"].HeaderText = "���l";
                dataGridView1.Columns["note"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private async Task<DataTable> GetUserStatusDataAsync()
        {
            DataTable dataTable = new DataTable();
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // m_user�At_status_period�At_zaiseki��JOIN
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
