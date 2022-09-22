using Com.Zoho.Crm.API.Tags;
using Com.Zoho.Crm.API.Users;
using Com.Zoho.Crm.API.Util;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Com.Zoho.Crm.API.Record;

public class Record : Model
{
    readonly Dictionary<string, object> keyValues = new();
    readonly Dictionary<string, bool> keyModified = new();

    public long? Id => GetKeyValue("id") != null ? (long?) GetKeyValue("id") : null;

    public User CreatedBy
    {
        get => GetKeyValue("Created_By") != null ? (User) GetKeyValue("Created_By") : null;
        set => AddKeyValue("Created_By", value);
    }

    public DateTimeOffset? CreatedTime
    {
        get => GetKeyValue("Created_Time") != null ? (DateTimeOffset?) GetKeyValue("Created_Time") : null;
        set => AddKeyValue("Created_Time", value);
    }

    public User ModifiedBy
    {
        get => GetKeyValue("Modified_By") != null ? (User) GetKeyValue("Modified_By") : null;
        set => AddKeyValue("Modified_By", value);
    }

    public DateTimeOffset? ModifiedTime
    {
        get => GetKeyValue("Modified_Time") != null ? (DateTimeOffset?) GetKeyValue("Modified_Time") : null;
        set => AddKeyValue("Modified_Time", value);
    }

    public List<Tag> Tag
    {
        get => GetKeyValue("Tag") != null ? (List<Tag>) GetKeyValue("Tag") : null;
        set => AddKeyValue("Tag", value);
    }

    public void AddFieldValue<T>(Field<T> field, T value) => AddKeyValue(field.APIName, value);

    public void AddKeyValue(string apiName, object value)
    {
        keyValues[apiName] = value;
        keyModified[apiName] = true;
    }

    protected object GetKeyValue(string apiName) => keyValues.ContainsKey(apiName) ? keyValues[apiName] : null;

    public Dictionary<string, object> GetKeyValues() => keyValues;
    public bool IsKeyModified(string key) => keyModified.ContainsKey(key) && keyModified[key];
    public void SetKeyModified(string key, bool modification) => keyModified[key] = modification;

    public override string ToString() => JsonConvert.SerializeObject(keyValues, Formatting.Indented);
}