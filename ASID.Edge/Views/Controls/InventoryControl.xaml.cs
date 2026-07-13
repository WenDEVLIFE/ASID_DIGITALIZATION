using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ASID.Edge.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for InventoryControl.xaml
    /// </summary>
    public partial class InventoryControl : UserControl
    {
        public InventoryControl()
        {
            InitializeComponent();
        }
        public void Load(IEnumerable<PUBodyInventoryItem> items)
        {
            MyDataGrid.ItemsSource = null;
            MyDataGrid.ItemsSource = items.ToList();
        }


        private bool modelAsc = true;

        private void SortByModel(object sender, RoutedEventArgs e)
        {
            if (MyDataGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(MyDataGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Model",
                modelAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            modelAsc = !modelAsc;
        }
    }
}


