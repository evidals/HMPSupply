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
using RohdeSchwarz.Hmp4000; //If missing, add the reference from:
using Ivi.Driver;
using System.Diagnostics;
using HMPSupply.components;

namespace HMPSupply
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Hmp4000 driver;

        public MainWindow()
        {
            InitializeComponent();
            DisableChannelEdition();
            TurnOffLedIndicator();
        }

        private void buConnect_Click(object sender, RoutedEventArgs e)
        {
            tbStatus.Text = "Driver session initializing ... \n";
            try
            {
                string sLAN = "TCPIP::" + tbHost.Text + "::" + tbPort.Text + "::SOCKET";
                driver = new Hmp4000(sLAN, false, true);
                tbStatus.Text += "finished.\n";
                tbStatus.Text += "Instrument ID: " + driver.System.IDQueryResponse + "\n";
                var selftestResult = driver.System.SelfTest();
                tbStatus.Text += "Instrument Selftest: " + selftestResult.Message + "\n";
                TurnOnLedIndicator();
                EnableChannelEdition();
                DisableCheckValuesOnAllChannels();

            }
            catch (Ivi.Driver.IOException ex)
            {
                tbStatus.Text += "\nERROR: Cannot initialize the driver session.\nDetails:";
                tbStatus.Text += ex.Message;
            }
        }

        private void EnableChannelEdition()
        {
            //gbCH01.IsEnabled = true;
            gbCH02.IsEnabled = true;
            gbCH03.IsEnabled = true;
            gbCH04.IsEnabled = true;

            //cbCH01.IsEnabled = true;
            cbCH02.IsEnabled = true;
            cbCH03.IsEnabled = true;
            cbCH04.IsEnabled = true;
        }

        private void DisableCheckValuesOnAllChannels()
        {
            //cbCH01.IsChecked = false;
            //cbCH01.IsChecked = false;
            //cbCH01.IsChecked = false;
            //cbCH01.IsChecked = false;
        }
        private void DisableChannelEdition()
        {
            //gbCH01.IsEnabled = false;
            gbCH02.IsEnabled = false;
            gbCH03.IsEnabled = false;
            gbCH04.IsEnabled = false;

            //cbCH01.IsEnabled = false;
            cbCH02.IsEnabled = false;
            cbCH03.IsEnabled = false;
            cbCH04.IsEnabled = false;

            //buCH01_Read_Volt_Curr.IsEnabled = false;
            buCH02_Read_Volt_Curr.IsEnabled = false;
            buCH03_Read_Volt_Curr.IsEnabled = false;
            buCH04_Read_Volt_Curr.IsEnabled = false;

            //buCH01_Set_Volt_Curr.IsEnabled = false;
            buCH02_Set_Volt_Curr.IsEnabled = false;
            buCH03_Set_Volt_Curr.IsEnabled = false;
            buCH04_Set_Volt_Curr.IsEnabled = false;

            //gbCH01_U.IsEnabled = false;
            //gbCH01_I.IsEnabled = false;
        }

        private void TurnOffLedIndicator()
        {
            LedOnOffStatusHMP4040.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFEE6E4");
        }
        private void TurnOnLedIndicator()
        {
            LedOnOffStatusHMP4040.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF63E62C");
        }

        private void rBuCH01_1V_Copy2_Checked(object sender, RoutedEventArgs e)
        {

        }

        //private void cbCH01_Click(object sender, RoutedEventArgs e)
        //{
        //    CheckBox cb = sender as CheckBox;
        //    string checkState = string.Empty;
        //    if (!cb.IsChecked.HasValue)
        //    {
        //        checkState = "Indeterminate";
        //    }
        //    else
        //    {
        //        checkState = (true == cb.IsChecked) ? "Checked" : "Unchecked";

        //        if (!cbCH01.IsChecked.Value)
        //        {
        //            buCH01_Read_Volt_Curr.IsEnabled = false;
        //            buCH01_Set_Volt_Curr.IsEnabled = false;
        //            gbCH01_U.IsEnabled = false;
        //            gbCH01_I.IsEnabled = false;
        //        }
        //        else
        //        {
        //            buCH01_Read_Volt_Curr.IsEnabled = true;
        //            buCH01_Set_Volt_Curr.IsEnabled = true;
        //            gbCH01_U.IsEnabled = true;
        //            gbCH01_I.IsEnabled = true;
        //        }
        //    }

        //    Trace.WriteLine(string.Format("Click event on CH01: {0}", checkState));
        //}
    }
}
