﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Com.Zoho.Crm.API;
using Com.Zoho.Crm.API.Record;
using Com.Zoho.Crm.API.Util;
using CSharpFunctionalExtensions;
using Serilog;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public static class ResultExtensions
{
    public static Result<TSuccess> Switch<T, TSuccess>(this Result<T> resultT, Func<T, TSuccess> successFunc, Func<string, string> failureFunc) =>
        resultT.IsSuccess switch
        {
            true => successFunc(resultT.Value),
            _ => resultT.MapError(failureFunc)
                .ConvertFailure<TSuccess>()
        };
}

public static class ZohoItemOperations
{
    static Result<ZohoItemBaseWithId<T>> UpdateCore<T>(this ZohoItemBaseWithId<T> zohoItemBase) where T : ZohoItemBase
    {
        if (!zohoItemBase.ZohoId.HasValue) throw new InvalidOperationException("Can't be updating zohoItemBase w/o zohoId specified");

        var parsedDataCore = ParseResult(zohoItemBase,
            tuple => () => tuple.ro.UpdateRecord(zohoItemBase.ZohoId.Value, zohoItemBase.Item.ZohoModule.ToString(), tuple.bw, tuple.hm));

        var updatedResult = parsedDataCore
            .OnFailureCompensate(error => error.Contains("the id given seems to be invalid")
                ? zohoItemBase.CreateCore()
                : parsedDataCore);

        return updatedResult;
    }

    static Result<IReadOnlyCollection<OriginalWithSameResult<ZohoItemBaseWithId<T>>>> UpdateManyCore<T>(this IReadOnlyCollection<ZohoItemBaseWithId<T>> zohoItemBase,
        Action<(string sourceModule, string altaId, Result<long> zohoId, OperationTypeNeededInZohoEnum operationTypeNeededInZohoEnum)> updateHandler) where T : ZohoItemBase
    {
        if (zohoItemBase.Any(z => z.ZohoId.HasNoValue)) throw new InvalidOperationException("Can't be updating zohoItemBase w/o zohoId specified");
        if (!zohoItemBase.Any()) return Result.Success(Enumerable.Empty<OriginalWithSameResult<ZohoItemBaseWithId<T>>>().AsReadOnlyList());

        var parsedDataCores = ParseManyResults(zohoItemBase, tuple => () => tuple.ro.UpdateRecords(tuple.moduleName, tuple.bw, tuple.hm));
        if (parsedDataCores.IsFailure) return parsedDataCores.ConvertFailure<IReadOnlyCollection<OriginalWithSameResult<ZohoItemBaseWithId<T>>>>();

        parsedDataCores.Value.Where(v => v.IsFailure)
                .ForEach(c =>
                {
                    Log.Error("Error updating record with {Error}", c.Error);
                });
        
        OriginalWithSameResult<ZohoItemBaseWithId<T>> Get(ZohoItemBaseWithId<T> original, Record? maybeResult)
            => maybeResult != null
                ? original.Create(original.SetZohoId(maybeResult.Id!.Value))
                : original.CreateFailure($"Update failed for {original.ZohoId} - the id given seems to be invalid");

        var parsedWithInitial = zohoItemBase.Select(i => Get(i, parsedDataCores.Value.SingleOrDefault(pdc => pdc.IsSuccess && pdc.Value.Id == i.ZohoId).Value))
            .AsReadOnlyList();

        var updatedResult = parsedWithInitial
                .Select(parsedDataCore =>
                    parsedDataCore.Original.Create(parsedDataCore.Result.OnFailureCompensate(error => error.Contains("the id given seems to be invalid")
                        ? parsedDataCore.Original.UpdateToCreate().CreateCore()
                        : parsedDataCore.Result)))
                .AsReadOnlyList()
            ;

        // var sourceModuleName = zohoItemBase.Select(z => z.Item.SourceRecordTypeName).Distinct().Single().ToString();

        updatedResult.ForEach(ur =>
        {
            updateHandler((ur.Original.Item.SourceRecordTypeName, ur.Original.Item.SourceRecordIdentifier, ur.Result.Use(r => r.IsSuccess ? r.Value.ZohoId.Value : r.ConvertFailure<long>()),
                OperationTypeNeededInZohoEnum.Update));
        });

        return Result.Success(updatedResult);
    }


