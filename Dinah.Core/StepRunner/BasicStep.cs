﻿using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Dinah.Core.StepRunner
{
    public class BasicStep : BaseStep
    {
        public Func<bool>? Fn { get; set; }
        protected override bool RunRaw() => Fn is not null && Fn();
    }
}
