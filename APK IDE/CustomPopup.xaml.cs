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
using System.Windows.Shapes;

namespace APK_IDE
{
    /// <summary>
    /// Lógica interna para CustomPopup.xaml
    /// </summary>
    public partial class CustomPopup : Window
    {
        public CustomPopup()
        {
            InitializeComponent();
            Offset_In.Focus();
        }

        private void Offset_Input(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Hex_Form.singleton.GoEx(Offset_In.Text);
                Close();
            }
        }

        private void Offset_In_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
