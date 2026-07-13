using ASID.Edge.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Services
{
    public class WorkflowManager
    {
        public IWorkflow? CurrentWorkflow { get; private set; }

        public void LoadWorkflow(IWorkflow workflow)
        {
            CurrentWorkflow = workflow;
            CurrentWorkflow.Start();
        }
    }
}
