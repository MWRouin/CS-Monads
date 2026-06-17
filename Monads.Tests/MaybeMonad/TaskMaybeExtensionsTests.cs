using MWR.Monads.MaybeMonad;

namespace MWR.Monads.Tests.MaybeMonad;

public class TaskMaybeExtensionsTests
{
    // ── ToResult ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToResult_FastPath_OnSome_ReturnsSuccess()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello")).ToResult();
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task ToResult_SlowPath_OnSome_ReturnsSuccess()
    {
        var result = await Async.Slow(Maybe<string>.Some("hello")).ToResult();
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task ToResult_FastPath_OnNone_ReturnsFailure()
    {
        var result = await Task.FromResult(Maybe<string>.None).ToResult();
        result.IsFailure().Should().BeTrue();
    }

    [Fact]
    public async Task ToResult_WithError_OnNone_ReturnsFailureWithError()
    {
        var result = await Task.FromResult(Maybe<string>.None).ToResult(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Map_FastPath_OnSome_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello")).Map(v => v.Length);
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task Map_SlowPath_OnSome_ReturnsMappedValue()
    {
        var result = await Async.Slow(Maybe<string>.Some("hello")).Map(v => v.Length);
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task Map_FastPath_OnNone_ReturnsNone()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None).Map(_ => { called = true; return 0; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void Map_FastPath_DoesNotAwait()
    {
        var maybeTask = Task.FromResult(Maybe<string>.Some("hello"));
        var resultTask = maybeTask.Map(v => v.Length);
        resultTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── MapAsync (Func<T, Task<TOut>>) ────────────────────────────────────────

    [Fact]
    public async Task MapAsync_FastOuterFastInner_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .MapAsync(_ => Task.FromResult(42));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_SlowOuterFastInner_ReturnsMappedValue()
    {
        var result = await Async.Slow(Maybe<string>.Some("hello"))
            .MapAsync(_ => Task.FromResult(42));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_FastOuterSlowInner_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .MapAsync(v => Async.Slow(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task MapAsync_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .MapAsync(_ => { called = true; return Task.FromResult(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── MapAsync (Func<T, CancellationToken, Task<TOut>>) ────────────────────

    [Fact]
    public async Task MapAsync_CT_FastOuter_ReturnsMappedValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .MapAsync((_, _) => Task.FromResult(42));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_CT_SlowOuter_ReturnsMappedValue()
    {
        var result = await Async.Slow(Maybe<string>.Some("hello"))
            .MapAsync((v, _) => Async.Slow(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task MapAsync_CT_OnNone_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .MapAsync((_, _) => { called = true; return Task.FromResult(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── Bind ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Bind_OnSome_ReturnsMappedSome()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .Bind(v => Maybe<int>.Some(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task Bind_OnNone_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .Bind(_ => { called = true; return Maybe<int>.Some(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── BindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BindAsync_OnSome_ReturnsBoundValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .BindAsync(v => Task.FromResult(Maybe<int>.Some(v.Length)));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task BindAsync_OnNone_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .BindAsync(_ => { called = true; return Task.FromResult(Maybe<int>.Some(42)); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_CT_OnSome_ReturnsBoundValue()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .BindAsync((v, _) => Task.FromResult(Maybe<int>.Some(v.Length)));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    // ── Filter ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Filter_OnSome_PredicateTrue_ReturnsSome()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello")).Filter(v => v.Length > 3);
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task Filter_OnSome_PredicateFalse_ReturnsNone()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hi")).Filter(v => v.Length > 3);
        result.HasNoValue().Should().BeTrue();
    }

    // ── Tap ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Tap_OnSome_ActionCalled()
    {
        var captured = string.Empty;
        var result = await Task.FromResult(Maybe<string>.Some("hello")).Tap(v => captured = v);
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task Tap_OnNone_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None).Tap(_ => called = true);
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── TapAsync (Func<T, Task>) ──────────────────────────────────────────────

    [Fact]
    public async Task TapAsync_FastOuterFastInner_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task TapAsync_SlowOuter_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var result = await Async.Slow(Maybe<string>.Some("hello"))
            .TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_FastOuterSlowInner_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_OnNone_ActionNotCalled_ReturnsNone()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void TapAsync_FastOuterFastInner_DoesNotAwait()
    {
        var task = Task.FromResult(Maybe<string>.Some("hello")).TapAsync(_ => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ── TapAsync (Func<T, CancellationToken, Task>) ───────────────────────────

    [Fact]
    public async Task TapAsync_CT_FastOuter_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .TapAsync((v, _) => { captured = v; return Task.CompletedTask; });
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_SlowOuter_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var result = await Async.Slow(Maybe<string>.Some("hello"))
            .TapAsync((v, _) => { captured = v; return Task.CompletedTask; });
        captured.Should().Be("hello");
        result.HasValue().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_OnNone_ActionNotCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.None)
            .TapAsync((_, _) => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_TokenForwardedToAction()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Maybe<string>.Some("hello"))
            .TapAsync((_, ct) => { capturedToken = ct; return Task.CompletedTask; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Match_OnSome_SomeFuncCalled()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .Match(v => v.ToUpper(), () => "none");
        result.Should().Be("HELLO");
    }

    [Fact]
    public async Task Match_OnNone_NoneFuncCalled()
    {
        var result = await Task.FromResult(Maybe<string>.None)
            .Match(v => v.ToUpper(), () => "none");
        result.Should().Be("none");
    }

    // ── MatchAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_OnSome_SomeFuncCalled()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .MatchAsync(
                (v, _) => Task.FromResult(v.ToUpper()),
                _ => Task.FromResult("none"));
        result.Should().Be("HELLO");
    }

    [Fact]
    public async Task MatchAsync_OnNone_NoneFuncCalled()
    {
        var result = await Task.FromResult(Maybe<string>.None)
            .MatchAsync(
                (v, _) => Task.FromResult(v.ToUpper()),
                _ => Task.FromResult("none"));
        result.Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_OnSome_CancellationTokenForwarded()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Task.FromResult(Maybe<string>.Some("hello"))
            .MatchAsync(
                (_, ct) => { capturedToken = ct; return Task.FromResult("ok"); },
                _ => Task.FromResult("none"),
                cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    // ── MapAsync null-return consistency (Bug 1 regression) ──────────────────

    [Fact]
    public async Task MapAsync_FastPath_NullReturn_Throws()
    {
        var act = async () => await Task.FromResult(Maybe<string>.Some("hello"))
            .MapAsync(_ => Task.FromResult<string>(null!));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── Or ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Or_Maybe_OnSome_ReturnsSelf()
    {
        var result = await Task.FromResult(Maybe<string>.Some("hello")).Or(Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task Or_Maybe_OnNone_ReturnsFallback()
    {
        var result = await Task.FromResult(Maybe<string>.None).Or(Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("fallback");
    }

    [Fact]
    public async Task Or_Func_OnSome_FuncNeverCalled()
    {
        var called = false;
        var result = await Task.FromResult(Maybe<string>.Some("hello"))
            .Or(() => { called = true; return Maybe<string>.Some("fallback"); });
        called.Should().BeFalse();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public async Task Or_Func_OnNone_ReturnsFallback()
    {
        var result = await Task.FromResult(Maybe<string>.None)
            .Or(() => Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("fallback");
    }
}
