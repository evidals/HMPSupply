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
using Utils;
using System.Diagnostics.Eventing.Reader;
using System.ComponentModel;
using System.Windows.Media.TextFormatting;

namespace HMPSupply
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // this enum can be bound on namespace RohdeSchwarz.Hmp4000
        Channel on_use_ch = Channel.NotUsed;
        static bool DO_NOT_RESET = false;
        Hmp4000 driver;
        static bool bOutputActive = false;
        const string CH01_NAME = "CH_01";
        const string CH02_NAME = "CH_02";
        const string CH03_NAME = "CH_03";
        const string CH04_NAME = "CH_04";
        //driver.System.SystemRemoteMix();

        public MainWindow()
        {
            InitializeComponent();
            DisableChannelEdition();
            TurnOffLedIndicator();
            DisableOutputButton();
        }

        private void buConnect_Click(object sender, RoutedEventArgs e)
        {
            tbLogStatus.Text = "Driver session initializing ... \n";
            try
            {
                string sLAN = "TCPIP::" + tbHost.Text + "::" + tbPort.Text + "::SOCKET";
                driver = new Hmp4000(sLAN, false, DO_NOT_RESET);
                tbLogStatus.Text += "finished.\n";
                tbLogStatus.Text += "Instrument ID: " + driver.System.IDQueryResponse + "\n";
                var selftestResult = driver.System.SelfTest();
                tbLogStatus.Text += "Instrument Selftest: " + selftestResult.Message + "\n";
                TurnOnLedIndicator();
                EnableChannelEdition();
                DisableCheckValuesOnAllChannels();
                EnableOutputButton();
                DisableAllSelectedChannels();
                DisableMasterOutput();
                ReadAllChannels();

            }
            catch (Ivi.Driver.IOException ex)
            {
                tbLogStatus.Text += "[ERROR] Cannot initialize the driver session.\n";
                tbLogStatus.Text += (ex.Message + "\n");
            }
            catch (Ivi.Driver.OperationNotSupportedException ex)
            {
                tbLogStatus.Text += "[ERROR] Instrument doesn't support the function:\n";
                tbLogStatus.Text += (ex.Message + "\n");
            }

            //when a task takes longer than the defined timeout
            catch (Ivi.Driver.IOTimeoutException ex)
            {
                tbLogStatus.Text += "[ERROR] Operation took longer than the maximum defined time:\n";
                tbLogStatus.Text += (ex.Message + "\n");
            }
            catch (Ivi.Driver.MaxTimeExceededException ex)
            {
                tbLogStatus.Text += "[ERROR] Operation took longer than the maximum defined time:\n";
                tbLogStatus.Text += (ex.Message + "\n");
            }
            //if the instrument returns an error in the error queue
            catch (Ivi.Driver.InstrumentStatusException ex)
            {
                tbLogStatus.Text += "[ERROR] Instrument system error occured:\n";
                tbLogStatus.Text += (ex.Message + "\n");
            }
            //if the instrument returns an error in the error queue
            catch (Ivi.Driver.SelectorNameException ex)
            {
                tbLogStatus.Text += "[ERROR] Invalid selector name used:\n";
                tbLogStatus.Text += (ex.Message + "\n");
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
            LedOnOffStatusHMP4040.Fill = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
        }
        private void TurnOnLedIndicator()
        {
            LedOnOffStatusHMP4040.Fill = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
        }

        private void DisableOutputButton()
        {
            buOutput.IsEnabled = false;
        }

        private void EnableOutputButton()
        {
            buOutput.IsEnabled = true;
        }

        private void ReadAllChannels()
        {
            this.OnReadVoltageAndCurrentOnCHXX(this.gbCH01, null);
            this.OnReadVoltageAndCurrentOnCHXX(this.gbCH02, null);
            this.OnReadVoltageAndCurrentOnCHXX(this.gbCH03, null);
            this.OnReadVoltageAndCurrentOnCHXX(this.gbCH04, null);
        }

        private void EnableOutputButtonOnChannelIfIsSelected()
        {
            if (gbCH01.IsChActive) { gbCH01.EnableOutput = true; }
            if (gbCH02.IsChActive) { gbCH02.EnableOutput = true; }
            if (gbCH03.IsChActive) { gbCH03.EnableOutput = true; }
            if (gbCH04.IsChActive) { gbCH04.EnableOutput = true; }
        }

        private void DisableOutputButtonOnALLChannels()
        {
            if (gbCH01.cbCHXX.IsEnabled == true)
            {
                gbCH01.EnableOutput = false;

                if (gbCH01.cbCHXX.IsChecked == false)
                {
                    gbCH01.SetOutputButtonColorAsGray();
                }
            }

            if (gbCH02.cbCHXX.IsEnabled == true)
            {
                gbCH02.EnableOutput = false;

                if (gbCH02.cbCHXX.IsChecked == false)
                {
                    gbCH02.SetOutputButtonColorAsGray();
                }
            }

            if (gbCH03.cbCHXX.IsEnabled == true)
            {
                gbCH03.EnableOutput = false;

                if (gbCH03.cbCHXX.IsChecked == false)
                {
                    gbCH03.SetOutputButtonColorAsGray();
                }
            }

            if (gbCH04.cbCHXX.IsEnabled == true)
            {
                gbCH04.EnableOutput = false;

                if (gbCH04.cbCHXX.IsChecked == false)
                {
                    gbCH04.SetOutputButtonColorAsGray();
                }
            }
        }


        private void DisableAllSelectedChannels()
        {
            try
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = false;

                driver.BasicOperation.SelectedChannel = Output.Channel2;
                driver.BasicOperation.ChannelOnlyEnabled = false;

                driver.BasicOperation.SelectedChannel = Output.Channel3;
                driver.BasicOperation.ChannelOnlyEnabled = false;

                driver.BasicOperation.SelectedChannel = Output.Channel4;
                driver.BasicOperation.ChannelOnlyEnabled = false;

                //by default select the channel 1

                driver.BasicOperation.SelectedChannel = Output.Channel1;
            }
            catch (Ivi.Driver.IOException ex)
            {
                tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
            }
        }

        private void DisableMasterOutput()
        {
            try
            {
                driver.BasicOperation.MasterEnabled = false;
            }
            catch (Ivi.Driver.IOException ex)
            {
                tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
            }
        }

        private void buOutput_Click(object sender, RoutedEventArgs e)
        {
            bool bAtLeasOneChannelIsSelected = gbCH01.IsChActive || gbCH02.IsChActive || gbCH03.IsChActive || gbCH04.IsChActive;

            if (!bAtLeasOneChannelIsSelected)
            {
                tbLogStatus.Text += ("[INFO] Select at least one channel, then press output.\n");
                return;
            }

            bOutputActive ^= true; //toogle value

            if (bOutputActive == true)
            {
                try
                {
                    driver.BasicOperation.MasterEnabled = true;
                    EnableOutputButtonOnChannelIfIsSelected();
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }

            }
            else
            {
                try
                {
                    driver.BasicOperation.MasterEnabled = false;
                    DisableOutputButtonOnALLChannels();
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
            }
        }

        private void buClearLog_Click(object sender, RoutedEventArgs e)
        {
            this.tbLogStatus.Clear();
        }

        #region Menu and MenuItems
        private void miProperties_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private Channel IdentifyChannel(string channelName)
        {
            Channel result = Channel.NotUsed;

            if ( String.IsNullOrEmpty(channelName) )
            {
                result = Channel.NotUsed;
            }

            if      ( String.Equals(channelName, CH01_NAME) ) { result = Channel.Channel1; }
            else if ( String.Equals(channelName, CH02_NAME) ) { result = Channel.Channel2; }
            else if ( String.Equals(channelName, CH03_NAME) ) { result = Channel.Channel3; }
            else if ( String.Equals(channelName, CH04_NAME) ) { result = Channel.Channel4; }
            else                                              { result = Channel.NotUsed;  }

            return result;
        }

        private void SelectChannelOnPowerSupply(Channel eCH)
        {
            switch (eCH)
            {
                case Channel.Channel1:
                    driver.BasicOperation.SelectedChannel = Output.Channel1;
                    break;

                case Channel.Channel2:
                    driver.BasicOperation.SelectedChannel = Output.Channel2;
                    break;

                case Channel.Channel3:
                    driver.BasicOperation.SelectedChannel = Output.Channel3;
                    break;

                case Channel.Channel4:
                    driver.BasicOperation.SelectedChannel = Output.Channel4;
                    break;

                case Channel.NotUsed:
                    break;
            }
        }

        private Output SelectOutputChannelOnPowerSupply(Channel eCH)
        {
            Output outputChannel = Output.Channel1;
           
            switch (eCH)
            {
                case Channel.Channel1:
                    outputChannel = Output.Channel1;
                    break;

                case Channel.Channel2:
                    outputChannel = Output.Channel2;
                    break;

                case Channel.Channel3:
                    outputChannel = Output.Channel3;
                    break;

                case Channel.Channel4:
                    outputChannel = Output.Channel4;
                    break;

                case Channel.NotUsed:
                    outputChannel = Output.Channel1;
                    break;
            }

            return outputChannel;
        }

        /*THE FOLLOWING EVENTS SHOULD BE EQUAL FOR EACH GROUPBOX*/
        #region Events GroupBox CHANNEL XX

        private void OnSetCurrentOnCHXX(object sender, RoutedEventArgs e)
        {
            gbVoltAndCurrent gbCHXX = sender as gbVoltAndCurrent;

            if (gbCHXX == null)
            {
                return;
            }

            Channel eCH = IdentifyChannel(gbCHXX.MainName);

            this.SelectChannelOnPowerSupply(eCH);

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.PrevTargetVoltage, gbCHXX.TargetVoltage, gbCHXX.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.PrevTargetCurrent, gbCHXX.TargetCurrent, gbCHXX.Delta));

            if (!bChangedVoltage && !bChangedCurrent)
            {
                return;
            }

            // SET VOLTAGE IF NECESSARY
            if (bChangedVoltage)
            {
                driver.VoltageAndCurrent.OutputVoltageLevel = (double)gbCHXX.TargetVoltage;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCHXX.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCHXX.PrevTargetVoltage = gbCHXX.TargetVoltage;
                    gbCHXX.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power on" + gbCHXX.MainName + "have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCHXX.PrevTargetVoltage = gbCHXX.TargetVoltage;
                }

                gbCHXX.UpdateSelectedVoltage();
            }

            // SET CURRENT IF NECESSARY
            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = (double)gbCHXX.TargetCurrent;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCHXX.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCHXX.PrevTargetCurrent = gbCHXX.TargetCurrent;
                    gbCHXX.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power on " + gbCHXX.MainName + "have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCHXX.PrevTargetCurrent = gbCHXX.TargetCurrent;
                }

                gbCHXX.UpdateSelectedCurrent();
            }

            this.SelectChannelOnPowerSupply(eCH);
        }

        private void OnEnableCHXX(object sender, RoutedEventArgs e)
        {
            gbVoltAndCurrent gbCHXX = sender as gbVoltAndCurrent;

            if (gbCHXX == null)
            {
                return;
            }

            if (!gbCHXX.cbCHXX.IsChecked == true)
            {
                tbLogStatus.Text += ("[INFO] Select the checkbox on " + gbCHXX.MainName + ".\n");
                return;
            }

            Channel eCH = IdentifyChannel(gbCHXX.MainName);

            if (gbCHXX.IsChActive)
            {
                this.SelectChannelOnPowerSupply(eCH);
                driver.BasicOperation.ChannelOnlyEnabled = true;

                if (bOutputActive == true)
                {
                    gbCHXX.EnableOutput = true;
                }
            }
            else
            {
                this.SelectChannelOnPowerSupply(eCH);
                driver.BasicOperation.ChannelOnlyEnabled = false;
                gbCHXX.EnableOutput = false;
            }

        }

        private void OnOutputCHXX(object sender, RoutedEventArgs e)
        {
            gbVoltAndCurrent gbCHXX = sender as gbVoltAndCurrent;

            if (gbCHXX == null)
            {
                return;
            }

            if (!gbCHXX.IsChActive && !gbCHXX.EnableOutput)
            {
                //TODO: remove the "_" in MainName
                string channelName = gbCHXX.MainName.Trim(new char[] { '_' });
                tbLogStatus.Text += ("[INFO] Select first the button" + channelName + ", then press output.\n");
                return;
            }

            Channel eCH = IdentifyChannel(gbCHXX.MainName);

            if (!gbCHXX.EnableOutput)
            {
                try
                {
                    this.SelectChannelOnPowerSupply(eCH);
                    driver.BasicOperation.OutputEnabled = false;
                    gbCHXX.IsChActive = false;
                    OnEnableCHXX(sender, e);

                    if (!(gbCH01.EnableOutput || gbCH02.EnableOutput || gbCH03.EnableOutput || gbCH04.EnableOutput))
                    {
                        driver.BasicOperation.MasterEnabled = false;
                        buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                        bOutputActive = false;
                    }

                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR][" + gbCHXX.MainName + "]" + ex.Message + "\n");
                }
            }
            else
            {
                try
                {
                    EnableOutputButtonOnChannelIfIsSelected();
                    driver.BasicOperation.OutputEnabled = true;
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                    bOutputActive = true;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR][" + gbCHXX.MainName + "]" + ex.Message + "\n");
                }
                //[TODO] Check if other channels have EnableOutput == true, if not deactivate master output. 
            }
        }

        private void OnReadVoltageAndCurrentOnCHXX(object sender, RoutedEventArgs e)
        {
            gbVoltAndCurrent gbCHXX = sender as gbVoltAndCurrent;

            if (gbCHXX == null)
            {
                return;
            }

            Channel eCH = IdentifyChannel(gbCHXX.MainName);

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCHXX.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCHXX.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCHXX.Delta));

            if (bChangedVoltage)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = SelectOutputChannelOnPowerSupply(eCH); // e.g. Output.Channel4;
                gbCHXX.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                gbCHXX.UpdateSelectedVoltage();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;

            }
            if (bChangedCurrent)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = SelectOutputChannelOnPowerSupply(eCH); // e.g. Output.Channel4;
                gbCHXX.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCHXX.UpdateSelectedCurrent();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;
            }

        }
        #endregion
    }
}