    static Result<ZohoItemBaseWithId<T>> CreateCore<T>(this ZohoItemBaseWithId<T> zohoItemBase, CreateOperationType createOperationType = CreateOperationType.CreateAndUpdateExisting)
        where T : ZohoItemBase
    {
        if (zohoItemBase.ZohoId.HasValue) throw new InvalidOperationException("Can't be CREATING zohoItemBase with zohoId specified");

        var parsedDataCore = ParseResult(zohoItemBase, tuple => () => tuple.ro.CreateRecords(zohoItemBase.Item.ZohoModule.ToString(), tuple.bw, tuple.hm))
            .Use(parsedDataCore =>
                parsedDataCore.IsSuccess
                    ? parsedDataCore
                    : parsedDataCore.IsFailure
                        ? ZohoApiErrorParser.ParseZohoApiError(parsedDataCore.Error)
                            .Use(altaIdParser => altaIdParser is {IsSuccess : true}
                                ? zohoItemBase.Item.ZohoModule.GetSingleRecord(altaIdParser.Value)
                                    .Use(existingRecord =>
                                        existingRecord switch
                                        {
                                            {IsSuccess: true} => createOperationType switch
                                            {
                                                CreateOperationType.CreateOrAcceptExisting => zohoItemBase.SetZohoId(existingRecord.Value.Record.Id!.Value),
                                                CreateOperationType.CreateAndUpdateExisting => ParseResult(zohoItemBase, tuple =>
                                                    () => tuple.ro.UpdateRecord(existingRecord.Value.Record.Id, zohoItemBase.Item.ZohoModule.ToString(), tuple.bw, tuple.hm)),
                                                _ => throw new ArgumentOutOfRangeException(nameof(createOperationType), createOperationType, null)
                                            },
                                            {IsFailure: true} => parsedDataCore
                                                .Combine(altaIdParser.ConvertFailure<ZohoItemBaseWithId<T>>())
                                                .ConvertFailure<ZohoItemBaseWithId<T>>(),
                                            _ => throw new ArgumentOutOfRangeException(nameof(existingRecord), existingRecord, null)
                                        }
                                    )
                                : altaIdParser is {IsFailure: true}
                                    ? parsedDataCore
                                        .Combine(altaIdParser.ConvertFailure<ZohoItemBaseWithId<T>>())
                                        .ConvertFailure<ZohoItemBaseWithId<T>>()
                                    : throw new ArgumentOutOfRangeException(nameof(altaIdParser), altaIdParser, null))
                        : throw new ArgumentOutOfRangeException());

        return parsedDataCore;
    }

