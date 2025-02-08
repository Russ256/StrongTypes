namespace StrongTypes;

using System;
using System.Diagnostics;

[Conditional("Include_attibutes")] // So not included in compiled application
public class GuidAttribute : Attribute
{
}