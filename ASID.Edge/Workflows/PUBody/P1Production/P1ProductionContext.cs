using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Workflows.PUBody.P1Production
{
    public class P1ProductionContext
    {
        public WorkflowState State { get; set; }

        public string DataMatrix { get; set; } = "";

        public StorageTransaction? Transaction { get; set; }

        public string ValidationMessage { get; set; } = "";

    }
}
