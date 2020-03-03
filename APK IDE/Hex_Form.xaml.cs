using Microsoft.Win32;
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
    /// Lógica interna para Hex_Form.xaml
    /// </summary>
    public partial class Hex_Form : Window
    {
        public static Hex_Form singleton;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        public Hex_Form()
        {
            InitializeComponent();
            singleton = this;
        }

        private void OpenF_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "All Files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                HexView.FileName = openFileDialog.FileName;
                FileNameT.Text = openFileDialog.FileName;
            }
        }

        private void HxD_GoExpression(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.G)
            {
                CustomPopup popup = new CustomPopup();
                if (popup.ShowDialog() == true)
                {

                }
            }
        }

        public void GoEx(string offset)
        {
            HexView.SetPosition(offset);
        }

        private void SaveF_Copy_Click(object sender, RoutedEventArgs e)
        {
            HexView.SubmitChanges();
        }
        private void HexView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
