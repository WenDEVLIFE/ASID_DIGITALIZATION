using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class WorkflowContext
    {
        public string OperatorId { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public string LineNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";

        public string LaneNo { get; set; } = "";

        public string DataMatrix { get; set; } = "";

        public bool ThresholdValidated { get; set; }

        public bool LaneValidated { get; set; }

        public bool PrintCompleted { get; set; }

        public bool VerificationCompleted { get; set; }

        public WorkflowState State { get; set; } = WorkflowState.Ready;
    }
}