    static Result<IEnumerable<OriginalWithSameResult<ZohoItemBaseWithId<T>>>> CreateManyCore<T>(this IEnumerable<ZohoItemBaseWithId<T>> zohoItemBase,
        Action<(string sourceModule, string altaId, Result<long> zohoId, OperationTypeNeededInZohoEnum operationTypeNeededInZohoEnum)> updateHandler,
        CreateOperationType createOperationType = CreateOperationType.CreateAndUpdateExisting)
        where T : ZohoItemBase
    {
        var zohoItemBaseWithIds = zohoItemBase.AsReadOnlyList();

        if (zohoItemBaseWithIds.Any(z => z.ZohoId.HasValue)) throw new InvalidOperationException("Can't be CREATING zohoItemBase with zohoId specified");

        if (!zohoItemBaseWithIds.Any()) return Result.Success(Enumerable.Empty<OriginalWithSameResult<ZohoItemBaseWithId<T>>>());

        var sourceModuleName = zohoItemBaseWithIds.Select(z => z.Item.SourceRecordTypeName).Distinct().Single().ToString();
        Log.Information("Intending to create {Module} - {Record} records", sourceModuleName, zohoItemBaseWithIds.Count);

        var counter = zohoItemBaseWithIds.Count;
        
        var parsedDataCores = zohoItemBaseWithIds
            .AsParallel()
            .WithDegreeOfParallelism(10)
            .Select(z =>
            {
                var value2Return = z.Create(ParseResult(z, tuple =>
                {
                    Log.Information("Creating {Module} - {Record} ({RemainingCount}/{TotalCount})", sourceModuleName, z.Item.SourceRecordIdentifier, counter--, zohoItemBaseWithIds.Count);
                    return () => tuple.ro.CreateRecords(tuple.moduleName, tuple.bw, tuple.hm);
                }));

                updateHandler((sourceModuleName, value2Return.Original.Item.SourceRecordIdentifier, value2Return.Result.Use(r => r.IsSuccess ? r.Value.ZohoId.Value : r.ConvertFailure<long>()),
                    OperationTypeNeededInZohoEnum.Create));

                return value2Return;
            })
            .AsReadOnlyList();
        // var parsedDataCores = ParseManyResults(zohoItemBaseWithIds, tuple => () => tuple.ro.CreateRecords(tuple.moduleName, tuple.bw, tuple.hm));

        // if (parsedDataCores.IsFailure) return parsedDataCores.ConvertFailure<IEnumerable<OriginalWithSameResult<ZohoItemBaseWithId<T>>>>();

        var result = parsedDataCores.Select(parsedDataCore =>
            parsedDataCore.Result.IsSuccess
                ? parsedDataCore
                : parsedDataCore.Result.IsFailure
                    ? ZohoApiErrorParser.ParseZohoApiError(parsedDataCore.Result.Error)
                        .Use(altaIdParser =>
                        {
                            Log.Information("Parsed error and got result {Result}", altaIdParser);
                            
                            return altaIdParser.IsSuccess
                                ? parsedDataCore.Original.Item.ZohoModule.GetSingleRecord(altaIdParser.Value)
                                    .Use(existingRecord =>
                                        existingRecord switch
                                        {
                                            {IsSuccess: true} => createOperationType switch
                                            {
                                                CreateOperationType.CreateOrAcceptExisting => parsedDataCore.Original.SetZohoId(existingRecord.Value.Record.Id!.Value).CreateSuccess(),
                                                CreateOperationType.CreateAndUpdateExisting => parsedDataCore.Original.Create(ParseResult(parsedDataCore.Original, tuple =>
                                                    () =>
                                                    {
                                                        Log.Warning("Updating instead of creating record {Record} in module {Module}", existingRecord.Value.Record.Id, parsedDataCore.Original.Item.ZohoModule);
                                                        
                                                        return tuple.ro.UpdateRecord(existingRecord.Value.Record.Id, parsedDataCore.Original.Item.ZohoModule.ToString(), tuple.bw, tuple.hm);
                                                    })),
                                                _ => throw new ArgumentOutOfRangeException(nameof(createOperationType), createOperationType, null)
                                            },
                                            {IsFailure: true} => parsedDataCore.Original.Create(parsedDataCore.Result
                                                .Combine(altaIdParser.ConvertFailure<ZohoItemBaseWithId<T>>())
                                                .ConvertFailure<ZohoItemBaseWithId<T>>()),
                                            _ => throw new ArgumentOutOfRangeException(nameof(existingRecord), existingRecord, null)
                                        }
                                    )
                                : altaIdParser is {IsFailure: true}
                                    ? parsedDataCore.Original.Create(parsedDataCore.Result
                                        .Combine(altaIdParser.ConvertFailure<ZohoItemBaseWithId<T>>())
                                        .ConvertFailure<ZohoItemBaseWithId<T>>())
                                    : throw new ArgumentOutOfRangeException(nameof(altaIdParser), altaIdParser, null);
                        })
                    : throw new ArgumentOutOfRangeException());

        return Result.Success(result);
    }


    public enum CreateOperationType
    {
        CreateOrAcceptExisting,
        CreateAndUpdateExisting
    }


    public static Result<ZohoItemBaseWithId<T>> Save<T>(this ZohoItemBaseWithId<T> zohoItemBase, CreateOperationType createOperationType = CreateOperationType.CreateAndUpdateExisting)
        where T : ZohoItemBase
    {
        var parsedData =
            zohoItemBase.OperationTypeNeededInZoho switch
            {
                OperationTypeNeededInZohoEnum.Create => CreateCore(zohoItemBase, createOperationType),
                OperationTypeNeededInZohoEnum.Update => UpdateCore(zohoItemBase),
                OperationTypeNeededInZohoEnum.IgnoreDueToError => Result.Failure<ZohoItemBaseWithId<T>>("IgnoredDueToError"),
                OperationTypeNeededInZohoEnum.LeaveUnchanged => zohoItemBase
                    // .ZohoRecord
                    .UseThenReturnSelf(zr =>
                    {
                        if (zr.ZohoId.HasNoValue)
                            throw new InvalidOperationException("Can't Leave unsaved record Unchanged");
                    }),
                _ => throw new ArgumentOutOfRangeException()
            };

        return parsedData.Map(pd =>
        {
            if (pd.ZohoId.HasNoValue)
            {
                Debugger.Break();
            }

            // Debug.Assert(pd.ZohoId.HasValue, "pd.Id != null");

            return zohoItemBase.SetZohoId(pd.ZohoId.Value);
        });
    }


