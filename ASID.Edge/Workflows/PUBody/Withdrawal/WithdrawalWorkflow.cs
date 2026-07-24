using ASID.Edge.Models;
using ASID.Edge.Services;
using ASID.Edge.Validation;
using ASID.Edge.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ASID.Edge.Workflows.PUBody.Withdrawal
{
    public class WithdrawalWorkflow : IWorkflow
    {
        private readonly WithdrawalContext _context = new();
        public WithdrawalContext Context => _context;
        public WorkflowState CurrentState => _context.State;
        private readonly WithdrawalValidationService _validation = new();

        public bool IsCompleted => false;
        public event EventHandler? WorkflowChanged;
        public event EventHandler? Completed;

        public DataMatrixData? Data { get; set; }
        private void NotifyChanged()
        {
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }
        private void NotifyCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
        public WorkflowMessage CurrentMessage
        {
            get
            {
                return _context.State switch
                {

                    WorkflowState.ReadyForValidation =>
                        new WorkflowMessage
                        {
                            Title = "REVIEW",
                            Message = "Press Enter to Validate",
                            Type = WorkflowMessageType.Information
                        },

                    WorkflowState.Validating =>
                        new WorkflowMessage
                        {
                            Title = "VALIDATING",
                            Message = "Checking Serial No...",
                            Type = WorkflowMessageType.Warning
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
                        Message = "Withdrawal transaction completed successfully.",
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
            _context.State = WorkflowState.WaitingForVerification;
            NotifyChanged();
        }

        public void ProcessScan(string barcode)
        {
            _context.State = WorkflowState.Validating;

            NotifyChanged();

            var result = _validation.Validate(barcode);

            if (!result.Success)
            {
                _context.State = WorkflowState.Error;

                NotifyChanged();

                AutoCloseMessageBox.Show("Error", result.Message);

                return;
            }

            _context.Transaction = result.Transaction;

            _context.State = WorkflowState.Completed;

            NotifyChanged();

            NotifyCompleted();

        }

        public void Apply() { }

        public void Print() { }

        public void Cancel() { }

        public void Reset()
        {
            _context.DataMatrix = "";

            Start();
        }
   


    }
}
