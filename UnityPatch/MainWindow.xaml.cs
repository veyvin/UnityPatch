using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace UnityPatch
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
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
            var str1 = string.Format("{0}-{1}-{2}-{3}-{4}", "U3", "AAAA", "AAAA", "AAAA", "AAAA");

            var byteList1 = new List<byte>();
            var byteList2 = byteList1;
            var numArray = new byte[4];
            numArray[0] = 1;
            byteList2.AddRange(numArray);
            byteList1.AddRange(Encoding.ASCII.GetBytes(string.Format("{0}-{1}", str1, "NUUN")));

            var stringList = new List<string>
            {
                "<root>",
                "  <License id=\"Terms\">",
                "    <ClientProvidedVersion Value=\"\"/>",
                string.Format("    <DeveloperData Value=\"{0}\"/>", Convert.ToBase64String(byteList1.ToArray())),
                "    <Features>"
            };
            foreach (var num in LicHeader.ReadAll())
                stringList.Add(string.Format("      <Feature Value=\"{0}\"/>", num));
            stringList.Add("    </Features>");
            if (version != 4)
                if (version == 5 || version == 53)
                    stringList.Add("    <LicenseVersion Value=\"5.x\"/>");
                else
                    stringList.Add("    <LicenseVersion Value=\"2017.x\"/>");
            else
                stringList.Add("    <LicenseVersion Value=\"4.x\"/>");
            stringList.Add("    <MachineBindings>");
            stringList.Add("    </MachineBindings>");
            stringList.Add("    <MachineID Value=\"\"/>");
            stringList.Add("    <SerialHash Value=\"\"/>");
            stringList.Add(string.Format("    <SerialMasked Value=\"{0}-XXXX\"/>", str1));
            var now = DateTime.Now;
            stringList.Add(string.Format("    <StartDate Value=\"{0}T00:00:00\"/>",
                now.AddDays(-1.0).ToString("yyyy-MM-dd")));
            stringList.Add("    <StopDate Value=\"\"/>");
            stringList.Add(string.Format("    <UpdateDate Value=\"{0}T00:00:00\"/>",
                now.AddYears(10).ToString("yyyy-MM-dd")));
            stringList.Add("  </License>");
            stringList.Add("");
            stringList.Add("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">");
            stringList.Add("<SignedInfo>");
            stringList.Add(
                "<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\"/>");
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
            var str2 = version == 4
                ? "Unity_v4.x.ulf"
                : (version == 5 || version == 53 ? "Unity_v5.x.ulf" : "Unity_v2017.x.ulf");
            var path = string.Empty;
            if (spfold)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Unity";
                if (!Directory.Exists(path))
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        spfold = false;
                        var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                    }
            }
            if (spfold)
            {
                if (File.Exists(path + "/" + str2))
                {
                    spfold = TestAtr(path + "/" + str2);
                    if (spfold && MessageBox.Show(string.Format("Replace the \"{0}\\{1}\"?", path, str2), string.Empty,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
                    {
                        stringList.Clear();
                        return true;
                    }
                }
                if (spfold)
                    try
                    {
                        File.WriteAllLines(path + "/" + str2, stringList);
                    }
                    catch (Exception ex)
                    {
                        spfold = false;
                        var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                    }
            }
            if (!spfold)
            {
                if (File.Exists(appDir + "/" + str2))
                    if (TestAtr(appDir + "/" + str2))
                    {
                        if (MessageBox.Show(string.Format("Replace the \"{0}\\{1}\"?", appDir, str2), string.Empty,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                            System.Windows.Forms.DialogResult.OK)
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
                try
                {
                    File.WriteAllLines(appDir + "/" + str2, stringList);
                }
                catch (Exception ex)
                {
                    stringList.Clear();
                    var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
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
                var fileAttributes = FileAttributes.Normal;
                File.SetAttributes(path, fileAttributes);
            }
            catch (Exception ex)
            {
                var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                Description = "Please select the folder where Unity.exe is...",
                ShowNewFolderButton = false
            };
            if (Directory.Exists(UnityPath.Text))
                folderBrowserDialog.SelectedPath = UnityPath.Text;
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            UnityPath.Text = folderBrowserDialog.SelectedPath;
            try
            {
                Directory.SetCurrentDirectory(folderBrowserDialog.SelectedPath);
                if (!File.Exists(folderBrowserDialog.SelectedPath + "/Unity.exe"))
                    throw new IOException("Application not found!");
                appPath = folderBrowserDialog.SelectedPath + "/Unity.exe";
                if (FileVersionInfo.GetVersionInfo(appPath).FileVersion.Substring(0, 1) != "2")
                {
                    VersionText.Text = FileVersionInfo.GetVersionInfo(appPath).FileVersion.Substring(0, 5);
                    RegistBtn.IsEnabled = true;
                    var versionInfo = FileVersionInfo.GetVersionInfo(appPath);
                    patchAs = int.Parse(versionInfo.FileVersion[0].ToString()) * 100;
                    patchAs = patchAs + int.Parse(versionInfo.FileVersion[2].ToString()) * 10;
                    patchAs = patchAs + int.Parse(versionInfo.FileVersion[4].ToString());
                }
                else
                {
                    VersionText.Text = FileVersionInfo.GetVersionInfo(appPath).FileVersion.Substring(0, 8);
                    RegistBtn.IsEnabled = true;
                    patchAs = 2017;
                }
            }
            catch (Exception ex)
            {
                var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void RegistBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UnityPath.Text.Length < 3)
                UnityPath.Text = Application.StartupPath;
            var text = UnityPath.Text;
            try
            {
                if (Process.GetProcessesByName("unity").Length != 0)
                    throw new Exception("Need to close application first.");
            }
            catch (Exception ex)
            {
                var num = (int) MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            File.Copy(text + "/Unity.exe", text + "/Unity.exe.bak", true);
            NLogger.Clear();
            var se1 = "";
            var rep1 = "";
            var se2 = "";
            var rep2 = "";
            if (patchAs < 530)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 40 57 48 83 EC 30 80 79 ?? ?? 48 8B F9 75 ??";
                rep2 = "CC B0 01 C3 90 90 90 80 79 ?? ?? 48 8B F9 75 ??";
            }
            if (patchAs >= 530)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 40 57 48 83 EC 30 80 79 ?? ?? 48 8B F9 75 ??";
                rep2 = "CC B0 01 C3 90 90 90 80 79 ?? ?? 48 8B F9 75 ??";
            }
            var patcher = new Patcher(text + "/Unity.exe");
            if (patcher.AddString(se1, rep1, 1U, 0U))
            {
                if (patcher.Patch())
                {
                    NLogger.LastMessage();
                    WriteLicenseToFile(text, true, patchAs);
                    return;
                }
                if (patchAs >= 500)
                {
                    patcher.Patterns.Clear();
                    if (patcher.AddString(se2, rep2, 1U, 0U) && patcher.Patch())
                    {
                        NLogger.LastMessage();
                        WriteLicenseToFile(text, true, patchAs);
                        return;
                    }
                }
            }
            NLogger.LastMessage();
        }
    }
}