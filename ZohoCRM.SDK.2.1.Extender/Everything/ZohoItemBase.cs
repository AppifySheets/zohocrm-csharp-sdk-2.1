﻿using System;
using Com.Zoho.Crm.API.Record;
using CSharpFunctionalExtensions;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public static class ZohoItemBaseWithId
{
    public static ZohoItemBaseWithId<T> ForTransferring<T>(this T item) where T : ZohoItemBase => new(item.ZohoId, item);
}

public class ZohoItemBaseWithId<T> where T : ZohoItemBase
{
    public ZohoItemBaseWithId(Maybe<long> zohoId, T item, bool allowZohoIdDifferentFromItem = false)
    {
        this.zohoId = zohoId;
        Item = item;

        if (!allowZohoIdDifferentFromItem && this.zohoId.HasValue && item.ZohoId.HasValue && this.zohoId.Value != item.ZohoId.Value)
            throw new InvalidOperationException($"Existing and New ZohoId's can't be different: Existing [{item.ZohoId}], New [{zohoId.Value}]");
    }

    ZohoItemBaseWithId(T item, bool forceCreate)
    {
        Item = item;
        ForceCreate = forceCreate;
    }

    public OperationTypeNeededInZohoEnum OperationTypeNeededInZoho =>
        ForceCreate
            ? OperationTypeNeededInZohoEnum.Create
            : ZohoId
                .HasNoValue
                .UseThenReturnSelf(hasNoValue =>
                {
                    if (hasNoValue && Item.OperationTypeNeededInZoho.Use(ot => ot is not OperationTypeNeededInZohoEnum.Create && ot is not OperationTypeNeededInZohoEnum.IgnoreDueToError))
                        throw new InvalidOperationException("You must specify OperationTypeNeededInZohoEnum.Create for when ZohoId is missing");
                })
                ? Item.OperationTypeNeededInZoho // Used to be OperationTypeNeededInZohoEnum.Create, needs attention!
                : Item.OperationTypeNeededInZoho == OperationTypeNeededInZohoEnum.Create // ZohoItemBase doesn't YET know that the record has already been updated
                    ? OperationTypeNeededInZohoEnum.Update
                    : Item.OperationTypeNeededInZoho;

    readonly Maybe<long> zohoId;

    public Maybe<long> ZohoId => ForceCreate
        ? Maybe.None
        : Item.ZohoId.HasValue
            ? Item.ZohoId.Value
            : zohoId;

    public T Item { get; }
    public bool ForceCreate { get; }

    public ZohoItemBaseWithId<T> SetZohoId(long zohoIdParam) => new(zohoIdParam, Item, true);
    public ZohoItemBaseWithId<T> UpdateToCreate() => new(Item, true);

    Record ZohoRecordInternal()
    {
        var record = new Record();
        if (zohoId.HasValue)
            record.AddKeyValue("id", zohoId.Value);

        var item = Item.CreateRecord(record);

        return Item.OwnerId.HasValue
            ? item.SetOwner(Item.OwnerId.Value)
            : item;
    }

    public Record ZohoRecord => ZohoRecordInternal();
}

public abstract class ZohoItemBase
{
    public abstract OperationTypeNeededInZohoEnum OperationTypeNeededInZoho { get; }
    public abstract Maybe<long> ZohoId { get; }
    public abstract ZohoModules ZohoModule { get; }

    public abstract string RecordIdentifierExtended { get; }
    public abstract string SourceRecordIdentifier { get; }
    public abstract string SourceRecordTypeName { get; }
    public abstract Maybe<long> OwnerId { get; }
    public abstract Record CreateRecord(Record initialRecord);
}