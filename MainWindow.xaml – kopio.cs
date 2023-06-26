using System;
using System.IO;
using System.Collections.Generic;
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
using System.Xml.Linq;
using System.Diagnostics;

namespace TaskGenerator
{

    public class PDV_ELEMENTS
    {
        public PDV_ELEMENTS(string GML)
        {
            GMLelement = GML;
            ISOElement = "";
            scale = 1.0;
        }

        public string GMLelement { get; set; }
        public string ISOElement { get; set; }
        public double scale { get; set; }
    }

    public class LOG_ELEMENTS
    {
        public LOG_ELEMENTS(string ISO)
        {
            Element = ISO;
            TimeInterval = 0;
            DistanceInterval = 0;
            ThresholdMin = 2147483647;
            ThresholdMax = -2147483647;
            ThresholdChange = 0;

            time = false;
            distance = false;
            limits = false;
            change = false;
        }

        public string Element { get; set; }
        public long TimeInterval { get; set; }
        public long DistanceInterval { get; set; }
        public long ThresholdMin { get; set; }
        public long ThresholdMax { get; set; }
        public long ThresholdChange { get; set; }

        public bool time { get; set; }
        public bool distance { get; set; }
        public bool limits { get; set; }
        public bool change { get; set; }
        
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<PDV_ELEMENTS> PDVelements { get; set; }
        public List<string> ISOElementNames { get; set; }

        public List<LOG_ELEMENTS> LOGelements { get; set; }

        public String CMDConvertTSK;
        public String CMDConvertDVC;
        public String CMDConvertPFD;
        public String CMDConvertTZN;

        string[] RecommendedDDIList = new string[] { "1", "0001", "2", "0002", "6", "0006", "7", "0007", "B", "000B", "b", "000b", "C", "000C", "c", "000c", "10", "0010", "11", "0011", "54", "0054", "B5", "00B5", "b5", "00b5" };
        Dictionary<string, string> genericDDIList = new Dictionary<string, string> { { "1", "0001" }, { "2", "0002" }, { "6", "0006" }, { "7", "0007" }, { "B", "000B" }, { "C", "000C" }, { "10", "0010" }, { "11", "0011" }, { "54", "0054" }, { "B5", "00B5" } };


        public MainWindow()
        {
            PDVelements = new List<PDV_ELEMENTS>();
            ISOElementNames = new List<string>();
            LOGelements = new List<LOG_ELEMENTS>();
            
            InitializeComponent();

            TZN_PDV_grid.ItemsSource = PDVelements;
            ISOElement.ItemsSource = ISOElementNames;

            LOG_grid.ItemsSource = LOGelements;
        }

        private void Device_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                DeviceFile.Text = openFileDialog.FileName;

                recommendedDDIs_Checked(sender, e);


                XDocument Device;
                try
                {
                    Device = XDocument.Load(DeviceFile.Text);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                LOGelements.Clear();
                foreach (var DVC in Device.Descendants("DVC"))
                {
                    foreach (var DET in DVC.Descendants("DET"))
                    {
                        foreach (var DOR in DET.Descendants("DOR"))
                        {
                            foreach (var DPD in DVC.Descendants("DPD").Where(dpd => dpd.Attribute("A").Value == DOR.Attribute("A").Value && (int.Parse(dpd.Attribute("D").Value) & (int)15) > (int)0))
                            {
                                LOG_ELEMENTS element = new LOG_ELEMENTS(DVC.Attribute("B").Value + "->" + DET.Attribute("D").Value + "->" + DPD.Attribute("E").Value);

                                if((int.Parse(DPD.Attribute("D").Value) & (int)1) == (int)1)
                                    element.time = true;
                                if((int.Parse(DPD.Attribute("D").Value) & (int)2) == (int)2)
                                    element.distance = true;
                                if((int.Parse(DPD.Attribute("D").Value) & (int)4) == (int)4)
                                    element.limits = true;
                                if((int.Parse(DPD.Attribute("D").Value) & (int)8) == (int)8)
                                    element.change = true;

                                LOGelements.Add(element);
                            }
                        }
                    }
                }

                
                LOG_grid.Items.Refresh();
            }
        }

