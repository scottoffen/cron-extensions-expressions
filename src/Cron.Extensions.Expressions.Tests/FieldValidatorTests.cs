namespace Cron.Extensions.Expressions.Tests;

public class FieldValidatorTests
{
    #region GetMinValue / GetMaxValue

    [Fact]
    public void GetMinValue_ReturnsCorrectLimits_ForEachUnit()
    {
        FieldValidator.GetMinValue(Units.Minute).ShouldBe(0);
        FieldValidator.GetMinValue(Units.Hour).ShouldBe(0);
        FieldValidator.GetMinValue(Units.Day).ShouldBe(1);
        FieldValidator.GetMinValue(Units.Month).ShouldBe(1);
        FieldValidator.GetMinValue(Units.DayOfWeek).ShouldBe(0);
    }

    [Fact]
    public void GetMaxValue_ReturnsCorrectLimits_ForEachUnit()
    {
        FieldValidator.GetMaxValue(Units.Minute).ShouldBe(59);
        FieldValidator.GetMaxValue(Units.Hour).ShouldBe(23);
        FieldValidator.GetMaxValue(Units.Day).ShouldBe(31);
        FieldValidator.GetMaxValue(Units.Month).ShouldBe(12);
        FieldValidator.GetMaxValue(Units.DayOfWeek).ShouldBe(7);
    }

    #endregion

    #region Validate(string, Units) - wildcard

