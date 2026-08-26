
//#define OFFLINE
using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Services;
using ASID.Edge.Views.Controllers;
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Workflows;
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


namespace ASID.Edge.Views.PUBody
{
    /// <summary>
    /// Interaction logic for WorkStationView.xaml
    /// </summary>
    public partial class StorageWorkStationView : UserControl
    {
        private readonly WorkflowManager _workflowManager = new();
        private readonly TcpScannerService _scanner;
        private bool _isListening;
        private readonly DashboardController _dashboardController;
        private readonly StorageService _storageService =
    ServiceProvider.Storage;



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

            _dashboardController =
                new DashboardController(
                    ServiceProvider.Dashboard,
                    TransactionHistory,
                    Inventory,
                    Withdrawal,
                    DailyDemand);
        }

        private void CheckAndShowLaneSequenceDialog()
        {
            if (_workflowManager.CurrentWorkflow is StorageWorkflow workflow &&
                workflow.CurrentState == WorkflowState.WaitingForLane)
            {
                try
                {
                    var seqDlg = new LaneSequenceDialog();
                    var window = Window.GetWindow(this);
                    if (window != null && window.IsLoaded && window.IsVisible)
                    {
                        seqDlg.Owner = window;
                    }

                    if (seqDlg.ShowDialog() == true && !string.IsNullOrEmpty(seqDlg.SelectedLane))
                    {
                        var barcodeDlg = new LaneBarcodeDialog(seqDlg.SelectedLane);
                        if (window != null && window.IsLoaded && window.IsVisible)
                        {
                            barcodeDlg.Owner = window;
                        }

                        if (barcodeDlg.ShowDialog() == true)
                        {
                            workflow.ProcessScan(barcodeDlg.LaneCode);
                            WorkflowStatus.UpdateMessage(workflow.CurrentMessage);
                            LoginPortal.UpdateFromContext(workflow.Context);
                            RefreshUI();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Toast?.Error($"Lane dialog error: {ex.Message}");
                }
            }
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

        /// <summary>Called by MainShellView when USB scanner detects a barcode.</summary>
        public void AcceptBarcode(string barcode)
        {
            Scanner_BarcodeReceived(this, barcode);
        }

        private void LoginPortal_ScanCompleted(object? sender, string barcode)
        {
            if (_workflowManager.CurrentWorkflow == null)
                return;

            _workflowManager.CurrentWorkflow.ProcessScan(barcode);

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow.CurrentMessage);

            RefreshUI();
            CheckAndShowLaneSequenceDialog();
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

                CheckAndShowLaneSequenceDialog();
            });

            //RefreshUI();
        }

        private void LoginPortal_ApplyRequested(object? sender, EventArgs e)
        {
            _workflowManager.CurrentWorkflow?.Apply();

            WorkflowStatus.UpdateMessage(
                _workflowManager.CurrentWorkflow!.CurrentMessage);

            LoginPortal.UpdateFromContext(
                ((StorageWorkflow)_workflowManager.CurrentWorkflow).Context);

            //RefreshUI();
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
                ["LINENO"] = workflow.Context.CellNo,
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
        private ToastNotification? _toast;
        private ToastNotification Toast => _toast ??= FindToast();
        private ToastNotification FindToast()
        {
            var w = Window.GetWindow(this) as MainWindow;
            return w?.MainShell?.Toasts ?? new ToastNotification();
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
                Toast.Warning("Transaction cancelled.");
                RefreshUI();
            }
        }


        private async void Workflow_Completed(object? sender, EventArgs e)
        {
            var workflow = (StorageWorkflow)_workflowManager.CurrentWorkflow!;

            try
            {
                _storageService.Commit(workflow.Context);
                Toast.Success("Storage Transaction Completed");
            }
            catch (Exception ex)
            {
                Toast.Error($"Storage failed: {ex.Message}");
            }

            await Task.Delay(2000);
            LoginPortal.ClearFields();
            workflow.Reset();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_workflowManager.CurrentWorkflow is not StorageWorkflow workflow)
                return;

            WorkflowStatus.UpdateMessage(
                workflow.CurrentMessage);

            LoginPortal.UpdateFromContext(
                workflow.Context);

            _dashboardController.Refresh();
        }
    }
}