    public static Result<IEnumerable<OriginalWithSameResult<ZohoItemBaseWithId<T>>>> SaveMany<T>(this IEnumerable<ZohoItemBaseWithId<T>> zohoItemBase,
        Action<(string sourceModule, string altaId, Result<long> zohoId, OperationTypeNeededInZohoEnum operationTypeNeededInZohoEnum)> updateHandler,
        CreateOperationType createOperationType = CreateOperationType.CreateAndUpdateExisting)
        where T : ZohoItemBase
    {
        var zohoItemBasesArol = zohoItemBase.AsReadOnlyList();

        var created = CreateManyCore(zohoItemBasesArol.Where(z => z.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.Create).AsReadOnlyList(), updateHandler, createOperationType);

        var updated = UpdateManyCore(zohoItemBasesArol.Where(z => z.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.Update).AsReadOnlyList(), updateHandler);

        if (zohoItemBasesArol.Any(z => z.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.LeaveUnchanged && z.ZohoId.HasNoValue))
            throw new InvalidOperationException("Can't Leave unsaved record Unchanged");

        var createdUpdated = Result.Combine(created, updated);
        if (createdUpdated.IsFailure) return createdUpdated.ConvertFailure<IEnumerable<OriginalWithSameResult<ZohoItemBaseWithId<T>>>>();

        var final = created.Value.Union(updated.Value)
            .Union(zohoItemBasesArol.Where(z => z.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.LeaveUnchanged)
                .Select(v => v.CreateSuccess())
            )
            .Union(zohoItemBasesArol.Where(z => z.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.IgnoreDueToError)
                .Select(v => v.Create(Result.Failure<ZohoItemBaseWithId<T>>("IgnoredDueToError")))
            );

        return Result.Success(final);
        //
        // var parsedData =
        //     zohoItemBase.OperationTypeNeededInZoho switch
        //     {
        //         OperationTypeNeededInZohoEnum.Create => CreateManyCore(zohoItemBasesArol, createOperationType),
        //         OperationTypeNeededInZohoEnum.Update => UpdateCore(zohoItemBasesArol),
        //         OperationTypeNeededInZohoEnum.IgnoreDueToError => Result.Failure<ZohoItemBaseWithId<T>>("IgnoredDueToError"),
        //         OperationTypeNeededInZohoEnum.LeaveUnchanged => zohoItemBase
        //             // .ZohoRecord
        //             .UseThenReturnSelf(zr =>
        //             {
        //                 if (zr.ZohoId.HasNoValue)
        //     
        //             }),
        //         _ => throw new ArgumentOutOfRangeException()
        //     };
        //
        // return parsedData.Map(pd =>
        // {
        //     if (pd.ZohoId.HasNoValue)
        //     {
        //         Debugger.Break();
        //     }
        //
        //     // Debug.Assert(pd.ZohoId.HasValue, "pd.Id != null");
        //
        //     return zohoItemBase.SetZohoId(pd.ZohoId.Value);
        // });
    }

    static Result<ZohoItemBaseWithId<T>> ParseResult<T>(ZohoItemBaseWithId<T> zohoItemBase,
        Func<(string moduleName, BodyWrapper bw, HeaderMap hm, RecordOperations ro), Func<APIResponse<ActionHandler>>> apiResponseHandler)
        where T : ZohoItemBase
    {
        if (!Initialize.IsInitialized) throw new InvalidOperationException("Please initialize the SDK first!");

        var bodyWrapper = new BodyWrapper();
        var recordOperations = new RecordOperations();
        var headerInstance2 = new HeaderMap();
        bodyWrapper.Data = new[] {zohoItemBase.ZohoRecord}.ToList();

        Result<Record> ParseResultCore(Func<APIResponse<ActionHandler>> parseFunc)
        {
            Result<TY> EnrichFailure<TY>(Result<TY> result)
                => result.IsSuccess ? result : Result.Failure<TY>($"{result.Error}\r\nError saving [{zohoItemBase.Item.ZohoModule}][{zohoItemBase.Item.RecordIdentifierExtended}]");

            var parseFuncResult = Result.Try(parseFunc, e => e.ToString());

            return parseFuncResult
                    .Bind(response => RecordsParser.ParseData(response)
                        .Use(r => r.IsFailure
                            ? r.ConvertFailure<Record>()
                            : r.Value.Single()))
                    .Use(EnrichFailure)
                //.LogErrorToZoho(zohoItemBase)
                ;
        }

        var parsedResult = ParseResultCore(apiResponseHandler((zohoItemBase.Item.ZohoModule.ToString(), bodyWrapper, headerInstance2, recordOperations)));

        var withBind = parsedResult
            // .Map(r => )
            .Map(r => zohoItemBase.SetZohoId(r.Id!.Value));

        return withBind;
    }

