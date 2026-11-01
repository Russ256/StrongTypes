namespace StrongTypes;

public class Exceptions
{
    public const string Source = """
                                    namespace StrongTypes;

                                    public class MaxLengthException : global::System.Exception
                                    {
                                        public MaxLengthException(global::System.Type type, int maxLength, int stringLength)
                                             : base($"String of length {stringLength} exceeds the maximum of {maxLength} for type {type.Name}")
                                        {
                                            Type = type;
                                            MaxLength = maxLength;
                                            StringLength = StringLength;
                                        }
                                    
                                        public global::System.Type Type { get; }
                                        public int MaxLength { get; }    
                                        public int StringLength { get; }
                                    
                                    }

                                    public class EmailAddressFormatException : global::System.Exception
                                    {
                                        public EmailAddressFormatException()
                                        {
                                        }
                                    }
                                    """;
}
