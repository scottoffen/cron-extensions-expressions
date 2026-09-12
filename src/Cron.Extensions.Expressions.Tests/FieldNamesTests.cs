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
        // The digit "7" itself has no name - passing it through Translate leaves it untouched,
        // since translation only ever acts on recognized name tokens like "SUN", never on an
        // already-numeric value. (SUN itself can become "7" - see the range-end tests below -
        // but that's SUN being translated, not "7" being translated.)
        FieldNames.Translate("7", Units.DayOfWeek).ShouldBe("7");
    }

    #region Translate - DayOfWeek - SUN at the end of a range

    [Fact]
    public void Translate_DayOfWeek_SunAtEndOfRange_BecomesSeven()
    {
        // A range's end can never validly be 0 (nothing is less than 0), so "X-SUN" would
        // always have been rejected as reversed before this rule existed. Translating SUN to 7
        // there instead makes it a valid forward range, and can never change a range that
        // already worked, since "X-0" was never valid to begin with.
        FieldNames.Translate("MON-SUN", Units.DayOfWeek).ShouldBe("1-7");
        FieldNames.Translate("SAT-SUN", Units.DayOfWeek).ShouldBe("6-7");
        FieldNames.Translate("FRI-SUN", Units.DayOfWeek).ShouldBe("5-7");
    }

    [Fact]
    public void Translate_DayOfWeek_SunAtBothEndsOfRange_StartStaysZero()
    {
        // Only the end position is context-sensitive - SUN at the start of "SUN-SUN" still
        // means 0, giving "0-7", the same full-week span the numeric form already expresses.
        FieldNames.Translate("SUN-SUN", Units.DayOfWeek).ShouldBe("0-7");
    }

    [Fact]
    public void Translate_DayOfWeek_SunAtStartOfRange_StillBecomesZero()
    {
        // SUN at the *start* of a range is unaffected by the end-of-range rule - it still means
        // the field's own minimum, exactly as a bare SUN does.
        FieldNames.Translate("SUN-FRI", Units.DayOfWeek).ShouldBe("0-5");
        FieldNames.Translate("SUN-MON", Units.DayOfWeek).ShouldBe("0-1");
    }

    [Fact]
    public void Translate_DayOfWeek_SunInList_IsNotARangeEnd_StaysZero()
    {
        // The end-of-range rule only applies within a single range segment - SUN appearing
        // after a comma is a separate list element, not the end of a range, so it still means 0.
        FieldNames.Translate("MON,SUN", Units.DayOfWeek).ShouldBe("1,0");
    }

    [Fact]
    public void Translate_DayOfWeek_ExplicitNumericZeroAtEndOfRange_IsNotReinterpreted()
    {
        // Only the name "SUN" is context-sensitive - an explicit digit "0" is never
        // reinterpreted as 7, so "MON-0" is still a genuinely reversed range.
        FieldNames.Translate("MON-0", Units.DayOfWeek).ShouldBe("1-0");
    }

    [Fact]
    public void Translate_DayOfWeek_SunAtEndOfRange_IsCaseInsensitive()
    {
        FieldNames.Translate("mon-sun", Units.DayOfWeek).ShouldBe("1-7");
        FieldNames.Translate("MON-Sun", Units.DayOfWeek).ShouldBe("1-7");
    }

    [Fact]
    public void Translate_Month_HasNoEndOfRangeSpecialCase()
    {
        // The SUN-at-end-of-range rule is specific to DayOfWeek - Month has no equivalent
        // wraparound alias, so its ranges translate the same way regardless of position.
        FieldNames.Translate("JAN-DEC", Units.Month).ShouldBe("1-12");
    }

    #endregion

    #endregion
}
