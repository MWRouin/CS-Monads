namespace MWR.Monads.Tests.MaybeMonad;

public class MaybeTests
{
    [Fact]
    public void None_HasNoValue()
    {
        var maybe = Maybe<string>.None;
        maybe.HasValue().Should().BeFalse();
        maybe.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void Some_WithValue_HasValue()
    {
        var maybe = Maybe<string>.Some("hello");
        maybe.HasValue().Should().BeTrue();
        maybe.HasNoValue().Should().BeFalse();
    }

    [Fact]
    public void Some_NullValue_Throws()
    {
        var act = () => Maybe<string>.Some(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void From_NonNull_ReturnsSome()
    {
        var maybe = Maybe<string>.From("hello");
        maybe.HasValue().Should().BeTrue();
        maybe.GetValue().Should().Be("hello");
    }

    [Fact]
    public void From_Null_ReturnsNone()
    {
        var maybe = Maybe<string>.From(null);
        maybe.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_NonNull_ReturnsSome()
    {
        Maybe<string> m = "hello";
        m.HasValue().Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_Null_ReturnsNone()
    {
        Maybe<string> m = (string?)null;
        m.HasNoValue().Should().BeTrue();
    }

    [Fact]
    public void ExplicitCast_OnSome_ReturnsValue()
    {
        var maybe = Maybe<string>.Some("hello");
        var value = (string)maybe;
        value.Should().Be("hello");
    }

    [Fact]
    public void ExplicitCast_OnNone_Throws()
    {
        var maybe = Maybe<string>.None;
        var act = () => (string)maybe;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetValue_OnNone_Throws()
    {
        var maybe = Maybe<string>.None;
        var act = () => maybe.GetValue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetValueOr_OnSome_ReturnsValue()
    {
        var maybe = Maybe<string>.Some("hello");
        maybe.GetValueOr("default").Should().Be("hello");
    }

    [Fact]
    public void GetValueOr_OnNone_ReturnsDefault()
    {
        var maybe = Maybe<string>.None;
        maybe.GetValueOr("default").Should().Be("default");
    }

    [Fact]
    public void GetValueOrDefault_OnSome_ReturnsValue()
    {
        var maybe = Maybe<string>.Some("hello");
        maybe.GetValueOrDefault().Should().Be("hello");
    }

    [Fact]
    public void GetValueOrDefault_OnNone_ReturnsNull()
    {
        var maybe = Maybe<string>.None;
        maybe.GetValueOrDefault().Should().BeNull();
    }

    [Fact]
    public void Equals_TwoNones_AreEqual()
    {
        var n1 = Maybe<string>.None;
        var n2 = Maybe<string>.None;
        n1.Should().Be(n2);
    }

    [Fact]
    public void Equals_SomeSameValue_AreEqual()
    {
        var m1 = Maybe<string>.Some("hello");
        var m2 = Maybe<string>.Some("hello");
        m1.Should().Be(m2);
    }

    [Fact]
    public void Equals_SomeDifferentValues_NotEqual()
    {
        var m1 = Maybe<string>.Some("hello");
        var m2 = Maybe<string>.Some("world");
        m1.Should().NotBe(m2);
    }

    [Fact]
    public void Equals_SomeVsNone_NotEqual()
    {
        var some = Maybe<string>.Some("hello");
        var none = Maybe<string>.None;
        some.Should().NotBe(none);
    }

    [Fact]
    public void GetHashCode_MatchesEquals()
    {
        var m1 = Maybe<string>.Some("hello");
        var m2 = Maybe<string>.Some("hello");
        m1.GetHashCode().Should().Be(m2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_None_IsZero()
    {
        var none = Maybe<string>.None;
        none.GetHashCode().Should().Be(0);
    }

    // ── Operators == / != ─────────────────────────────────────────────────────

    [Fact]
    public void OperatorEqual_TwoNones_IsTrue()
    {
        var a = Maybe<string>.None;
        var b = Maybe<string>.None;
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void OperatorEqual_TwoSomeSameValue_IsTrue()
    {
        (Maybe<string>.Some("hello") == Maybe<string>.Some("hello")).Should().BeTrue();
    }

    [Fact]
    public void OperatorEqual_SomeVsNone_IsFalse()
    {
        (Maybe<string>.Some("hello") == Maybe<string>.None).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEqual_DifferentValues_IsTrue()
    {
        (Maybe<string>.Some("hello") != Maybe<string>.Some("world")).Should().BeTrue();
    }

    [Fact]
    public void OperatorNotEqual_SomeVsNone_IsTrue()
    {
        (Maybe<string>.Some("hello") != Maybe<string>.None).Should().BeTrue();
    }

    [Fact]
    public void Default_Maybe_IsNone()
    {
        var d = default(Maybe<string>);
        d.HasNoValue().Should().BeTrue();
        (d == Maybe<string>.None).Should().BeTrue();
    }
}
