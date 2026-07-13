using ASID.Edge.Models;
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

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for WithdrawalControl.xaml
    /// </summary>
    public partial class WithdrawalControl : UserControl
    {
        public WithdrawalControl()
        {
            InitializeComponent();
        }
        private bool modelAsc = true;
        private bool dateAsc = true;

        public void Load(IEnumerable<PUBodyWithdrawalItem> items)
        {
            WithdrawalGrid.ItemsSource = null;

            WithdrawalGrid.ItemsSource = items.ToList();
        }


        private void SortByModel(object sender, RoutedEventArgs e)
        {
            if (WithdrawalGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(WithdrawalGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "ModelName",   // ⚠️ must match binding
                modelAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            modelAsc = !modelAsc;
        }

        private void SortByDate(object sender, RoutedEventArgs e)
        {
            if (WithdrawalGrid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(WithdrawalGrid.ItemsSource);
            view.SortDescriptions.Clear();

            view.SortDescriptions.Add(new SortDescription(
                "Date",
                dateAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));

            dateAsc = !dateAsc;
        }

    }

}
