using StructureMap.AutoMocking;
using System;

namespace RhDev.Common.Web.Core.Test._Setup
{
    public abstract class UnitTestOf<TSUT> : UnitTestBase where TSUT : class
    {
        protected AutoMocker<TSUT> Mocker { get; private set; }

        protected virtual TSUT SUT
        {

            get { return Mocker.ClassUnderTest; }
            set { throw new NotSupportedException(); }
        }

        protected override void Setup()
        {
            Mocker = new AutoMocker<TSUT>(new NSubstituteServiceLocator());
        }
    }
}
