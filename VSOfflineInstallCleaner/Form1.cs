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
using System.Xml.Linq;

namespace VSOfflineInstallCleaner
{
    public partial class Form1 : Form
    {
        string vsOfflineDirectory = "";     // System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string catalogFileName = "";        // System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Catalog.json";
        string removedFolderName = "ToBeRemoved";
        string language = "";
        string titleText = "";
        public Form1()
        {
            InitializeComponent();

            // 获取当前线程的当前UI用户界面语言
            CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
            language = currentUICulture.Name;   // 例如 "zh-CN"
            if (language == "zh-CN")
            {
                titleText = "Visual Studio 离线安装包清理器 2.0";
                label1.Text = "Visual Studio 离线安装包文件夹";
                label2.Text = "Visual Studio 安装包目录文件名 [ 默认为：Catalog.json ]";
                label3.Text = "清理前的文件夹数量：";
                label4.Text = "需要清理的文件夹数量：";
                label5.Text = "不需要的文件夹将被移动到：";
                label6.Text = " 文件夹";
                lbMovetoFolderName.Text = removedFolderName;
                button1.Text = "浏览(&B)";
                btnClean.Text = "清理(&C)";
                btnRefresh.Text = "刷新(&R)";
                btnExit.Text = "退出(&E)";
            }
            else
            {
                titleText = "Visual Studio Offline Install Cleaner 2.0";
                label1.Text = "Visual Studio Offline Directory";
                label2.Text = "Visual Studio Offline Update Catalog FileName [ Default : Catalog.json ]";
                label3.Text = "Number of Folder before cleanup :";
                label4.Text = "Number of Folder Needs to be Cleaned :";
                label5.Text = "Unneeded Folder will be moved to :";
                label6.Text = " Folder";
                lbMovetoFolderName.Text = removedFolderName;
                button1.Text = "&Browse";
                btnClean.Text = "&Clean)";
                btnRefresh.Text = "&Refresh)";
                btnExit.Text = "&Exit)";
            }
            this.Text = titleText;

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
            if (language == "zh-CN")
            {
                textColumn.HeaderText = "文件夹名称";
            }
            else
            {
                textColumn.HeaderText = "FolderName";
            }
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

            if (language == "zh-CN")
            {
                folderBrowserDialog.Description = "请选择 Visual Studio 离线安装包文件夹";
            }
            else
            {
                folderBrowserDialog.Description = "Please Select Visual Studio Offline Directory";
            }

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
                    if (language == "zh-CN")
                    {
                        MessageBox.Show("你选择的文件夹不是有效的 Visual Studio 离线安装包文件夹，该文件夹下应包含：nCatalog.json、vs_installer.opc、vs_setup.exe 等文件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("The folder you selected is not a valid Visual Studio offline installation package folder.The folder should contain files such as Catalog.json, vs_installer.opc, and vs_setup.exe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
                if (language == "zh-CN")
                {
                    MessageBox.Show($@"清理过程已完成，节省了大约 {savedDiskSpace} 的磁盘空间，请删除“{removedFolderName}”文件夹以节省磁盘空间", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($@"Cleanup process is Done, Saved about {savedDiskSpace} in disk Space, Please Remove the “{removedFolderName}” directory to Save Disk Space", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                if (language == "zh-CN")
                {
                    MessageBox.Show($@"清理过程已完成，请删除 “{removedFolderName}” 文件夹以节省磁盘空间", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($@"Cleanup process is Done, Please Remove the ""{removedFolderName}"" directory to Save Disk Space", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            SetLabelDisplay();
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
                if (language == "zh-CN")
                {
                    MessageBox.Show("请选择离线安装包目录文件，默认文件名为：Catalog.json", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Please select the offline installation package catalog file. The default file name is: Catalog.json", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            else
            {
                string catalogFile = Path.GetFileName(catalogFileName);
                if (catalogFile != "Catalog.json")
                {
                    if (language == "zh-CN")
                    {
                        MessageBox.Show("离线安装包目录文件错误，默认安装包目录文件名为：Catalog.json", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("The offline installation package catalog file error.The default installation package catalog file name is: Catalog.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
            }

            string vs_installer = vsOfflineDirectory + "\\vs_installer.opc";
            string vs_setup = vsOfflineDirectory + "\\vs_setup.exe";
            if (!File.Exists(catalogFileName) || !File.Exists(vs_installer) || !File.Exists(vs_setup))
            {
                if (language == "zh-CN")
                {
                    MessageBox.Show("安装包目录文件所在的文件夹不是有效的 Visual Studio 离线安装包文件夹，该文件夹下应包含：Catalog.json、vs_installer.opc、vs_setup.exe 等文件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("The folder where the installation package catalog file is not the Visual Studio offline installation package folder.This folder should contain files such as Catalog.json, vs_installer.opc, and vs_setup.exe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            //获取版本信息
            Catalog vsInfo = new CleanVs().GetInfo(catalogFileName);
            if (vsInfo == null || String.IsNullOrEmpty(vsInfo.Info.ProductName) || vsInfo.Info.ProductName!= "Visual Studio") 
            {
                if (language == "zh-CN")
                {
                    MessageBox.Show("离线安装包目录文件没有包含有效的安装信息。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("The offline installation package catalog file does not contain valid installation information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return; 
            }

            string Info = "  [ " + vsInfo.Info.ProductName + " " + vsInfo.Info.ProductLineVersion + "  V" + vsInfo.Info.ProductDisplayVersion +" ]";
            this.Text = titleText + Info;

            //文件夹名称
            HashSet<string> folderNames = new CleanVs().GetFolderNames(vsOfflineDirectory);
            lbFolderNamesCount.Text = folderNames.Count().ToString();

            //日志文件中的文件夹名称
            HashSet<string> packageNames = new CleanVs().GetPackageNames(catalogFileName, removedFolderName);
            IEnumerable<string> pakagesTobeMoved = folderNames.Except(packageNames).ToHashSet();
            lbNeedstobeCleaned.Text = pakagesTobeMoved.Count().ToString();

            if (pakagesTobeMoved.Count() > 0)
            {
                btnClean.Enabled = true;
            }
            else
            {
                btnClean.Enabled = false;
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
