namespace GBEmu.Tests
{
    using System;
    using Moq;
    using Moq.AutoMock;
    using NUnit.Framework;

    [TestFixture]
    public abstract class UnitTestBase<TClassUnderTest> where TClassUnderTest : class
    {
        private AutoMocker _autoMocker;
        private Lazy<TClassUnderTest> _classUnderTest;

        protected TClassUnderTest ClassUnderTest => _classUnderTest.Value;

        [SetUp]
        protected virtual void Setup()
        {
            _autoMocker = new AutoMocker(MockBehavior.Loose);
            _classUnderTest = new Lazy<TClassUnderTest>(() => _autoMocker.CreateInstance<TClassUnderTest>());
        }

        protected Mock<TMock> GetMock<TMock>() where TMock : class =>
            _autoMocker.GetMock<TMock>();

        protected void Bind<T>(T instance) =>
            _autoMocker.Use(instance);
    }
}