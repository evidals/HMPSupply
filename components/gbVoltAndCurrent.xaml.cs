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


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(gbVoltAndCurrent), new PropertyMetadata(string.Empty));



        public gbVoltAndCurrent()
        {
            InitializeComponent();
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

                if (!cbCH01.IsChecked.Value)
                {
                    buCH01_Read_Volt_Curr.IsEnabled = false;
                    buCH01_Set_Volt_Curr.IsEnabled = false;
                    gbCH01_U.IsEnabled = false;
                    gbCH01_I.IsEnabled = false;
                }
                else
                {
                    buCH01_Read_Volt_Curr.IsEnabled = true;
                    buCH01_Set_Volt_Curr.IsEnabled = true;
                    gbCH01_U.IsEnabled = true;
                    gbCH01_I.IsEnabled = true;
                }
            }

            Trace.WriteLine(string.Format("Click event on CH01: {0}", checkState));
        }
    }
}
