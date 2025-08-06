using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.VisualBasic.FileIO;

namespace VSOfflineInstallCleaner
{
    public class CleanVs
    {
        internal bool MoveToFolder(string vsOfflineDirectory, IEnumerable<string> pakagesTobeMoved, string unneededPackagesfolderName)
        {
            bool exists = Directory.Exists($@"{vsOfflineDirectory}\{unneededPackagesfolderName}");

            if (!exists)
                Directory.CreateDirectory($@"{vsOfflineDirectory}\{unneededPackagesfolderName}");

            foreach (string packageFolderName in pakagesTobeMoved)
            {
                string sourceDirName = $@"{vsOfflineDirectory}\{packageFolderName}";
                string destinationDirName = $@"{vsOfflineDirectory}\{unneededPackagesfolderName}\{packageFolderName}";

                if (packageFolderName == unneededPackagesfolderName || packageFolderName == "Archive" || packageFolderName == "certificates") continue;
                try
                {
                    // 检查目标目录是否存在
                    if (Directory.Exists(destinationDirName))
                    {
                        // 删除目标目录及其所有内容
                        Directory.Delete(destinationDirName, true);
                    }
                    Directory.Move(sourceDirName, destinationDirName);
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }
            return true;
        }

        internal bool MoveToRecycle(string vsOfflineDirectory, IEnumerable<string> pakagesTobeMoved, bool useRecycle)
        {
            foreach (string packageFolderName in pakagesTobeMoved)
            {
                string sourceDirName = $@"{vsOfflineDirectory}\{packageFolderName}";
                if (packageFolderName == "Archive" || packageFolderName == "certificates") continue;
                try
                {
                    //Directory.Delete(sourceDirName, true);
                    if (useRecycle)
                    {
                        FileSystem.DeleteDirectory(
                        sourceDirName,
                        UIOption.OnlyErrorDialogs,      // 只在發生錯誤時顯示對話方塊
                        RecycleOption.SendToRecycleBin, // 指定移至回收筒
                        UICancelOption.ThrowException   // 如果使用者取消操作，則擲回例外狀況
                    );
                    }
                    else
                    {
                        //Directory.Delete(sourceDirName, true);
                        FileSystem.DeleteDirectory(
                        sourceDirName,
                        UIOption.OnlyErrorDialogs,          // 只在發生錯誤時顯示對話方塊
                        RecycleOption.DeletePermanently,    // 永久删除
                        UICancelOption.ThrowException       // 如果使用者取消操作，則擲回例外狀況
                    );
                    }
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }

            return true;
        }

        internal HashSet<string> GetPackageNames(string catalogFileName, string unneededPackagesfolderName)
        {

            string catalogFileContent = File.ReadAllText(catalogFileName);

            Catalog catalog = JsonConvert.DeserializeObject<Catalog>(catalogFileContent);

            List<string> packageNames = new List<string>();

            packageNames.Add("Archive");
            packageNames.Add("certificates");
            packageNames.Add(unneededPackagesfolderName);

            foreach (Package package in catalog.Packages)
            {
                string currentpackageName = package.Id;

                if (!string.IsNullOrEmpty(package.Version))
                    currentpackageName += $",version={package.Version}";

                if (!string.IsNullOrEmpty(package.Chip))
                    currentpackageName += $",chip={package.Chip}";               

                if (!string.IsNullOrEmpty(package.Language))
                    currentpackageName += $",language={package.Language}";

                if (!string.IsNullOrEmpty(package.ProductArch))
                    currentpackageName += $",productarch={package.ProductArch}";

                if (!string.IsNullOrEmpty(package.MachineArch))
                    currentpackageName += $",machinearch={package.MachineArch}";

                packageNames.Add(currentpackageName);
            }

            return packageNames.ToHashSet();
        }

        internal HashSet<string> GetFolderNames(string vsOfflineDirectory)
        {
            List<string> vsFolderNames = Directory.GetDirectories(vsOfflineDirectory).Select(folderpath => new DirectoryInfo(folderpath).Name).ToList();

            return vsFolderNames.ToHashSet();
        }

        internal Catalog GetInfo(string catalogFileName)
        {
            string catalogFile = File.ReadAllText(catalogFileName);

            Catalog catalog = JsonConvert.DeserializeObject<Catalog>(catalogFile);

            return catalog;
        }
        
    }
}