    static Result<IEnumerable<Result<Record>>> ParseManyResults<T>(IEnumerable<ZohoItemBaseWithId<T>> zohoItemBases,
        Func<(string moduleName, BodyWrapper bw, HeaderMap hm, RecordOperations ro), Func<APIResponse<ActionHandler>>> apiResponseHandler) where T : ZohoItemBase
    {
        var zohoItemBasesOrderedAllArol = zohoItemBases.Select((zohoItemBase, index) => new {zohoItemBase, index}).OrderBy(z => z.zohoItemBase.Item.RecordIdentifierExtended).AsReadOnlyList();

        if (!zohoItemBasesOrderedAllArol.Any()) return Result.Success(Enumerable.Empty<Result<Record>>());

        if (!Initialize.IsInitialized) throw new InvalidOperationException("Please initialize the SDK first!");

        var operationType = zohoItemBasesOrderedAllArol.Select(z => z.zohoItemBase.OperationTypeNeededInZoho).Distinct().AsReadOnlyList();
        var zohoModule = zohoItemBasesOrderedAllArol.Select(z => z.zohoItemBase.Item.ZohoModule).Distinct().AsReadOnlyList();

        if (operationType.Count != 1) throw new InvalidOperationException("No no");
        if (zohoModule.Count != 1) throw new InvalidOperationException("No no");

        var parsedDataResult = zohoItemBasesOrderedAllArol
            .ChunkLocal(100)
            .Select(zohoItemBasesOrdered =>
            {
                var itemBasesOrdered = zohoItemBasesOrdered.AsReadOnlyList();

                if (itemBasesOrdered.Count > 100) throw new InvalidOperationException("Can't process more than 100 items!");

                var bodyWrapper = new BodyWrapper();
                var recordOperations = new RecordOperations();
                var headerInstance2 = new HeaderMap();
                bodyWrapper.Data = itemBasesOrdered.Select(z => z.zohoItemBase.ZohoRecord).ToList();

                Log.Information("Updating {Module} - {RecordCount} items", zohoModule.Single(), itemBasesOrdered.Count);

                var parsedResult = apiResponseHandler((zohoModule.Single().ToString(), bodyWrapper, headerInstance2, recordOperations));
                var parseFuncResult = Result.Try(parsedResult, e => e.ToString());

                if (parseFuncResult.IsFailure) parseFuncResult.ConvertFailure<IEnumerable<Result<Record>>>();

                var parsedData = RecordsParser.ParseData(parseFuncResult.Value);
                if (parseFuncResult.IsFailure) parseFuncResult.ConvertFailure<IEnumerable<Result<Record>>>();

                return parsedData.Value;
            }).AsReadOnlyList();
        // .Use(r => r.IsFailure
        //     ? r.ConvertFailure<Result<Record>>()
        //     : r.Value.Select(ri => ri) )

        // if (parsedDataResult.IsFailure) return parsedDataResult.ConvertFailure<IEnumerable<Result<Record>>>();

        var finalResult = parsedDataResult.SelectMany(i => i.Select(ii => ii));

        return Result.Success(finalResult);

        // if (parseFuncResult.IsSuccess)
        //     return
        //
        // if (parseFuncResult.IsFailure)
        // {
        // }
        //
        //
        // var parsedDataResultIndexed = parsedDataResult.Value.Select((data, index) => new {data, index}).AsReadOnlyList();
        //
        // var zohoItemBasesWithParsedData = zohoItemBasesOrderedArol.Select(z => new {z.zohoItemBase, Data = parsedDataResultIndexed.Single(pd => pd.index == z.index).data}).AsReadOnlyList();
        //
        // var withBind = zohoItemBasesWithParsedData
        //     .Select(r => new {r.zohoItemBase, mapped = r.Data.Map(ri => r.zohoItemBase.SetZohoId(ri.Id!.Value))}).AsReadOnlyList();
        //
        //
        // return Result.Success(withBind.Select(b => b.zohoItemBase.Create(b.mapped)));
    }

