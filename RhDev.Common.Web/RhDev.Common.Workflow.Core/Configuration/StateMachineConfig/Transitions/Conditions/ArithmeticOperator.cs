using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig.Transitions.Conditions
{
    public enum ArithmeticOperator
    {
        Unknown = 0,
        Add = 1,
        Sub = 1 << 1,
        Div = 1 << 2,
        Mul = 1 << 3,
        Substring = 1 << 4,
        AddYears = 1 << 5,
        AddMonths = 1 << 6,
        AddDays = 1 << 7,
        AddHours = 1 << 8,
        AddMinutes = 1 << 9,
        Push = 1 << 10,
        Clear = 1 << 11,
        Length = 1 << 12,
        Neg = 1 << 13,
        Format = 1 << 14,
        Index = 1 << 15,
        Pop = 1 << 16,
        Abs = 1 << 17
    }
}
