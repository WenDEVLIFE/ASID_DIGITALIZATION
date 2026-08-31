using System;
using System.Collections.Generic;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class LaneSequenceDialog : Window
    {
        public string? SelectedLane { get; private set; }

        public LaneSequenceDialog(Dictionary<string, int>? occupancy = null)
        {
            InitializeComponent();
            LaneSequence.LaneSelected += LaneSequence_LaneSelected;

            if (occupancy != null)
                LaneSequence.SetOccupancy(occupancy);
        }

        private void LaneSequence_LaneSelected(object? sender, string laneCode)
        {
            SelectedLane = laneCode;
            DialogResult = true;
            Close();
        }
    }
}
