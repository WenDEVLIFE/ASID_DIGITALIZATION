using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace ASID.Edge.Services
{
    /// <summary>
    /// Detects USB HID barcode scanners (keyboard wedge mode).
    /// 
    /// Strategy: NEVER consume text input — just buffer silently.
    /// When Enter is pressed, check if the buffer looks like a scanner barcode
    /// (fast inter-key timing + min length). If so, fire the event.
    /// This way manual typing is never affected.
    /// </summary>
    public class UsbScannerService
    {
        private readonly StringBuilder _buffer = new();
        private readonly Stopwatch _sw = new();
        private bool _isReceiving;
        private const int MaxInterKeyMs = 60;   // max ms between chars to count as scanner
        private const int MinBarcodeLength = 4;  // shortest plausible barcode

        /// <summary>Fires when a complete barcode is detected from a USB scanner.</summary>
        public event EventHandler<string>? BarcodeReceived;

        /// <summary>True while characters are being accumulated.</summary>
        public bool IsReceiving => _isReceiving;

        /// <summary>
        /// Call from PreviewTextInput. NEVER consumes input — just tracks timing.
        /// Always returns false so textboxes always receive input.
        /// </summary>
        public bool ProcessTextInput(TextCompositionEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Text))
                return false;

            foreach (char c in e.Text)
            {
                TrackChar(c);
            }

            return false; // NEVER consume — let all text reach the textbox
        }

        /// <summary>
        /// Call from PreviewKeyDown. Only consumes Enter when a barcode is detected.
        /// </summary>
        public bool ProcessKeyDown(Key key)
        {
            if (key == Key.Return)
            {
                if (_isReceiving && _buffer.Length >= MinBarcodeLength)
                {
                    string barcode = _buffer.ToString();
                    Reset();
                    BarcodeReceived?.Invoke(this, barcode);
                    return false; // don't consume Enter either — let it reach the textbox
                }
                Reset();
                return false;
            }

            // Backspace removes last char from buffer
            if (key == Key.Back && _isReceiving && _buffer.Length > 0)
            {
                _buffer.Remove(_buffer.Length - 1, 1);
                if (_buffer.Length == 0)
                    Reset();
                return false;
            }

            // Reset buffer only on explicit navigation/cancel keys like Escape or Tab
            if (_isReceiving && key is Key.Escape or Key.Tab)
            {
                Reset();
            }

            return false; // never consume
        }

        private void TrackChar(char c)
        {
            if (!_isReceiving)
            {
                _buffer.Clear();
                _buffer.Append(c);
                _sw.Restart();
                _isReceiving = true;
                return;
            }

            long elapsed = _sw.ElapsedMilliseconds;
            _sw.Restart();

            if (elapsed <= MaxInterKeyMs)
            {
                // Fast — scanner pattern
                _buffer.Append(c);
            }
            else
            {
                // Slow — manual typing, reset
                Reset();
                _buffer.Clear();
                _buffer.Append(c);
                _sw.Restart();
                _isReceiving = true;
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
