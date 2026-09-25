using ASID.Edge.Models;
using System;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class TransactionEditDialog : Window
    {
        /// <summary>
        /// The mutated transaction. Persisting it (RepositoryProvider.Transactions.UpdateDetails)
        /// is the caller's responsibility so the dialog stays free of data-access concerns.
        /// </summary>
        public StorageTransaction UpdatedTransaction { get; }

        public TransactionEditDialog(StorageTransaction transaction)
        {
            InitializeComponent();

            UpdatedTransaction = transaction;

            // Read-only context
            txtDataMatrix.Text = transaction.DataMatrix;
            txtStatus.Text = transaction.Status.ToString();

            // Editable fields
            txtModel.Text = transaction.Model;
            txtPartNo.Text = transaction.PartNo;
            txtSerialNo.Text = transaction.SerialNo;
            txtSNP.Text = transaction.SNP.ToString();
            txtOperator.Text = transaction.OperatorId;
            txtLineNo.Text = transaction.LineNo;
            txtTrolleyNo.Text = transaction.TrolleyNo;
            txtLaneNo.Text = transaction.LaneNo;

            var created = transaction.CreatedAt == default ? DateTime.Now : transaction.CreatedAt;
            dpDate.SelectedDate = created.Date;
            txtTime.Text = created.ToString("HH:mm:ss");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";

            string model = txtModel.Text?.Trim() ?? "";
            string partNo = txtPartNo.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(model))
            {
                ShowError("Model is required.");
                return;
            }

            if (string.IsNullOrEmpty(partNo))
            {
                ShowError("Part No is required.");
                return;
            }

            if (!int.TryParse(txtSNP.Text?.Trim(), out int snp) || snp <= 0)
            {
                ShowError("SNP must be a positive integer.");
                return;
            }

            if (dpDate.SelectedDate is not DateTime date)
            {
                ShowError("Please choose a valid date.");
                return;
            }

            if (!TimeSpan.TryParse(txtTime.Text?.Trim(), out var time))
            {
                ShowError("Time must be a valid value (HH:mm:ss).");
                return;
            }

            UpdatedTransaction.Model = model;
            UpdatedTransaction.PartNo = partNo;
            UpdatedTransaction.SerialNo = txtSerialNo.Text?.Trim() ?? "";
            UpdatedTransaction.SNP = snp;
            UpdatedTransaction.OperatorId = txtOperator.Text?.Trim() ?? "";
            UpdatedTransaction.LineNo = txtLineNo.Text?.Trim() ?? "";
            UpdatedTransaction.TrolleyNo = txtTrolleyNo.Text?.Trim() ?? "";
            UpdatedTransaction.LaneNo = txtLaneNo.Text?.Trim() ?? "";
            UpdatedTransaction.CreatedAt = date.Date + time;

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
        }
    }
}
