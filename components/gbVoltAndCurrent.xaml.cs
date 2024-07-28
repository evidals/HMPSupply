using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Utils;

namespace HMPSupply.components
{
    /// <summary>
    /// Interaction logic for gbVoltAndCurrent.xaml
    /// </summary>
    public partial class gbVoltAndCurrent : UserControl
    {
        const float EPSILON                     = 0.001f;
        const float ZERO_F                      = 0.0f;

        const float ONE_VOLT_F                  = 1.0f;
        const float THREE_POINT_THREE_VOLT_F    = 3.3f;
        const float FIVE_VOLT_F                 = 5.0f;
        const float EIGHT_VOLT_F                = 8.0f;
        const float TWELVE_VOLT_F               = 12.0f;
        const float MAX_VOLT_F                  = 32.0f;

        const float ONE_AMP_F                   = 1.0f;
        const float TWO_AMP_F                   = 2.0f;
        const float THREE_AMP_F                 = 3.0f;
        const float FOUR_AMP_F                  = 4.0f;
        const float EIGHT_AMP_F                 = 8.0f;
        const float MAX_AMP_F                   = 10.0f;

        bool bChIsActive = false;
        bool bChOutputEnabled = false;


        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(gbVoltAndCurrent), new PropertyMetadata(string.Empty));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty MainNameProperty =
            DependencyProperty.Register("MainName", typeof(string), typeof(gbVoltAndCurrent), new PropertyMetadata(string.Empty));

        public string MainName
        {
            get { return (string)GetValue(MainNameProperty); }
            set { SetValue(MainNameProperty, value); }
        }

        public static readonly RoutedEvent ReadVoltageAndCurrentOnChannelEvent =
            EventManager.RegisterRoutedEvent(nameof(ReadVoltageAndCurrentOnChannel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(gbVoltAndCurrent));

        public event RoutedEventHandler ReadVoltageAndCurrentOnChannel
        {
            add { AddHandler(ReadVoltageAndCurrentOnChannelEvent, value);}
            remove { RemoveHandler(ReadVoltageAndCurrentOnChannelEvent, value); }
        }

        public static readonly RoutedEvent SetCurrentOnChannelEvent =
            EventManager.RegisterRoutedEvent(nameof(SetCurrentOnChannel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(gbVoltAndCurrent));

        public event RoutedEventHandler SetCurrentOnChannel
        {
            add { AddHandler(SetCurrentOnChannelEvent, value); }
            remove { RemoveHandler(SetCurrentOnChannelEvent, value); }
        }

        public static readonly RoutedEvent EnableChannelOnChannelEvent =
            EventManager.RegisterRoutedEvent(nameof(EnableCH_OnChannel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(gbVoltAndCurrent));

        public event RoutedEventHandler EnableCH_OnChannel
        {
            add { AddHandler(EnableChannelOnChannelEvent, value); }
            remove { RemoveHandler(EnableChannelOnChannelEvent, value); }
        }

        public static readonly RoutedEvent EnableOutputOnChannelEvent =
            EventManager.RegisterRoutedEvent(nameof(EnableOutputOnChannel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(gbVoltAndCurrent));

        public event RoutedEventHandler EnableOutputOnChannel
        {
            add { AddHandler(EnableOutputOnChannelEvent, value); }
            remove { RemoveHandler(EnableOutputOnChannelEvent, value); }
        }


        #region Variablen

        float prevTargetCurrent = (float)0.0;
        float prevTargetVoltage = (float)0.0;
        float targetCurrent = (float)0.0;
        float targetVoltage = (float)0.0;
        eSEL_U eSelectedVoltage = eSEL_U.None;
        eSEL_I eSelectedCurrent = eSEL_I.None;

        #endregion

        #region Konstruktor
        public gbVoltAndCurrent()
        {
            InitializeComponent();
            buActivateChannel.Background = Utils.ColorUtilities.scbTurnOffCh;
        }
        #endregion

        #region Methods

        public float Delta
        {
            get {return EPSILON; }
        }
        public float TargetCurrent
        {
            get { return this.targetCurrent; }
            set { this.targetCurrent = value;}
        }

        public float PrevTargetCurrent
        {
            get { return this.prevTargetCurrent; }
            set { this.prevTargetCurrent = value;}
        }

        public float TargetVoltage
        {
            get { return this.targetVoltage; }
            set { this.targetVoltage = value;}
        }

        public float PrevTargetVoltage
        {
            get { return this.prevTargetVoltage; }
            set { this.prevTargetVoltage = value;}
        }
        public bool IsChActive
        {
            get { return this.bChIsActive; }
            set 
            { 
                this.bChIsActive = value;
                if (this.bChIsActive == false)
                {
                    buActivateChannel.Background = Utils.ColorUtilities.scbTurnOffCh;
                }
                else
                {
                    buActivateChannel.Background = Utils.ColorUtilities.scbTurnOnCh;
                }
            }
        }
        public bool EnableOutput
        {
            get { return this.bChOutputEnabled; }
            set 
            { 
                this.bChOutputEnabled = value;

                if (this.bChOutputEnabled == false)
                {
                    buOutThisChannel.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOff;
                }
                else 
                {
                    buOutThisChannel.Background = (SolidColorBrush)Utils.ColorUtilities.scbTurnOn;
                }
            }
        }

        


        public void DisableChannelEdition()
        {
            this.IsEnabled = false;
            this.cbCHXX.IsEnabled = false;
            this.buCHXX_Read_Volt_Curr.IsEnabled = false;
            this.buCHXX_Set_Volt_Curr.IsEnabled = false;
            this.gbCHXX_I.IsEnabled = false;
            this.gbCHXX_U.IsEnabled = false;
        }

        #endregion

        #region Events

        private void OnSetCurrentOnChannel(object sneder, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SetCurrentOnChannelEvent));
        }

        private void OnReadVoltageAndCurrentOnChannel(object sneder, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ReadVoltageAndCurrentOnChannelEvent));
        }

