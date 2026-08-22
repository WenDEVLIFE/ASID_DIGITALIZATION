using System;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class LaneSequenceDialog : Window
    {
        public string? SelectedLane { get; private set; }

        public LaneSequenceDialog()
        {
            InitializeComponent();
            LaneSequence.LaneSelected += LaneSequence_LaneSelected;
        }

        private void LaneSequence_LaneSelected(object? sender, string laneCode)
        {
            SelectedLane = laneCode;
            DialogResult = true;
            Close();
        }
    }
}
