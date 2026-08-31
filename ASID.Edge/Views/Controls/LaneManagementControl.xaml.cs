using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ASID.Edge.Views.Controls
{
    public partial class LaneManagementControl : UserControl
    {
        private System.Windows.Threading.DispatcherTimer? _autoRefreshTimer;

        public LaneManagementControl()
        {
            InitializeComponent();
            Loaded += LaneManagementControl_Loaded;
        }

        private void LaneManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            StartAutoRefresh();
        }

        private void LaneManagementControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopAutoRefresh();
        }

        public void Load()
        {
            LoadData();
        }

        /// <summary>Auto-refresh every 5 seconds to pick up storage/withdrawal changes</summary>
        private void StartAutoRefresh()
        {
            StopAutoRefresh();
            _autoRefreshTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _autoRefreshTimer.Tick += (_, _) => LoadData();
            _autoRefreshTimer.Start();
        }

        private void StopAutoRefresh()
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer = null;
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

                int updatedCount = 0;

                foreach (var lane in lanes)
                {
                    // Recalculate color/status based on quantities
                    if (lane.ActualStoredQty >= lane.MaxQtyStored)
                    {
                        lane.LaneStatus = "Full";
                        lane.ColorStatus = "Red";
                    }
                    else if (lane.ActualStoredQty > 0 && lane.PartNo != "Not Assigned")
                    {
                        lane.LaneStatus = "Occupied";
                        lane.ColorStatus = "Green";
                    }
                    else if (lane.ActualStoredQty == 0 && lane.PartNo != "Not Assigned")
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
                    updatedCount++;
                }

                MessageBox.Show($"Successfully saved {updatedCount} lane configurations!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving lane data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditLane_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button btn) return;
            if (btn.Tag is not LaneManagement selectedLane) return;

            OpenEditDialog(selectedLane);
        }

        /// <summary>Double-click a row to open the edit dialog</summary>
        private void DgLanes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgLanes.SelectedItem is not LaneManagement selectedLane)
                return;

            OpenEditDialog(selectedLane);
        }

        private void OpenEditDialog(LaneManagement selectedLane)
        {
            var freshLane = RepositoryProvider.LaneManagement.GetByLaneNo(selectedLane.LaneNo);
            if (freshLane == null)
            {
                MessageBox.Show("Lane not found in database.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new LaneEditDialog(freshLane);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.WasSaved)
            {
                LoadData();
            }
        }

        private void BtnDeleteLane_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not LaneManagement selectedLane) return;

            // Confirm delete
            var result = MessageBox.Show(
                $"Are you sure you want to delete lane '{selectedLane.LaneNo}'?\n\nThis will reset the lane to Not Assigned.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Reset lane to defaults instead of deleting the row
                selectedLane.PartNo = "Not Assigned";
                selectedLane.ActualStoredQty = 0;
                selectedLane.WithdrawnQty = 0;
                selectedLane.LaneStatus = "Not Assigned";
                selectedLane.ColorStatus = "Gray";

                RepositoryProvider.LaneManagement.Update(selectedLane);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting lane: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
