using System;
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
using System.Collections.ObjectModel;

using Bobs_Parser.Models;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace Bobs_Parser
{
    /// <summary>
    /// Lazy single class main window
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<TextBox> PhraseBoxes;
        private List<Int32> PhraseCount;
        private List<Int32> PhraseCompareAgainst;
        private ObservableCollection<GridStat> DataGridContent;
        private List<ComboBox> comboBoxes;
        private Boolean IsMonitoring = false;
        private Boolean hasFileStream = false;
        private FileStream fileStream;
        private string fileMonitoring;


        private const Int16 PHRASE_BOXES_COUNT = 7;

        public MainWindow()
        {
            InitializeComponent();
            this.setup();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void startMonitoring(object sender, RoutedEventArgs e)
        {
            this.startMonitoringButton.Visibility = Visibility.Hidden;
            this.buildDataGrid();
            this.IsMonitoring = true;
            this.monitor();
            this.stopMonitoringButton.Visibility = Visibility.Visible;
            this.stopMonitoringButton.IsEnabled = true;
            this.clearCurrentStats();
        }

        private void setup()
        {
            // Quick Reference for Later
            this.PhraseBoxes = new List<TextBox>() { Phrase1_textBox, Phrase2_textBox, Phrase3_textBox, Phrase4_textBox, Phrase5_textBox, Phrase6_textBox, Phrase7_textBox, Phrase8_textBox, Phrase9_textBox, Phrase10_textBox };
            this.DataGridContent = new ObservableCollection<GridStat>();
            this.mainDataGrid.ItemsSource = this.DataGridContent;
            this.populateComboBoxes();

            this.setupInitialValues();
        }

        public async void monitor()
        {
            this.disableChanges();
            int numBytesRead = 0;
            while (this.IsMonitoring)
            {
                // poll every 1 second
                await Task.Delay(1000);
 
                // Set the stream position to the beginning of the file.
                this.fileStream.Seek(0 + numBytesRead, SeekOrigin.Begin);

                byte[] buffer = new byte[0]; // We redeclare this array when we add a byte
                // Read and verify the data.
                for (int i = 0+ numBytesRead; i < fileStream.Length; i++)
                {
                    Byte newByte = (byte)this.fileStream.ReadByte();
                    buffer = this.addByteToArray(buffer, newByte);

                    numBytesRead++;
                }

                this.processBuffer(buffer);
            }
        }

        private void processBuffer(Byte[] buffer)
        {
           
            if(buffer.Length == 0)
            {
                return;
            }
            char[] charArray = System.Text.Encoding.UTF8.GetString(buffer).ToCharArray();
            Array.Reverse(charArray);
            string fullBuffer = new String(charArray);
                
            for (int i = 0; i < this.PhraseBoxes.Count; i++)
            {
                // If the phrase is setup
                if (this.PhraseBoxes[i].Text.Length > 0)
                {
                    // Count Each Phrase and increment the found
                    int countFound = Regex.Matches(fullBuffer, this.PhraseBoxes[i].Text.ToString()).Count;
                    this.PhraseCount[i] += countFound;
                    this.updateDataGrid();
                }
            }
        }
        private void setupInitialValues()
        {
            // Initial Values
            this.PhraseCount = new List<Int32>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            this.PhraseCompareAgainst = new List<Int32>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        private void buildDataGrid()
        {
            this.DataGridContent.Clear();
            for (int i = 0; i < this.PhraseBoxes.Count; i++)
            {
                if (this.buildGridStat(i) != null)
                {
                    this.DataGridContent.Add(this.buildGridStat(i));
                }
            }
        }

        private void updateDataGrid() {
            for (int i = 0; i < this.PhraseBoxes.Count; i++)
            {
                for (int o = 0; o < this.DataGridContent.Count; o++)
                {
                    if (this.PhraseBoxes[i].Text.Length > 0 && this.PhraseBoxes[i] == this.DataGridContent[o].Source)
                    {
                        // Update the count
                        this.DataGridContent[o].Count = this.PhraseCount[i];
                        // Update the compare count
                        int total = 0;
                        switch (this.comboBoxes[i].SelectedItem.ToString())
                        {
                            case "Phrase 1":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[0];
                                break;
                            case "Phrase 2":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[1];
                                break;
                            case "Phrase 3":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[2];
                                break;
                            case "Phrase 4":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[3];
                                break;
                            case "Phrase 5":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[4];
                                break;
                            case "Phrase 6":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[5];
                                break;
                            case "Phrase 7":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[6];
                                break;
                            case "Phrase 8":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[7];
                                break;
                            case "Phrase 9":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[8];
                                break;
                            case "Phrase 10":
                                this.DataGridContent[o].ComparedCount = this.PhraseCount[9];
                                break;

                        }
                        // Update Perecent
                        if(this.DataGridContent[o].Count != 0 && this.DataGridContent[o].ComparedCount != 0)
                        {
                            decimal percentage = (decimal)this.DataGridContent[o].ComparedCount/ (decimal)this.DataGridContent[o].Count;
                            this.DataGridContent[o].OccuranceRate = String.Format("{0:P2}.", percentage);
                        }

                    }
                }
            }
            // Now update the data grid
            CollectionViewSource.GetDefaultView(this.mainDataGrid.ItemsSource).Refresh();
        }

        private GridStat buildGridStat(int index)
        {
            if (this.PhraseBoxes[index].Text.Length == 0)
            {
                return null;
            }

            GridStat gridStat = new GridStat();
            gridStat.Phrase =  (index + 1).ToString();

            gridStat.Source = this.PhraseBoxes[index];
            gridStat.Count = this.PhraseCount[index];
            gridStat.ComparedCount = this.PhraseCompareAgainst[index];

            if ((this.PhraseCount[index] == 0) || (this.PhraseCompareAgainst[index] == 0))
            {
                gridStat.OccuranceRate = "...";
            }
            else
            {
                gridStat.OccuranceRate = (this.PhraseCount[index] / (this.PhraseCount[index] + this.PhraseCompareAgainst[index])).ToString() + "%";
            }

            return gridStat;
        }
        private void populateComboBoxes()
        {
            this.comboBoxes = new List<ComboBox>() { Phrase1_ComboBox, Phrase2_ComboBox, Phrase3_ComboBox, Phrase4_ComboBox, Phrase5_ComboBox, Phrase6_ComboBox, Phrase7_ComboBox, Phrase8_ComboBox, Phrase9_ComboBox, Phrase10_ComboBox };
            for (int o = 0; o < this.PhraseBoxes.Count; o++)
            {
                for (int i = 1; i <= this.PhraseBoxes.Count; i++)
                {
                    this.comboBoxes[o].Items.Add("Phrase " + i);
                }
                this.comboBoxes[o].SelectedIndex = 0;
            }
        }
        private void disableChanges()
        {
            for(int i=0; i < this.PhraseBoxes.Count; i++)
            {
                this.PhraseBoxes[i].IsEnabled = false;
            }
            for (int i = 0; i < this.comboBoxes.Count; i++)
            {
                this.comboBoxes[i].IsEnabled = false;
            }
        }
        public void openBobsFileDialogBox(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true) {
                this.loadFile(fileDialog.FileName);
            }
        }

        public void loadFile(string fileName)
        {
            // Does the file exist
            if (!File.Exists(fileName))
            {
                MessageBox.Show(fileName + " does not exist", "Error");
                return;
            }

            try
            {
                // Read only open file non-locking
                this.fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                this.hasFileStream = true;
                this.startMonitoringButton.IsEnabled = true;
            }
            // is the file locked?
            catch (IOException e)
            {
                MessageBox.Show(fileName + " is locked", "Error");
                return;
            }
        }

        private byte[] addByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }

        private void menuExit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void menuAbout(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("For my dad and his games.\n" +
                "Made by https://github.com/bobby5892 \n" +
                "2020\n" + 
                "Version: 1.1\n");
        }
        private void clearStats(object sender, RoutedEventArgs e)
        {
            this.clearCurrentStats();
        }
        private void clearCurrentStats()
        {
            this.setupInitialValues();
            this.updateDataGrid();

            for (int i = 0; i < this.DataGridContent.Count; i++)
            {
                this.DataGridContent[i].OccuranceRate = "...";
            }
        }
        private void stopMonitoring(object sender, RoutedEventArgs e)
        {
            this.IsMonitoring = false;
            this.reEnableControls();
            this.startMonitoringButton.Visibility = Visibility.Visible;
            this.stopMonitoringButton.Visibility = Visibility.Hidden;
            this.startMonitoringButton.IsEnabled = true;
        }
        private void reEnableControls()
        {
            for (int i = 0; i < this.PhraseBoxes.Count; i++)
            {
                this.PhraseBoxes[i].IsEnabled = true;
            }
            for (int i = 0; i < this.comboBoxes.Count; i++)
            {
                this.comboBoxes[i].IsEnabled = true;
            }
        }
    }
}


