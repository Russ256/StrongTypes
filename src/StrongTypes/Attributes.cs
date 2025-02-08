namespace StrongTypes;

using System;
using System.Diagnostics;

[Conditional("Include_attibutes")] // So not included in compiled application
[AttributeUsage(AttributeTargets.Struct)]
public class GuidAttribute : Attribute
{
}