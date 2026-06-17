namespace MWR.Monads.Tests.MaybeMonad;

public class MaybeExtensionsTests
{
    // ── ToResult ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToResult_OnSome_ReturnsSuccess()
    {
        var result = Maybe<string>.Some("hello").ToResult();
        result.IsSuccess().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public void ToResult_OnNone_ReturnsFailure()
    {
        var result = Maybe<string>.None.ToResult();
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void ToResult_WithError_OnSome_ReturnsSuccess()
    {
        var result = Maybe<string>.Some("hello").ToResult(Fixtures.AnyError);
        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public void ToResult_WithError_OnNone_ReturnsFailureWithError()
    {
        var result = Maybe<string>.None.ToResult(Fixtures.AnyError);
        result.IsFailure().Should().BeTrue();
        result.GetErrors().Should().Contain(Fixtures.AnyError);
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSome_ReturnsMappedSome()
    {
        var called = false;
        var result = Maybe<string>.Some("hello").Map(v => { called = true; return v.Length; });
        called.Should().BeTrue();
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public void Map_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = Maybe<string>.None.Map(_ => { called = true; return 0; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── MapAsync (Func<T, Task<TOut>>) ────────────────────────────────────────

    [Fact]
    public async Task MapAsync_OnSome_FastPath_ReturnsMappedValue()
    {
        var result = await Maybe<string>.Some("hello").MapAsync(_ => Task.FromResult(42));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_OnSome_SlowPath_ReturnsMappedValue()
    {
        var result = await Maybe<string>.Some("hello").MapAsync(v => Async.Slow(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task MapAsync_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = await Maybe<string>.None.MapAsync(_ => { called = true; return Task.FromResult(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── MapAsync (Func<T, CancellationToken, Task<TOut>>) ────────────────────

    [Fact]
    public async Task MapAsync_CT_OnSome_FastPath_ReturnsMappedValue()
    {
        var result = await Maybe<string>.Some("hello").MapAsync((_, _) => Task.FromResult(42));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_CT_OnSome_SlowPath_ReturnsMappedValue()
    {
        var result = await Maybe<string>.Some("hello").MapAsync((v, _) => Async.Slow(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task MapAsync_CT_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = await Maybe<string>.None.MapAsync((_, _) => { called = true; return Task.FromResult(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── MapAsync null-return consistency (Bug 1 regression) ───────────────────

    [Fact]
    public async Task MapAsync_FastPath_NullReturn_Throws()
    {
        // Fast path (Task.FromResult) must behave the same as slow path for null
        var act = async () => await Maybe<string>.Some("hello")
            .MapAsync(_ => Task.FromResult<string>(null!));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MapAsync_SlowPath_NullReturn_Throws()
    {
        var act = async () => await Maybe<string>.Some("hello")
            .MapAsync(async _ => { await Task.Yield(); return (string)null!; });
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MapAsync_CT_FastPath_NullReturn_Throws()
    {
        var act = async () => await Maybe<string>.Some("hello")
            .MapAsync((_, _) => Task.FromResult<string>(null!));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── TapAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TapAsync_OnSome_FastPath_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var maybe = Maybe<string>.Some("hello");
        var result = await maybe.TapAsync(v => { captured = v; return Task.CompletedTask; });
        captured.Should().Be("hello");
        result.Should().Be(maybe);
    }

    [Fact]
    public async Task TapAsync_OnSome_SlowPath_ActionCalled_ReturnsSameMaybe()
    {
        var captured = string.Empty;
        var maybe = Maybe<string>.Some("hello");
        var result = await maybe.TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be("hello");
        result.Should().Be(maybe);
    }

    [Fact]
    public async Task TapAsync_OnNone_ActionNotCalled_ReturnsNone()
    {
        var called = false;
        var result = await Maybe<string>.None.TapAsync(_ => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void TapAsync_OnSome_FastPath_DoesNotAwait()
    {
        var task = Maybe<string>.Some("hello").TapAsync(_ => Task.CompletedTask);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_CT_OnSome_FastPath_ActionCalledWithToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Maybe<string>.Some("hello").TapAsync((_, ct) => { capturedToken = ct; return Task.CompletedTask; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task TapAsync_CT_OnSome_SlowPath_ActionCalledWithToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        await Maybe<string>.Some("hello").TapAsync(async (_, ct) => { await Task.Yield(); capturedToken = ct; }, cts.Token);
        capturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task TapAsync_CT_OnNone_ActionNotCalled()
    {
        var called = false;
        var result = await Maybe<string>.None.TapAsync((_, _) => { called = true; return Task.CompletedTask; });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── Bind ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSome_FuncReturnsSome_ReturnsSome()
    {
        var result = Maybe<string>.Some("hello").Bind(v => Maybe<int>.Some(v.Length));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public void Bind_OnSome_FuncReturnsNone_ReturnsNone()
    {
        var result = Maybe<string>.Some("hello").Bind(_ => Maybe<int>.None);
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void Bind_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = Maybe<string>.None.Bind(_ => { called = true; return Maybe<int>.Some(42); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── BindAsync (Func<T, Task<Maybe<TOut>>>) ────────────────────────────────

    [Fact]
    public async Task BindAsync_OnSome_FuncReturnsSome_ReturnsSome()
    {
        var result = await Maybe<string>.Some("hello").BindAsync(v => Task.FromResult(Maybe<int>.Some(v.Length)));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task BindAsync_OnNone_FuncNeverCalled_ReturnsNone()
    {
        var called = false;
        var result = await Maybe<string>.None.BindAsync(_ => { called = true; return Task.FromResult(Maybe<int>.Some(42)); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── BindAsync (Func<T, CancellationToken, Task<Maybe<TOut>>>) ─────────────

    [Fact]
    public async Task BindAsync_CT_OnSome_ReturnsSome()
    {
        var result = await Maybe<string>.Some("hello").BindAsync((v, _) => Task.FromResult(Maybe<int>.Some(v.Length)));
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be(5);
    }

    [Fact]
    public async Task BindAsync_CT_OnNone_FuncNeverCalled()
    {
        var called = false;
        var result = await Maybe<string>.None.BindAsync((_, _) => { called = true; return Task.FromResult(Maybe<int>.Some(42)); });
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── Filter ────────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_OnSome_PredicateTrue_ReturnsSome()
    {
        var result = Maybe<string>.Some("hello").Filter(v => v.Length > 3);
        result.HasValue().Should().BeTrue();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public void Filter_OnSome_PredicateFalse_ReturnsNone()
    {
        var result = Maybe<string>.Some("hi").Filter(v => v.Length > 3);
        result.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void Filter_OnNone_ReturnsNone()
    {
        var result = Maybe<string>.None.Filter(_ => true);
        result.HasNoValue().Should().BeTrue();
    }

    // ── Tap ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Tap_OnSome_ActionCalled_ReturnsSomeMaybe()
    {
        var captured = string.Empty;
        var maybe = Maybe<string>.Some("hello");
        var result = maybe.Tap(v => captured = v);
        captured.Should().Be("hello");
        result.Should().Be(maybe);
    }

    [Fact]
    public void Tap_OnNone_ActionNotCalled_ReturnsNone()
    {
        var called = false;
        var result = Maybe<string>.None.Tap(_ => called = true);
        called.Should().BeFalse();
        result.HasNoValue().Should().BeTrue();
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSome_SomeFuncCalled()
    {
        var result = Maybe<string>.Some("hello").Match(v => v.ToUpper(), () => "none");
        result.Should().Be("HELLO");
    }

    [Fact]
    public void Match_OnNone_NoneFuncCalled()
    {
        var result = Maybe<string>.None.Match(v => v.ToUpper(), () => "none");
        result.Should().Be("none");
    }

    // ── Or(Maybe<T>) ─────────────────────────────────────────────────────────

    [Fact]
    public void Or_Maybe_OnSome_ReturnsSelf()
    {
        var maybe = Maybe<string>.Some("hello");
        var result = maybe.Or(Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public void Or_Maybe_OnNone_ReturnsFallback()
    {
        var result = Maybe<string>.None.Or(Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("fallback");
    }

    // ── Or(Func<Maybe<T>>) ────────────────────────────────────────────────────

    [Fact]
    public void Or_Func_OnSome_FuncNeverCalled_ReturnsSelf()
    {
        var called = false;
        var maybe = Maybe<string>.Some("hello");
        var result = maybe.Or(() => { called = true; return Maybe<string>.Some("fallback"); });
        called.Should().BeFalse();
        result.GetValue().Should().Be("hello");
    }

    [Fact]
    public void Or_Func_OnNone_FuncCalled_ReturnsFallback()
    {
        var result = Maybe<string>.None.Or(() => Maybe<string>.Some("fallback"));
        result.GetValue().Should().Be("fallback");
    }
}
