namespace StrongTypes;

public class Attributes
{
    public const string Source = """
                                    namespace StrongTypes;

                                    // Retained at runtime for optional integrations, without a runtime library dependency.
                                    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Struct | global::System.AttributeTargets.Class, Inherited = false)]
                                    internal sealed class StrongTypeMetadataAttribute : global::System.Attribute
                                    {
                                        public StrongTypeMetadataAttribute(global::System.Type valueType) => ValueType = valueType;
                                        public global::System.Type ValueType { get; }
                                    }

                                    [global::System.Diagnostics.ConditionalAttribute("Include_attributes")] // So not included in compiled application
                                    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Struct | global::System.AttributeTargets.Class, AllowMultiple = false)]
                                    internal class StrongTypeAttribute : global::System.Attribute
                                    {
                                        internal StrongTypeAttribute(BaseType baseType, int maxLength)
                                             : this(baseType)
                                        {
                                            MaxLength = maxLength;
                                        }

                                        internal StrongTypeAttribute(BaseType baseType)
                                        {
                                            BaseType = baseType;
                                        }
                                    
                                        internal BaseType BaseType { get; }
                                        internal int? MaxLength { get; }
                                    }

                                    internal enum BaseType
                                    {
                                        Integer = 0,
                                        Guid = 1,
                                        Uri = 2,
                                        String = 3,
                                        EmailAddress = 4
                                    }

                                    
                                    """;
}
