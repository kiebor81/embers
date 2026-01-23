using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Embers.ISE.Services;

internal static class CompletionAnalysisCache
{
    private const int IdleDelayMs = 300;
    private static readonly Lock SyncLock = new();
    private static CancellationTokenSource? _analysisCts;
    private static AnalysisSnapshot? _snapshot;
    private static int _pendingHash;
    private static long _analysisCount;
    private static long _totalParseMs;
    private static long _completionCount;
    private static long _cacheHits;

    internal sealed record AnalysisSnapshot(
        int TextHash,
        AstReturnTypeService.MethodReturnMaps ReturnMaps,
        IReadOnlyDictionary<string, string> TypeTable,
        TimeSpan ParseDuration,
        DateTimeOffset Timestamp);

    public static void UpdateBuffer(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int hash = HashCode.Combine(text.Length, text.GetHashCode());
        CancellationTokenSource? ctsToCancel = null;

        lock (SyncLock)
        {
            if (hash == _pendingHash)
                return;

            _pendingHash = hash;
            ctsToCancel = _analysisCts;
            _analysisCts = new CancellationTokenSource();
        }

        ctsToCancel?.Cancel();
        ctsToCancel?.Dispose();

        var textSnapshot = text;
        var token = _analysisCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(IdleDelayMs, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
                return;

            var stopwatch = Stopwatch.StartNew();
            var returnMaps = AstReturnTypeService.GetMethodReturnMaps(textSnapshot);
            var typeTable = CompletionService.BuildTypeTableSnapshot(textSnapshot, returnMaps);
            stopwatch.Stop();

            var snapshot = new AnalysisSnapshot(
                hash,
                returnMaps,
                typeTable,
                stopwatch.Elapsed,
                DateTimeOffset.Now);

            lock (SyncLock)
            {
                _snapshot = snapshot;
                _analysisCount++;
                _totalParseMs += (long)stopwatch.Elapsed.TotalMilliseconds;
            }

            Debug.WriteLine($"[CompletionAnalysis] Snapshot updated in {stopwatch.Elapsed.TotalMilliseconds:0.##} ms.");
        }, token);
    }

    public static bool TryGetSnapshot(string text, out AnalysisSnapshot snapshot)
    {
        snapshot = null!;
        if (string.IsNullOrEmpty(text))
            return false;

        int hash = HashCode.Combine(text.Length, text.GetHashCode());

        lock (SyncLock)
        {
            if (_snapshot == null || _snapshot.TextHash != hash)
                return false;

            snapshot = _snapshot;
            return true;
        }
    }

    public static void RecordCompletion(TimeSpan duration, bool cacheHit)
    {
        lock (SyncLock)
        {
            _completionCount++;
            if (cacheHit)
                _cacheHits++;

            if (_completionCount % 25 == 0)
                WriteMetrics(duration);
        }
    }

    private static void WriteMetrics(TimeSpan lastDuration)
    {
        var parseAvg = _analysisCount == 0 ? 0 : _totalParseMs / (double)_analysisCount;
        var hitRate = _completionCount == 0 ? 0 : _cacheHits / (double)_completionCount;
        Debug.WriteLine($"[CompletionAnalysis] Avg parse {parseAvg:0.##} ms, last completion {lastDuration.TotalMilliseconds:0.##} ms, cache hit rate {hitRate:P0}.");
    }
}
