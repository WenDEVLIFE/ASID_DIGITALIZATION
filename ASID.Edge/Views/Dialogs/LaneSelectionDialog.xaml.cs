using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge.Views.Dialogs;

/// <summary>
/// Interaction logic for LaneSelectionDialog.xaml
/// </summary>
public partial class LaneSelectionDialog : Window
{
    private readonly List<string> _vacantLanes;

    public string SelectedLane { get; private set; } = "";

    public LaneSelectionDialog(IReadOnlyList<string> vacantLanes)
    {
        InitializeComponent();

        _vacantLanes = vacantLanes.ToList();

        VacantLanesList.ItemsSource = _vacantLanes;

        Loaded += (_, _) => LaneTextBox.Focus();
    }

    private void LaneTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        var scanned = LaneTextBox.Text.Trim();

        var match = _vacantLanes.FirstOrDefault(lane =>
            string.Equals(lane, scanned, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            // Occupied or unknown lane: reject and keep the dialog open.
            RejectionText.Text =
                $"Lane '{scanned}' is occupied or unknown. Scan a vacant lane listed above.";
            RejectionText.Visibility = Visibility.Visible;

            LaneTextBox.Clear();
            LaneTextBox.Focus();

            return;
        }

        SelectedLane = match;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}