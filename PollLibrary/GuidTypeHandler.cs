using System;
using System.Data;
using Dapper;

namespace PollLibrary.DataAccess;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    // How to write a Guid parameter back to the DB
    public override void SetValue(IDbDataParameter parameter, Guid value)
        => parameter.Value = value.ToString();

    // How to parse a DB value into a Guid
    public override Guid Parse(object value)
    {
        if (value is Guid g)
            return g;
        if (value is string s && Guid.TryParse(s, out var parsed))
            return parsed;

        throw new DataException($"Cannot convert {value?.GetType()} to Guid");
    }
}
