using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Services;
using ASID.Edge.Views.Controllers;								  
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.Dialogs;
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

        private bool _isListening;
		private readonly DashboardController _dashboardController;
       private readonly P2LoadingBayService _p2LoadingBayService =
            ServiceProvider.P2LoadingBay;

        public P2LoadingBayWorkStationView(TcpScannerService scanner)
        {
            InitializeComponent();
            P2LoadingBayPortal.ScanCompleted += P2LoadingBayPortal_ScanCompleted;
            _scanner = scanner;
			            _dashboardController =
                new DashboardController(
                    ServiceProvider.Dashboard,
                    TransactionHistory,
                    Inventory,
                    Withdrawal,
                    DailyDemand);

            TransactionHistory.RefreshRequested += (_, _) => _dashboardController.Refresh();
            DailyDemand.ImportCompleted += (_, _) => _dashboardController.Refresh();

            var workflow = new P2LoadingBayWorkflow();

            workflow.Start();


            _workflowManager.LoadWorkflow(workflow);

            workflow.Completed += Workflow_Completed;

            Loaded += (_, _) => RefreshUI();


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
				
				RefreshUI();
        }

        private void Scanner_BarcodeReceived(object? sender, string barcode)
        {
            Dispatcher.Invoke(() =>
            {
                if (_workflowManager.CurrentWorkflow is null)
                    return;

                _workflowManager.CurrentWorkflow.ProcessScan(barcode);

                WorkflowStatus.UpdateMessage(
                    _workflowManager.CurrentWorkflow.CurrentMessage);

            });

        }

        private async void Workflow_Completed(object? sender, EventArgs e)
        {

            var workflow =
                (P2LoadingBayWorkflow)_workflowManager.CurrentWorkflow!;

            _p2LoadingBayService.Commit(workflow.Context);

            AutoCloseMessageBox.Show("Success", "P2 Loading Bay Transaction Completed");

            await Task.Delay(3000);

            workflow.Reset();

            RefreshUI();
        }


        private void RefreshUI()
        {
            if (_workflowManager.CurrentWorkflow is not P2LoadingBayWorkflow workflow)
                return;
	 

            WorkflowStatus.UpdateMessage(
                workflow.CurrentMessage);

            _dashboardController.Refresh();				 
											 
        }

    }
}
