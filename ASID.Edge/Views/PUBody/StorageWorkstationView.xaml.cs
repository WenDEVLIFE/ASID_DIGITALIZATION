
//#define OFFLINE
using ASID.Edge.Models;
using ASID.Edge.Repositories.Memory;
using ASID.Edge.Services;
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.Dialogs;
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
using System.Windows.Shapes;
using ASID.Edge.Repositories;


namespace ASID.Edge.Views.PUBody
{
    /// <summary>
    /// Interaction logic for WorkStationView.xaml
    /// </summary>
    public partial class StorageWorkStationView : UserControl
    {
        private readonly WorkflowManager _workflowManager = new();
        private readonly TcpScannerService _scanner;
        private readonly MemoryTransactionRepository _transactionRepository = RepositoryProvider.Transactions;

        private readonly StorageService _storageService;

        private readonly List<PUBodyTransactionHistoryItem> _history =
            RepositoryProvider.TransactionHistory;

        private readonly List<PUBodyInventoryItem> _inventory =
            RepositoryProvider.Inventory;

        private readonly List<PUBodyDailyDemandItem> _dailyDemand =
    RepositoryProvider.DailyDemand;

        private bool _isListening;


        public StorageWorkStationView(TcpScannerService scanner)
        {
            InitializeComponent();
            LoginPortal.ScanCompleted += LoginPortal_ScanCompleted;
            LoginPortal.ApplyRequested += LoginPortal_ApplyRequested;
            LoginPortal.PrintRequested += LoginPortal_PrintRequested;
            LoginPortal.CancelRequested += LoginPortal_CancelRequested;
            //Loaded += WorkStationView_Loaded;
            _scanner = scanner;

            var workflow = new StorageWorkflow();

            workflow.Start();
            Loaded += (_, _) => RefreshUI();

            _workflowManager.LoadWorkflow(workflow);
            workflow.Completed += Workflow_Completed;

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow!.CurrentMessage);

            WorkflowStatus.UpdateMessage(workflow.CurrentMessage);

            _storageService = new StorageService(_transactionRepository);


        }
        private void WorkStationView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI();
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



        private void LoginPortal_ScanCompleted(object? sender, string barcode)
        {
            if (_workflowManager.CurrentWorkflow == null)
                return;

            _workflowManager.CurrentWorkflow.ProcessScan(barcode);

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow.CurrentMessage);

            RefreshUI();

        }

        private void Scanner_BarcodeReceived(object? sender, string barcode)
        {
            Dispatcher.Invoke(() =>
            {
                _workflowManager.CurrentWorkflow!
                    .ProcessScan(barcode);
                
                WorkflowStatus.UpdateMessage(
                    _workflowManager.CurrentWorkflow.CurrentMessage);
                
                LoginPortal.UpdateFromContext(
                    ((StorageWorkflow)_workflowManager.CurrentWorkflow).Context);
            });

            RefreshUI();
        }

        private void LoginPortal_ApplyRequested(object? sender, EventArgs e)
        {
            _workflowManager.CurrentWorkflow?.Apply();

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow!.CurrentMessage);

            LoginPortal.UpdateFromContext(
                ((StorageWorkflow)_workflowManager.CurrentWorkflow).Context);

            RefreshUI();
        }

        private void LoginPortal_PrintRequested(object? sender, EventArgs e)
        {
            if (_workflowManager.CurrentWorkflow is not StorageWorkflow workflow)
                return;

            workflow.Print();

            var parser = new KanbanParser();
            var kanban = parser.Parse(workflow.Context.KanbanNo);
            var tokens = new Dictionary<string, string>
            {
                ["DATAMATRIX"] = workflow.Context.DataMatrix,
                ["PARTNO"] = kanban.PartNo,
                ["MODEL"] = kanban.Model,
                ["KANBAN"] = kanban.KanbanNo,
                ["QTY"] = kanban.Quantity.ToString(),
                ["LINENO"] = workflow.Context.LineNo,
                ["LANENO"] = workflow.Context.LaneNo,
                ["TROLLEYNO"] = workflow.Context.TrolleyNo
            };

            var zpl = new LabelTemplateService()
                .LoadTemplate(
                    "StorageLabelTemplate.txt",
                    tokens);


            //var dialog = new PrintPreviewDialog(workflow.Context.DataMatrix);

            var dialog = new PrintPreviewDialog(zpl);

            dialog.Owner = Window.GetWindow(this);

            bool? result = dialog.ShowDialog();

            if (result == true)
            {

#if !OFFLINE
                var printer = new PrinterService();

                printer.Print(zpl);
#endif
            }

            RefreshUI();
        }
        private void LoginPortal_CancelRequested(object? sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Cancel current transaction?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_workflowManager.CurrentWorkflow is StorageWorkflow workflow)
            {
                LoginPortal.ClearFields();

                workflow.Cancel();

                RefreshUI();
            }
        }


        private async void Workflow_Completed(object? sender, EventArgs e)
        {
            var workflow = (StorageWorkflow)_workflowManager.CurrentWorkflow!;

            var item = _storageService.Commit(workflow.Context);

            //Update transaction history

            _history.Insert(0, item);

            //Update Daily Demand

            var dailyDemand = RepositoryProvider.DailyDemand
                .FirstOrDefault(x =>
                    x.PartNo == item.PartNo &&
                    x.Date == item.Date);

            if (dailyDemand == null)
            {
                dailyDemand = new PUBodyDailyDemandItem
                {
                    Date = item.Date,
                    Model = item.Model,
                    PartNo = item.PartNo,
                    Demand = item.SNP,
                    DeliveredToP1 = 0
                };

                //RepositoryProvider.DailyDemand.Add(demand);
                _dailyDemand.Add(dailyDemand);
            }
            else
            {
                dailyDemand.Demand += item.SNP;
            }


            //Update Inventory

            var inventoryItem = _inventory.FirstOrDefault(x =>
                x.Model == item.Model &&
                x.PartNo == item.PartNo);

            if (inventoryItem == null)
            {
                inventoryItem = new PUBodyInventoryItem
                {
                    Model = item.Model,
                    PartNo = item.PartNo,
                    InventoryByLocation_PUBodySupermarket = item.SNP
                };

                _inventory.Add(inventoryItem);
            }
            else
            {
                inventoryItem.InventoryByLocation_PUBodySupermarket += item.SNP;
            }


            TransactionHistory.Load(_history);
            Inventory.Load(_inventory);
            DailyDemand.Load(_dailyDemand);

            MessageBox.Show("Storage Transaction Completed");

            await Task.Delay(3000);
            LoginPortal.ClearFields();
            workflow.Reset();
            RefreshUI();

            //debug
            var transactions = RepositoryProvider.Transactions.GetAll();

            var text = string.Join(Environment.NewLine,
                transactions.Select(t =>
                    $"{t.DataMatrix} | {t.Model} | {t.SerialNo}"));

            //MessageBox.Show(text);

        }

        private void RefreshUI()
        {
            var workflow = (StorageWorkflow)_workflowManager.CurrentWorkflow!;

            WorkflowStatus.UpdateMessage(workflow.CurrentMessage);

            LoginPortal.UpdateFromContext(workflow.Context);

            //TransactionHistory.Load(_history);

            //Inventory.Load(_inventory);

            TransactionHistory.Load(RepositoryProvider.TransactionHistory);

            Inventory.Load(RepositoryProvider.Inventory);

            Withdrawal.Load(RepositoryProvider.Withdrawal);

            DailyDemand.Load(RepositoryProvider.DailyDemand);
        }
    }
}
