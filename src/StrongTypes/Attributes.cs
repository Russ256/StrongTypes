namespace StrongTypes;

public class Attributes
{
    public const string Source = """
                                    namespace StrongTypes;

                                    using System;
                                    using System.Diagnostics;

                                    [Conditional("Include_attributes")] // So not included in compiled application  
                                    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
                                    internal class StrongTypeAttribute : System.Attribute
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
                                        internal int MaxLength { get; }
                                    }

                                    public enum BaseType
                                    {
                                        Integer = 0,
                                        Guid = 1,
                                        Uri = 2,
                                        String = 3,
                                        EmailAddress = 4
                                    }

                                    
                                    """;
}
