namespace StrongTypes.Tests.MemberNames.Integer
{
    [StrongType(BaseType.Integer)]
    public readonly partial struct JsonConverter { }
}

namespace StrongTypes.Tests.MemberNames.Guid
{
    [StrongType(BaseType.Guid)]
    public readonly partial struct JsonConverter { }
}

namespace StrongTypes.Tests.MemberNames.String
{
    [StrongType(BaseType.String)]
    public partial class JsonConverter { }

    // Parse is only reserved for types that generate parsing methods.
    [StrongType(BaseType.String)]
    public partial class Parse { }
}

namespace StrongTypes.Tests.MemberNames.Email
{
    [StrongType(BaseType.EmailAddress)]
    public partial class JsonConverter { }
}

namespace StrongTypes.Tests.MemberNames.Uri
{
    [StrongType(BaseType.Uri)]
    public partial class JsonConverter { }
}

namespace StrongTypes.Tests.MemberNames
{
    // Intentionally matches the generated backing field to test collision handling.
#pragma warning disable IDE1006 // Naming Styles
    [StrongType(BaseType.Integer)]
    public readonly partial struct _value { }
#pragma warning restore IDE1006 // Naming Styles

    [StrongType(BaseType.Integer)]
    public partial class ExistingHelpers
    {
        private readonly int _value = 7;
        public int UserValue => _value;
        public class JsonConverter { }
        public class JsonConverter1 { }
    }
}
