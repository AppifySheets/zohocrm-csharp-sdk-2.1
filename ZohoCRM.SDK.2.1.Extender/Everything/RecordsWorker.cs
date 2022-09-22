﻿using System.Collections.Generic;
using System.Linq;
using Com.Zoho.Crm.API;
using Com.Zoho.Crm.API.Record;
using CSharpFunctionalExtensions;
using Serilog;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public static class RecordsWorker
{
    public static Result<IEnumerable<RecordsParser.RecordT>> GetRecordsBy<T>(this ZohoModules moduleName, string fieldName, T value)
    {
        var recordOperations = new RecordOperations();
        var parameterMap = new ParameterMap();
        var headerInstance = new HeaderMap();

        // var filterParam = new Param<T>("CRITERIA", "com.zoho.crm.api.Record.SearchRecordsParam");
        // parameterMap.Add(filterParam, value);

        parameterMap.Add(RecordOperations.SearchRecordsParam.CRITERIA, $"{fieldName}:equals:{value}");

        var result = recordOperations.SearchRecords(moduleName.ToString(), parameterMap, headerInstance);

        var companyRecordsMany = RecordsParser.ParseData(result);
        return companyRecordsMany;
    }

    public static Result DeleteRecordsBy<T>(this ZohoModules moduleName, string fieldName, T value, bool shouldBeOneRecord)
    {
        // var recordResult = GetRecordsBy(moduleName, fieldName, value);

        // if (recordResult.IsFailure) return recordResult.ConvertFailure();

        var recordOperations = new RecordOperations();

        // Result<IEnumerable<RecordsParser.RecordT>> GetRecordsBy() => RecordsWorker.GetRecordsBy(moduleName, fieldName, value);

        var records = GetRecordsBy(moduleName, fieldName, value);

        do
        {
            if (records.IsFailure) return records.ConvertFailure<Record>();

            var headerInstance = new HeaderMap();

            var result = records
                .Value
                .ChunkLocal(90)
                .Select(recordTs =>
                {
                    var paramInstance = new ParameterMap();

                    recordTs.ForEach(id => paramInstance.Add(RecordOperations.DeleteRecordsParam.IDS, id.Record.Id.ToString()));
                    paramInstance.Add(RecordOperations.DeleteRecordsParam.WF_TRIGGER, false);

                    return recordOperations.DeleteRecords(moduleName.ToString(), paramInstance, headerInstance);
                }).ToList();

            records = GetRecordsBy(moduleName, fieldName, value);
        } while (records.IsSuccess && records.Value.Any());

        return Result.Success();
    }

    public static Result<IEnumerable<RecordsParser.RecordT>> GetRecords(this ZohoModules moduleName)
    {
        var recordOperations = new RecordOperations();
        var parameterMap = new ParameterMap();
        var headerInstance = new HeaderMap();

        var companyRecordsMany = RecordsParser.ParseData(recordOperations.GetRecords(moduleName.ToString(), parameterMap, headerInstance));
        return companyRecordsMany;
    }

    public static Result<RecordsParser.RecordT> GetSingleRecord(this ZohoModules moduleName, long id)
    {
        var recordOperations = new RecordOperations();
        var parameterMap = new ParameterMap();
        var headerInstance = new HeaderMap();

        var companyRecordsMany = RecordsParser.ParseData(recordOperations.GetRecord(id, moduleName.ToString(), parameterMap, headerInstance));

        Log.Information("Received information from Zoho for {ModuleName} with Id {Id}", moduleName, id);
        return companyRecordsMany.Map(c => c.Single());
    }

    public static Result<Record> CreateRecord(this ZohoModules moduleName, Record record)
    {
        var bodyWrapper = new BodyWrapper(record.ToEnumerable());

        var recordOperations = new RecordOperations();

        var headerInstance2 = new HeaderMap();

        var response = recordOperations.CreateRecords(moduleName.ToString(), bodyWrapper, headerInstance2);

        var parsed = RecordsParser.ParseData(response);
        return parsed.IsFailure ? parsed.ConvertFailure<Record>() : parsed.Value.Single();
    }

    public static Result<Record> UpdateRecord(this ZohoModules moduleName, Record record)
    {
        var bodyWrapper = new BodyWrapper(new[] {record});

        var recordOperations = new RecordOperations();

        var headerInstance2 = new HeaderMap();

        var response = recordOperations.UpdateRecord(record.Id, moduleName.ToString(), bodyWrapper, headerInstance2);

        var parsed = RecordsParser.ParseData(response);
        return parsed.IsFailure ? parsed.ConvertFailure<Record>() : parsed.Value.Single();
    }
    public static Result<IEnumerable<Result<Record>>> UpdateRecords(this ZohoModules moduleName, IEnumerable<Record> record)
    {
        // bodyWrapper.Data = new[] {record}.ToList();

        var bodyWrapper = new BodyWrapper(record);
        var recordOperations = new RecordOperations();
        var headerInstance2 = new HeaderMap();

        var response = recordOperations.UpdateRecords(moduleName.ToString(), bodyWrapper, headerInstance2);

        var parsed = RecordsParser.ParseData(response);
        return parsed;
    }
}

public enum ZohoModules
{
    Accounts,
    Contacts,
    Transactions,
    Deals,
    Employees,
    Logs
}