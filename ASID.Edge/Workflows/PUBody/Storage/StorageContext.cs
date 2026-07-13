using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Workflows.PUBody.Storage
{
    public class StorageContext
    {
        public string OperatorId { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public string LineNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";

        public string LaneNo { get; set; } = "";

        public string DataMatrix { get; set; } = "";

        public bool ThresholdValidated { get; set; }

        public bool LaneValidated { get; set; }

        public WorkflowState State { get; set; } =
            WorkflowState.WaitingForOperator;
    }
}
