using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ASID.Edge.Services
{
    public class TcpScannerService
    {
        public bool IsRunning { get; private set; }
        public DateTime? LastScanTime { get; private set; }
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

        public event EventHandler<string>? BarcodeReceived;
        public event EventHandler? Started;
        public event EventHandler? Stopped;

        public async Task StartAsync(int port = 58627)
        {
            _cts = new CancellationTokenSource();

            _listener = new TcpListener(IPAddress.Any, port);

            _listener.Start();
            IsRunning = true;
            Started?.Invoke(this, EventArgs.Empty);

            while (!_cts.Token.IsCancellationRequested)
            {
                TcpClient client =
                    await _listener.AcceptTcpClientAsync();

                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            using (client)
            using (var reader =
                new StreamReader(client.GetStream()))
            {
                string? barcode =
                    await reader.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    LastScanTime = DateTime.Now;

                    BarcodeReceived?.Invoke(this, barcode);
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;

            _cts?.Cancel();

            _listener?.Stop();

            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}