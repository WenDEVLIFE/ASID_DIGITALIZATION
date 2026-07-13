using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Workflows
{
    public interface IWorkflow
    {

        WorkflowMessage CurrentMessage { get; }

        WorkflowState CurrentState { get; }

        bool IsCompleted { get; }

        void Start();

        void ProcessScan(string barcode);

        void Apply();

        void Cancel();

        void Print();

        event EventHandler? WorkflowChanged;

        event EventHandler? Completed;

    }
}