        private void recommendedDDIs_Checked(object sender, RoutedEventArgs e)
        {
            ISOElementNames.Clear();
            ISOElementNames.Add("  ");

            if (!String.IsNullOrEmpty(DeviceFile.Text))
            {
                XDocument Device;
                try
                {
                    Device = XDocument.Load(DeviceFile.Text);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                foreach (var DVC in Device.Descendants("DVC"))
                {
                    foreach (var DET in DVC.Descendants("DET"))
                    {
                        foreach (var DOR in DET.Descendants("DOR"))
                        {
                            foreach (var DPD in DVC.Descendants("DPD").Where(dpd => dpd.Attribute("A").Value == DOR.Attribute("A").Value && (int.Parse(dpd.Attribute("C").Value) & (int)2) == (int)2))
                            {
                                if (recommendedDDIs != null && recommendedDDIs.IsChecked == true)
                                {
                                    if (RecommendedDDIList.Any(s => DPD.Attribute("B").Value.Equals(s)))
                                    {
                                        ISOElementNames.Add(DVC.Attribute("B").Value + "->" + DET.Attribute("D").Value + "->" + DPD.Attribute("E").Value);
                                    }
                                }
                                else
                                {
                                    ISOElementNames.Add(DVC.Attribute("B").Value + "->" + DET.Attribute("D").Value + "->" + DPD.Attribute("E").Value);
                                }
                            }
                        }
                    }
                }
            }

            if (genericDDIs != null && genericDDIs.IsChecked == true)
            {
                foreach(var DDI in genericDDIList.Keys)
                {
                    ISOElementNames.Add(DDI);
                }

            }                       
        }

        private void Partfield_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GML files (*.gml)|*.gml|XML files (*.xml)|*.xml";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                PartfieldFile.Text = openFileDialog.FileName;

                if (PartfieldFile.Text.EndsWith(".gml", true, null))
                {
                    XDocument GMLFields;
                    try
                    {
                        GMLFields = XDocument.Load(openFileDialog.FileName);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    var namespaces = GMLFields.Root.Attributes().
                            Where(a => a.IsNamespaceDeclaration).
                            GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                                    a => XNamespace.Get(a.Value)).
                            ToDictionary(g => g.Key,
                                         g => g.First().ToString());


                    PFDNamespace.IsEnabled = true;
                    PFDNamespace.Items.Clear();
                    foreach (var ns in namespaces)
                    {
                        PFDNamespace.Items.Add(ns);
                        if (!ns.Key.Equals("gml"))
                            PFDNamespace.SelectedIndex = PFDNamespace.Items.Count - 1;
                    }
                }
                else if(PartfieldFile.Text.EndsWith(".xml",true,null))
                {
                    // ready ISO xml file
                    PFDNamespace.Items.Clear();
                    PFDNamespace.IsEnabled = false;


                    XDocument PFDZones;
                    try
                    {
                        PFDZones = XDocument.Load(PartfieldFile.Text);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    var ElementNames = PFDZones.Descendants("PFD")
                                        .Select(elem => elem.Attribute("C").Value)
                                        .ToList();

                    Partfield.IsEnabled = true;
                    Partfield.Items.Clear();
                    foreach (var name in ElementNames)
                    {
                        Partfield.Items.Add(name);
                    }
                }
            }

            UpdateCommands();
        }

