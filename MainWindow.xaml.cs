﻿using System;
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
using Utils;

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
            gbCH01.IsEnabled = true;
            gbCH02.IsEnabled = true;
            gbCH03.IsEnabled = true;
            gbCH04.IsEnabled = true;

            gbCH01.cbCHXX.IsEnabled = true;
            gbCH02.cbCHXX.IsEnabled = true;
            gbCH03.cbCHXX.IsEnabled = true;
            gbCH04.cbCHXX.IsEnabled = true;
        }

        private void DisableCheckValuesOnAllChannels()
        {
            gbCH01.cbCHXX.IsChecked = false;
            gbCH02.cbCHXX.IsChecked = false;
            gbCH03.cbCHXX.IsChecked = false;
            gbCH04.cbCHXX.IsChecked = false;
        }
        private void DisableChannelEdition()
        {
            gbCH01.DisableChannelEdition();
            gbCH02.DisableChannelEdition();
            gbCH03.DisableChannelEdition();
            gbCH04.DisableChannelEdition();
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

        private void OnSetCurrentOnCh01(object sender, RoutedEventArgs e)
        {
            driver.BasicOperation.SelectedChannel = Output.Channel1;

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.PrevTargetVoltage, gbCH01.TargetVoltage, gbCH01.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.PrevTargetCurrent, gbCH01.TargetCurrent, gbCH01.Delta));

            if (!bChangedVoltage && !bChangedCurrent)
            {
                return;
            }

            if (bChangedVoltage)
            {
                driver.VoltageAndCurrent.OutputVoltageLevel = gbCH01.TargetVoltage;
                gbCH01.PrevTargetVoltage = gbCH01.TargetVoltage;
            }

            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = gbCH01.TargetCurrent;
                gbCH01.PrevTargetCurrent= gbCH01.TargetCurrent;
            }
            driver.BasicOperation.SelectedChannel = Output.Channel1;
        }

        private void OnReadVoltageAndCurrentOnCH01(object sender, RoutedEventArgs e)
        {
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH01.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH01.Delta));

            if(!bChangedVoltage) 
            {
                gbCH01.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                gbCH01.UpdateSelectedVoltage();

            }
            if (bChangedCurrent)
            {
                gbCH01.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCH01.UpdateSelectedCurrent();
            }

        }
    }

    public enum eCHANNEL : int
    {
        CH01 = 0,
        CH02 = 1,
        CH03 = 2,
        CH04 = 3,
        None = -1,
    }
}
