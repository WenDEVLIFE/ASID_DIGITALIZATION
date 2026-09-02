using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.SQLite;
using Dapper;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ASID.Edge.Services;

/// <summary>
/// Realtime sync service: pushes local SQLite transactions to PostgreSQL.
///
/// Strategy:
///   1. FIRE-AND-FORGET on every write (Add/Update) — immediate push.
///   2. Network change detection — when internet comes back, flush the queue.
///   3. Safety-net timer (10 seconds) — catches any stragglers.
///   4. Exponential backoff on failure — 1s → 2s → 4s → … → 30s max.
///
/// Conflict resolution: UPSERT with "last write wins" based on updated_at.
/// Each row has a `synced` flag (0 = pending, 1 = pushed).
/// </summary>
public class SyncService
{
    private readonly SqliteTransactionRepository _sqlite;
    private readonly DispatcherTimer _timer;
    private bool _running;
    private bool _isOnline = true;
    private int _retryDelayMs = 1000;
    private const int MaxRetryDelayMs = 30_000;
    private bool _syncInProgress;
    private readonly object _syncLock = new();

    /// <summary>Raised after each sync batch.  Args = rows pushed.</summary>
    public event Action<int>? SyncCompleted;

    /// <summary>Raised when network status changes.</summary>
    public event Action<bool>? NetworkStatusChanged;

    public SyncService(SqliteTransactionRepository sqlite)
    {
        _sqlite = sqlite;

        // Safety-net timer: every 10 seconds, flush anything left.
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _timer.Tick += (_, _) => _ = SyncOnceAsync();

        // Listen for network changes.
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

        // Push immediately after every local write.
        sqlite.TransactionChanged += PushNow;
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        _isOnline = NetworkInterface.GetIsNetworkAvailable();
        _timer.Start();
    }

    public void Stop()
    {
        _running = false;
        _timer.Stop();
        NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        _sqlite.TransactionChanged -= PushNow;
    }

    /// <summary>
    /// Call immediately after every SQLite write (Add or Update).
    /// Fire-and-forget: attempts sync instantly.
    /// </summary>
    public void PushNow()
    {
        if (!_isOnline) return;
        _ = SyncOnceAsync();
    }

    private void OnNetworkAvailabilityChanged(
        object sender, NetworkAvailabilityEventArgs e)
    {
        bool wasOnline = _isOnline;
        _isOnline = e.IsAvailable;

        NetworkStatusChanged?.Invoke(_isOnline);

        // Network just came back online → flush the entire queue.
        if (!wasOnline && _isOnline)
        {
            _retryDelayMs = 1000; // reset backoff
            _ = SyncOnceAsync();
        }
    }

    /// <summary>
    /// Push all un-synced rows to PostgreSQL.
    /// Thread-safe: only one sync runs at a time.
    /// </summary>
    private async Task SyncOnceAsync()
    {
        if (!_isOnline) return;

        lock (_syncLock)
        {
            if (_syncInProgress) return;
            _syncInProgress = true;
        }

        try
        {
            var unsynced = _sqlite.GetUnsynced();
            if (unsynced.Count == 0)
            {
                _retryDelayMs = 1000; // success resets backoff
                return;
            }

            using var pgConnection = Database.Database.Engine.Equals("mssql", StringComparison.OrdinalIgnoreCase)
                ? (System.Data.Common.DbConnection)Database.Database.CreateMssqlConnection()
                : Database.Database.CreateConnection();
            pgConnection.Open();

            int pushed = 0;

            foreach (var tx in unsynced)
            {
                try
                {
                    PushSingle(pgConnection, tx);
                    _sqlite.MarkSynced(tx.DataMatrix);
                    pushed++;
                }
                catch
                {
                    // Row failed — will retry on next cycle.
                }
            }

            if (pushed > 0)
            {
                _retryDelayMs = 1000; // success resets backoff
                SyncCompleted?.Invoke(pushed);
            }
            else
            {
                // All rows failed — apply backoff before next attempt.
                await ApplyBackoffAsync();
            }
        }
        catch
        {
            // PostgreSQL unreachable — apply backoff.
            await ApplyBackoffAsync();
        }
        finally
        {
            lock (_syncLock)
            {
                _syncInProgress = false;
            }
        }
    }

    private async Task ApplyBackoffAsync()
    {
        await Task.Delay(_retryDelayMs);
        _retryDelayMs = Math.Min(_retryDelayMs * 2, MaxRetryDelayMs);
    }

    private static void PushSingle(
        System.Data.Common.DbConnection conn,
        StorageTransaction tx)
    {
        const string upsertSql = @"
INSERT INTO transactions
(
    id,
    data_matrix,
    serial_no,
    model,
    part_no,
    quantity,
    kanban_no,
    operator_id,
    line_no,
    lane_no,
    trolley_no,
    station,
    status,
    created_at,
    updated_at,
    withdrawn_at,
    forpickup_at,
    received_at,
    consumed_at,
    is_suspected_nc
)
VALUES
(
    gen_random_uuid(),
    @DataMatrix,
    @SerialNo,
    @Model,
    @PartNo,
    @SNP,
    @KanbanNo,
    @OperatorId,
    @LineNo,
    @LaneNo,
    @TrolleyNo,
    @Station,
    @Status,
    @CreatedAt,
    @UpdatedAt,
    @WithdrawnAt,
    @ForPickupAt,
    @ReceivedAt,
    @ConsumedAt,
    @IsSuspectedNC
)
ON CONFLICT (data_matrix) DO UPDATE SET
    status          = EXCLUDED.status,
    is_suspected_nc = EXCLUDED.is_suspected_nc,
    station         = EXCLUDED.station,
    withdrawn_at    = EXCLUDED.withdrawn_at,
    forpickup_at    = EXCLUDED.forpickup_at,
    received_at     = EXCLUDED.received_at,
    consumed_at     = EXCLUDED.consumed_at,
    updated_at      = EXCLUDED.updated_at;";

        conn.Execute(upsertSql, new
        {
            tx.DataMatrix,
            tx.SerialNo,
            tx.Model,
            tx.PartNo,
            tx.SNP,
            tx.KanbanNo,
            tx.OperatorId,
            tx.LineNo,
            tx.LaneNo,
            tx.TrolleyNo,
            tx.Station,
            Status = tx.Status.ToString(),
            CreatedAt = tx.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            WithdrawnAt = tx.WithdrawnAt,
            ForPickupAt = tx.ForPickupAt,
            ReceivedAt = tx.ReceivedAt,
            ConsumedAt = tx.ConsumedAt,
            IsSuspectedNC = tx.IsSuspectedNC
        });
    }
}
