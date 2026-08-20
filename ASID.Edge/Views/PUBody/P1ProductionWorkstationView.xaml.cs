using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Repositories.Interfaces;
using ASID.Edge.Services;
using ASID.Edge.Views.Controllers;								  
using ASID.Edge.Views.Controls;
using ASID.Edge.Views.Dialogs;
using ASID.Edge.Workflows.PUBody.P1Production;  										  
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
    public partial class P1ProductionWorkStationView : UserControl
    {
        private readonly WorkflowManager _workflowManager = new();
        private readonly TcpScannerService _scanner =new();					 

        private bool _isListening;
		private readonly DashboardController _dashboardController;
       private readonly P1ProductionService _p1ProductionService =
            ServiceProvider.P1Production;

        public P1ProductionWorkStationView(TcpScannerService scanner)
        {
            InitializeComponent();
            P1ProductionPortal.ScanCompleted += P1ProductionPortal_ScanCompleted;
																   
            _scanner = scanner;
			            _dashboardController =
                new DashboardController(
                    ServiceProvider.Dashboard,
                    TransactionHistory,
                    Inventory,
                    Withdrawal,
                    DailyDemand);							

            var workflow = new P1ProductionWorkflow();

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

        private void P1ProductionPortal_ScanCompleted(
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
                (P1ProductionWorkflow)_workflowManager.CurrentWorkflow!;

            _p1ProductionService.Commit(workflow.Context);

            AutoCloseMessageBox.Show("Success", "P1 Production Transaction Completed");

            await Task.Delay(3000);

            workflow.Reset();
																	

            RefreshUI();
        }


        private void RefreshUI()
        {
            if (_workflowManager.CurrentWorkflow is not P1ProductionWorkflow workflow)
                return;

            WorkflowStatus.UpdateMessage(
                workflow.CurrentMessage);

            _dashboardController.Refresh();				 
	 
											 
        }

    }
}
