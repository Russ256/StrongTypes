namespace StrongTypes;

public class Exceptions
{
    public const string Source = """
                                    namespace StrongTypes;

                                    using System;

                                    public class MaxLengthException : System.Exception
                                    {
                                        public MaxLengthException(Type type, int maxLength, int stringLength)
                                             : base($"String of length {stringLength} exceeds the maximum of {maxLength} for type {type.Name}")
                                        {
                                            Type = type;
                                            MaxLength = maxLength;
                                            StringLength = StringLength;
                                        }
                                    
                                        public Type Type { get; }
                                        public int MaxLength { get; }    
                                        public int StringLength { get; }
                                    
                                    }

                                    public class EmailAddressFormatException : System.Exception
                                    {
                                        public EmailAddressFormatException()
                                        {
                                        }
                                    }
                                    """;
}