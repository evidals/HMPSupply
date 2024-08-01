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
            this.OnReadVoltageAndCurrentOnCH01(null, null);
            this.OnReadVoltageAndCurrentOnCH02(null, null);
            this.OnReadVoltageAndCurrentOnCH03(null, null);
            this.OnReadVoltageAndCurrentOnCH04(null, null);
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
            bOutputActive ^= true; //toogle value

            if (!gbCH01.IsChActive)
            {
                tbLogStatus.Text += ("[INFO] Select at least one channel, then press output.\n");
                return;
            }

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

        /*THE FOLLOWING EVENTS SHOULD BE EQUAL FOR EACH GROUPBOX*/

        #region Events Groupbox CHANNEL 01

        private void OnSetCurrentOnCH01(object sender, RoutedEventArgs e)
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
                driver.VoltageAndCurrent.OutputVoltageLevel = (double)gbCH01.TargetVoltage;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH01.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH01.PrevTargetVoltage = gbCH01.TargetVoltage;
                    gbCH01.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH01.PrevTargetVoltage = gbCH01.TargetVoltage;
                }

                gbCH01.UpdateSelectedVoltage();
            }

            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = (double)gbCH01.TargetCurrent;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH01.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH01.PrevTargetCurrent = gbCH01.TargetCurrent;
                    gbCH01.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH01.PrevTargetCurrent = gbCH01.TargetCurrent;
                }

                gbCH01.UpdateSelectedCurrent();
            }
            driver.BasicOperation.SelectedChannel = Output.Channel1;
        }

        private void OnReadVoltageAndCurrentOnCH01(object sender, RoutedEventArgs e)
        {
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH01.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH01.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH01.Delta));

            if(bChangedVoltage) 
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                gbCH01.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                gbCH01.UpdateSelectedVoltage();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;

            }
            if (bChangedCurrent)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                gbCH01.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCH01.UpdateSelectedCurrent();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;
            }

        }

        private void OnEnableCH01(object sender, RoutedEventArgs e)
        {
            if(gbCH01.IsChActive)
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = true;
            }
            else
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = false;
            }
            
        }

        private void OnOutputCH01(object sender, RoutedEventArgs e)
        {
            if (!gbCH01.IsChActive)
            {
                tbLogStatus.Text += ("[INFO] Select first the button CH01, then press output.\n");
                return;
            }

            if (!gbCH01.EnableOutput)
            {
                try
                {
                    driver.BasicOperation.OutputEnabled = false;
                    gbCH01.IsChActive = false;
                    OnEnableCH01(null, null);

                    if (!(gbCH01.EnableOutput || gbCH02.EnableOutput || gbCH03.EnableOutput || gbCH04.EnableOutput))
                    {
                        driver.BasicOperation.MasterEnabled = false;
                        buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                        bOutputActive = false;
                    }

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
                    driver.BasicOperation.OutputEnabled = true;
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                    bOutputActive = true;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
                //[TODO] Check if other channels have EnableOutput == true, if not deactivate master output. 

            }
        }

        #endregion

        #region Events Groupbox CHANNEL 02

        private void OnSetCurrentOnCH02(object sender, RoutedEventArgs e)
        {
            driver.BasicOperation.SelectedChannel = Output.Channel1;

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH02.PrevTargetVoltage, gbCH02.TargetVoltage, gbCH02.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH02.PrevTargetCurrent, gbCH02.TargetCurrent, gbCH02.Delta));

            if (!bChangedVoltage && !bChangedCurrent)
            {
                return;
            }

            if (bChangedVoltage)
            {
                driver.VoltageAndCurrent.OutputVoltageLevel = (double)gbCH02.TargetVoltage;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH02.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH02.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH02.PrevTargetVoltage = gbCH02.TargetVoltage;
                    gbCH02.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH02.PrevTargetVoltage = gbCH02.TargetVoltage;
                }

                gbCH02.UpdateSelectedVoltage();
            }

            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = (double)gbCH02.TargetCurrent;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH02.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH02.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH02.PrevTargetCurrent = gbCH02.TargetCurrent;
                    gbCH02.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH02.PrevTargetCurrent = gbCH02.TargetCurrent;
                }

                gbCH02.UpdateSelectedCurrent();
            }
            driver.BasicOperation.SelectedChannel = Output.Channel1;
        }

        private void OnReadVoltageAndCurrentOnCH02(object sender, RoutedEventArgs e)
        {
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH02.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH02.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH02.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH02.Delta));

            if (bChangedVoltage)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel2;

                try
                {
                    double aux_double = driver.VoltageAndCurrent.OutputVoltageLevel;
                    gbCH02.TargetVoltage = (float)aux_double;
                    while (!(MathUtilities.ApproximatelyEqualEpsilon(gbCH02.TargetVoltage, (float)aux_double, gbCH02.Delta)))
                    {
                        gbCH02.TargetVoltage = (float)aux_double;
                    }

                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
                
                gbCH02.UpdateSelectedVoltage();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;

            }
            if (bChangedCurrent)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel2;
                gbCH02.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCH02.UpdateSelectedCurrent();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;
            }

        }

        private void OnEnableCH02(object sender, RoutedEventArgs e)
        {
            if (gbCH02.IsChActive)
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = true;
            }
            else
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = false;
            }

        }

        private void OnOutputCH02(object sender, RoutedEventArgs e)
        {
            if (!gbCH02.IsChActive)
            {
                tbLogStatus.Text += ("[INFO] Select first the button CH02, then press output.\n");
                return;
            }

            if (!gbCH02.EnableOutput)
            {
                try
                {
                    driver.BasicOperation.OutputEnabled = false;
                    gbCH02.IsChActive = false;
                    OnEnableCH02(null, null);

                    if (!(gbCH01.EnableOutput || gbCH02.EnableOutput || gbCH03.EnableOutput || gbCH04.EnableOutput))
                    {
                        driver.BasicOperation.MasterEnabled = false;
                        buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                        bOutputActive = false;
                    }

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
                    driver.BasicOperation.OutputEnabled = true;
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                    bOutputActive = true;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
                //[TODO] Check if other channels have EnableOutput == true, if not deactivate master output. 

            }
        }

        #endregion

        #region Events Groupbox CHANNEL 03

        private void OnSetCurrentOnCH03(object sender, RoutedEventArgs e)
        {
            driver.BasicOperation.SelectedChannel = Output.Channel1;

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH03.PrevTargetVoltage, gbCH03.TargetVoltage, gbCH03.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH03.PrevTargetCurrent, gbCH03.TargetCurrent, gbCH03.Delta));

            if (!bChangedVoltage && !bChangedCurrent)
            {
                return;
            }

            if (bChangedVoltage)
            {
                driver.VoltageAndCurrent.OutputVoltageLevel = (double)gbCH03.TargetVoltage;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH03.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH03.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH03.PrevTargetVoltage = gbCH03.TargetVoltage;
                    gbCH03.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH03.PrevTargetVoltage = gbCH03.TargetVoltage;
                }

                gbCH03.UpdateSelectedVoltage();
            }

            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = (double)gbCH03.TargetCurrent;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH03.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH03.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH03.PrevTargetCurrent = gbCH03.TargetCurrent;
                    gbCH03.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH03.PrevTargetCurrent = gbCH03.TargetCurrent;
                }

                gbCH03.UpdateSelectedCurrent();
            }
            driver.BasicOperation.SelectedChannel = Output.Channel1;
        }

        private void OnReadVoltageAndCurrentOnCH03(object sender, RoutedEventArgs e)
        {
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH03.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH03.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH03.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH03.Delta));

            if (bChangedVoltage)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel3;
                gbCH03.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                gbCH03.UpdateSelectedVoltage();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;

            }
            if (bChangedCurrent)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel3;
                gbCH03.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCH03.UpdateSelectedCurrent();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;
            }

        }

        private void OnEnableCH03(object sender, RoutedEventArgs e)
        {
            if (gbCH03.IsChActive)
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = true;
            }
            else
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = false;
            }

        }

        private void OnOutputCH03(object sender, RoutedEventArgs e)
        {
            if (!gbCH03.IsChActive)
            {
                tbLogStatus.Text += ("[INFO] Select first the button CH03, then press output.\n");
                return;
            }

            if (!gbCH03.EnableOutput)
            {
                try
                {
                    driver.BasicOperation.OutputEnabled = false;
                    gbCH03.IsChActive = false;
                    OnEnableCH03(null, null);

                    if (!(gbCH01.EnableOutput || gbCH02.EnableOutput || gbCH03.EnableOutput || gbCH04.EnableOutput))
                    {
                        driver.BasicOperation.MasterEnabled = false;
                        buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                        bOutputActive = false;
                    }

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
                    driver.BasicOperation.OutputEnabled = true;
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                    bOutputActive = true;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
                //[TODO] Check if other channels have EnableOutput == true, if not deactivate master output. 
            }
        }

        #endregion

        #region Events Groupbox CHANNEL 04

        private void OnSetCurrentOnCH04(object sender, RoutedEventArgs e)
        {
            driver.BasicOperation.SelectedChannel = Output.Channel1;

            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH04.PrevTargetVoltage, gbCH04.TargetVoltage, gbCH04.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH04.PrevTargetCurrent, gbCH04.TargetCurrent, gbCH04.Delta));

            if (!bChangedVoltage && !bChangedCurrent)
            {
                return;
            }

            if (bChangedVoltage)
            {
                driver.VoltageAndCurrent.OutputVoltageLevel = (double)gbCH04.TargetVoltage;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH04.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH04.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH04.PrevTargetVoltage = gbCH04.TargetVoltage;
                    gbCH04.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH04.PrevTargetVoltage = gbCH04.TargetVoltage;
                }

                gbCH04.UpdateSelectedVoltage();
            }

            if (bChangedCurrent)
            {
                driver.VoltageAndCurrent.OutputCurrentLevel = (double)gbCH04.TargetCurrent;
                if (false == MathUtilities.ApproximatelyEqualEpsilon(gbCH04.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH04.Delta))
                {
                    //max.power have been exceed 160W max.
                    gbCH04.PrevTargetCurrent = gbCH04.TargetCurrent;
                    gbCH04.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                    tbLogStatus.Text += "[RE-SIZED]Max.power have been exceed. HMP4040 is 160W max.\n";
                }
                else
                {
                    gbCH04.PrevTargetCurrent = gbCH04.TargetCurrent;
                }

                gbCH04.UpdateSelectedCurrent();
            }
            driver.BasicOperation.SelectedChannel = Output.Channel1;
        }

        private void OnReadVoltageAndCurrentOnCH04(object sender, RoutedEventArgs e)
        {
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH04.TargetVoltage, (float)driver.VoltageAndCurrent.OutputVoltageLevel, gbCH04.Delta));
            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(gbCH04.TargetCurrent, (float)driver.VoltageAndCurrent.OutputCurrentLevel, gbCH04.Delta));

            if (bChangedVoltage)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel4;
                gbCH04.TargetVoltage = (float)driver.VoltageAndCurrent.OutputVoltageLevel;
                gbCH04.UpdateSelectedVoltage();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;

            }
            if (bChangedCurrent)
            {
                Output aux_currentChannel = driver.BasicOperation.SelectedChannel;
                driver.BasicOperation.SelectedChannel = Output.Channel4;
                gbCH04.TargetCurrent = (float)driver.VoltageAndCurrent.OutputCurrentLevel;
                gbCH04.UpdateSelectedCurrent();
                driver.BasicOperation.SelectedChannel = aux_currentChannel;
            }

        }

        private void OnEnableCH04(object sender, RoutedEventArgs e)
        {
            if (gbCH04.IsChActive)
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = true;
            }
            else
            {
                driver.BasicOperation.SelectedChannel = Output.Channel1;
                driver.BasicOperation.ChannelOnlyEnabled = false;
            }

        }

        private void OnOutputCH04(object sender, RoutedEventArgs e)
        {
            if (!gbCH04.IsChActive)
            {
                tbLogStatus.Text += ("[INFO] Select first the button CH04, then press output.\n");
                return;
            }

            if (!gbCH04.EnableOutput)
            {
                try
                {
                    driver.BasicOperation.OutputEnabled = false;
                    gbCH04.IsChActive = false;
                    OnEnableCH04(null, null);

                    if (!(gbCH01.EnableOutput || gbCH02.EnableOutput || gbCH03.EnableOutput || gbCH04.EnableOutput))
                    {
                        driver.BasicOperation.MasterEnabled = false;
                        buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                        bOutputActive = false;
                    }

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
                    driver.BasicOperation.OutputEnabled = true;
                    buOutput.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                    bOutputActive = true;
                }
                catch (Ivi.Driver.IOException ex)
                {
                    tbLogStatus.Text += ("[ERROR]:" + ex.Message + "\n");
                }
                //[TODO] Check if other channels have EnableOutput == true, if not deactivate master output. 
            }
        }

        #endregion

    }
}
