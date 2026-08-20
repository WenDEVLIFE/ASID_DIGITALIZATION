
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
        private LaneSelectionDialog? _laneDialog;



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

            workflow.LaneSelectionRequested += Workflow_LaneSelectionRequested;

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

            TransactionHistory.RefreshRequested += (_, _) => _dashboardController.Refresh();
            DailyDemand.ImportCompleted += (_, _) => _dashboardController.Refresh();



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

        /// <summary>
        /// Raised by the workflow on entering the lane step (and on any
        /// re-scan while waiting for the lane). Queries the vacant lanes and
        /// shows the modal selection dialog; on confirmation, hands the
        /// selected lane back to the workflow.
        /// </summary>
        private void Workflow_LaneSelectionRequested(object? sender, EventArgs e)
        {
            // The dialog is modal and pumps the dispatcher; a hardware scan
            // arriving while it is open re-enters this handler. Skip it — the
            // dialog itself validates scans against the vacant list.
            if (_laneDialog != null)
                return;

            var vacantLanes =
                ServiceProvider.StorageValidation.GetVacantLanes();

            if (vacantLanes.Count == 0)
            {
                AutoCloseMessageBox.Show(
                    "No Vacant Lanes",
                    "All lanes are currently occupied. Please wait until a lane becomes vacant.");

                return;
            }

            var dialog = new LaneSelectionDialog(vacantLanes)
            {
                Owner = Window.GetWindow(this)
            };

            _laneDialog = dialog;

            var result = dialog.ShowDialog();

            _laneDialog = null;

            if (result == true)
            {
                if (_workflowManager.CurrentWorkflow is StorageWorkflow workflow)
                {
                    workflow.ConfirmLane(dialog.SelectedLane);

                    RefreshUI();
                }
            }
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

            _storageService.Commit(workflow.Context);

            AutoCloseMessageBox.Show("Success", "Storage Transaction Completed");

            await Task.Delay(3000);
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
