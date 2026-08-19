using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ASID.Edge.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NonConformanceScanDialog.xaml
    /// </summary>
    public partial class NonConformanceScanDialog : Window
    {

        public string DataMatrix { get; private set; } = "";
        public NonConformanceScanDialog()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                DataMatrixTextBox.Focus();
            };
        }

        private void DataMatrixTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            DataMatrix = DataMatrixTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
