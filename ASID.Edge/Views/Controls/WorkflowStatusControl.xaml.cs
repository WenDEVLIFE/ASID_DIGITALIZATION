using ASID.Edge.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for WorkflowStatusControl.xaml
    /// </summary>
    public partial class WorkflowStatusControl : UserControl
    {
        public WorkflowStatusControl()
        {
            InitializeComponent();
        }

        public void UpdateMessage(WorkflowMessage message)
        {
            TitleText.Text = message.Title;

            MessageText.Text = message.Message;
        }
    }
}
