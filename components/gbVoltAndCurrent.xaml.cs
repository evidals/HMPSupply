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
        const float EPSILON = 0.1f;

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

        #region Variablen

        float prevTargetCurrent = (float)0.0;
        float prevTargetVoltage = (float)0.0;
        float targetCurrent = (float)0.0;
        float targetVoltage = (float)0.0;

        #endregion

        #region Konstruktor
        public gbVoltAndCurrent()
        {
            InitializeComponent();
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
                targetVoltage = (float)1.0;
            }
        }

        private void rBuCHXX_33V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_33V.IsChecked == true)
            {
                targetVoltage = (float)3.3;
            }
        }

        private void rBuCHXX_50V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_50V.IsChecked == true)
            {
                targetVoltage = (float)5.0;
            }
        }

        private void rBuCHXX_80V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_80V.IsChecked == true)
            {
                targetVoltage = (float)8.0;
            }
        }

        private void rBuCHXX_12V_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_12V.IsChecked == true)
            {
                targetVoltage = (float)12.0;
            }
        }

        private void rBuCHXX_InputVol_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_InputVol.IsChecked == true)
            {
                float auxFloat_Volt = float.Parse(this.CHXX_InVol.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (auxFloat_Volt >= (float)0 && auxFloat_Volt <= (float)32.0)
                {
                    targetVoltage = auxFloat_Volt;
                }
            }
        }

        private void rBuCHXX_1A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_1A.IsChecked == true)
            {
                targetCurrent = (float)1.0;
            }
        }

        private void rBuCHXX_2A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_2A.IsChecked == true)
            {
                targetCurrent = (float)2.0;
            }
        }

        private void rBuCHXX_3A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_3A.IsChecked == true)
            {
                targetCurrent = (float)3.0;
            }
        }

        private void rBuCHXX_4A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_4A.IsChecked == true)
            {
                targetCurrent = (float)4.0;
            }
        }

        private void rBuCHXX_8A_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_8A.IsChecked == true)
            {
                targetCurrent = (float)8.0;
            }

        }

        private void rBuCHXX_InputCurr_Checked(object sender, RoutedEventArgs e)
        {
            if (rBuCHXX_InputCurr.IsChecked == true)
            {
                float auxFloat_Current = float.Parse(this.CHXX_InCurr.Text, System.Globalization.CultureInfo.InvariantCulture);

                if (auxFloat_Current >= (float)0 && auxFloat_Current <= (float)10.0)
                {
                    targetCurrent = auxFloat_Current;
                }
            }
        }
    }
}
