namespace StrongTypes;

public class Attributes
{
    public const string Source = """
                                        namespace StrongTypes;

                                        using System.Diagnostics;

                                        [Conditional("Include_attibutes")] // So not included in compiled application  
                                        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
                                        internal class StrongTypeAttribute : System.Attribute
                                        {
                                            internal StrongTypeAttribute(Type baseType)
                                            {
                                                BaseType = baseType;
                                            }

                                            internal Type BaseType { get; }
                                        }
                                     
                                     """;
}