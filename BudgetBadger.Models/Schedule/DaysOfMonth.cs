using System;
namespace BudgetBadger.Models.Schedule
{
    public enum DaysOfMonth
    {
        None = 0,
        Day01 = 1 << 0,
        Day02 = 1 << 1,
        Day03 = 1 << 2,
        Day04 = 1 << 3,
        Day05 = 1 << 4,
        Day06 = 1 << 5,
        Day07 = 1 << 6,
        Day08 = 1 << 7,
        Day09 = 1 << 8,
        Day10 = 1 << 9,
        Day11 = 1 << 10,
        Day12 = 1 << 11,
        Day13 = 1 << 12,
        Day14 = 1 << 13,
        Day15 = 1 << 14,
        Day16 = 1 << 15,
        Day17 = 1 << 16,
        Day18 = 1 << 17,
        Day19 = 1 << 18,
        Day20 = 1 << 19,
        Day21 = 1 << 20,
        Day22 = 1 << 21,
        Day23 = 1 << 22,
        Day24 = 1 << 23,
        Day25 = 1 << 24,
        Day26 = 1 << 25,
        Day27 = 1 << 26,
        Day28 = 1 << 27,
        Day29 = 1 << 28,
        Day30 = 1 << 29,
        Day31 = 1 << 30,
        DayLast = 1 << 31,
        All = Day01 | Day02 | Day03 | Day04 | Day05 | Day06 | Day07 | Day08 | Day09 | Day10 | Day11 | Day12 | Day13 | Day14 | Day15 | Day16 | Day17 | Day18 | Day19 | Day20 | Day21 | Day22 | Day23 | Day24 | Day25 | Day26 | Day27 | Day28 | Day29 | Day30 | Day31
    }
}