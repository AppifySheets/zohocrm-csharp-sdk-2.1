using Com.Zoho.Crm.API.Record;
using Com.Zoho.Crm.API.Users;
using Com.Zoho.Crm.API.Util;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

// public class  Record : Record
// {
//     // public IEnumerable<Record> RecordsToSaveBeforeSavingSelf
//     
//     public void AddRecordValue(string fieldName, Record record)
//     {
//         record.AddFieldValue(new Field<Record>(fieldName), record);
//     }
// }

public static class RecordExtensions
{
    public static Record AddChoiceValue<T>(this Record record, string fieldName, T choiceValue)
    {
        record.AddFieldValue(new Field<Choice<T>>(fieldName), new Choice<T>(choiceValue));

        return record;
    }

    public static Record AddRecordValue(this Record record, string fieldName, long recordId)
    {
        var dummyRecord = new Record();
        dummyRecord.AddFieldValue(Accounts.ID, recordId);
        record.AddFieldValue(new Field<Record>(fieldName), dummyRecord);
        return record;
    }

    // public static Record SetOwner(this Record record, long ownerId)
    // {
    //     // var dummyRecord = new Record();
    //     // dummyRecord.AddFieldValue(Products.OWNER, ownerId);
    //     
    //     // record.AddFieldValue(Products.OWNER, ownerId);
    //     record.AddKeyValue(Products.OWNER.APIName, new User{Id = ownerId});
    //     return record;
    // }

    public static Record SetOwnerX(this Record record, long ownerId)
    {
        // var dummyRecord = new Record();
        // dummyRecord.AddFieldValue(Products.OWNER, ownerId);

        // record.AddFieldValue(Products.OWNER, ownerId);
        record.AddKeyValue("Owner", new {id = ownerId});
        return record;
    }

    public static Record AddRecordValue(this Record record, string fieldName, Record value)
    {
        record.AddFieldValue(new Field<Record>(fieldName), value);
        return record;
    }

    public static Record AddRecordValue<T>(this Record record, string fieldName, ZohoItemBaseWithId<T>? zohoItem) where T : ZohoItemBase
    {
        if (zohoItem != null)
            record.AddKeyValue(fieldName, new {id = zohoItem?.ZohoRecord.Id});
        return record;
    }

    public static Record AddFieldValueX<T>(this Record record, string fieldName, T fieldValue)
    {
        record.AddFieldValue(new Field<T>(fieldName), fieldValue);

        return record;
    }

    public static Record AddSubmoduleValue<T>(this Record record, string fieldName, T fieldValue)
    {
        record.AddFieldValue(new Field<T>(fieldName), fieldValue);

        return record;
    }

    public static Record AddFieldValueAsString<T>(this Record record, string fieldName, T fieldValue)
    {
        record.AddFieldValue(new Field<string?>(fieldName), fieldValue?.ToString());

        return record;
    }

    public static Record CreateRecordWith<T>(this string fieldName, T fieldValue)
    {
        var record = new Record();
        record.AddFieldValueX(fieldName, fieldValue);
        return record;
    }

    // public static Result<Record> Save(this ZohoItemBase zohoItemBase) => ZohoItemOperations.Save(zohoItemBase);
    // public static Result<VersionZohoItem<T>> Save<T>(this VersionZohoItem<T> zohoItemBase) where T : VersionZohoItem<T> => ZohoItemOperations.Save(zohoItemBase);

    // public static (Result<Record> recordResult, TZohoItemBase original) SaveWithOriginal<TZohoItemBase>(this TZohoItemBase zohoItemBase) where TZohoItemBase : ZohoItemBase =>
    //     (ZohoItemOperations.Save(zohoItemBase), zohoItemBase);
}

// public class RecordWithJustId
// {
//     public int id { get; init; }
// }