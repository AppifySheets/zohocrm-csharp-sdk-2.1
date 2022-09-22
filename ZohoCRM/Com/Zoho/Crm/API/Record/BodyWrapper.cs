using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Com.Zoho.Crm.API.Record;

public class BodyWrapper : Model
{
    public BodyWrapper(IEnumerable<Record> data)
    {
        Records = data.ToImmutableList();
    }

    public IEnumerable<Record> Records { get; }

    public override string ToString() => JsonConvert.SerializeObject(new BodyWrapperSerializer(this), Formatting.Indented);
}

public class BodyWrapperSerializer
{
    readonly BodyWrapper _bodyWrapper;
    
    // ReSharper disable once UnusedMember.Global
    public IEnumerable<ReadOnlyDictionary<string, object>> data
        => _bodyWrapper.Records.Select(r => new ReadOnlyDictionary<string, object>(r.GetKeyValues()));

    public BodyWrapperSerializer(BodyWrapper bodyWrapper) => _bodyWrapper = bodyWrapper;
}