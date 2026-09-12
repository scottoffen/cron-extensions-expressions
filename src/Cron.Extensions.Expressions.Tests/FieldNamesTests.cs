namespace Cron.Extensions.Expressions.Tests;

public class FieldNamesTests
{
    #region Translate - units with no names

    [Fact]
    public void Translate_UnitWithNoNames_ReturnsValueUnchanged()
    {
        // Minute, Hour, and Day have no name syntax in any supported cron dialect - only Month
        // and DayOfWeek do. Translate must be a pure no-op for the other three, not just
        // "usually" a no-op. Units is internal, so this stays a single Fact with the values
        // checked in the body rather than a Theory with Units as a parameter - a public Theory
        // method cannot take an internal type as a parameter (CS0051).
        FieldNames.Translate("30", Units.Minute).ShouldBe("30");
        FieldNames.Translate("12", Units.Hour).ShouldBe("12");
        FieldNames.Translate("15", Units.Day).ShouldBe("15");
    }

    #endregion

    #region Translate - Month

    [Theory]
    [InlineData("JAN", "1")]
    [InlineData("FEB", "2")]
    [InlineData("MAR", "3")]
    [InlineData("APR", "4")]
    [InlineData("MAY", "5")]
    [InlineData("JUN", "6")]
    [InlineData("JUL", "7")]
    [InlineData("AUG", "8")]
    [InlineData("SEP", "9")]
    [InlineData("OCT", "10")]
    [InlineData("NOV", "11")]
    [InlineData("DEC", "12")]
    public void Translate_Month_SingleName_ReturnsNumericEquivalent(string name, string expected)
    {
        FieldNames.Translate(name, Units.Month).ShouldBe(expected);
    }

    [Theory]
    [InlineData("jan")]
    [InlineData("Jan")]
    [InlineData("jAn")]
    public void Translate_Month_SingleName_IsCaseInsensitive(string name)
    {
        FieldNames.Translate(name, Units.Month).ShouldBe("1");
    }

    [Fact]
    public void Translate_Month_Wildcard_ReturnsUnchanged()
    {
        FieldNames.Translate("*", Units.Month).ShouldBe("*");
    }

    [Fact]
    public void Translate_Month_NumericValue_ReturnsUnchanged()
    {
        // Already-numeric input must round-trip untouched - translation only ever replaces a
        // recognized name, never rewrites a value that was never a name to begin with.
        FieldNames.Translate("6", Units.Month).ShouldBe("6");
        FieldNames.Translate("1-12", Units.Month).ShouldBe("1-12");
    }

    [Fact]
    public void Translate_Month_List_TranslatesEachElement()
    {
        FieldNames.Translate("JAN,APR,JUL,OCT", Units.Month).ShouldBe("1,4,7,10");
    }

    [Fact]
    public void Translate_Month_Range_TranslatesBothSides()
    {
        FieldNames.Translate("JAN-JUN", Units.Month).ShouldBe("1-6");
    }

    [Fact]
    public void Translate_Month_RangeWithOneNamedSide_TranslatesOnlyThatSide()
    {
        FieldNames.Translate("JAN-6", Units.Month).ShouldBe("1-6");
        FieldNames.Translate("1-JUN", Units.Month).ShouldBe("1-6");
    }

    [Fact]
    public void Translate_Month_RangeWithStep_TranslatesRangeButNotInterval()
    {
        // The interval side of a step is always numeric - this composes with the range-with-step
        // support added separately, translating only the range portion before that logic runs.
        FieldNames.Translate("JAN-JUN/2", Units.Month).ShouldBe("1-6/2");
    }

    [Fact]
    public void Translate_Month_WildcardStep_LeavesWildcardAlone()
    {
        FieldNames.Translate("*/3", Units.Month).ShouldBe("*/3");
    }

    [Fact]
    public void Translate_Month_StepIntervalPosition_IsNeverTranslated()
    {
        // A name has no meaning as a step interval in any dialect that supports names - "JAN/JUN"
        // must come out with JUN untouched, left for FieldValidator to reject as malformed on its
        // own terms, the same way any other non-numeric interval is rejected.
        FieldNames.Translate("JAN/JUN", Units.Month).ShouldBe("1/JUN");
    }

    [Fact]
    public void Translate_Month_UnrecognizedToken_LeftUnchanged()
    {
        // Not every three-letter uppercase word is a month name - an unrecognized token must
        // pass through untouched so FieldValidator reports it as the malformed token it is.
        FieldNames.Translate("FOO", Units.Month).ShouldBe("FOO");
    }

    #endregion

    #region Translate - DayOfWeek

    [Theory]
    [InlineData("SUN", "0")]
    [InlineData("MON", "1")]
    [InlineData("TUE", "2")]
    [InlineData("WED", "3")]
    [InlineData("THU", "4")]
    [InlineData("FRI", "5")]
    [InlineData("SAT", "6")]
    public void Translate_DayOfWeek_SingleName_ReturnsNumericEquivalent(string name, string expected)
    {
        FieldNames.Translate(name, Units.DayOfWeek).ShouldBe(expected);
    }

    [Fact]
    public void Translate_DayOfWeek_SingleName_IsCaseInsensitive()
    {
        FieldNames.Translate("mon", Units.DayOfWeek).ShouldBe("1");
        FieldNames.Translate("Mon", Units.DayOfWeek).ShouldBe("1");
    }

    [Fact]
    public void Translate_DayOfWeek_Range_TranslatesBothSides()
    {
        FieldNames.Translate("MON-FRI", Units.DayOfWeek).ShouldBe("1-5");
    }

    [Fact]
    public void Translate_DayOfWeek_List_TranslatesEachElement()
    {
        FieldNames.Translate("MON,WED,FRI", Units.DayOfWeek).ShouldBe("1,3,5");
    }

    [Fact]
    public void Translate_DayOfWeek_HasNoNameForSeven()
    {
        // Only "SUN" (0) has a name - the numeric alias "7" for Sunday has no three-letter
        // equivalent in this dialect, so it is untouched by translation either way.
        FieldNames.Translate("7", Units.DayOfWeek).ShouldBe("7");
    }

    #endregion
}
