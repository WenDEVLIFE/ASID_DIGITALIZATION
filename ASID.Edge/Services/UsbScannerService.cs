using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace ASID.Edge.Services
{
    /// <summary>
    /// Detects USB HID barcode scanners (keyboard wedge mode).
    /// 
    /// How it works:
    /// - USB scanners send all characters rapidly (~5-30ms per char) then press Enter
    /// - Manual typing is slower (~50-200ms per char) with backspaces
    /// - We measure inter-key timing to distinguish the two
    /// </summary>
    public class UsbScannerService
    {
        private readonly StringBuilder _buffer = new();
        private readonly Stopwatch _sw = new();
        private bool _isReceiving;
        private const int MaxInterKeyMs = 80;   // max ms between chars to count as scanner
        private const int MinBarcodeLength = 4;  // shortest plausible barcode

        /// <summary>Fires when a complete barcode is detected from a USB scanner.</summary>
        public event EventHandler<string>? BarcodeReceived;

        /// <summary>True while characters are being accumulated (scanner is mid-scan).</summary>
        public bool IsReceiving => _isReceiving;

        /// <summary>
        /// Call this from a PreviewTextInput handler on your Window/UserControl.
        /// Returns true if the input was consumed by the scanner detector.
        /// </summary>
        public bool ProcessTextInput(TextCompositionEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Text))
                return false;

            foreach (char c in e.Text)
            {
                if (ProcessChar(c))
                    return true; // consumed — don't let the textbox see it
            }

            return false;
        }

        /// <summary>
        /// Call this from a PreviewKeyDown handler to catch Enter key.
        /// </summary>
        public bool ProcessKeyDown(Key key)
        {
            if (key == Key.Return && _isReceiving && _buffer.Length >= MinBarcodeLength)
            {
                string barcode = _buffer.ToString();
                _buffer.Clear();
                _isReceiving = false;
                BarcodeReceived?.Invoke(this, barcode);
                return true; // consumed
            }

            // Any non-printable key resets (Escape, etc.)
            if (_isReceiving && key != Key.None)
            {
                // Allow Shift, Ctrl, Alt modifiers but reset on others
                if (key is not (Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
                    or Key.LeftAlt or Key.RightAlt or Key.System))
                {
                    Reset();
                }
            }

            return false;
        }

        private bool ProcessChar(char c)
        {
            if (!_isReceiving)
            {
                // Start a new scan
                _isReceiving = true;
                _buffer.Clear();
                _buffer.Append(c);
                _sw.Restart();
                return true;
            }

            // Already receiving — check timing
            long elapsed = _sw.ElapsedMilliseconds;
            _sw.Restart();

            if (elapsed <= MaxInterKeyMs)
            {
                // Fast enough — this is part of the scanner input
                _buffer.Append(c);
                return true;
            }
            else
            {
                // Too slow — was manual typing, reset and start over
                Reset();
                _isReceiving = true;
                _buffer.Append(c);
                _sw.Restart();
                return true;
            }
        }

        private void Reset()
        {
            _buffer.Clear();
            _isReceiving = false;
            _sw.Stop();
        }
    }
}
