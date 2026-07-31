#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class TypeHelperTests
{
    [Fact]
    public void IsNumeric_ReturnsTrue_ForNumericTypes()
    {
        // Arrange
        var numericTypes = new[]
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        // Act & Assert
        foreach (var t in numericTypes)
        {
            t.IsNumeric().Should().BeTrue($"{t.Name} should be recognised as numeric");
        }
    }

    [Fact]
    public void IsNumeric_ReturnsFalse_ForNonNumericTypes()
    {
        // Arrange
        var nonNumeric = new[] { typeof(string), typeof(bool), typeof(object), typeof(DateTime) };

        // Act & Assert
        foreach (var t in nonNumeric)
        {
            t.IsNumeric().Should().BeFalse($"{t.Name} should not be recognised as numeric");
        }
    }

    [Fact]
    public void IsNumeric_Generic_Version_Matches_Type_Version()
    {
        // Act
        var resultInt = TypeHelper.IsNumeric<int>();
        var resultString = TypeHelper.IsNumeric<string>();

        // Assert
        resultInt.Should().BeTrue();
        resultString.Should().BeFalse();
    }

    [Fact]
    public void IsNullable_Identifies_Nullable_And_NonNullable_Types()
    {
        // Arrange
        Type nullable = typeof(int?);
        Type nonNullable = typeof(int);

        // Act & Assert
        nullable.IsNullable().Should().BeTrue();
        nonNullable.IsNullable().Should().BeFalse();
    }

    [Fact]
    public void GetUnderlyingType_Returns_Underlying_Type_For_Nullable_And_Same_For_NonNullable()
    {
        // Arrange
        Type nullable = typeof(double?);
        Type nonNullable = typeof(double);

        // Act
        var underlying = nullable.GetUnderlyingType();
        var same = nonNullable.GetUnderlyingType();

        // Assert
        underlying.Should().Be(typeof(double));
        same.Should().Be(typeof(double));
    }

    [Fact]
    public void ImplementsInterface_Returns_True_When_Type_Implements_Interface()
    {
        // Arrange
        Type type = typeof(List<int>);

        // Act
        var implements = type.ImplementsInterface<ICollection<int>>();

        // Assert
        implements.Should().BeTrue();
    }

    [Fact]
    public void ImplementsInterface_Returns_False_When_Type_Does_Not_Implement_Interface()
    {
        // Arrange
        Type type = typeof(string);

        // Act
        var implements = type.ImplementsInterface<IDisposable>();

        // Assert
        implements.Should().BeFalse();
    }

    [Fact]
    public void IsEnum_Generic_Returns_True_For_Enum_Types_And_False_For_NonEnum()
    {
        // Act
        var enumResult = TypeHelper.IsEnum<DayOfWeek>();
        var nonEnumResult = TypeHelper.IsEnum<int>();

        // Assert
        enumResult.Should().BeTrue();
        nonEnumResult.Should().BeFalse();
    }

    [Fact]
    public void IsCollection_Detects_Collections_But_Not_String_Or_Primitive()
    {
        // Arrange
        Type arrayType = typeof(int[]);
        Type listType = typeof(List<string>);
        Type stringType = typeof(string);
        Type intType = typeof(int);

        // Act & Assert
        arrayType.IsCollection().Should().BeTrue();
        listType.IsCollection().Should().BeTrue();
        stringType.IsCollection().Should().BeFalse();
        intType.IsCollection().Should().BeFalse();
    }

    [Fact]
    public void GetGenericArguments_Returns_Array_For_Generic_Type_And_Null_For_NonGeneric()
    {
        // Arrange
        Type generic = typeof(Dictionary<string, int>);
        Type nonGeneric = typeof(DateTime);

        // Act
        var genericArgs = generic.GetGenericArguments();
        var nonGenericArgs = nonGeneric.GetGenericArguments();

        // Assert
        genericArgs.Should().NotBeNull().And.HaveCount(2);
        genericArgs![0].Should().Be(typeof(string));
        genericArgs[1].Should().Be(typeof(int));

        nonGenericArgs.Should().BeNull();
    }

    [Fact]
    public void IsGeneric_Identifies_Generic_And_NonGeneric_Types()
    {
        // Arrange
        Type generic = typeof(List<double>);
        Type nonGeneric = typeof(Guid);

        // Act & Assert
        generic.IsGeneric().Should().BeTrue();
        nonGeneric.IsGeneric().Should().BeFalse();
    }

    private class SampleClass
    {
        public void NoParams() { }

        public int Add(int a, int b) => a + b;
    }

    [Fact]
    public void GetMethodBySignature_Finds_Method_When_Signature_Matches()
    {
        // Arrange
        Type type = typeof(SampleClass);
        var paramTypes = new[] { typeof(int), typeof(int) };

        // Act
        MethodInfo? method = type.GetMethodBySignature("Add", paramTypes);

        // Assert
        method.Should().NotBeNull();
        method!.Name.Should().Be("Add");
        method.ReturnType.Should().Be(typeof(int));
    }

    [Fact]
    public void GetMethodBySignature_Returns_Null_When_No_Matching_Method()
    {
        // Arrange
        Type type = typeof(SampleClass);
        var paramTypes = new[] { typeof(string) };

        // Act
        MethodInfo? method = type.GetMethodBySignature("Add", paramTypes);

        // Assert
        method.Should().BeNull();
    }
}
