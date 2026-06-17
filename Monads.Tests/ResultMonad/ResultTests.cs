using MWR.Monads.Messages;
using MWR.Monads.ResultMonad;

namespace MWR.Monads.Tests.ResultMonad;

public class ResultTests
{
    // ── Factory / State ───────────────────────────────────────────────────────

    [Fact]
    public void Success_IsSuccess_IsNotFailure()
    {
        var result = Results.Success();
        result.IsSuccess().Should().BeTrue();
        result.IsFailure().Should().BeFalse();
    }

    [Fact]
    public void Failure_IsFailure_IsNotSuccess()
    {
        var result = Results.Failure(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.IsSuccess().Should().BeFalse();
    }

    [Fact]
    public void Success_WithInfos_HasInfos()
    {
        var result = Results.Success(Fixtures.AnyInfo);
        result.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
    }

    [Fact]
    public void Success_WithWarnings_HasWarnings()
    {
        var result = Results.Success(null, [Fixtures.AnyWarning]);
        result.GetWarnings().Should().ContainSingle().Which.Should().Be(Fixtures.AnyWarning);
    }

    [Fact]
    public void Failure_WithErrors_HasErrors()
    {
        var result = Results.Failure(Fixtures.AnyError, Fixtures.AnotherError);
        result.GetErrors().Should().HaveCount(2);
        result.GetErrors().Should().Contain(Fixtures.AnyError);
        result.GetErrors().Should().Contain(Fixtures.AnotherError);
    }

    [Fact]
    public void Success_GetErrors_ReturnsEmpty()
    {
        var result = Results.Success();
        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Failure_GetInfos_ReturnsEmpty()
    {
        var result = Results.Failure(Fixtures.AnyError);
        result.GetInfos().Should().BeEmpty();
    }

    // ── Implicit conversions ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitFrom_Error_IsFailure()
    {
        Result result = Fixtures.AnyError;
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void ImplicitFrom_ErrorArray_IsFailure_WithAllErrors()
    {
        Result result = new Error[] { Fixtures.AnyError, Fixtures.AnotherError };
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void ImplicitFrom_Information_IsSuccess_WithInfo()
    {
        Result result = Fixtures.AnyInfo;
        result.IsSuccess().Should().BeTrue();
        result.GetInfos().Should().Contain(Fixtures.AnyInfo);
    }

    [Fact]
    public void ImplicitFrom_Warning_IsSuccess_WithWarning()
    {
        Result result = Fixtures.AnyWarning;
        result.IsSuccess().Should().BeTrue();
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
    }

    // ── SuccessInfo ───────────────────────────────────────────────────────────

    [Fact]
    public void SuccessInfo_OnSuccess_AppendsInfo()
    {
        var result = Results.Success(Fixtures.AnyInfo);
        var withExtra = result.WithSuccessInfo(Fixtures.AnyInfo);
        withExtra.GetInfos().Should().HaveCount(2);
    }

    [Fact]
    public void SuccessInfo_OnFailure_Unchanged()
    {
        var result = Results.Failure(Fixtures.AnyError);
        var unchanged = result.WithSuccessInfo(Fixtures.AnyInfo);
        unchanged.IsFailure().Should().BeTrue();
        unchanged.GetInfos().Should().BeEmpty();
    }

    // ── Operator true/false ───────────────────────────────────────────────────

    [Fact]
    public void OperatorTrue_OnSuccess_IsTrue()
    {
        var result = Results.Success();
        (result ? "yes" : "no").Should().Be("yes");
    }

    [Fact]
    public void OperatorFalse_OnFailure_IsFalse()
    {
        var result = Results.Failure(Fixtures.AnyError);
        (result ? "yes" : "no").Should().Be("no");
    }
}
