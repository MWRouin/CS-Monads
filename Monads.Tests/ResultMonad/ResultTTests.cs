using MWR.Monads.Messages;
using MWR.Monads.ResultMonad;

namespace MWR.Monads.Tests.ResultMonad;

public class ResultTTests
{
    // ── Factory / State ───────────────────────────────────────────────────────

    [Fact]
    public void Success_NullValue_Throws()
    {
        var act = () => Results.Success<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Success_IsSuccess()
    {
        var result = Results.Success(42);
        result.IsSuccess().Should().BeTrue();
        result.IsFailure().Should().BeFalse();
    }

    [Fact]
    public void Failure_IsFailure()
    {
        var result = Results.Failure<int>(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.IsSuccess().Should().BeFalse();
    }

    // ── Value access ──────────────────────────────────────────────────────────

    [Fact]
    public void GetValue_OnSuccess_ReturnsValue()
    {
        var result = Results.Success(42);
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public void GetValue_OnFailure_Throws()
    {
        var result = Results.Failure<int>(Fixtures.AnyError);
        var act = () => result.GetValue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOr_OnSuccess_ReturnsValue()
    {
        var result = Results.Success(42);
        result.GetValueOr(-1).Should().Be(42);
    }

    [Fact]
    public void GetValueOr_OnFailure_ReturnsDefault()
    {
        var result = Results.Failure<int>(Fixtures.AnyError);
        result.GetValueOr(-1).Should().Be(-1);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsTypeDefault()
    {
        var result = Results.Failure<int>(Fixtures.AnyError);
        result.GetValueOrDefault().Should().Be(0);
    }

    // ── Implicit conversions ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitFrom_Value_IsSuccess()
    {
        Result<int> r = 42;
        r.IsSuccess().Should().BeTrue();
        r.GetValue().Should().Be(42);
    }

    [Fact]
    public void ImplicitFrom_Error_IsFailure()
    {
        Result<int> r = Fixtures.AnyError;
        r.IsFailure().Should().BeTrue();
        r.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void ImplicitFrom_ErrorArray_IsFailure_WithAllErrors()
    {
        Result<int> r = new Error[] { Fixtures.AnyError, Fixtures.AnotherError };
        r.IsFailure().Should().BeTrue();
        r.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void ExplicitCast_OnSuccess_ReturnsValue()
    {
        Result<int> r = 42;
        ((int)r).Should().Be(42);
    }

    [Fact]
    public void ExplicitCast_OnFailure_Throws()
    {
        Result<int> r = Fixtures.AnyError;
        var act = () => (int)r;
        act.Should().Throw<InvalidCastException>();
    }

    // ── Conversion to Result ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitTo_Result_PreservesSuccess()
    {
        Result<int> typed = 42;
        Result untyped = typed;
        untyped.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void ImplicitTo_Result_PreservesFailure()
    {
        Result<int> typed = Fixtures.AnyError;
        Result untyped = typed;
        untyped.IsFailure().Should().BeTrue();
        untyped.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void ImplicitTo_Result_PreservesInfosAndWarnings()
    {
        var result = Results.Success(42, [Fixtures.AnyInfo], [Fixtures.AnyWarning]);
        Result untyped = result;
        untyped.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
        untyped.GetWarnings().Should().ContainSingle().Which.Should().Be(Fixtures.AnyWarning);
    }

    // ── SuccessInfo ───────────────────────────────────────────────────────────

    [Fact]
    public void SuccessInfo_OnSuccess_AddsInfo()
    {
        var result = Results.Success(42);
        var withInfo = result.WithSuccessInfo(Fixtures.AnyInfo);
        withInfo.IsSuccess().Should().BeTrue();
        withInfo.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
    }

    [Fact]
    public void SuccessInfo_OnFailure_Unchanged()
    {
        var result = Results.Failure<int>(Fixtures.AnyError);
        var unchanged = result.WithSuccessInfo(Fixtures.AnyInfo);
        unchanged.IsFailure().Should().BeTrue();
        unchanged.GetInfos().Should().BeEmpty();
    }
}
