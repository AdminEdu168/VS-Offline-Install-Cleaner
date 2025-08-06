using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;


namespace VSOfflineInstallCleaner
{
    public partial class Form1 : Form
    {
        string currentLanguage = CultureInfo.CurrentUICulture.Name;    // 获取当前UI用户界面语言
        dynamic language = new Language().GetLanguage(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Language.json");

        string vsOfflineDirectory = "";     // System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string catalogFileName = "";        // System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Catalog.json";
        string removedFolderName = "_Backup";
        string title = "Visual Studio Offline Install Cleaner 2.0";
        public Form1()
        {
            InitializeComponent();

            if (language == null || language[currentLanguage] == null) { currentLanguage = "en-US"; }

            if (language[currentLanguage] != null)
            {
                label1.Text = language[currentLanguage].Libel1;
                label2.Text = language[currentLanguage].Libel2;
                label3.Text = language[currentLanguage].Libel3;
                label4.Text = language[currentLanguage].Libel4;
                label5.Text = language[currentLanguage].Libel5;
                label6.Text = language[currentLanguage].Libel6;

                button1.Text = language[currentLanguage].Button1;
                button2.Text = language[currentLanguage].Button2;
                button3.Text = language[currentLanguage].Button3;
                button4.Text = language[currentLanguage].Button4;
                button5.Text = language[currentLanguage].Button5;
            }

            this.Text = title;
            lbMovetoFolderName.Text = removedFolderName;
            lbFolderNamesCount.Left = label3.Left + label3.Width + 2;
            lbNeedstobeCleaned.Left = label4.Left + label4.Width + 2;
            lbMovetoFolderName.Left = label5.Left + label5.Width + 2;
            label6.Left = label5.Left + label5.Width + lbMovetoFolderName.Width + 2;

            splitContainer1.Orientation = Orientation.Horizontal;   //上下分栏
            splitContainer1.SplitterDistance = 160;                 //先设置分割线位置
            splitContainer1.IsSplitterFixed = true;                 //面板大小不允许改动
            splitContainer1.FixedPanel = FixedPanel.Panel1;         //面板1大小固定

            // dataGridView1 创建并添加一个文本列
            dataGridView1.AutoGenerateColumns = false;
            DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
            textColumn.Name = "FolderName";
            textColumn.HeaderText = language[currentLanguage].TextColumnHeaderText;
            dataGridView1.Columns.Add(textColumn);

            Form1_Resize(null, null);

            textBox1.Text = "";
            textBox2.Text = "";

            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string catalog = path + "\\Catalog.json";
            string vs_installer = path + "\\vs_installer.opc";
            string vs_setup = path + "\\vs_setup.exe";
            if (File.Exists(catalog) && File.Exists(vs_installer) && File.Exists(vs_setup))
            {
                textBox1.Text = path;
                textBox2.Text = catalog;

                vsOfflineDirectory = path;
                catalogFileName = catalog;
                SetLabelDisplay();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = "";

            // 创建并配置对话框
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            if (!String.IsNullOrEmpty(vsOfflineDirectory))
            {
                folderBrowserDialog.SelectedPath = vsOfflineDirectory;
            }
            else
            {
                folderBrowserDialog.SelectedPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            folderBrowserDialog.Description = language[currentLanguage].FolderBrowserDialogDescription;

            // 显示对话框
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                string catalog = path + "\\Catalog.json";
                string vs_installer = path + "\\vs_installer.opc";
                string vs_setup = path + "\\vs_setup.exe";
                if (File.Exists(catalog) && File.Exists(vs_installer) && File.Exists(vs_setup))
                {
                    textBox1.Text = path;
                    textBox2.Text = catalog;

                    vsOfflineDirectory = path;
                    catalogFileName = catalog;
                    SetLabelDisplay();
                }
                else
                {
                    MessageBox.Show($@"{language[currentLanguage].MsgValidVsFolder}","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnClean_Click(object sender, EventArgs e)
        {
            HashSet<string> folderNames = new CleanVs().GetFolderNames(vsOfflineDirectory);
            HashSet<string> packageNames = new CleanVs().GetPackageNames(catalogFileName, removedFolderName);
            IEnumerable<string> pakagesNotListedInCatalog = folderNames.Except(packageNames).ToHashSet();

            bool moving = new CleanVs().MoveToFolder(vsOfflineDirectory, pakagesNotListedInCatalog, removedFolderName);
            if (!moving) return;
            try
            {
                string savedDiskSpace = DirectorySize.GetFolderSize($@"{vsOfflineDirectory}\{removedFolderName}");               
                string msg = language[currentLanguage].MsgCleanupDoneDiskSpace;
                MessageBox.Show(string.Format(msg, savedDiskSpace, removedFolderName), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {

                string msg = language[currentLanguage].MsgCleanupDoneNoDiskSpace;
                MessageBox.Show(string.Format(msg, removedFolderName), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetLabelDisplay();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            HashSet<string> folderNames = new CleanVs().GetFolderNames(vsOfflineDirectory);
            HashSet<string> packageNames = new CleanVs().GetPackageNames(catalogFileName, removedFolderName);
            IEnumerable<string> pakagesNotListedInCatalog = folderNames.Except(packageNames).ToHashSet();
            bool moving = false;
            DialogResult result = DialogResult.Yes;

            result = MessageBox.Show($@"{language[currentLanguage].MsgMoveToRecycle}", "Info", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    moving = new CleanVs().MoveToRecycle(vsOfflineDirectory, pakagesNotListedInCatalog, true);
                    break;
                case DialogResult.No:
                    moving = new CleanVs().MoveToRecycle(vsOfflineDirectory, pakagesNotListedInCatalog, false);
                    break;
                case DialogResult.Cancel:
                    break;
                default:
                    moving = new CleanVs().MoveToRecycle(vsOfflineDirectory, pakagesNotListedInCatalog, true);
                    break;
            }
            
            SetLabelDisplay();
            if (!moving) return;
            if (result == DialogResult.Yes)
            {
                MessageBox.Show($@"{language[currentLanguage].MsgEmptyRecycleBin}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SetLabelDisplay();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Left = (this.Width - panel1.Width) / 2;
            splitContainer1.Width = this.Width - 40;
            splitContainer1.Height = this.Height - panel1.Height - 70;
            textBox1.Width = splitContainer1.Width - button1.Width - 25;
            textBox2.Width = splitContainer1.Width - button1.Width - 25;

            dataGridView1.Width = splitContainer1.Width;
            dataGridView1.Columns["FolderName"].Width = dataGridView1.Width - 43;
        }

        private void SetLabelDisplay()
        {
            if (String.IsNullOrEmpty(catalogFileName))
            {
                MessageBox.Show($@"{language[currentLanguage].MsgSelectCatalogFile}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                string catalogFile = Path.GetFileName(catalogFileName);
                if (catalogFile != "Catalog.json")
                {
                    MessageBox.Show($@"{language[currentLanguage].MsgCatalogFileNameError}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string vs_installer = vsOfflineDirectory + "\\vs_installer.opc";
            string vs_setup = vsOfflineDirectory + "\\vs_setup.exe";
            if (!File.Exists(catalogFileName) || !File.Exists(vs_installer) || !File.Exists(vs_setup))
            {
                MessageBox.Show($@"{language[currentLanguage].MsgCatalogFolderError}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //获取版本信息
            Catalog vsInfo = new CleanVs().GetInfo(catalogFileName);
            if (vsInfo == null || String.IsNullOrEmpty(vsInfo.Info.ProductName) || vsInfo.Info.ProductName != "Visual Studio")
            {
                MessageBox.Show($@"{language[currentLanguage].MsgCatalogContentsError}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string Info = "  [ " + vsInfo.Info.ProductName + " " + vsInfo.Info.ProductLineVersion + "  V" + vsInfo.Info.ProductDisplayVersion + " ]";
            this.Text = title + Info;

            //文件夹名称
            HashSet<string> folderNames = new CleanVs().GetFolderNames(vsOfflineDirectory);
            lbFolderNamesCount.Text = folderNames.Count().ToString();

            //日志文件中的文件夹名称
            HashSet<string> packageNames = new CleanVs().GetPackageNames(catalogFileName, removedFolderName);
            IEnumerable<string> pakagesTobeMoved = folderNames.Except(packageNames).ToHashSet();
            lbNeedstobeCleaned.Text = pakagesTobeMoved.Count().ToString();

            if (pakagesTobeMoved.Count() > 0)
            {
                button2.Enabled = true;
                button3.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
                button3.Enabled = false;
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FolderName");

            foreach (string packageFolderName in pakagesTobeMoved)
            {
                string sourceDirName = $@"{vsOfflineDirectory}\{packageFolderName}";
                if (packageFolderName == removedFolderName || packageFolderName == "Archive" || packageFolderName == "certificates") continue;
                dataTable.Rows.Add(sourceDirName);
            }

            this.dataGridView1.DataSource = dataTable;
            this.dataGridView1.Columns["FolderName"].DataPropertyName = "FolderName";
            this.dataGridView1.Sort(dataGridView1.Columns["FolderName"], ListSortDirection.Ascending);
            this.dataGridView1.Columns["FolderName"].Width = dataGridView1.Width - 43;
            this.dataGridView1.Refresh();
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // 获取当前行索引（从1开始）
            var rowIndex = (e.RowIndex + 1).ToString();

            // 设置行号显示格式
            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // 获取行头区域
            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dataGridView1.RowHeadersWidth - 4,
                e.RowBounds.Height);

            // 绘制行号
            e.Graphics.DrawString(
                rowIndex,
                this.Font,
                SystemBrushes.ControlText,
                headerBounds,
                centerFormat);
        }

        
    }
}
