using ASID.Edge.Models;
using ASID.Edge.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ASID.Edge.Workflows.PUBody.Storage
{
    public class StorageWorkflow : IWorkflow
    {
        private readonly StorageContext _context = new();
        public StorageContext Context => _context;
        public WorkflowState CurrentState => _context.State;

        private readonly StorageValidationService _validation =
            ServiceProvider.StorageValidation;
        public event EventHandler? WorkflowChanged;
        public event EventHandler? Completed;
        public event EventHandler? LaneSelectionRequested;

        private readonly DataMatrixService _dmService = new();


        private void NotifyChanged()
        {
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }
        private void NotifyCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }


        public bool IsCompleted => _context.State == WorkflowState.Completed;


        public WorkflowMessage CurrentMessage
        {
            get
            {
                return _context.State switch
                {
                    WorkflowState.WaitingForOperator =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Please Scan Operator ID",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.WaitingForKanban =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Please Scan Kanban",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.WaitingForLine =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Please Scan Line Number",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.WaitingForTrolley =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Please Scan Trolley Number",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.WaitingForLane =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Please Scan Lane Number",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.ReadyForValidation =>
                        new WorkflowMessage
                        {
                            Title = "REVIEW",
                            Message = "Press APPLY CHANGES to Validate",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.Validating =>
                        new WorkflowMessage
                        {
                            Title = "VALIDATING",
                            Message = "Checking Inventory Threshold...",
                            Type = WorkflowMessageType.Warning
                        },

                    WorkflowState.ReadyToPrint =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Press PRINT to Generate Data Matrix",
                            Type = WorkflowMessageType.Success
                        },

                    WorkflowState.WaitingForVerification => new WorkflowMessage
                    {
                        Title = "VERIFY",
                        Message = $"Scan Printed Label\n{_context.DataMatrix}",
                        Type = WorkflowMessageType.Warning
                    },

                    WorkflowState.Completed => new WorkflowMessage
                    {
                            Title = "SUCCESS",
                            Message = "Storage transaction completed successfully.",
                            Type = WorkflowMessageType.Success
                    },

                    WorkflowState.Error =>
                        new WorkflowMessage
                        {
                            Title = "ERROR",
                            Message = "Workflow Error",
                            Type = WorkflowMessageType.Error
                        },

                    _ =>
                        new WorkflowMessage
                        {
                            Title = "READY",
                            Message = "Waiting...",
                            Type = WorkflowMessageType.Information
                        }
                };
            }
        }

        public void Start()
        {
            _context.State = WorkflowState.WaitingForOperator;
            NotifyChanged();
        }

        public void ProcessScan(string barcode)
        {

            if (_context.State == WorkflowState.WaitingForVerification)

            {
                if (barcode == _context.DataMatrix)
                {
                    _context.State = WorkflowState.Completed;
                    NotifyChanged();

                    NotifyCompleted();
                }
                else
                {
                    _context.State = WorkflowState.Error;
                    NotifyChanged();
                }

                return;
            }

            switch (_context.State)
            {
                case WorkflowState.WaitingForOperator:
                    HandleOperator(barcode);
                    break;

                case WorkflowState.WaitingForKanban:
                    HandleKanban(barcode);
                    break;

                case WorkflowState.WaitingForLine:
                    HandleLine(barcode);
                    break;

                case WorkflowState.WaitingForTrolley:
                    HandleTrolley(barcode);
                    break;

                case WorkflowState.WaitingForLane:
                    HandleLane(barcode);
                    break;


            }
        }

        private void HandleOperator(string barcode)
        {
            _context.OperatorId = barcode;
            _context.State = WorkflowState.WaitingForKanban;
            NotifyChanged();
        }
        private void HandleKanban(string barcode)
        {
            _context.KanbanNo = barcode;
            _context.State = WorkflowState.WaitingForLine;
            NotifyChanged();
        }
        private void HandleLine(string barcode)
        {
            _context.LineNo = barcode;
            _context.State = WorkflowState.WaitingForTrolley;
            NotifyChanged();
        }
        private void HandleTrolley(string barcode)
        {
            _context.TrolleyNo = barcode;
            _context.State = WorkflowState.WaitingForLane;
            NotifyChanged();

            // The view owns the vacancy UI: it queries vacant lanes,
            // shows the selection popup and calls ConfirmLane with the result.
            LaneSelectionRequested?.Invoke(this, EventArgs.Empty);
        }
        private void HandleLane(string barcode)
        {
            // While waiting for the lane, any scan re-triggers the vacancy
            // popup (fresh vacancy query). The lane is only confirmed through
            // ConfirmLane, which is called by the view after the operator
            // selects a vacant lane in the popup.
            LaneSelectionRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Confirms the selected lane and moves the workflow to validation.
        /// Only valid while waiting for the lane scan.
        /// </summary>
        public void ConfirmLane(string laneNo)
        {
            if (_context.State != WorkflowState.WaitingForLane)
                return;

            _context.LaneNo = laneNo;
            _context.State = WorkflowState.ReadyForValidation;
            NotifyChanged();
        }
        public void Apply()
        {
            _context.State = WorkflowState.Validating;

            NotifyChanged();

            var threshold =
                _validation.CheckInventoryThreshold(_context.KanbanNo);

            _context.ThresholdValidated = threshold.Success;

            if (!threshold.Success)
            {
                _context.State = WorkflowState.Error;

                NotifyChanged();

                return;
            }

            var lane =
                _validation.CheckAssignedLane(_context.LaneNo);

            _context.LaneValidated = lane.Success;

            if (!lane.Success)
            {
                _context.State = WorkflowState.Error;

                NotifyChanged();

                return;
            }

            _context.State = WorkflowState.ReadyToPrint;

            NotifyChanged();
        }

        public void Print()
        {
            if (_context.State != WorkflowState.ReadyToPrint)
                return;

            var parser = new KanbanParser();

            var kanban =
                parser.Parse(_context.KanbanNo);

            var dm = new DataMatrixData
            {
                TransactionId = DateTime.Now.ToString("HHmmss"),

                PartNo = kanban.PartNo,

                KanbanNo = kanban.KanbanNo,

                Quantity = kanban.Quantity,

                Model = kanban.Model,

                Location = "AZP",

                Timestamp = DateTime.Now
            };

            _context.DataMatrix =
                _dmService.Generate(dm);

            Console.WriteLine(_context.DataMatrix);

            _context.State =
                WorkflowState.WaitingForVerification;

            NotifyChanged();
        }

        public void Cancel()
        {
            Reset();
        
        }
        public void Reset()
        {
            _context.OperatorId = "";
            _context.KanbanNo = "";
            _context.LineNo = "";
            _context.TrolleyNo = "";
            _context.LaneNo = "";
            _context.DataMatrix = "";
            _context.ThresholdValidated = false;
            _context.LaneValidated = false;

            Start();

            NotifyChanged();
        }

    }
}
