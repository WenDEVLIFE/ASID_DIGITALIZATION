using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Memory;
using ASID.Edge.Services;
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Workflows.PUBody.P1LoadingBay;
using ASID.Edge.Workflows.PUBody.P2LoadingBay;
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


namespace ASID.Edge.Views.PUBody
{
    /// <summary>
    /// Interaction logic for WorkStationView.xaml
    /// </summary>
    public partial class P2LoadingBayWorkStationView : UserControl
    {
        private readonly WorkflowManager _workflowManager = new();
        private readonly TcpScannerService _scanner =new();
        private readonly MemoryTransactionRepository _transactionRepository = RepositoryProvider.Transactions;
        private readonly List<PUBodyTransactionHistoryItem> _history =
    RepositoryProvider.TransactionHistory;

        private readonly List<PUBodyInventoryItem> _inventory =
            RepositoryProvider.Inventory;

        private bool _isListening;

        public P2LoadingBayWorkStationView(TcpScannerService scanner)
        {
            InitializeComponent();
            P2LoadingBayPortal.ScanCompleted += P2LoadingBayPortal_ScanCompleted;
            // _scanner.BarcodeReceived += Scanner_BarcodeReceived;
            _scanner = scanner;
            //_ = _scanner.StartAsync();

            var workflow = new P2LoadingBayWorkflow();

            workflow.Start();
            RefreshUI();
            Loaded += (_, _) => RefreshUI();

            _workflowManager.LoadWorkflow(workflow);

            workflow.Completed += Workflow_Completed;

            WorkflowStatus.UpdateMessage(workflow.CurrentMessage);


        }

        public void Activate()
        {
            if (_isListening)
                return;

            _scanner.BarcodeReceived += Scanner_BarcodeReceived;

            _isListening = true;
        }

        public void Deactivate()
        {
            if (!_isListening)
                return;

            _scanner.BarcodeReceived -= Scanner_BarcodeReceived;

            _isListening = false;
        }

        private void P2LoadingBayPortal_ScanCompleted(
                object? sender,
                string barcode)
        {
            if (_workflowManager.CurrentWorkflow == null)
                return;

            _workflowManager.CurrentWorkflow.ProcessScan(barcode);

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow.CurrentMessage);
        }

        private void Scanner_BarcodeReceived(object? sender, string barcode)
        {
            Dispatcher.Invoke(() =>
            {
                if (_workflowManager.CurrentWorkflow == null)
                    return;

                _workflowManager.CurrentWorkflow.ProcessScan(barcode);

                WorkflowStatus.UpdateMessage(
                    _workflowManager.CurrentWorkflow.CurrentMessage);

                //WithdrawalPortal.UpdateFromContext(
                //    ((StorageWorkflow)_workflowManager.CurrentWorkflow).Context);
            });

            //RefreshUI();
        }

        private void Workflow_Completed(object? sender, EventArgs e)
        {

            //MessageBox.Show("Workflow_Completed fired");

            var workflow =
                (P2LoadingBayWorkflow)_workflowManager.CurrentWorkflow!;

            //MessageBox.Show(workflow.Context.Transaction.DataMatrix);

            var transaction = workflow.Context.Transaction;

            transaction.Status = MaterialStatus.ForPickup;

            //MessageBox.Show(transaction.Status.ToString());


            var inventory = RepositoryProvider.Inventory
            .FirstOrDefault(x =>
                x.Model == transaction.Model &&
                x.PartNo == transaction.PartNo);

            var history = RepositoryProvider.TransactionHistory
                .FirstOrDefault(x =>
                    x.SerialNo == transaction.SerialNo);

            if (history != null)
            {
                history.Status = MaterialStatus.ForPickup;
                transaction.Status = MaterialStatus.ForPickup;
            }

            if (inventory != null)
            {

                inventory.InventoryFloating -= transaction.SNP;

                inventory.InventoryByLocation_P2LoadingBay += transaction.SNP;
            }

                    Inventory.Load(
            RepositoryProvider.Inventory);

            TransactionHistory.Load(
                RepositoryProvider.TransactionHistory);

            MessageBox.Show("P2 Loading Bay Transaction Completed");

            RefreshUI();
        }
            

        private void RefreshUI()
        {
            if (_workflowManager.CurrentWorkflow is P2LoadingBayWorkflow workflow)
            {
                WorkflowStatus.UpdateMessage(workflow.CurrentMessage);
            }

            //TransactionHistory.Load(_history);

            //Inventory.Load(_inventory);
            TransactionHistory.Load(RepositoryProvider.TransactionHistory);

            Inventory.Load(RepositoryProvider.Inventory);

            Withdrawal.Load(RepositoryProvider.Withdrawal);

            DailyDemand.Load(RepositoryProvider.DailyDemand);

        }

    }
}
