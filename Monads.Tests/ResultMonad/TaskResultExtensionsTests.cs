namespace MWR.Monads.Tests.ResultMonad;

public class TaskResultExtensionsTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  extension<T>(Task<Result<T>> resultTask)
    // ═══════════════════════════════════════════════════════════════════════════

    // ── SuccessInfo ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SuccessInfo_FastPath_OnSuccess_AppendsInfo()
    {
        var result = await Task.FromResult(Results.Success(42)).WithSuccessInfo(Fixtures.AnyInfo);
        result.IsSuccess().Should().BeTrue();
        result.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
    }

    [Fact]
    public async Task SuccessInfo_SlowPath_OnSuccess_AppendsInfo()
    {
        var result = await Async.Slow(Results.Success(42)).WithSuccessInfo(Fixtures.AnyInfo);
        result.IsSuccess().Should().BeTrue();
        result.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
    }

    [Fact]
    public async Task SuccessInfo_OnFailure_Unchanged()
    {
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError)).WithSuccessInfo(Fixtures.AnyInfo);
        result.IsFailure().Should().BeTrue();
    }

    // ── ToMaybe ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToMaybe_OnSuccess_ReturnsSome()
    {
        var maybe = await Task.FromResult(Results.Success(42)).ToMaybe();
        maybe.HasValue().Should().BeTrue();
        maybe.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task ToMaybe_OnFailure_ReturnsNone()
    {
        var maybe = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError)).ToMaybe();
        maybe.HasNoValue().Should().BeTrue();
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Map_FastPath_OnSuccess_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Results.Success(5)).Map(v => v.ToString());
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task Map_SlowPath_OnSuccess_ReturnsMappedValue()
    {
        var result = await Async.Slow(Results.Success(5)).Map(v => v.ToString());
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task Map_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError)).Map(v => { called = true; return v.ToString(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── MapAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task MapAsync_OnSuccess_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Results.Success(5)).MapAsync(v => Async.Slow(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task MapAsync_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .MapAsync(v => { called = true; return Async.Slow(v.ToString()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task MapAsync_CT_OnSuccess_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Results.Success(5)).MapAsync((v, _) => Async.Slow(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task MapAsync_CT_TokenForwardedToFunc()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Results.Success(5)).MapAsync((v, ct) =>
        {
            capturedToken = ct;
            return Async.Slow(v.ToString());
        }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── Bind ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Bind_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5)).Bind(v => Results.Success(v.ToString()));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task Bind_ToResultT_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .Bind(v => { called = true; return Results.Success(v.ToString()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task Bind_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5)).Bind(_ => Results.Success());
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Bind_ToResult_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .Bind(_ => { called = true; return Results.Success(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── BindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BindAsync_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5))
            .BindAsync(v => Task.FromResult(Results.Success(v.ToString())));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_ToResultT_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .BindAsync(v => { called = true; return Task.FromResult(Results.Success(v.ToString())); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResultT_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5))
            .BindAsync((v, _) => Task.FromResult(Results.Success(v.ToString())));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_CT_ToResultT_TokenForwardedToFunc()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Results.Success(5))
            .BindAsync((v, ct) =>
            {
                capturedToken = ct;
                return Task.FromResult(Results.Success(v.ToString()));
            }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task Bind_ToResultT_OnFailure_PreservesWarningsAndInfos()
    {
        var source = Results.Failure<int>(
            new Error[] { Fixtures.AnyError },
            new Warning[] { Fixtures.AnyWarning },
            new Information[] { Fixtures.AnyInfo });
        var result = await Task.FromResult(source).Bind(v => Results.Success(v.ToString()));
        result.IsFailure().Should().BeTrue();
        result.GetWarnings().Should().Contain(Fixtures.AnyWarning);
        result.GetInfos().Should().Contain(Fixtures.AnyInfo);
    }

    [Fact]
    public async Task BindAsync_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5))
            .BindAsync(_ => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_ToResult_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .BindAsync(_ => { called = true; return Task.FromResult(Results.Success()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_ToResult_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success(5))
            .BindAsync((_, _) => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    // ── Ensure / EnsureNot ────────────────────────────────────────────────────

    [Fact]
    public async Task Ensure_OnSuccess_PredicatePasses_Unchanged()
    {
        var result = await Task.FromResult(Results.Success(10)).Ensure(v => v > 5, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Ensure_OnSuccess_PredicateFails_ReturnsFailure()
    {
        var result = await Task.FromResult(Results.Success(3)).Ensure(v => v > 5, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public async Task EnsureNot_OnSuccess_PredicateFalse_Unchanged()
    {
        var result = await Task.FromResult(Results.Success(3)).EnsureNot(v => v > 5, Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task EnsureNot_OnSuccess_PredicateTrue_ReturnsFailure()
    {
        var result = await Task.FromResult(Results.Success(10)).EnsureNot(v => v > 5, Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    // ── Tap / TapError ────────────────────────────────────────────────────────

    [Fact]
    public async Task Tap_OnSuccess_ActionCalled()
    {
        var captured = 0;
        var result = await Task.FromResult(Results.Success(42)).Tap(v => captured = v);
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Tap_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError)).Tap(_ => called = true);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task TapError_OnFailure_ActionCalled()
    {
        Error[]? captured = null;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError)).TapError(e => captured = e);
        captured.Should().NotBeNull();
        captured!.Should().Contain(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task TapError_OnSuccess_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success(42)).TapError(_ => called = true);
        called.Should().BeFalse();
        result.IsSuccess().Should().BeTrue();
    }

    // ── TapAsync (Func<T, Task>) ──────────────────────────────────────────────

    [Fact]
    public async Task TapAsync_FastOuter_FastInner_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Task.FromResult(Results.Success(42)).TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_SlowOuter_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Async.Slow(Results.Success(42)).TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_FastOuter_SlowInner_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Task.FromResult(Results.Success(42)).TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void TapAsync_FastOuterFastInner_DoesNotAwait()
    {
        var task = Task.FromResult(Results.Success(42)).TapAsync(_ => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── TapAsync (Func<T, CancellationToken, Task>) ───────────────────────────

    [Fact]
    public async Task TapAsync_CT_FastOuter_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Task.FromResult(Results.Success(42)).TapAsync((v, _) => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_SlowOuter_ActionCalled_ResultUnchanged()
    {
        var captured = 0;
        var result = await Async.Slow(Results.Success(42)).TapAsync((v, _) => { captured = v; return Task.CompletedTask; });
        captured.Should().Be(42);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .TapAsync((_, _) => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_TokenForwardedToAction()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Results.Success(42)).TapAsync((_, ct) =>
        {
            capturedToken = ct;
            return Task.CompletedTask;
        }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Match_OnSuccess_OnSuccessFuncCalled()
    {
        var result = await Task.FromResult(Results.Success(5)).Match(v => $"ok:{v}", _ => "fail");
        result.Should().Be("ok:5");
    }

    [Fact]
    public async Task Match_OnFailure_OnFailureFuncCalled()
    {
        var result = await Task.FromResult(Results.Failure<int>(Fixtures.AnyError))
            .Match(v => $"ok:{v}", errors => $"fail:{errors.Length}");
        result.Should().Be("fail:1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  extension(Task<Result> resultTask) — non-generic block
    // ═══════════════════════════════════════════════════════════════════════════

    // ── SuccessInfo ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_SuccessInfo_OnSuccess_AppendsInfo()
    {
        var result = await Task.FromResult(Results.Success()).WithSuccessInfo(Fixtures.AnyInfo);
        result.IsSuccess().Should().BeTrue();
        result.GetInfos().Should().ContainSingle().Which.Should().Be(Fixtures.AnyInfo);
    }

    [Fact]
    public async Task Result_SuccessInfo_OnFailure_Unchanged()
    {
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError)).WithSuccessInfo(Fixtures.AnyInfo);
        result.IsFailure().Should().BeTrue();
        result.GetInfos().Should().BeEmpty();
    }

    // ── Map<T> ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_Map_OnSuccess_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Results.Success()).Map(() => 42);
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task Result_Map_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError)).Map(() => { called = true; return 42; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── MapAsync<T> ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_MapAsync_OnSuccess_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Results.Success()).MapAsync(() => Async.Slow(42));
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task Result_MapAsync_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .MapAsync(() => { called = true; return Async.Slow(42); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Bind(Func<Result>) ────────────────────────────────────────────────────

    [Fact]
    public async Task Result_Bind_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success()).Bind(() => Results.Success(Fixtures.AnyInfo));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_Bind_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .Bind(() => { called = true; return Results.Success(); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── BindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_BindAsync_OnSuccess_ReturnsFuncResult()
    {
        var result = await Task.FromResult(Results.Success())
            .BindAsync(() => Task.FromResult(Results.Success()));
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_BindAsync_OnFailure_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .BindAsync(() => { called = true; return Task.FromResult(Results.Success()); });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    // ── Tap / TapError ────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_Tap_OnSuccess_ActionCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success()).Tap(() => called = true);
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_Tap_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError)).Tap(() => called = true);
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapError_OnFailure_ActionCalled()
    {
        Error[]? captured = null;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError)).TapError(e => captured = e);
        captured.Should().NotBeNull();
        captured!.Should().Contain(Fixtures.AnyError);
    }

    [Fact]
    public async Task Result_TapError_OnSuccess_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success()).TapError(_ => called = true);
        called.Should().BeFalse();
        result.IsSuccess().Should().BeTrue();
    }

    // ── Result.TapAsync (Func<Task>) ──────────────────────────────────────────

    [Fact]
    public async Task Result_TapAsync_FastOuter_FastInner_ActionCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success()).TapAsync(() => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_SlowOuter_ActionCalled()
    {
        var called = false;
        var result = await Async.Slow(Results.Success()).TapAsync(() => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_FastOuter_SlowInner_ActionCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success()).TapAsync(async () => { await Task.Yield(); called = true; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .TapAsync(() => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public void Result_TapAsync_FastOuterFastInner_DoesNotAwait()
    {
        var task = Task.FromResult(Results.Success()).TapAsync(() => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── Result.TapAsync (Func<CancellationToken, Task>) ───────────────────────

    [Fact]
    public async Task Result_TapAsync_CT_FastOuter_ActionCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Success()).TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_SlowOuter_ActionCalled()
    {
        var called = false;
        var result = await Async.Slow(Results.Success()).TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeTrue();
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_OnFailure_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task Result_TapAsync_CT_TokenForwardedToAction()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Results.Success())
            .TapAsync(ct => { capturedToken = ct; return Task.CompletedTask; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Result_Match_OnSuccess_OnSuccessFuncCalled()
    {
        var result = await Task.FromResult(Results.Success()).Match(() => "ok", _ => "fail");
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Result_Match_OnFailure_OnFailureFuncCalled()
    {
        var result = await Task.FromResult(Results.Failure(Fixtures.AnyError))
            .Match(() => "ok", errors => $"fail:{errors.Length}");
        result.Should().Be("fail:1");
    }
}
