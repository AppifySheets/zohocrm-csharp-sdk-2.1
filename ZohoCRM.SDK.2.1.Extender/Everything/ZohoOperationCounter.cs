using System;
using Serilog;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public class ZohoOperationCounter : IDisposable
{
    public ZohoOperationCounter(string moduleName, ZohoOperationType operationType, int totalRecordsCount)
    {
        ModuleName = moduleName;
        OperationType = operationType;
        TotalRecordsCount = totalRecordsCount;
    }

    public string ModuleName { get; }
    public int Counter { get; private set; } = 0;

    public void IncreaseCounter()
    {
        ++Counter;
        if (Counter % 100 == 0)
            LogStatus("InProgress");
    }

    public void IncreaseCounterBy(int count)
    {
        Counter += count;

        LogStatus("InProgress (Bulk) ");
    }

    public ZohoOperationType OperationType { get; }
    public int TotalRecordsCount { get; }

    void LogStatus(string prefix) => Log.Information(prefix + "ZohoOperationCounter: {ModuleName}, {OperationType}, {Count}/{TotalRecordsCount}", ModuleName,
        OperationType, Counter, TotalRecordsCount);

    public void Dispose() => LogStatus("- - - Done - - - ");
}

public enum ZohoOperationType
{
    Insert,
    Update,
    Get
}