namespace Cron.Extensions.Expressions.Tests;

public class CronMacrosTests
{
    #region Expand - recognized macros

    [Theory]
    [InlineData("@yearly", "0 0 1 1 *")]
    [InlineData("@annually", "0 0 1 1 *")]
    [InlineData("@monthly", "0 0 1 * *")]
    [InlineData("@weekly", "0 0 * * 0")]
    [InlineData("@daily", "0 0 * * *")]
    [InlineData("@midnight", "0 0 * * *")]
    [InlineData("@hourly", "0 * * * *")]
    public void Expand_RecognizedMacro_ReturnsFiveFieldEquivalent(string macro, string expected)
    {
        CronMacros.Expand(macro).ShouldBe(expected);
    }

    [Theory]
    [InlineData("@DAILY")]
    [InlineData("@Daily")]
    [InlineData("@dAiLy")]
    public void Expand_RecognizedMacro_IsCaseInsensitive(string macro)
    {
        CronMacros.Expand(macro).ShouldBe("0 0 * * *");
    }

    [Fact]
    public void Expand_RecognizedMacro_TolerantOfSurroundingWhitespace()
    {
        CronMacros.Expand("  @daily  ").ShouldBe("0 0 * * *");
    }

    #endregion

    #region Expand - values that are not expanded

    [Fact]
    public void Expand_Reboot_ReturnsUnchanged()
    {
        // @reboot is deliberately not a recognized macro - it has no five-field schedule to
        // expand into, so it must fall through exactly like any other unrecognized token.
        CronMacros.Expand("@reboot").ShouldBe("@reboot");
    }

    [Fact]
    public void Expand_UnrecognizedMacro_ReturnsUnchanged()
    {
        CronMacros.Expand("@fortnightly").ShouldBe("@fortnightly");
    }

    [Fact]
    public void Expand_MacroWithTrailingExtraTokens_ReturnsUnchanged()
    {
        // A macro must be the entire (trimmed) value on its own - "@daily" followed by anything
        // else is not a recognized macro invocation.
        CronMacros.Expand("@daily extra").ShouldBe("@daily extra");
    }

    [Fact]
    public void Expand_OrdinaryFiveFieldExpression_ReturnsUnchanged()
    {
        CronMacros.Expand("30 6 * * 1-5").ShouldBe("30 6 * * 1-5");
    }

    [Fact]
    public void Expand_EmptyString_ReturnsUnchanged()
    {
        CronMacros.Expand("").ShouldBe("");
    }

    #endregion
}
