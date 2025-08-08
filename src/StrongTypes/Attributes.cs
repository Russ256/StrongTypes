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
                                        internal StrongTypeAttribute(Type baseType, int maxLength)
                                             : this(baseType)
                                        {
                                            MaxLength = maxLength;
                                        }

                                        internal StrongTypeAttribute(Type baseType)
                                        {
                                            BaseType = baseType;
                                        }
                                    
                                        internal Type BaseType { get; }
                                        internal int MaxLength { get; }
                                    }
                                    """;
}