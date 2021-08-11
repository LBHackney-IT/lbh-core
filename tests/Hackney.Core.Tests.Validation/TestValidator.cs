using FluentValidation;
using System;

namespace Hackney.Core.Tests.Validation
{
    public class TestValidator : InlineValidator<Dummy>
    {
        public new CascadeMode CascadeMode
        {
            get => base.CascadeMode;
            set => base.CascadeMode = value;
        }

        public TestValidator() { }

        public TestValidator(params Action<TestValidator>[] actions)
        {
            foreach (var action in actions)
            {
                action(this);
            }
        }
    }
}
