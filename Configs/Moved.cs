using System;

namespace SPIC.Configs;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class MovedTo : Attribute {
    public Type? Host {get;}
    public string[] Members {get;}

    public MovedTo(params string[] members) : this(null, members) { }
    public MovedTo(Type? host, params string[] members) {
        Host = host;
        Members = members.Length == 1 ? members[0].Split('.') : members;
    }
}