using MWR.Monads.Messages;

namespace MWR.Monads.Tests.Messages;

public class MessageTests
{
    [Fact]
    public void Code_Error_IsPrefixedWithError()
    {
        var error = new BadRequestError("SomeCode", "some message");
        error.Code.Should().StartWith("Error.");
    }

    [Fact]
    public void Code_Information_IsPrefixedWithInfo()
    {
        var info = new Information("SomeCode", "some message");
        info.Code.Should().StartWith("Info.");
    }

    [Fact]
    public void Code_Warning_IsPrefixedWithWarn()
    {
        var warning = new Warning("SomeCode", "some message");
        warning.Code.Should().StartWith("Warn.");
    }

    [Fact]
    public void Equals_SameCode_IsTrue()
    {
        var e1 = new BadRequestError("Test.Error", "message 1");
        var e2 = new BadRequestError("Test.Error", "message 2");
        e1.Should().Be(e2);
    }

    [Fact]
    public void Equals_DifferentCode_IsFalse()
    {
        var e1 = new BadRequestError("Test.Error1", "message");
        var e2 = new BadRequestError("Test.Error2", "message");
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void GetHashCode_ConsistentWithEquals()
    {
        var e1 = new BadRequestError("Test.Error", "message 1");
        var e2 = new BadRequestError("Test.Error", "message 2");
        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void ImplicitToString_ReturnsContent()
    {
        string s = Fixtures.AnyError;
        s.Should().Be(Fixtures.AnyError.Content);
    }

    [Fact]
    public void ToString_FormatC_ContainsCodeOnly()
    {
        var formatted = Fixtures.AnyError.ToString("C");
        formatted.Should().Contain("Code:");
        formatted.Should().NotContain("Content:");
    }

    [Fact]
    public void ToString_FormatM_ContainsContentOnly()
    {
        var formatted = Fixtures.AnyError.ToString("M");
        formatted.Should().Contain("Content:");
        formatted.Should().NotContain("Code:");
    }

    [Fact]
    public void ToString_FormatCM_ContainsBoth()
    {
        var formatted = Fixtures.AnyError.ToString("CM");
        formatted.Should().Contain("Code:");
        formatted.Should().Contain("Content:");
    }

    [Fact]
    public void ToString_FormatT_WithTarget_ContainsTarget()
    {
        var error = new BadRequestError("SomeCode", "message", "fieldName");
        var formatted = error.ToString("T");
        formatted.Should().Contain("For Target:");
    }

    [Fact]
    public void ToString_FormatT_WithoutTarget_Empty()
    {
        var formatted = Fixtures.AnyError.ToString("T");
        formatted.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_NullCode_Throws()
    {
        var act = () => new BadRequestError(null!, "message");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhitespaceCode_Throws()
    {
        var act = () => new BadRequestError("   ", "message");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptyMessage_Throws()
    {
        var act = () => new BadRequestError("TestCode", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OperatorEqual_SameCode_ReturnsTrue()
    {
        var e1 = new BadRequestError("Test.Error", "a");
        var e2 = new BadRequestError("Test.Error", "b");
        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void OperatorNotEqual_DifferentCode_ReturnsTrue()
    {
        var e1 = new BadRequestError("Test.Error1", "a");
        var e2 = new BadRequestError("Test.Error2", "a");
        (e1 != e2).Should().BeTrue();
    }
}
