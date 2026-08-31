using ASID.Edge.Models;
using ASID.Edge.Workflows.PUBody.Storage;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shapes;

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for LoginPortalControl.xaml
    /// </summary>
    public partial class LoginPortalControl : UserControl
    {
        public event EventHandler<string>? ScanCompleted;
        public event EventHandler? ApplyRequested;
        public event EventHandler? PrintRequested;

        public event EventHandler? CancelRequested;
        public WorkflowState State { get; set; }
        public LoginPortalControl()
        {
            InitializeComponent();
        }

        private void RaiseScan(string barcode)
        {
            ScanCompleted?.Invoke(this, barcode);
        }
        private void Scan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is not TextBox textbox)
                return;

            RaiseScan(textbox.Text);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateFromContext(StorageContext context)
        {
            txtKanban.Text = context.KanbanNo;
            txtLane.Text = context.LaneNo;
            txtTrolley.Text = context.TrolleyNo;
            txtLine.Text = context.CellNo;
            txtOperator.Text = context.OperatorId;

            // Show green checkmarks for fields that have data
            SetCheckmark(chkKanban, txtKanban, !string.IsNullOrEmpty(context.KanbanNo));
            SetCheckmark(chkLane, txtLane, !string.IsNullOrEmpty(context.LaneNo));
            SetCheckmark(chkTrolley, txtTrolley, !string.IsNullOrEmpty(context.TrolleyNo));
            SetCheckmark(chkLine, txtLine, !string.IsNullOrEmpty(context.CellNo));
            SetCheckmark(chkOperator, txtOperator, !string.IsNullOrEmpty(context.OperatorId));

            btnApply.IsEnabled =
                context.State == WorkflowState.ReadyForValidation;

            btnPrint.IsEnabled =
                context.State == WorkflowState.ReadyToPrint;

            btnCancel.IsEnabled =
                context.State != WorkflowState.Completed;

            VerificationPanel.Visibility = context.State == WorkflowState.WaitingForVerification
                ? Visibility.Visible
                : Visibility.Collapsed;

            txtVerificationPrompt.Text =
                $"Expected Data Matrix : {context.DataMatrix}";

            switch (context.State)
            {
                case WorkflowState.WaitingForKanban:
                    EnableOnly(txtKanban);
                    break;

                case WorkflowState.WaitingForLane:
                    EnableOnly(txtLane);
                    break;

                case WorkflowState.WaitingForTrolley:
                    EnableOnly(txtTrolley);
                    break;

                case WorkflowState.WaitingForLine:
                    EnableOnly(txtLine);
                    break;

                case WorkflowState.WaitingForOperator:
                    EnableOnly(txtOperator);
                    break;

                case WorkflowState.WaitingForVerification:
                    EnableOnly(txtVerification);
                    break;
            }

        }

        private void SetCheckmark(TextBlock check, TextBox textbox, bool hasData)
        {
            check.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;
            textbox.Background = hasData
                ? new SolidColorBrush(Color.FromRgb(220, 245, 220))  // light green
                : new SolidColorBrush(Colors.White);
        }

        private void Verification_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            ScanCompleted?.Invoke(this, txtVerification.Text);

            txtVerification.Clear();

            e.Handled = true;
        }
        private void EnableOnly(TextBox active)
        {
            txtOperator.IsEnabled = false;
            txtKanban.IsEnabled = false;
            txtLine.IsEnabled = false;
            txtTrolley.IsEnabled = false;
            txtLane.IsEnabled = false;
            txtVerification.IsEnabled = false;

            active.IsEnabled = true;
            active.Focus();
            active.SelectAll();
        }
        public void ClearFields()
        {
            txtOperator.Clear();
            txtKanban.Clear();
            txtLine.Clear();
            txtTrolley.Clear();
            txtLane.Clear();
            txtVerification.Clear();

            // Reset checkmarks and backgrounds
            chkKanban.Visibility = Visibility.Collapsed;
            chkLane.Visibility = Visibility.Collapsed;
            chkTrolley.Visibility = Visibility.Collapsed;
            chkLine.Visibility = Visibility.Collapsed;
            chkOperator.Visibility = Visibility.Collapsed;

            txtKanban.Background = new SolidColorBrush(Colors.White);
            txtLane.Background = new SolidColorBrush(Colors.White);
            txtTrolley.Background = new SolidColorBrush(Colors.White);
            txtLine.Background = new SolidColorBrush(Colors.White);
            txtOperator.Background = new SolidColorBrush(Colors.White);
        }

    }
}
