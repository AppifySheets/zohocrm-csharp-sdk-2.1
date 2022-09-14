using System;
using System.Collections.Generic;
using System.Linq;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public class ZohoCounters : IDisposable
{
    static readonly object Locker = new();

    public void IncreaseCountForModule(string moduleName, ZohoOperationType zohoOperationType, int totalRecordsCount)
    {
        lock (Locker)
            (zohoOperationCounters.SingleOrDefault(zc => zc.ModuleName == moduleName && zc.OperationType == zohoOperationType)
             ?? new ZohoOperationCounter(moduleName, zohoOperationType, totalRecordsCount)
                 .UseThenReturnSelf(zc => zohoOperationCounters.Add(zc)))
                .IncreaseCounter();
    }

    public void IncreaseCountForModuleBy(string moduleName, ZohoOperationType zohoOperationType, int totalRecordsCount, int increaseBy)
    {
        lock (Locker)
            (zohoOperationCounters.SingleOrDefault(zc => zc.ModuleName == moduleName && zc.OperationType == zohoOperationType)
             ?? new ZohoOperationCounter(moduleName, zohoOperationType, totalRecordsCount)
                 .UseThenReturnSelf(zc => zohoOperationCounters.Add(zc)))
                .IncreaseCounterBy(increaseBy);
    }

    readonly List<ZohoOperationCounter> zohoOperationCounters = new();

    public IEnumerable<ZohoOperationCounter> ZohoOperationCounters
        => zohoOperationCounters.Use(z =>
        {
            lock (Locker)
                return z.AsReadOnlyList();
        });

    public void Dispose()
    {
        zohoOperationCounters.ForEach(zo => zo.Dispose());
        zohoOperationCounters.Clear();
    }
}