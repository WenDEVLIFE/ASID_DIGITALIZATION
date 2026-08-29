using System;
using System.Windows;
using System.Windows.Controls;

namespace ASID.Edge.Views.Controls
{
    public partial class LaneSequenceControl : UserControl
    {
        public event EventHandler<string>? LaneSelected;

        public LaneSequenceControl()
        {
            InitializeComponent();
        }

        private void Lane_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string laneName)
            {
                LaneSelected?.Invoke(this, laneName);
            }
        }
    }
}