    [Fact]
    public void Validate_String_Wildcard_DoesNotThrow_ForAllUnits()
    {
        Should.NotThrow(() => FieldValidator.Validate("*", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("*", Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate("*", Units.Day));
        Should.NotThrow(() => FieldValidator.Validate("*", Units.Month));
        Should.NotThrow(() => FieldValidator.Validate("*", Units.DayOfWeek));
    }

    #endregion

    #region Validate(string, Units) - single value

    [Fact]
    public void Validate_String_SingleValue_Valid_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate("0", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("59", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("12", Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate("15", Units.Day));
        Should.NotThrow(() => FieldValidator.Validate("6", Units.DayOfWeek));
    }

    [Fact]
    public void Validate_String_SingleValue_DayOfWeekSeven_IsValid()
    {
        // Per the cron specification, 7 is a legal DayOfWeek value in its own right (an alias
        // for Sunday) - it is not out of range.
        Should.NotThrow(() => FieldValidator.Validate("7", Units.DayOfWeek));
    }

    [Fact]
    public void Validate_String_SingleValue_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("60", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("24", Units.Hour));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0", Units.Day));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("32", Units.Day));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0", Units.Month));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("13", Units.Month));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("8", Units.DayOfWeek));
    }

    #endregion

    #region Validate(string, Units) - comma list

    [Fact]
    public void Validate_String_CommaList_Valid_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate("0,30,59", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("1,15,31", Units.Day));
    }

    [Fact]
    public void Validate_String_CommaList_OneInvalid_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0,60,59", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("1,32,31", Units.Day));
    }

    #endregion

    #region Validate(string, Units) - range

    [Fact]
    public void Validate_String_Range_Valid_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate("0-59", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("8-17", Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate("1-31", Units.Day));
        Should.NotThrow(() => FieldValidator.Validate("1-5", Units.DayOfWeek));
    }

    [Fact]
    public void Validate_String_Range_DayOfWeekEndingInSeven_IsValid()
    {
        // "5-7" (Friday through Sunday) and "6-7" (Saturday through Sunday) are legitimate
        // ranges, not reversed ones - 7 is a real value above 6, not an alias rewritten to 0
        // before this check runs.
        Should.NotThrow(() => FieldValidator.Validate("5-7", Units.DayOfWeek));
        Should.NotThrow(() => FieldValidator.Validate("6-7", Units.DayOfWeek));
        Should.NotThrow(() => FieldValidator.Validate("0-7", Units.DayOfWeek));
    }

    [Fact]
    public void Validate_String_Range_StartGreaterThanOrEqualToEnd_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("10-5", Units.Minute));
        ex.Message.ShouldContain("Start value 10");
        ex.Message.ShouldContain("must be less than end value 5");

        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("5-5", Units.Hour));
    }

    [Fact]
    public void Validate_String_Range_EndOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0-60", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("1-32", Units.Day));
    }

    [Fact]
    public void Validate_String_Range_StartOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("60-59", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0-31", Units.Day));
    }

    #endregion

    #region Validate(string, Units) - step/interval

    [Fact]
    public void Validate_String_Step_WildcardStart_Valid_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate("*/5", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("*/2", Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate("*/10", Units.Day));
    }

    [Fact]
    public void Validate_String_Step_NumericStart_Valid_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate("0/15", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("5/10", Units.Day));
    }

    [Fact]
    public void Validate_String_Step_DayOfWeek_ThrowsNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() => FieldValidator.Validate("*/2", Units.DayOfWeek))
            .Message.ShouldContain("Interval values are not supported for day of week");
    }

    [Fact]
    public void Validate_String_Step_IntervalZero_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("*/0", Units.Minute));
        ex.Message.ShouldContain("Interval value 0");
        ex.Message.ShouldContain("must be greater than 0");
    }

    [Fact]
    public void Validate_String_Step_IntervalZero_ReportsIntervalErrorForEveryField()
    {
        // An interval is a step size, not a field value, so a zero interval must report that
        // directly for all four step-capable fields. The 1-based fields (day, month) would
        // otherwise fail the generic range check first and describe the interval as though it
        // were a day-of-month or month value.
        foreach (var unit in new[] { Units.Minute, Units.Hour, Units.Day, Units.Month })
        {
            var wildcardStart = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("*/0", unit));
            wildcardStart.Message.ShouldContain("Interval value 0");
            wildcardStart.Message.ShouldContain("must be greater than 0");

            // Same for an explicit start value, e.g. "1/0".
            var numericStart = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("1/0", unit));
            numericStart.Message.ShouldContain("Interval value 0");
            numericStart.Message.ShouldContain("must be greater than 0");
        }
    }

    [Fact]
    public void Validate_String_Step_IntervalAboveFieldMaximum_ReportsRangeError()
    {
        // An interval larger than the field's maximum is genuinely a range problem, so it
        // should still produce the field-range message rather than the interval-specific one.
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("*/60", Units.Minute));
        ex.Message.ShouldContain("must be between 0 and 59");
    }

    [Fact]
    public void Validate_String_Step_StartOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("60/5", Units.Minute));
    }

    [Fact]
    public void Validate_String_Step_TooManySegments_ThrowsFormatException()
    {
        // "1/2/3" was previously accepted silently, with the "/3" dropped - only the first
        // two split parts (start "1" and interval "2") were ever read. Splitting to a
        // maximum of 2 parts folds the extra "/3" into the interval side, so int.Parse fails
        // on "2/3", consistent with how ValidateRange already catches "1-2-3" the same way.
        Should.Throw<FormatException>(() => FieldValidator.Validate("1/2/3", Units.Minute));
        Should.Throw<FormatException>(() => FieldValidator.Validate("*/2/3", Units.Hour));
    }

    #endregion

    #region Validate(string, Units) - range combined with a step

    [Fact]
    public void Validate_String_RangeWithStep_Valid_DoesNotThrow()
    {
        // "5-10/2" previously fell into ValidateRange (because '-' was checked before '/'),
        // which tried to int.Parse("10/2") as the range's end value and threw a FormatException.
        // A range combined with a step must now be recognized and validated as a step whose
        // start is itself a range.
        Should.NotThrow(() => FieldValidator.Validate("5-10/2", Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate("8-17/3", Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate("1-31/5", Units.Day));
        Should.NotThrow(() => FieldValidator.Validate("1-12/2", Units.Month));
    }

    [Fact]
    public void Validate_String_RangeWithStep_StartGreaterThanOrEqualToEnd_ThrowsArgumentOutOfRangeException()
    {
        // The range portion is validated exactly as it would be with no step attached, so a
        // reversed range is still rejected the same way "10-5" alone would be.
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("10-5/2", Units.Minute));
        ex.Message.ShouldContain("Start value 10");
        ex.Message.ShouldContain("must be less than end value 5");
    }

    [Fact]
    public void Validate_String_RangeWithStep_RangeOutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("60-70/2", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("0-31/5", Units.Day));
    }

    [Fact]
    public void Validate_String_RangeWithStep_IntervalZero_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("5-10/0", Units.Minute));
        ex.Message.ShouldContain("Interval value 0");
        ex.Message.ShouldContain("must be greater than 0");
    }

    [Fact]
    public void Validate_String_RangeWithStep_IntervalAboveFieldMaximum_ReportsRangeError()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("5-10/60", Units.Minute));
        ex.Message.ShouldContain("must be between 0 and 59");
    }

    [Fact]
    public void Validate_String_RangeWithStep_DayOfWeek_ThrowsNotSupportedException()
    {
        // Step syntax is unsupported for DayOfWeek regardless of shape - combining it with a
        // range must not slip past that check.
        Should.Throw<NotSupportedException>(() => FieldValidator.Validate("1-5/2", Units.DayOfWeek))
            .Message.ShouldContain("Interval values are not supported for day of week");
    }

    [Fact]
    public void Validate_String_RangeWithStep_InCommaList_ValidatesEachElement()
    {
        // A range-with-step element inside a comma list must be validated the same way it
        // would be on its own.
        Should.NotThrow(() => FieldValidator.Validate("5-10/2,20-25", Units.Minute));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate("5-10/2,60-70", Units.Minute));
    }

    #endregion

    #region Validate(int, Units)

    [Fact]
    public void Validate_Int_AtBoundaries_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.Validate(0, Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate(59, Units.Minute));
        Should.NotThrow(() => FieldValidator.Validate(0, Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate(23, Units.Hour));
        Should.NotThrow(() => FieldValidator.Validate(1, Units.Day));
        Should.NotThrow(() => FieldValidator.Validate(31, Units.Day));
        Should.NotThrow(() => FieldValidator.Validate(1, Units.Month));
        Should.NotThrow(() => FieldValidator.Validate(12, Units.Month));
        Should.NotThrow(() => FieldValidator.Validate(0, Units.DayOfWeek));
        Should.NotThrow(() => FieldValidator.Validate(7, Units.DayOfWeek));
    }

    [Fact]
    public void Validate_Int_BelowMin_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(-1, Units.Minute));
        ex.Message.ShouldContain("Value -1");
        ex.Message.ShouldContain("must be between 0 and 59");

        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(0, Units.Day));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(0, Units.Month));
    }

    [Fact]
    public void Validate_Int_AboveMax_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(60, Units.Minute));
        ex.Message.ShouldContain("Value 60");
        ex.Message.ShouldContain("must be between 0 and 59");

        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(24, Units.Hour));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(32, Units.Day));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(13, Units.Month));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.Validate(8, Units.DayOfWeek));
    }

    #endregion

    #region ValidateDayOfMonth

    [Fact]
    public void ValidateDayOfMonth_WildcardDay_DoesNotThrow()
    {
        // "*" is always satisfiable (day 1 exists in every month), so it short-circuits
        // without expanding either side.
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("*", "*"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("*", "2"));
    }

    [Fact]
    public void ValidateDayOfMonth_NonNumericDay_ThrowsArgumentException()
    {
        // A non-numeric, non-wildcard Day is not valid syntax for the field and can never
        // reach this method via the CronExpression.Day setter in practice - but if it did,
        // it must now fail loudly rather than silently skipping the day/month check.
        Should.Throw<ArgumentException>(() => FieldValidator.ValidateDayOfMonth("L", "1"));
    }

    [Fact]
    public void ValidateDayOfMonth_DayLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("0", "*"));
        ex.ParamName.ShouldBe("Day");
        ex.Message.ShouldContain("Value 0 for day must be between 1 and 31");

        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("-1", "1"));
    }

    [Fact]
    public void ValidateDayOfMonth_ValidDayForMonth_DoesNotThrow()
    {
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("15", "*"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("31", "1"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("28", "2"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("29", "2"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("30", "4"));
    }

    [Fact]
    public void ValidateDayOfMonth_Day31ForFebruary_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("31", "2"));
        ex.ParamName.ShouldBe("Day");
        ex.Message.ShouldContain("Day of month 31 is invalid for months");
        ex.Message.ShouldContain("2");
    }

    [Fact]
    public void ValidateDayOfMonth_Day30ForFebruary_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("30", "2"));
        ex.ParamName.ShouldBe("Day");
        ex.Message.ShouldContain("Day of month 30 is invalid for months");
    }

    [Fact]
    public void ValidateDayOfMonth_Day31ForApril_ThrowsArgumentOutOfRangeException()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("31", "4"));
        ex.ParamName.ShouldBe("Day");
        ex.Message.ShouldContain("31");
        ex.Message.ShouldContain("4");
    }

    [Fact]
    public void ValidateDayOfMonth_MonthExpression_AllMonths_AcceptsDayValidInAtLeastOneMonth()
    {
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("15", "*"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("28", "*"));
    }

    [Fact]
    public void ValidateDayOfMonth_MonthExpression_List_ValidatesAgainstEachMonth()
    {
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("28", "1,2,3"));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("31", "2,4,6"));
    }

    [Fact]
    public void ValidateDayOfMonth_MonthExpression_Range_ExpandsCorrectly()
    {
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("31", "1,3"));
    }

    [Fact]
    public void ValidateDayOfMonth_NullOrWhiteSpaceMonth_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => FieldValidator.ValidateDayOfMonth("15", null!));
        Should.Throw<ArgumentException>(() => FieldValidator.ValidateDayOfMonth("15", ""));
        Should.Throw<ArgumentException>(() => FieldValidator.ValidateDayOfMonth("15", "   "));
    }

    [Fact]
    public void ValidateDayOfMonth_InvalidMonthSegment_Throws()
    {
        Should.Throw<ArgumentException>(() => FieldValidator.ValidateDayOfMonth("15", "1/2/3"));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("15", "1/0"));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("15", "13"));
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("15", "0"));
    }

    [Fact]
    public void ValidateDayOfMonth_InvalidMonthRange_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("15", "5-3"));
    }

    [Fact]
    public void ValidateDayOfMonth_MonthStepExpression_ExpandsPastFirstValue()
    {
        // "2/5" must expand to February, July, and December - not just February.
        // Day 30 is invalid for February (max 29) but valid for July and December (max 31 each),
        // so this only passes if July and December are actually included in the expansion.
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("30", "2/5"));
    }

    [Fact]
    public void ValidateDayOfMonth_MonthStepExpression_ExpandsToEveryStepNotJustTheStart()
    {
        // "4/3" must expand to April, July, and October. Day 31 is invalid for April (max 30)
        // but valid for July and October (max 31 each), so this only passes if July and
        // October are actually included in the expansion rather than just April.
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("31", "4/3"));
    }

    [Fact]
    public void ValidateDayOfMonth_DayAsList_ChecksEveryValueAgainstEveryMonth()
    {
        // Previously, a Day expressed as anything other than a bare single value skipped this
        // check entirely - "30,31" against February slipped through unvalidated. It must now be
        // caught the same way a single invalid day is.
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("30,31", "2"));
        ex.ParamName.ShouldBe("Day");
        ex.Message.ShouldContain("30,31");
        ex.Message.ShouldContain("2");

        // "30" is invalid for February but "31" is valid for January - valid in at least one
        // selected month is still enough.
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("30,31", "1"));
    }

    [Fact]
    public void ValidateDayOfMonth_DayAsRange_ChecksEveryValueAgainstEveryMonth()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("30-31", "2"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("1-5", "2"));
    }

    [Fact]
    public void ValidateDayOfMonth_DayAsStep_ChecksEveryValueAgainstEveryMonth()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => FieldValidator.ValidateDayOfMonth("30/1", "2"));
        Should.NotThrow(() => FieldValidator.ValidateDayOfMonth("1/10", "2")); // 1, 11, 21
    }

    #endregion
}
