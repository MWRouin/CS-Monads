using MWR.Monads.Messages;
using MWR.Monads.ResultMonad;

namespace MWR.Monads.Tests.ResultMonad;

public class ResultExtensionsTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  extension<T>(Result<T> result)
    // ═══════════════════════════════════════════════════════════════════════════

    // ── ToMaybe ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToMaybe_OnSuccess_ReturnsSome()
    {
        var maybe = Results.Success(42).ToMaybe();
        maybe.HasValue().Should().BeTrue();
        maybe.GetValue().Should().Be(42);
    }

    [Fact]
    public void ToMaybe_OnFailure_ReturnsNone()
    {
        var maybe = Results.Failure<int>(Fixtures.AnyError).ToMaybe();
        maybe.HasNoValue().Should().BeTrue();
    }

    // ── Map<TOut> ─────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_ReturnsMappedValue()
    {
        var result = Results.Success(5).Map(v => v.ToString());
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public void Map_OnFailure_FuncNeverCalled_PropagatesErrors()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Map(v => { called = true; return v.ToString(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Map_OnFailure_PreservesWarningsAndInfos()
    {
        var source = Results.Failure<int>(
            new Error[] { Fixtures.AnyError },
            new Warning[] { Fixtures.AnyWarning },
            new Information[] { Fixtures.AnyInfo });
        var result = source.Map(v => v.ToString());
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
        result.GetInfos().Should().Contain(Fixtures.AnyInfo);
    }

    // ── MapAsync (Func<T, Task<TOut>>) ────────────────────────────────────────

    [Fact]
    public async Task MapAsync_OnSuccess_FastPath_ReturnsMappedValue()
    {
        var result = await Results.Success(5).MapAsync(_ => Task.FromResult("hello"));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task MapAsync_OnSuccess_SlowPath_ReturnsMappedValue()
    {
        var result = await Results.Success(5).MapAsync(v => Async.Slow(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task MapAsync_OnFailure_FuncNeverCalled_PropagatesErrors()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).MapAsync(_ => { called = true; return Task.FromResult("x"); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    // ── MapAsync CancellationToken forwarding (T9) ────────────────────────────

    [Fact]
    public async Task MapAsync_CT_TokenForwardedToFunc()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Results.Success(5).MapAsync((v, ct) =>
        {
            capturedToken = ct;
            return Task.FromResult(v.ToString());
        }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── MapAsync (Func<T, CancellationToken, Task<TOut>>) ────────────────────

    [Fact]
    public async Task MapAsync_CT_OnSuccess_FastPath_ReturnsMappedValue()
    {
        var result = await Results.Success(5).MapAsync((_, _) => Task.FromResult("hello"));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task MapAsync_CT_OnSuccess_SlowPath_ReturnsMappedValue()
    {
        var result = await Results.Success(5).MapAsync((v, _) => Async.Slow(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task MapAsync_CT_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).MapAsync((_, _) => { called = true; return Task.FromResult("x"); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Bind<TOut> ────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = Results.Success(5).Bind(v => Results.Success(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public void Bind_ToResultT_OnFailure_FuncNeverCalled_PropagatesErrors()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Bind(v => { called = true; return Results.Success(v.ToString()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Bind_ToResultT_OnFailure_PreservesWarningsAndInfos()
    {
        var source = Results.Failure<int>(
            new Error[] { Fixtures.AnyError },
            new Warning[] { Fixtures.AnyWarning },
            new Information[] { Fixtures.AnyInfo });
        var result = source.Bind(v => Results.Success(v.ToString()));
        result.IsFailure().Should().BeTrue();
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
        result.GetInfos().Should().Contain(Fixtures.AnyInfo);
    }

    [Fact]
    public void Bind_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = Results.Success(5).Bind(_ => Results.Success());
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Bind_ToResult_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Bind(_ => { called = true; return Results.Success(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── BindAsync<TOut> ───────────────────────────────────────────────────────

    [Fact]
    public async Task BindAsync_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = await Results.Success(5).BindAsync(v => Task.FromResult(Results.Success(v.ToString())));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_ToResultT_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).BindAsync(v => { called = true; return Task.FromResult(Results.Success(v.ToString())); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = await Results.Success(5).BindAsync((v, _) => Task.FromResult(Results.Success(v.ToString())));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_CT_ToResultT_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).BindAsync((_, _) => { called = true; return Task.FromResult(Results.Success("x")); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResultT_TokenForwardedToFunc()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Results.Success(5).BindAsync((v, ct) =>
        {
            capturedToken = ct;
            return Task.FromResult(Results.Success(v.ToString()));
        }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task BindAsync_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = await Results.Success(5).BindAsync(_ => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_ToResult_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).BindAsync(_ => { called = true; return Task.FromResult(Results.Success()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = await Results.Success(5).BindAsync((_, _) => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResult_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).BindAsync((_, _) => { called = true; return Task.FromResult(Results.Success()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Ensure (single predicate) ─────────────────────────────────────────────

    [Fact]
    public void Ensure_Single_PredicatePasses_Unchanged()
    {
        var result = Results.Success(10).Ensure(v => v > 5, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(10);
    }

    [Fact]
    public void Ensure_Single_PredicateFails_ReturnsFailure()
    {
        var result = Results.Success(3).Ensure(v => v > 5, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Ensure_Single_FailureInput_ShortCircuits()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Ensure(v => { called = true; return v > 5; }, Fixtures.AnotherError);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void Ensure_Single_PredicateFails_PreservesExistingWarnings()
    {
        var source = Results.Failure<int>(
            new Error[] { Fixtures.AnyError },
            new Warning[] { Fixtures.AnyWarning },
            null);
        var result = source.Ensure(_ => true, Fixtures.AnotherError);
        result.IsFailure().Should().BeTrue();
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
    }

    [Fact]
    public void Ensure_WhenSuccessPredicateFails_PreservesExistingWarnings()
    {
        var source = Results.Success(3, null, [Fixtures.AnyWarning]);
        var result = source.Ensure(v => v > 5, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
    }

    // ── Ensure (batch) ────────────────────────────────────────────────────────

    [Fact]
    public void Ensure_Batch_AllPass_Unchanged()
    {
        var result = Results.Success(10).Ensure(
            (v => v > 5, Fixtures.AnyError),
            (v => v > 3, Fixtures.AnotherError));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Ensure_Batch_SomeConditionsFail_CollectsAllErrors()
    {
        var result = Results.Success(5).Ensure(
            (v => v > 10, Fixtures.AnyError),
            (v => v > 20, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    [Fact]
    public void Ensure_Batch_FailureInput_ShortCircuits()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Ensure(
            (v => { called = true; return v > 5; }, Fixtures.AnotherError));
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureNot (single predicate) ──────────────────────────────────────────

    [Fact]
    public void EnsureNot_Single_PredicateFalse_Unchanged()
    {
        var result = Results.Success(3).EnsureNot(v => v > 5, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNot_Single_PredicateTrue_ReturnsFailure()
    {
        var result = Results.Success(10).EnsureNot(v => v > 5, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void EnsureNot_Single_FailureInput_ShortCircuits()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).EnsureNot(v => { called = true; return v > 5; }, Fixtures.AnotherError);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── EnsureNotAny (batch) ──────────────────────────────────────────────────

    [Fact]
    public void EnsureNotAny_Batch_NoneTrue_Unchanged()
    {
        var result = Results.Success(5).EnsureNotAny(
            (v => v > 100, Fixtures.AnyError),
            (v => v > 200, Fixtures.AnotherError));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void EnsureNotAny_Batch_SomeConditionsTrue_CollectsAllErrors()
    {
        var result = Results.Success(5).EnsureNotAny(
            (v => v < 10, Fixtures.AnyError),
            (v => v < 20, Fixtures.AnotherError));
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().HaveCount(2);
    }

    // ── Tap ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Tap_OnSuccess_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = Results.Success(42).Tap(v => captured = v);
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public void Tap_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = Results.Failure<int>(Fixtures.AnyError).Tap(_ => called = true);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── TapAsync (Func<T, Task>) ──────────────────────────────────────────────

    [Fact]
    public async Task TapAsync_OnSuccess_FastPath_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Results.Success(42).TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_OnSuccess_SlowPath_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Results.Success(42).TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_OnFailure_ActionNotCalled_ResultUnchanged()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void TapAsync_OnSuccess_FastPath_DoesNotAwait()
    {
        var task = Results.Success(42).TapAsync(_ => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── TapAsync (Func<T, CancellationToken, Task>) ───────────────────────────

    [Fact]
    public async Task TapAsync_CT_OnSuccess_FastPath_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Results.Success(42).TapAsync((v, _) => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_CT_OnSuccess_SlowPath_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Results.Success(42).TapAsync(async (v, _) => { await Task.Yield(); captured = v; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Results.Failure<int>(Fixtures.AnyError).TapAsync((_, _) => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_TokenForwardedToAction()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Results.Success(42).TapAsync((_, ct) => { capturedToken = ct; return Task.CompletedTask; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── TapError ──────────────────────────────────────────────────────────────

    [Fact]
    public void TapError_OnFailure_ActionCalled()
    {
        Error[]? captured = null;
        var result = Results.Failure<int>(Fixtures.AnyError).TapError(errors => captured = errors);
        captured.Should().NotBeNull();
        captured.Should().Contain(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void TapError_OnSuccess_ActionNotCalled()
    {
        var called = false;
        var result = Results.Success(42).TapError(_ => called = true);
        called.Should().BeFalse();
        result.IsSuccess().Should().BeTrue();
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSuccess_OnSuccessFuncCalled()
    {
        var result = Results.Success(5).Match(v => $"ok:{v}", _ => "fail");
        result.Should().Be("ok:5");
    }

    [Fact]
    public void Match_OnFailure_OnFailureFuncCalled()
    {
        var result = Results.Failure<int>(Fixtures.AnyError).Match(v => $"ok:{v}", errors => $"fail:{errors.Length}");
        result.Should().Be("fail:1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  extension(Result result) — non-generic block
    // ═══════════════════════════════════════════════════════════════════════════

    // ── SuccessInfo ───────────────────────────────────────────────────────────

    [Fact]
    public void Result_SuccessInfo_OnSuccess_AppendsInfo()
    {
        var result = Results.Success(Fixtures.AnyInfo);
        var withExtra = result.WithSuccessInfo(Fixtures.AnyInfo);
        withExtra.IsSuccess().Should().BeTrue();
        withExtra.GetInfos().Should().HaveCount(2);
    }

    [Fact]
    public void Result_SuccessInfo_OnFailure_Unchanged()
    {
        var result = Results.Failure(Fixtures.AnyError);
        var unchanged = result.WithSuccessInfo(Fixtures.AnyInfo);
        unchanged.IsFailure().Should().BeTrue();
    }

    // ── Map<T> ────────────────────────────────────────────────────────────────

    [Fact]
    public void Result_Map_OnSuccess_ReturnsMappedValue()
    {
        var result = Results.Success().Map(() => 42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public void Result_Map_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = Results.Failure(Fixtures.AnyError).Map(() => { called = true; return 42; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── MapAsync<T> ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_MapAsync_OnSuccess_FastPath_ReturnsMappedValue()
    {
        var result = await Results.Success().MapAsync(() => Task.FromResult(42));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task Result_MapAsync_OnSuccess_SlowPath_ReturnsMappedValue()
    {
        var result = await Results.Success().MapAsync(() => Async.Slow(42));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task Result_MapAsync_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure(Fixtures.AnyError).MapAsync(() => { called = true; return Task.FromResult(42); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Bind(Func<Result>) ────────────────────────────────────────────────────

    [Fact]
    public void Result_Bind_OnSuccess_ReturnsFuncResult()
    {
        var result = Results.Success().Bind(() => Results.Success(Fixtures.AnyInfo));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Result_Bind_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = Results.Failure(Fixtures.AnyError).Bind(() => { called = true; return Results.Success(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── BindAsync(Func<Task<Result>>) ─────────────────────────────────────────

    [Fact]
    public async Task Result_BindAsync_OnSuccess_ReturnsFuncResult()
    {
        var result = await Results.Success().BindAsync(() => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Results.Failure(Fixtures.AnyError).BindAsync(() => { called = true; return Task.FromResult(Results.Success()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Tap / TapError ────────────────────────────────────────────────────────

    [Fact]
    public void Result_Tap_OnSuccess_ActionCalled()
    {
        var called = false;
        var result = Results.Success().Tap(() => called = true);
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void Result_Tap_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = Results.Failure(Fixtures.AnyError).Tap(() => called = true);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void Result_TapError_OnFailure_ActionCalled()
    {
        Error[]? captured = null;
        var result = Results.Failure(Fixtures.AnyError).TapError(errors => captured = errors);
        captured.Should().NotBeNull();
        captured.Should().Contain(Fixtures.AnyError);
        result.IsSuccess().Should().BeFalse();
    }

    [Fact]
    public void Result_TapError_OnSuccess_ActionNotCalled()
    {
        var called = false;
        var result = Results.Success().TapError(_ => called = true);
        called.Should().BeFalse();
        result.IsSuccess().Should().BeTrue();
    }

    // ── Result.TapAsync (Func<Task>) ──────────────────────────────────────────

    [Fact]
    public async Task Result_TapAsync_OnSuccess_FastPath_ActionCalled_ResultUnchanged()
    {
        var called = false;
        var result = await Results.Success().TapAsync(() => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_OnSuccess_SlowPath_ActionCalled_ResultUnchanged()
    {
        var called = false;
        var result = await Results.Success().TapAsync(async () => { await Task.Yield(); called = true; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_OnFailure_ActionNotCalled_ResultUnchanged()
    {
        var called = false;
        var result = await Results.Failure(Fixtures.AnyError).TapAsync(() => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public void Result_TapAsync_OnSuccess_FastPath_DoesNotAwait()
    {
        var task = Results.Success().TapAsync(() => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── Result.TapAsync (Func<CancellationToken, Task>) ───────────────────────

    [Fact]
    public async Task Result_TapAsync_CT_OnSuccess_FastPath_ActionCalled()
    {
        var called = false;
        var result = await Results.Success().TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_OnSuccess_SlowPath_ActionCalled()
    {
        var called = false;
        var result = await Results.Success().TapAsync(async _ => { await Task.Yield(); called = true; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Results.Failure(Fixtures.AnyError).TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_TokenForwardedToAction()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Results.Success().TapAsync(ct => { capturedToken = ct; return Task.CompletedTask; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Result_Match_OnSuccess_OnSuccessFuncCalled()
    {
        var result = Results.Success().Match(() => "ok", _ => "fail");
        result.Should().Be("ok");
    }

    [Fact]
    public void Result_Match_OnFailure_OnFailureFuncCalled()
    {
        var result = Results.Failure(Fixtures.AnyError).Match(() => "ok", errors => $"fail:{errors.Length}");
        result.Should().Be("fail:1");
    }
}
