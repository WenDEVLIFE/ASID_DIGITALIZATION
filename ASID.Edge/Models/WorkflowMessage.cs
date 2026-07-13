using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class WorkflowMessage
    {
        public string Title { get; set; } = "";

        public string Message { get; set; } = "";

        public WorkflowMessageType Type { get; set; } =
            WorkflowMessageType.Information;
    }
}