    static class ZohoApiErrorParser
    {
        public static Result<long> ParseZohoApiError(string text)
        {
            Log.Warning("Parsing error {Error}", text);
           
            var split = text.Split(';')
                    .Select(s => s.Split(':'))
                    .ToList()
                ;

            if (!split.Any()) return Result.Failure<long>($"No items [{text}]");

            var couplesOfTwo = split
                .Select(s => new {Key = s.First(), Value = s.Last()})
                .AsReadOnlyList();

            var first = couplesOfTwo.First();
            var second = couplesOfTwo.Skip(1).FirstOrDefault();
            if (first.Value == "Alta_ID" && second?.Key == "id")
                return long.TryParse(second.Value, out var parsedValue)
                    ? parsedValue
                    : Result.Failure<long>($"Couldn't convert [{second.Value}] to long");

            return Result.Failure<long>($"Couldn't get ID from [{text}]");
        }
    }

    public static Result DeleteAll(this ZohoModules module)
    {
        var recordOperations = new RecordOperations();

        Result<IEnumerable<RecordsParser.RecordT>> GetRecords() => module.GetRecords();

        var records = GetRecords();

        do
        {
            if (records.IsFailure) return records.ConvertFailure<Record>();

            $"Deleting {records.Value.Count()} {module} items".Dump();

            var headerInstance = new HeaderMap();

            var result = records
                .Value
                .ChunkLocal(90)
                .Select(recordTs =>
                {
                    var paramInstance = new ParameterMap();

                    recordTs.ForEach(id => paramInstance.Add(RecordOperations.DeleteRecordsParam.IDS, id.Record.Id.ToString()));
                    paramInstance.Add(RecordOperations.DeleteRecordsParam.WF_TRIGGER, false);

                    return recordOperations.DeleteRecords(module.ToString(), paramInstance, headerInstance);
                }).ToList();

            records = GetRecords();
        } while (records.IsSuccess && records.Value.Any());


        return Result.Success();
    }

    public static Result DeleteSpecific(ZohoModules module, long recordId)
    {
        var recordOperations = new RecordOperations();

        var headerInstance = new HeaderMap();

        var paramInstance = new ParameterMap();

        paramInstance.Add(RecordOperations.DeleteRecordsParam.IDS, recordId.ToString());
        paramInstance.Add(RecordOperations.DeleteRecordsParam.WF_TRIGGER, false);

        var response = recordOperations.DeleteRecords(module.ToString(), paramInstance, headerInstance);

        return Result.Success();
    }
}

public static class OriginalWithResultExtensions
{
    // public static OriginalWithResult<TOriginal, TResult> Create<TOriginal, TResult>(this TOriginal original, Result<TResult> result) => new(original, result);
    public static OriginalWithSameResult<TOriginal> Create<TOriginal>(this TOriginal original, Result<TOriginal> result) => new(original, result);
    public static OriginalWithSameResult<TOriginal> CreateSuccess<TOriginal>(this TOriginal original) => new(original, Result.Success(original));
    public static OriginalWithSameResult<TOriginal> CreateFailure<TOriginal>(this TOriginal original, string error) => new(original, Result.Failure<TOriginal>(error));

    public static OriginalWithSameResult<TOriginal> CreateFromSuccess<TOriginal>(this Result<TOriginal> result)
    {
        if (result.IsFailure) throw new InvalidOperationException(result.Error);

        return new OriginalWithSameResult<TOriginal>(result.Value, result);
    }
}

public class OriginalWithSameResult<TOriginal> : OriginalWithResult<TOriginal, TOriginal>
{
    public OriginalWithSameResult(TOriginal original, Result<TOriginal> result) : base(original, result)
    {
    }
}

public class OriginalWithResult<TOriginal, TResult>
{
    public OriginalWithResult(TOriginal original, Result<TResult> result)
    {
        Original = original;
        Result = result;
    }

    public TOriginal Original { get; }
    public Result<TResult> Result { get; }
}