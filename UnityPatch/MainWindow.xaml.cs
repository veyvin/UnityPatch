using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Reference = System.Security.Cryptography.Xml.Reference;

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
       
    
            byteList1.AddRange(Encoding.ASCII.GetBytes(string.Format("{0}-{1}", str1, "NUUN")));
            if (version != 20172)
            {
                List<string> stringList = new List<string>()
        {
          "<root>",
          "  <TimeStamp2 Value=\"cn/lkLOZ3vFvbQ==\"/>",
          "  <TimeStamp Value=\"jWj8PXAeZMPzUw==\"/>",
          "  <License id=\"Terms\">",
          "    <ClientProvidedVersion Value=\"\"/>",
          string.Format("    <DeveloperData Value=\"{0}\"/>", (object) Convert.ToBase64String(byteList1.ToArray())),
          "    <Features>"
        };

                foreach (var num in LicHeader.ReadAll())
                    stringList.Add(string.Format("      <Feature Value=\"{0}\"/>", (object)num));
                stringList.Add("    </Features>");
                if (version < 500)
                    stringList.Add("    <LicenseVersion Value=\"4.x\"/>");
                if (version >= 500 && version < 2017)
                    stringList.Add("    <LicenseVersion Value=\"5.x\"/>");
                if (version == 2017)
                    stringList.Add("    <LicenseVersion Value=\"2017.x\"/>");
                if (version == 20171)
                    stringList.Add("    <LicenseVersion Value=\"6.x\"/>");
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

                string str2 = "";
                if (version < 500)
                    str2 = "Unity_v4.x.ulf";
                if (version >= 500 && version < 2017)
                    str2 = "Unity_v5.x.ulf";
                if (version == 2017)
                    str2 = "Unity_v2017.x.ulf";
                if (version == 20171)
                    str2 = "Unity_lic.ulf";

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
                            var num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
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
                    {
                        try
                        {
                            if (version < 500)
                                str2 = "Unity_v4.x.ulf";
                            if (version >= 500 && version < 2017)
                                str2 = "Unity_v5.x.ulf";
                            if (version == 2017)
                                str2 = "Unity_v2017.x.ulf";
                            if (version == 20171)
                                str2 = "Unity_lic.ulf";
                            if (str2 == "Unity_lic.ulf")
                            {
                                using (FileStream fileStream = new FileStream(path + "/" + str2, FileMode.Append))
                                {
                                    foreach (object obj in stringList)
                                    {
                                        byte[] bytes = Encoding.ASCII.GetBytes(string.Format("{0}\r", obj));
                                        fileStream.Write(bytes, 0, bytes.Length);
                                    }
                                    fileStream.Flush();
                                    fileStream.Close();
                                }
                            }
                            else
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
                        if (version < 500)
                            str2 = "Unity_v4.x.ulf";
                        if (version >= 500 && version < 2017)
                            str2 = "Unity_v5.x.ulf";
                        if (version == 2017)
                            str2 = "Unity_v2017.x.ulf";
                        if (version == 20171)
                            str2 = "Unity_lic.ulf";
                        if (str2 == "Unity_lic.ulf")
                        {
                            using (FileStream fileStream = new FileStream(path + "/" + str2, FileMode.Append))
                            {
                                foreach (object obj in stringList)
                                {
                                    byte[] bytes = Encoding.ASCII.GetBytes(string.Format("{0}\r", obj));
                                    fileStream.Write(bytes, 0, bytes.Length);
                                }
                                fileStream.Flush();
                                fileStream.Close();
                            }
                        }
                        else
                            File.WriteAllLines(path + "/" + str2, (IEnumerable<string>)stringList);
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
            string s = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", "U3", "AAAA", "AAAA", "AAAA", "AAAA","AAAA");
            int[] numArray = LicHeader.ReadAll();
            if (s.Length != 27)
            {
                int num = (int)MessageBox.Show("Invalid Key must be \"27\" chars.", string.Empty, MessageBoxButtons.OK);
                return false;
            }
            string path2 = "Unity_lic.ulf";
            string str3 = s.Remove(s.Length - 4, 4) + "XXXX";
            string base64String = Convert.ToBase64String(((IEnumerable<byte>)((IEnumerable<byte>)new byte[4]
            {
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 0
            }).Concat<byte>((IEnumerable<byte>)Encoding.ASCII.GetBytes(s)).ToArray<byte>()).ToArray<byte>());
            string str4 = "6.x";
            string str5 = "false";
            string str6 = "";
            DateTime dateTime = DateTime.UtcNow;
            dateTime = dateTime.AddDays(-1.0);
            string str7 = dateTime.ToString("s", (IFormatProvider)CultureInfo.InvariantCulture);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string str8 = "";
            string str9 = "";
            string str10 = "";
            string str11 = "";
            dateTime = DateTime.UtcNow;
            dateTime = dateTime.AddYears(10);
            string str12 = dateTime.ToString("s", (IFormatProvider)CultureInfo.InvariantCulture);
            MemoryStream memoryStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                OmitXmlDeclaration = true,
                Encoding = Encoding.ASCII
            };
            using (XmlWriter xmlWriter = XmlWriter.Create((Stream)memoryStream, settings))
            {
                xmlWriter.WriteStartElement("root");
                xmlWriter.WriteStartElement("License");
                xmlWriter.WriteAttributeString("id", "Terms");
                xmlWriter.WriteStartElement("AlwaysOnline");
                xmlWriter.WriteAttributeString("Value", str5);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("ClientProvidedVersion");
                xmlWriter.WriteAttributeString("Value", str6);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("DeveloperData");
                xmlWriter.WriteAttributeString("Value", base64String);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Features");
                foreach (int num in numArray)
                {
                    xmlWriter.WriteStartElement("Feature");
                    xmlWriter.WriteAttributeString("Value", num.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteFullEndElement();
                xmlWriter.WriteStartElement("InitialActivationDate");
                xmlWriter.WriteAttributeString("Value", str7);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("LicenseVersion");
                xmlWriter.WriteAttributeString("Value", str4);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("MachineBindings");
                foreach (KeyValuePair<string, string> keyValuePair in dictionary)
                {
                    xmlWriter.WriteStartElement("Binding");
                    xmlWriter.WriteAttributeString("Key", keyValuePair.Key);
                    xmlWriter.WriteAttributeString("Value", keyValuePair.Value);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteFullEndElement();
                xmlWriter.WriteStartElement("MachineID");
                xmlWriter.WriteAttributeString("Value", str8);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("SerialHash");
                xmlWriter.WriteAttributeString("Value", str9);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("SerialMasked");
                xmlWriter.WriteAttributeString("Value", str3);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("StartDate");
                xmlWriter.WriteAttributeString("Value", str10);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("StopDate");
                xmlWriter.WriteAttributeString("Value", str11);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("UpdateDate");
                xmlWriter.WriteAttributeString("Value", str12);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
            }
            memoryStream.Position = 0L;
            XmlDocument document = new XmlDocument()
            {
                PreserveWhitespace = true
            };
            document.Load((Stream)memoryStream);
            SignedXml signedXml = new SignedXml(document)
            {
                SigningKey = (AsymmetricAlgorithm)new RSACryptoServiceProvider()
            };
            Reference reference = new Reference()
            {
                Uri = "#Terms"
            };
            reference.AddTransform((System.Security.Cryptography.Xml.Transform)new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();
            StringBuilder output = new StringBuilder();
            using (XmlWriter w = XmlWriter.Create(output, settings))
            {
                XmlDocument xmlDocument1 = new XmlDocument();
                string innerXml = document.InnerXml;
                xmlDocument1.InnerXml = innerXml;
                XmlDocument xmlDocument2 = xmlDocument1;
                XmlElement documentElement = xmlDocument2.DocumentElement;
                if (documentElement != null)
                {
                    XmlNode newChild = xmlDocument2.ImportNode((XmlNode)signedXml.GetXml(), true);
                    documentElement.AppendChild(newChild);
                }
                xmlDocument2.Save(w);
                w.Flush();
            }
            string contents = output.Replace(" />", "/>").ToString();
            string str13 = spfold ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Unity") : appDir;
            string path1 = Path.Combine(str13, path2);
            try
            {
                Directory.CreateDirectory(str13);
                if (File.Exists(path1) && this.TestAtr(path1) && MessageBox.Show(string.Format("Replace the \"{0}\"?", (object)path1), string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
                    return true;
                File.WriteAllText(path1, contents);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
                return false;
            }
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
                var num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK);
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

                    char ch = versionInfo.FileVersion[0];
                    this.patchAs = int.Parse(ch.ToString()) * 100;
                    int patchAs1 = this.patchAs;
                    ch = versionInfo.FileVersion[2];
                    int num1 = int.Parse(ch.ToString()) * 10;
                    this.patchAs = patchAs1 + num1;
                    int patchAs2 = this.patchAs;
                    ch = versionInfo.FileVersion[4];
                    int num2 = int.Parse(ch.ToString());
                    this.patchAs = patchAs2 + num2;
                    //patchAs = int.Parse(versionInfo.FileVersion[0].ToString()) * 100;
                    //patchAs = patchAs + int.Parse(versionInfo.FileVersion[2].ToString()) * 10;
                    //patchAs = patchAs + int.Parse(versionInfo.FileVersion[4].ToString());
                }
                else
                {
                    VersionText.Text = FileVersionInfo.GetVersionInfo(appPath).FileVersion.Substring(0, 8);
                    RegistBtn.IsEnabled = true;
                    int num = int.Parse(FileVersionInfo.GetVersionInfo(this.appPath).FileVersion.Substring(5, 1));
                    this.patchAs = num <= 0 ? 2017 : (num != 1 ? 20172 : 20171);
                }
            }
            catch (Exception ex)
            {
                var num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
                var num = (int)MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
            if (patchAs >= 530&&patchAs<20172)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 40 57 48 83 EC 30 80 79 ?? ?? 48 8B F9 75 ??";
                rep2 = "CC B0 01 C3 90 90 90 80 79 ?? ?? 48 8B F9 75 ??";
            }
            if (this.patchAs >= 20172)
            {
                se1 = "CC 55 8B EC 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                rep1 = "CC B0 01 C3 83 EC ?? 53 56 8B F1 80 7E ?? ?? 57 75 ??";
                se2 = "CC 48 89 5C 24 10 48 89 6C 24 18 56 41 54 41 55 ?? 83 EC 30 ?? 8B E9";
                rep2 = "CC B0 01 C3 90 90 48 89 6C 24 18 56 41 54 41 55 ?? 83 EC 30 ?? 8B E9";
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
                    if (patcher.AddString(se2, rep2, 1U, 0U)&&patcher.Patch())
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