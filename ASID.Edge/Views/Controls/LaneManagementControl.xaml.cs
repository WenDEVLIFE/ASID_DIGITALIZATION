using ASID.Edge.Models;
using ASID.Edge.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ASID.Edge.Views.Controls
{
    public partial class LaneManagementControl : UserControl
    {
        public LaneManagementControl()
        {
            InitializeComponent();
            Loaded += LaneManagementControl_Loaded;
        }

        private void LaneManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        public void Load()
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Seed default lanes if empty
                RepositoryProvider.LaneManagement.SeedDefaultLanes();

                var lanes = RepositoryProvider.LaneManagement.GetAll();
                dgLanes.ItemsSource = lanes;
                txtRecordCount.Text = $"Records: {lanes.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading lane data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lanes = dgLanes.ItemsSource as IEnumerable<LaneManagement>;
                if (lanes == null) return;

                foreach (var lane in lanes)
                {
                    // Recalculate color based on outstanding qty
                    if (lane.OutstandingQty >= lane.MaxQtyStored)
                    {
                        lane.LaneStatus = "Full";
                        lane.ColorStatus = "Red";
                    }
                    else if (lane.OutstandingQty > 0)
                    {
                        lane.LaneStatus = "Occupied";
                        lane.ColorStatus = "Green";
                    }
                    else if (lane.PartNo != "Not Assigned")
                    {
                        lane.LaneStatus = "Vacant";
                        lane.ColorStatus = "Green";
                    }
                    else
                    {
                        lane.LaneStatus = "Not Assigned";
                        lane.ColorStatus = "Gray";
                    }

                    RepositoryProvider.LaneManagement.Update(lane);
                }

                MessageBox.Show("Lane configuration saved successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData(); // Refresh
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving lane data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
