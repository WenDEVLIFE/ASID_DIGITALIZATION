using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ASID.Edge.Views.Controls
{
    public partial class LaneSequenceControl : UserControl
    {
        public event EventHandler<string>? LaneSelected;

        // Lane occupancy: lane_no -> open transaction count
        private Dictionary<string, int> _occupancy = new(StringComparer.OrdinalIgnoreCase);

        private const int TotalLanesPerColumn = 50; // A-01 to A-50, B-01 to B-50

        // Colors matching the design spec
        private static readonly SolidColorBrush VacantBrush = new(Color.FromRgb(46, 204, 113));   // Green
        private static readonly SolidColorBrush FullBrush = new(Color.FromRgb(231, 76, 60));       // Red
        private static readonly SolidColorBrush NotAssignedBrush = new(Color.FromRgb(189, 195, 199)); // Gray
        private static readonly SolidColorBrush VacantBorderBrush = new(Color.FromRgb(39, 174, 96));
        private static readonly SolidColorBrush FullBorderBrush = new(Color.FromRgb(192, 57, 43));
        private static readonly SolidColorBrush NotAssignedBorderBrush = new(Color.FromRgb(149, 165, 166));

        public LaneSequenceControl()
        {
            InitializeComponent();
            Loaded += LaneSequenceControl_Loaded;
        }

        private void LaneSequenceControl_Loaded(object sender, RoutedEventArgs e)
        {
            BuildLaneButtons();
        }

        /// <summary>
        /// Update occupancy data and refresh lane colors.
        /// Call this before showing the dialog.
        /// </summary>
        public void SetOccupancy(Dictionary<string, int> occupancy)
        {
            _occupancy = occupancy;
            RefreshLaneColors();
        }

        private void BuildLaneButtons()
        {
            ColumnA.Children.Clear();
            ColumnB.Children.Clear();

            for (int i = 1; i <= TotalLanesPerColumn; i++)
            {
                string laneA = $"A-{i:D2}";
                string laneB = $"B-{i:D2}";

                ColumnA.Children.Add(CreateLaneButton(laneA));
                ColumnB.Children.Add(CreateLaneButton(laneB));
            }

            RefreshLaneColors();
        }

        private Button CreateLaneButton(string laneName)
        {
            var btn = new Button
            {
                Content = laneName,
                Tag = laneName,
                Height = 28,
                Margin = new Thickness(0, 1, 0, 1),
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(1.5)
            };

            btn.Click += Lane_Click;
            return btn;
        }

        private void RefreshLaneColors()
        {
            foreach (var btn in GetAllLaneButtons())
            {
                string laneName = (string)btn.Tag;
                LaneStatus status = GetLaneStatus(laneName);

                switch (status)
                {
                    case LaneStatus.Vacant:
                        btn.Background = VacantBrush;
                        btn.Foreground = Brushes.White;
                        btn.BorderBrush = VacantBorderBrush;
                        btn.IsEnabled = true;
                        break;

                    case LaneStatus.Full:
                        btn.Background = FullBrush;
                        btn.Foreground = Brushes.White;
                        btn.BorderBrush = FullBorderBrush;
                        btn.IsEnabled = true; // Clickable but will show error
                        break;

                    case LaneStatus.NotAssigned:
                        btn.Background = NotAssignedBrush;
                        btn.Foreground = Brushes.White;
                        btn.BorderBrush = NotAssignedBorderBrush;
                        btn.IsEnabled = true;
                        break;
                }
            }
        }

        private LaneStatus GetLaneStatus(string laneName)
        {
            if (_occupancy.TryGetValue(laneName, out int openCount) && openCount > 0)
                return LaneStatus.Full;

            if (_occupancy.ContainsKey(laneName))
                return LaneStatus.Vacant;

            return LaneStatus.NotAssigned;
        }

        private IEnumerable<Button> GetAllLaneButtons()
        {
            foreach (Button btn in ColumnA.Children)
                yield return btn;
            foreach (Button btn in ColumnB.Children)
                yield return btn;
        }

        private void Lane_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string laneName)
                return;

            LaneStatus status = GetLaneStatus(laneName);

            if (status == LaneStatus.Full)
            {
                MessageBox.Show(
                    $"Lane {laneName} is FULL and can't be used.\nPlease select a Vacant or Not Assigned lane.",
                    "Lane Full",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            LaneSelected?.Invoke(this, laneName);
        }

        private enum LaneStatus
        {
            Vacant,
            Full,
            NotAssigned
        }
    }
}
