using MWR.Monads.ResultMonad;

namespace MWR.Monads.Tests.ResultMonad;

public class ResultsTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValue_IsSuccess()
    {
        var result = Results.Create("hello");
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public void Create_NullNoError_IsFailure()
    {
        var result = Results.Create<string>(null);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Create_NullWithError_IsFailureWithError()
    {
        var result = Results.Create<string>(null, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    // ── Success / Failure factories ───────────────────────────────────────────

    [Fact]
    public void Success_NonGeneric_IsSuccess()
    {
        Results.Success().IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Success_Generic_IsSuccess_WithValue()
    {
        var r = Results.Success(42);
        r.IsSuccess().Should().BeTrue();
        r.GetValue().Should().Be(42);
    }

    [Fact]
    public void Failure_NonGeneric_IsFailure()
    {
        Results.Failure(Fixtures.AnyError).IsFailure().Should().BeTrue();
    }

    [Fact]
    public void Failure_Generic_IsFailure()
    {
        Results.Failure<int>(Fixtures.AnyError).IsFailure().Should().BeTrue();
    }

    // ── Ensure ────────────────────────────────────────────────────────────────

    [Fact]
    public void Ensure_PredicateTrue_IsSuccess()
    {
        var result = Results.Ensure("hello", v => v.Length > 3, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Ensure_PredicateFalse_IsFailure()
    {
        var result = Results.Ensure("hi", v => v.Length > 3, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Ensure_NullValue_IsFailure()
    {
        var result = Results.Ensure<string>(null, _ => true, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureAll (per-condition errors) ──────────────────────────────────────

    [Fact]
    public void EnsureAll_AllPass_IsSuccess()
    {
        var result = Results.EnsureAll(
            "hello",
            (v => v.Length > 3, Fixtures.AnyError),
            (v => v.StartsWith("h"), Fixtures.AnotherError));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureAll_SomeFail_CollectsAllErrors()
    {
        var result = Results.EnsureAll(
            "hi",
            (v => v.Length > 10, Fixtures.AnyError),
            (v => v.Length > 20, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void EnsureAll_NullValue_CollectsAllErrors()
    {
        var result = Results.EnsureAll<string>(
            null,
            (_ => true, Fixtures.AnyError),
            (_ => true, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    // ── EnsureAll (single error, multiple predicates) ─────────────────────────

    [Fact]
    public void EnsureAll_SingleError_AllPredicatesPass_IsSuccess()
    {
        var result = Results.EnsureAll("hello", Fixtures.AnyError, v => v.Length > 3, v => v.Contains('h'));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureAll_SingleError_AnyPredicateFails_IsFailure()
    {
        var result = Results.EnsureAll("hello", Fixtures.AnyError, v => v.Length > 3, v => v.Length > 100);
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureAll (single error) — empty predicates ───────────────────────────

    [Fact]
    public void EnsureAll_SingleError_EmptyPredicates_IsSuccess()
    {
        // vacuously true: all of zero predicates pass
        var result = Results.EnsureAll("hello", Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    // ── EnsureAny ─────────────────────────────────────────────────────────────

    [Fact]
    public void EnsureAny_AnyPredicateTrue_IsSuccess()
    {
        var result = Results.EnsureAny("hello", Fixtures.AnyError, v => v.Length > 100, v => v.Length > 3);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureAny_AllPredicatesFalse_IsFailure()
    {
        var result = Results.EnsureAny("hello", Fixtures.AnyError, v => v.Length > 100, v => v.Length > 200);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void EnsureAny_NullValue_IsFailure()
    {
        var result = Results.EnsureAny<string>(null, Fixtures.AnyError, _ => true);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void EnsureAny_EmptyPredicates_IsSuccess()
    {
        // no constraints → nothing to violate → consistent with EnsureAll empty behaviour
        var result = Results.EnsureAny("hello", Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureAny_EmptyPredicates_NullValue_IsFailure()
    {
        var result = Results.EnsureAny<string>(null, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureNot ─────────────────────────────────────────────────────────────

    [Fact]
    public void EnsureNot_PredicateFalse_IsSuccess()
    {
        var result = Results.EnsureNot("hello", v => v.Length > 100, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNot_PredicateTrue_IsFailure()
    {
        var result = Results.EnsureNot("hello", v => v.Length > 3, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void EnsureNot_NullValue_IsFailure()
    {
        var result = Results.EnsureNot<string>(null, _ => false, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureNotAny (per-condition errors) ───────────────────────────────────

    [Fact]
    public void EnsureNotAny_NoneTrue_IsSuccess()
    {
        var result = Results.EnsureNotAny(
            "hello",
            (v => v.Length > 100, Fixtures.AnyError),
            (v => v.Length > 200, Fixtures.AnotherError));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAny_SomeTrue_CollectsAllMatchingErrors()
    {
        var result = Results.EnsureNotAny(
            "hello",
            (v => v.Length > 3, Fixtures.AnyError),
            (v => v.Length > 3, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void EnsureNotAny_NullValue_CollectsAllErrors()
    {
        var result = Results.EnsureNotAny<string>(
            null,
            (_ => false, Fixtures.AnyError),
            (_ => false, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    // ── EnsureNotAll ──────────────────────────────────────────────────────────

    [Fact]
    public void EnsureNotAll_NotAllTrue_IsSuccess()
    {
        var result = Results.EnsureNotAll("hello", Fixtures.AnyError, v => v.Length > 3, v => v.Length > 100);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAll_AllTrue_IsFailure()
    {
        var result = Results.EnsureNotAll("hello", Fixtures.AnyError, v => v.Length > 3, v => v.Length > 1);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAll_NullValue_IsFailure()
    {
        var result = Results.EnsureNotAll<string>(null, Fixtures.AnyError, _ => false);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAll_EmptyPredicates_IsSuccess()
    {
        // no constraints → nothing to violate → consistent with EnsureAll empty behaviour
        var result = Results.EnsureNotAll("hello", Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAll_EmptyPredicates_NullValue_IsFailure()
    {
        var result = Results.EnsureNotAll<string>(null, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    // ── Combine ───────────────────────────────────────────────────────────────

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess_MergesInfosAndWarnings()
    {
        var r1 = Results.Success(Fixtures.AnyInfo);
        var r2 = Results.Success(null, [Fixtures.AnyWarning]);
        var combined = Results.Combine(r1, r2);
        combined.IsSuccess().Should().BeTrue();
        combined.GetInfos().Should().HaveCount(1);
        combined.GetWarnings().Should().HaveCount(1);
    }

    [Fact]
    public void Combine_AnyFailure_ReturnsFailure_CollectsAllErrors()
    {
        var r1 = Results.Failure(Fixtures.AnyError);
        var r2 = Results.Failure(Fixtures.AnotherError);
        var r3 = Results.Success();
        var combined = Results.Combine(r1, r2, r3);
        combined.IsFailure().Should().BeTrue();
        combined.GetErrors().Should().HaveCount(2);
        combined.GetErrors().Should().Contain(Fixtures.AnyError);
        combined.GetErrors().Should().Contain(Fixtures.AnotherError);
    }

    // ── Combine<T> ────────────────────────────────────────────────────────────

    [Fact]
    public void CombineT_AllSuccess_ReturnsValuesArray()
    {
        var r1 = Results.Success(1);
        var r2 = Results.Success(2);
        var r3 = Results.Success(3);
        var combined = Results.Combine(r1, r2, r3);
        combined.IsSuccess().Should().BeTrue();
        combined.GetValue().Should().Equal(1, 2, 3);
    }

    [Fact]
    public void CombineT_AnyFailure_CollectsAllErrors()
    {
        var r1 = Results.Success(1);
        var r2 = Results.Failure<int>(Fixtures.AnyError);
        var r3 = Results.Failure<int>(Fixtures.AnotherError);
        var combined = Results.Combine(r1, r2, r3);
        combined.IsFailure().Should().BeTrue();
        combined.GetErrors().Should().HaveCount(2);
    }

    // ── Combine edge cases ────────────────────────────────────────────────────

    [Fact]
    public void Combine_EmptyArray_IsSuccess()
    {
        var combined = Results.Combine([]);
        combined.IsSuccess().Should().BeTrue();
        combined.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Combine_SingleFailure_IsFailure_WithError()
    {
        var combined = Results.Combine(Results.Failure(Fixtures.AnyError));
        combined.IsFailure().Should().BeTrue();
        combined.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Combine_AnyFailure_PreservesWarningsFromSuccesses()
    {
        var r1 = Results.Success(null, [Fixtures.AnyWarning]);
        var r2 = Results.Failure(Fixtures.AnyError);
        var combined = Results.Combine(r1, r2);
        combined.IsFailure().Should().BeTrue();
        combined.GetWarnings().Should().Contain(Fixtures.AnyWarning);
    }

    [Fact]
    public void CombineT_AllSuccess_PreservesInfosAndWarnings()
    {
        var r1 = Results.Success(1, [Fixtures.AnyInfo]);
        var r2 = Results.Success(2, null, [Fixtures.AnyWarning]);
        var combined = Results.Combine(r1, r2);
        combined.IsSuccess().Should().BeTrue();
        combined.GetInfos().Should().Contain(Fixtures.AnyInfo);
        combined.GetWarnings().Should().Contain(Fixtures.AnyWarning);
    }

    [Fact]
    public void CombineT_EmptyArray_IsSuccessWithEmptyValues()
    {
        var combined = Results.Combine<int>([]);
        combined.IsSuccess().Should().BeTrue();
        combined.GetValue().Should().BeEmpty();
    }
}
