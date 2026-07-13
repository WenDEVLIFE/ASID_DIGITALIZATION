using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public enum WorkflowState
    {
        Ready,
        WaitingForOperator,
        WaitingForKanban,
        WaitingForLine,
        WaitingForTrolley,
        WaitingForLane,
        ReadyForValidation,
        Validating,
        ReadyToPrint,
        WaitingForVerification,
        Completed,
        Cancelled,
        Error
    }
}
