using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace UnityPatch
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string appPath = "C:";
        private int patchAs;
        public MainWindow()
        {
            InitializeComponent();
        }
        private bool WriteLicenseToFile(string appDir, bool spfold, int version)
        {
            string str1 = string.Format("{0}-{1}-{2}-{3}-{4}", "U3", "AAAA", "AAAA", "AAAA", "AAAA");
      
            List<byte> byteList1 = new List<byte>();
            List<byte> byteList2 = byteList1;
            byte[] numArray = new byte[4];
            numArray[0] = (byte)1;
            byteList2.AddRange((IEnumerable<byte>)numArray);
            byteList1.AddRange((IEnumerable<byte>)Encoding.ASCII.GetBytes(string.Format("{0}-{1}", (object)str1, "NUUN")));
            List<string> stringList = new List<string>()
      {
        "<root>",
        "  <License id=\"Terms\">",
        "    <ClientProvidedVersion Value=\"\"/>",
        string.Format("    <DeveloperData Value=\"{0}\"/>", (object) Convert.ToBase64String(byteList1.ToArray())),
        "    <Features>"
      };
            foreach (int num in LicHeader.ReadAll())
                stringList.Add(string.Format("      <Feature Value=\"{0}\"/>", (object)num));
            stringList.Add("    </Features>");
            stringList.Add(version > 0 ? "    <LicenseVersion Value=\"5.x\"/>" : "    <LicenseVersion Value=\"4.x\"/>");
            stringList.Add("    <MachineBindings>");
            stringList.Add("    </MachineBindings>");
            stringList.Add("    <MachineID Value=\"\"/>");
            stringList.Add("    <SerialHash Value=\"\"/>");
            stringList.Add(string.Format("    <SerialMasked Value=\"{0}-XXXX\"/>", (object)str1));
            DateTime now = DateTime.Now;
            stringList.Add(string.Format("    <StartDate Value=\"{0}T00:00:00\"/>", (object)now.AddDays(-1.0).ToString("yyyy-MM-dd")));
            stringList.Add("    <StopDate Value=\"\"/>");
            stringList.Add(string.Format("    <UpdateDate Value=\"{0}T00:00:00\"/>", (object)now.AddYears(10).ToString("yyyy-MM-dd")));
            stringList.Add("  </License>");
            stringList.Add("");
            stringList.Add("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">");
            stringList.Add("<SignedInfo>");
            stringList.Add("<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\"/>");
            stringList.Add("<SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/>");
            stringList.Add("<Reference URI=\"#Terms\">");
            stringList.Add("<Transforms>");
            stringList.Add("<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/>");
            stringList.Add("</Transforms>");
            stringList.Add("<DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/>");
            stringList.Add("<DigestValue>oeMc1KScgy617DHMPTxbYhqNjIM=</DigestValue>");
            stringList.Add("</Reference>");
            stringList.Add("</SignedInfo>");
            stringList.Add("<SignatureValue>WuzMPTi0Ko1vffk9gf9ds/iU0b0K8UHaLpi4kWgm6q1am5MPTYYnzH1InaSWuzYo");
            stringList.Add("EpJThKspOZdO0JISeEolNdJVf3JpsY55OsD8UaruvhwZn4r9pLeNSC7SzQ1rvAWP");
            stringList.Add("h77XaHizhVVs15w6NYevP27LTxbZaem5L8Zs+34VKXQFeG4g0dEI/Jhl70TqE0CS");
            stringList.Add("YNF+D0zqEtyMNHsh0Rq/vPLSzPXUN12jfPLZ3dO9B+9/mG7Ljd6emZjjLZUVuSKQ");
            stringList.Add("uKxN5jlHZsm2kRMudijICV6YOWMPT+oZePlCg+BJQg5/xcN5aYVBDZhNeuNwQL1H");
            stringList.Add("MPT/GJPxVuETgd9k8c4uDg==</SignatureValue>");
            stringList.Add("</Signature>");
            stringList.Add("</root>");
            string str2 = version > 499 ? "Unity_v5.x.ulf" : "Unity_v4.x.ulf";
            string path = string.Empty;
            if (spfold)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Unity";
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        spfold = false;
                        int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                    }
                }
            }
            if (spfold)
            {
                if (File.Exists(path + "/" + str2))
                {
                    spfold = this.TestAtr(path + "/" + str2);
                    if (spfold && MessageBox.Show(string.Format("Replace the \"{0}\\{1}\"?", (object)path, (object)str2), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
                    {
                        stringList.Clear();
                        return true;
                    }
                }
                if (spfold)
                {
                    try
                    {
                        File.WriteAllLines(path + "/" + str2, (IEnumerable<string>)stringList);
                    }
                    catch (Exception ex)
                    {
                        spfold = false;
                        int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                    }
                }
            }
            if (!spfold)
            {
                if (File.Exists(appDir + "/" + str2))
                {
                    if (this.TestAtr(appDir + "/" + str2))
                    {
                        if (MessageBox.Show(string.Format("Replace the \"{0}\\{1}\"?", (object)appDir, (object)str2), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
                        {
                            stringList.Clear();
                            return true;
                        }
                    }
                    else
                    {
                        stringList.Clear();
                        return false;
                    }
                }
                try
                {
                    File.WriteAllLines(appDir + "/" + str2, (IEnumerable<string>)stringList);
                }
                catch (Exception ex)
                {
                    stringList.Clear();
                    int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                    return false;
                }
            }
            stringList.Clear();
            return true;
        }
        private bool TestAtr(string path)
        {
            try
            {
                File.GetAttributes(path);
                FileAttributes fileAttributes = FileAttributes.Normal;
                File.SetAttributes(path, fileAttributes);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                return false;
            }
            return true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                Description = "Please select the folder where Unity.exe is...",
                ShowNewFolderButton = false
            };
            if (Directory.Exists(unityPath.Text))
                folderBrowserDialog.SelectedPath = unityPath.Text;
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            this.unityPath.Text = folderBrowserDialog.SelectedPath;
            try
            {
                Directory.SetCurrentDirectory(folderBrowserDialog.SelectedPath);
                if (!File.Exists(folderBrowserDialog.SelectedPath + "/Unity.exe"))
                    throw new IOException("Application not found!");
                this.appPath = folderBrowserDialog.SelectedPath + "/Unity.exe";
                this.versionText.Text = FileVersionInfo.GetVersionInfo(this.appPath).FileVersion.Substring(0, 5);
                this.RegistBtn.IsEnabled = true;
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(this.appPath);
                this.patchAs = int.Parse(versionInfo.FileVersion[0].ToString()) * 100;
                this.patchAs = this.patchAs + int.Parse(versionInfo.FileVersion[2].ToString()) * 10;
                this.patchAs = this.patchAs + int.Parse(versionInfo.FileVersion[4].ToString());
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void RegistBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.unityPath.Text.Length < 3)
                this.unityPath.Text = System.Windows.Forms.Application.StartupPath;
            string text = this.unityPath.Text;
            try
            {
                if (Process.GetProcessesByName("unity").Length != 0)
                    throw new Exception("Need to close application first.");
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            File.Copy(text + "/Unity.exe", text + "/Unity.exe.bak", true);
            NLogger.Clear();
            string se1 = "";
            string rep1 = "";
            string se2 = "";
            string rep2 = "";
            if (this.patchAs < 530)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 40 57 48 83 EC 30 80 79 ?? ?? 48 8B F9 75 ??";
                rep2 = "CC B0 01 C3 90 90 90 80 79 ?? ?? 48 8B F9 75 ??";
            }
            if (this.patchAs >= 530)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 40 57 48 83 EC 30 80 79 ?? ?? 48 8B F9 75 ??";
                rep2 = "CC B0 01 C3 90 90 90 80 79 ?? ?? 48 8B F9 75 ??";
            }
            Patcher patcher = new Patcher(text + "/Unity.exe");
            if (patcher.AddString(se1, rep1, 1U, 0U))
            {
                if (patcher.Patch())
                {
                    NLogger.LastMessage();
                    this.WriteLicenseToFile(text, true, this.patchAs);
                    return;
                }
                if (this.patchAs >= 500)
                {
                    patcher.Patterns.Clear();
                    if (patcher.AddString(se2, rep2, 1U, 0U) && patcher.Patch())
                    {
                        NLogger.LastMessage();
                        this.WriteLicenseToFile(text, true, this.patchAs);
                        return;
                    }
                }
            }
            NLogger.LastMessage();
        }

       
    }
}