        private void OnEnableOnChannel(object sneder, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(EnableChannelOnChannelEvent));
        }

        private void OnOutputOnChannel(object sneder, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(EnableOutputOnChannelEvent));
        }

        private void buCHXX_Read_Volt_Curr_Click(object sender, RoutedEventArgs e)
        {
            OnReadVoltageAndCurrentOnChannel(null, null);
        }

        private void cbCH01_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            string checkState = string.Empty;
            if (!cb.IsChecked.HasValue)
            {
                checkState = "Indeterminate";
            }
            else
            {
                checkState = (true == cb.IsChecked) ? "Checked" : "Unchecked";

                if (!cbCHXX.IsChecked.Value)
                {
                    buCHXX_Read_Volt_Curr.IsEnabled = false;
                    buCHXX_Set_Volt_Curr.IsEnabled = false;
                    gbCHXX_U.IsEnabled = false;
                    gbCHXX_I.IsEnabled = false;
                }
                else
                {
                    buCHXX_Read_Volt_Curr.IsEnabled = true;
                    buCHXX_Set_Volt_Curr.IsEnabled = true;
                    gbCHXX_U.IsEnabled = true;
                    gbCHXX_I.IsEnabled = true;
                }
            }

            Trace.WriteLine(string.Format("Click event on CH01: {0}", checkState));
        }

        private void buCHXX_Set_Volt_Curr_Click(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_InputCurr.IsChecked == true)
            {
                rBuCHXX_InputCurr_Checked(null, null);
            }

            if (rBuCHXX_InputVol.IsChecked == true)
            {
                rBuCHXX_InputVol_Checked(null, null);
            }

            bool bChangedCurrent = !(MathUtilities.ApproximatelyEqualEpsilon(prevTargetCurrent, targetCurrent, EPSILON));
            bool bChangedVoltage = !(MathUtilities.ApproximatelyEqualEpsilon(prevTargetVoltage, targetVoltage, EPSILON));

            if (!(bChangedCurrent) && !(bChangedVoltage))
            {
                return;
            }

            if (bChangedCurrent || bChangedVoltage)
            {
                OnSetCurrentOnChannel(null,null);
            }
        }

        #endregion

        private void rBuCHXX_10V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_10V.IsChecked == true)
            {
                targetVoltage = ONE_VOLT_F;
            }
        }

        private void rBuCHXX_33V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_33V.IsChecked == true)
            {
                targetVoltage = THREE_POINT_THREE_VOLT_F;
            }
        }

        private void rBuCHXX_50V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_50V.IsChecked == true)
            {
                targetVoltage = FIVE_VOLT_F;
            }
        }

        private void rBuCHXX_80V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_80V.IsChecked == true)
            {
                targetVoltage = EIGHT_VOLT_F;
            }
        }

        private void rBuCHXX_12V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_12V.IsChecked == true)
            {
                targetVoltage = TWELVE_VOLT_F;
            }
        }

        private void rBuCHXX_InputVol_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_InputVol.IsChecked == true)
            {
                float auxFloat_Volt = float.Parse(this.CHXX_InVol.Text, System.Globalization.CultureInfo.InvariantCulture);
                
                if (auxFloat_Volt >= ZERO_F && auxFloat_Volt <= MAX_VOLT_F)
                {
                    targetVoltage = auxFloat_Volt;
                }
                else if (auxFloat_Volt > MAX_VOLT_F)
                {
                    targetVoltage = MAX_VOLT_F;
                }
                else
                {
                    targetVoltage = ZERO_F;
                }
            }
        }

        private void rBuCHXX_1A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_1A.IsChecked == true)
            {
                targetCurrent = ONE_AMP_F;
            }
        }

        private void rBuCHXX_2A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_2A.IsChecked == true)
            {
                targetCurrent = TWO_AMP_F;
            }
        }

        private void rBuCHXX_3A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_3A.IsChecked == true)
            {
                targetCurrent = THREE_AMP_F;
            }
        }

        private void rBuCHXX_4A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_4A.IsChecked == true)
            {
                targetCurrent = FOUR_AMP_F;
            }
        }

        private void rBuCHXX_8A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_8A.IsChecked == true)
            {
                targetCurrent = EIGHT_AMP_F;
            }

        }

        private void rBuCHXX_InputCurr_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_InputCurr.IsChecked == true)
            {
                float auxFloat_Current = float.Parse(this.CHXX_InCurr.Text, System.Globalization.CultureInfo.InvariantCulture);

                if (auxFloat_Current >= ZERO_F && auxFloat_Current <= MAX_AMP_F)
                {
                    targetCurrent = auxFloat_Current;
                }
                else if (auxFloat_Current > MAX_AMP_F)
                {
                    targetCurrent = MAX_AMP_F;
                }
                else
                {
                    targetCurrent = ZERO_F;
                }
            }
        }

        public void UpdateSelectedVoltage()
        {
            if ((MathUtilities.ApproximatelyEqualEpsilon(targetVoltage, ONE_VOLT_F, EPSILON)))
            {
                eSelectedVoltage = eSEL_U.IS_1V;
                rBuCHXX_10V.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetVoltage, THREE_POINT_THREE_VOLT_F, EPSILON)))
            {
                eSelectedVoltage = eSEL_U.IS_3p3V;
                rBuCHXX_33V.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetVoltage, FIVE_VOLT_F, EPSILON)))
            {
                eSelectedVoltage = eSEL_U.IS_5V;
                rBuCHXX_50V.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetVoltage, EIGHT_VOLT_F, EPSILON)))
            {
                eSelectedVoltage = eSEL_U.IS_8V;
                rBuCHXX_80V.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetVoltage, TWELVE_VOLT_F, EPSILON)))
            {
                eSelectedVoltage = eSEL_U.IS_12V;
                rBuCHXX_12V.IsChecked = true;
            }
            else 
            {
                eSelectedVoltage = eSEL_U.IS_UserInput;
                rBuCHXX_InputVol.IsChecked = true;
                CHXX_InVol.Text = string.Format("{0:N3}", targetVoltage);
            }
        }

        public void UpdateSelectedCurrent()
        {
            if ((MathUtilities.ApproximatelyEqualEpsilon(targetCurrent, ONE_AMP_F, EPSILON)))
            {
                eSelectedCurrent = eSEL_I.IS_1A;
                rBuCHXX_1A.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetCurrent, TWO_AMP_F, EPSILON)))
            {
                eSelectedCurrent = eSEL_I.IS_2A;
                rBuCHXX_2A.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetCurrent, THREE_AMP_F, EPSILON)))
            {
                eSelectedCurrent = eSEL_I.IS_3A;
                rBuCHXX_3A.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetCurrent, FOUR_AMP_F, EPSILON)))
            {
                eSelectedCurrent = eSEL_I.IS_4A;
                rBuCHXX_4A.IsChecked = true;
            }
            else if ((MathUtilities.ApproximatelyEqualEpsilon(targetCurrent, EIGHT_AMP_F, EPSILON)))
            {
                eSelectedCurrent = eSEL_I.IS_8A;
                rBuCHXX_8A.IsChecked = true;
            }
            else
            {
                eSelectedCurrent = eSEL_I.IS_UserInput;
                rBuCHXX_InputCurr.IsChecked = true;
                CHXX_InCurr.Text = string.Format("{0:N3}", targetCurrent);
            }
        }

        private void buActivateChannel_Click(object sender, RoutedEventArgs e)
        {
            IsChActive ^= true; //toogle value of bChIsActive see https://stackoverflow.com/questions/610916/easiest-way-to-flip-a-boolean-value
            OnEnableOnChannel(null, null);
        }

        private void buOutThisChannel_Click(object sender, RoutedEventArgs e)
        {
            if (bChOutputEnabled == false && IsChActive)
            {
                EnableOutput = true;
            }
            else if (bChOutputEnabled && IsChActive)
            {
                EnableOutput = false;
            }

            OnOutputOnChannel(null, null);
        }
    }

    public enum eSEL_U : int
    {
        IS_1V   = 0,
        IS_3p3V = 1,
        IS_5V   = 2,
        IS_8V   = 3,
        IS_12V  = 4,
        IS_UserInput = 5,
        None = -1,
    }

    public enum eSEL_I : int
    {
        IS_1A = 0,
        IS_2A = 1,
        IS_3A = 2,
        IS_4A = 3,
        IS_8A = 4,
        IS_UserInput = 5,
        None = -1,
    }
}
