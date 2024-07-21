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

namespace HMPSupply.components
{
    /// <summary>
    /// Interaction logic for gbVoltAndCurrent.xaml
    /// </summary>
    public partial class gbVoltAndCurrent : UserControl
    {

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


        public gbVoltAndCurrent()
        {
            InitializeComponent();
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
    }
}
