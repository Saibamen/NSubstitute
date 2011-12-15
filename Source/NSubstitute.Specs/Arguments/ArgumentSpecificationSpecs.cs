﻿using NSubstitute.Core.Arguments;
using NSubstitute.Specs.Infrastructure;
using NUnit.Framework;

namespace NSubstitute.Specs.Arguments
{
    public class ArgumentSpecificationSpecs
    {
        public interface IFoo { }
        public class Foo : IFoo { }

        public class Concern : ConcernFor<ArgumentSpecification>
        {
            protected IArgumentMatcher _argumentMatcher;
            protected object _argument;

            public override void Context()
            {
                base.Context();
                _argumentMatcher = mock<IArgumentMatcher>();
            }

            public override ArgumentSpecification CreateSubjectUnderTest()
            {
                return new ArgumentSpecification(typeof(IFoo), _argumentMatcher);
            }

            public void Matches(object argument) { _argumentMatcher.stub(x => x.IsSatisfiedBy(_argument)).Return(true); }
            public void DoesNotMatch(object argument) { _argumentMatcher.stub(x => x.IsSatisfiedBy(_argument)).Return(false); }
        }

        public class When_argument_is_compatible_type : Concern
        {
            public override void Context()
            {
                base.Context();
                _argument = new Foo();
            }

            [Test]
            public void Spec_is_satisfied_when_matches()
            {
                Matches(_argument);

                Assert.That(sut.IsSatisfiedBy(_argument));
            }

            [Test]
            public void Spec_is_not_satisfied_when_not_matched()
            {
                DoesNotMatch(_argument);

                Assert.False(sut.IsSatisfiedBy(_argument));
            }
        }

        public class When_argument_is_incompatible_type : Concern
        {
            private bool _result;

            public override void Context()
            {
                base.Context();
                _argument = new object();
            }

            public override void Because()
            {
                base.Because();
                _result = sut.IsSatisfiedBy(_argument);
            }

            [Test]
            public void Spec_is_not_satisfied()
            {
                Assert.False(_result);
            }

            [Test]
            public void Spec_does_not_need_to_check_matcher()
            {
                _argumentMatcher.did_not_receive_with_any_args(x => x.IsSatisfiedBy(null));
            }
        }

        public class When_argument_is_reference_type : Concern
        {
            public ArgumentSpecification CreateSubjectForByRefType<T>(ref T value)
            {
                var parameterType = GetType().GetMethod("MethodWithARefArgument").GetParameters()[0].ParameterType;
                return new ArgumentSpecification(parameterType, _argumentMatcher);
            }

            public void MethodWithARefArgument<T>(ref T arg) { }
        }
    }
}