        private void PFDNamespace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PFDNamespace.SelectedItem == null)
            {
                PFDCode.Items.Clear();
                PFDDesignator.Items.Clear();

                PFDCode.IsEnabled = false;
                PFDDesignator.IsEnabled = false;

                UpdateCommands();
                return;
            }

            XDocument PFDZones;
            try
            {
                PFDZones = XDocument.Load(PartfieldFile.Text);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            XNamespace usr = ((KeyValuePair<string, string>)PFDNamespace.SelectedItem).Value;


            // Find elements for this namespace

            var ElementNames = PFDZones.Descendants()
                                .Where(elem => elem.Name.Namespace.Equals(usr))
                                .Select(elem => elem.Name.LocalName.ToString())
                                .ToList()
                                .Distinct();

            PFDCode.IsEnabled = true;
            PFDDesignator.IsEnabled = true;

            PFDCode.Items.Clear();
            PFDDesignator.Items.Clear();
            foreach (var name in ElementNames)
            {
                PFDCode.Items.Add(name);
                PFDDesignator.Items.Add(name);
            }

            UpdateCommands();
        }

        private void PFDCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCommands();
        }

        private void PFDDesignator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PFDDesignator.SelectedItem == null)
            {
                Partfield.Items.Clear();
                Partfield.IsEnabled = false;
                return;
            }
                


            XDocument PFDZones;
            try
            {
                PFDZones = XDocument.Load(PartfieldFile.Text);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
            XNamespace usr = ((KeyValuePair<string, string>)PFDNamespace.SelectedItem).Value;

            
            var ElementNames = PFDZones.Descendants()
                                .Where(elem => elem.Name.Equals(usr + PFDDesignator.SelectedItem.ToString()))
                                .Select(elem => elem.Value)
                                .ToList()
                                .Distinct();

            Partfield.IsEnabled = true;
            Partfield.Items.Clear();
            foreach (var name in ElementNames)
            {
                Partfield.Items.Add(name);
            }

            UpdateCommands();
        }

        private void Treatmentzone_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GML files (*.gml)|*.gml";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                TreatmentzoneFile.Text = openFileDialog.FileName;

                XDocument GMLZones;
                try
                {
                    GMLZones = XDocument.Load(openFileDialog.FileName);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                var namespaces = GMLZones.Root.Attributes().
                        Where(a => a.IsNamespaceDeclaration).
                        GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                                a => XNamespace.Get(a.Value)).
                        ToDictionary(g => g.Key,
                                     g => g.First().ToString());


                TZNNamespace.IsEnabled = true;
                TZNNamespace.Items.Clear();
                foreach (var ns in namespaces)
                {
                    TZNNamespace.Items.Add(ns);
                    if (!ns.Key.Equals("gml"))
                        TZNNamespace.SelectedIndex = TZNNamespace.Items.Count-1;
                }
                
            }

            UpdateCommands();
        }

        private void TZNNamespace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TZNNamespace.SelectedItem == null)
            {
                PDVelements.Clear();
                UpdateCommands();

                return;
            }

            XDocument GMLZones;
            try
            {
                GMLZones = XDocument.Load(TreatmentzoneFile.Text);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            XNamespace usr = ((KeyValuePair<string, string>)TZNNamespace.SelectedItem).Value;


            // Find elements for this namespace

            var ElementNames = GMLZones.Descendants()
                    .Where(elem => elem.Name.Namespace.Equals(usr))
                    .Select(elem => elem.Name.LocalName.ToString())
                    .ToList()
                    .Distinct();

            PDVelements.Clear();
            foreach (var name in ElementNames)
            {
                PDVelements.Add(new PDV_ELEMENTS(name));
            }

            TZN_PDV_grid.Items.Refresh();

            UpdateCommands();
        }

        private void TZN_PDV_grid_CurrentCellChanged(object sender, EventArgs e)
        {
            UpdateCommands();
        }

        private void DeviceElementSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            UpdateCommands();
        }

        private void Select_Output_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.FileName = "TASKDATA.XML";
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == true)
            {
                OutputDirectory.Text = saveFileDialog.FileName;

                FarmName.IsEnabled = true;
                TaskDesignator.IsEnabled = true;

                TaskDesignator.Text = System.IO.Path.GetFileName(TreatmentzoneFile.Text).Replace(".gml", "").Replace(".GML", "");


                UpdateCommands();
            }
        }

        private void FarmName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCommands();
        }

        private void TaskDesignator_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCommands();
        }

        private void Partfield_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCommands();
        }

        private void CreateOneFile_Checked(object sender, RoutedEventArgs e)
        {
            if(CreateOneFile.IsChecked == false)
            {
                RemoveExcessData.IsChecked = false;
                RemoveExcessData.IsEnabled = false;
            }
            else
            {
                RemoveExcessData.IsEnabled = true;
            }

            UpdateCommands();
        }

        private void RemoveExcessData_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCommands();
        }

        private void UpdateCommands()
        {
            string directory = System.AppDomain.CurrentDomain.BaseDirectory;
            string taskfile = "TASKDATA.XML";

            if (!String.IsNullOrEmpty(OutputDirectory.Text))
            {
                directory = System.IO.Path.GetDirectoryName(OutputDirectory.Text) + "\\";
                taskfile = OutputDirectory.Text;
            }


            CMDConvertTSK = "GML_ISO_Converter -type=TSK -output=\"" + taskfile + "\"";

            if (!String.IsNullOrEmpty(FarmName.Text))
            {
                CMDConvertTSK += " -ELM=FRM:{A:FRM1,B:\"" + FarmName.Text + "\"}";
            }


            if (!String.IsNullOrEmpty(DeviceFile.Text))
            {
                CMDConvertDVC = "copy \"" + DeviceFile.Text + "\" \"" + directory + "DVC00001.XML\" /Y";
                CMDConvertTSK += " -ELM=XFR:{DVC00001.XML}";
            }
            else
            {
                CMDConvertDVC = "";
            }

            if (!String.IsNullOrEmpty(PartfieldFile.Text))
            {
                if (PartfieldFile.Text.EndsWith(".gml", true, null))
                {
                    CMDConvertPFD = "GML_ISO_Converter -type=PFD -input=\"" + PartfieldFile.Text + "\"";
                    CMDConvertPFD += " -output=\"" + directory + "PFD00001.XML\"";
                    CMDConvertTSK += " -ELM=XFR:{PFD00001.XML}";

                    if (PFDNamespace.SelectedItem != null)
                        CMDConvertPFD += " -namespace=\"" + ((KeyValuePair<string, string>)PFDNamespace.SelectedItem).Value.ToString() + "\"";

                    if (PFDCode.SelectedValue != null)
                        CMDConvertPFD += " -PFD:B=\"" + PFDCode.SelectedValue.ToString() + "\"";

                    if (PFDDesignator.SelectedValue != null)
                        CMDConvertPFD += " -PFD:C=\"" + PFDDesignator.SelectedValue.ToString() + "\"";
                }
                else
                {
                    CMDConvertPFD = "copy \"" + PartfieldFile.Text + "\" \"" + directory + "PFD00001.XML\" /Y";
                }
            }
            else
            {
                CMDConvertPFD = "";
            }


            if (!String.IsNullOrEmpty(TreatmentzoneFile.Text))
            {
                CMDConvertTZN = "GML_ISO_Converter -type=TZN -input=\"" + TreatmentzoneFile.Text + "\"";
                CMDConvertTZN += " -output=\"" + directory + "TSK00001.XML\"";
                CMDConvertTSK += " -ELM=XFR:{TSK00001.XML}";

                if (TZNNamespace.SelectedItem != null)
                    CMDConvertTZN += " -namespace=\"" + ((KeyValuePair<string, string>)TZNNamespace.SelectedItem).Value.ToString() + "\"";



                try
                {
                    XDocument Device = new XDocument();
                    if (!String.IsNullOrEmpty(DeviceFile.Text))
                    {
                        Device = XDocument.Load(DeviceFile.Text);
                    }

                    foreach (var item in PDVelements)
                    {
                        if (!String.IsNullOrEmpty(item.ISOElement))
                        {
                            if (item.ISOElement.Contains("->"))
                            {
                                string[] elements = item.ISOElement.Split("->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                foreach (var DVC in Device.Descendants("DVC").Where(dvc => dvc.Attribute("B").Value == elements[0]))
                                {
                                    foreach (var DET in DVC.Descendants("DET").Where(det => det.Attribute("D").Value == elements[1]))
                                    {
                                        foreach (var DOR in DET.Descendants("DOR"))
                                        {
                                            foreach (var DPD in DVC.Descendants("DPD").Where(dpd => dpd.Attribute("A").Value == DOR.Attribute("A").Value && dpd.Attribute("E").Value == elements[2]))
                                            {
                                                CMDConvertTZN += " -PDV=" + item.GMLelement + ":{A:" + DPD.Attribute("B").Value + ",D:" + DET.Attribute("A").Value + ",scale:" + item.scale + "}";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CMDConvertTZN += " -PDV=" + item.GMLelement + ":{A:" + genericDDIList[item.ISOElement] + ",scale:" + item.scale + "}";
                            }
                        }
                    }
                }
                catch (System.IO.FileNotFoundException ex)
                {

                }
                catch (System.InvalidOperationException e)
                {
                    MessageBox.Show("Device description exception! \n" + e.ToString());
                }

                if (!String.IsNullOrEmpty(TaskDesignator.Text))
                {
                    CMDConvertTZN += " -ATR=B:\"" + TaskDesignator.Text + "\"";
                }

                if (!String.IsNullOrEmpty(FarmName.Text))
                {
                    CMDConvertTZN += " -ATR=D:FRM1";
                }

                if (Partfield.SelectedIndex >= 0)
                {
                    CMDConvertTZN += " -ATR=E:PFD" + (Partfield.SelectedIndex + 1);
                }

                if (!String.IsNullOrEmpty(DeviceFile.Text))
                {
                    XDocument Device;
                    try
                    {
                        Device = XDocument.Load(DeviceFile.Text);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    var devices = Device.Descendants("DVC");

                    foreach (var DVC in devices)
                    {
                        CMDConvertTZN += " -ELM=DAN:{A:\"" + DVC.Attribute("D").Value + "\",C:\"" + DVC.Attribute("A").Value + "\"}";
                    }

                    foreach (var item in LOGelements)
                    {
                        if (!String.IsNullOrEmpty(item.Element))
                        {
                            if (item.TimeInterval > 0 || item.DistanceInterval > 0 || item.ThresholdMin < 2147483647 || item.ThresholdMax > -2147483647 || item.ThresholdChange > 0)
                            {
                                string[] elements = item.Element.Split("->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                foreach (var DVC in Device.Descendants("DVC").Where(dvc => dvc.Attribute("B").Value == elements[0]))
                                {
                                    foreach (var DET in DVC.Descendants("DET").Where(det => det.Attribute("D").Value == elements[1]))
                                    {
                                        foreach (var DOR in DET.Descendants("DOR"))
                                        {
                                            foreach (var DPD in DVC.Descendants("DPD").Where(dpd => dpd.Attribute("A").Value == DOR.Attribute("A").Value && dpd.Attribute("E").Value == elements[2]))
                                            {
                                                CMDConvertTZN += " -ELM=DLT:{A:" + DPD.Attribute("B").Value + ",H:" + DET.Attribute("A").Value;
                                                long method = 0;

                                                if (item.TimeInterval > 0)
                                                {
                                                    CMDConvertTZN += ",D:" + item.TimeInterval;
                                                    method += 1;
                                                }
                                                if (item.DistanceInterval > 0)
                                                {
                                                    CMDConvertTZN += ",C:" + item.DistanceInterval;
                                                    method += 2;
                                                }
                                                if (item.ThresholdMin < 2147483647 && item.ThresholdMax > -2147483647)
                                                {
                                                    CMDConvertTZN += ",E:" + item.ThresholdMin;
                                                    CMDConvertTZN += ",F:" + item.ThresholdMax;
                                                    method += 4;
                                                }
                                                if (item.ThresholdChange > 0)
                                                {
                                                    CMDConvertTZN += ",D:" + item.ThresholdChange;
                                                    method += 8;
                                                }

                                                CMDConvertTZN += ",B:" + method + "}";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                CMDConvertTZN = "";
            }


            if (CreateOneFile != null && CreateOneFile.IsChecked == true)
            {
                CMDConvertTSK += " -merge=true";
            }

            if (RemoveExcessData != null && RemoveExcessData.IsChecked == true)
            {
                CMDConvertTSK += " -shrink=true";
            }

            Command.Text = CMDConvertDVC + "\n" + CMDConvertPFD + "\n" + CMDConvertTZN + "\n" + CMDConvertTSK;
        }
        
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;  
            cmd.Start();

            if(!String.IsNullOrEmpty(CMDConvertDVC))
                cmd.StandardInput.WriteLine(CMDConvertDVC + "\n");

            if (!String.IsNullOrEmpty(CMDConvertPFD))
                cmd.StandardInput.WriteLine(CMDConvertPFD);

            if (!String.IsNullOrEmpty(CMDConvertTZN))
                cmd.StandardInput.WriteLine(CMDConvertTZN);

            if (!String.IsNullOrEmpty(CMDConvertTSK))
                cmd.StandardInput.WriteLine(CMDConvertTSK);

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();


            MessageBox.Show(cmd.StandardOutput.ReadToEnd());
        }
    }
}
