using System;
using System.Configuration;
using System.Xml.Schema;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repos;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfile _profileDao;
        private IHash _sha256Adapter;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private ILogger _logger;
        private INotification _notification;
        private IAuthenticationService _authenticationService;
        private const string DefaultAccountId = "levi";
        private const string DefaultPassword = "abc";
        private const string DefaultHashedPassword = "hashed password";
        private const string DefaultOtp = "123456";
        private const int DefaultFailedCount = 88;
        private const bool DefaultIsLocked = true;

        [SetUp]
        public void SetUp()
        {
            _profileDao = Substitute.For<IProfile>();
            _sha256Adapter = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            
            _authenticationService = new AuthenticationService(_profileDao, _failedCounter, _sha256Adapter, _otpService, _logger);
            _authenticationService = new NotificationDecorator(_authenticationService, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        private void WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
            ShouldBeInValid(isValid);
        }

        private void WhenInValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
            ShouldBeInValid(isValid);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultAccountId, DefaultFailedCount);
            WhenInValid();
            LogShouldContains();
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInValid();
            ShouldNotify(DefaultAccountId);
        }

        private void ShouldNotify(string accountId)
        {
            _notification.Received(1).Send(accountId);
        }

        [Test]
        public void account_id_locked()
        {
            GivenAccountIsLocked();
            ShouldThrow<FailedTooManyTimesException>();
        }

        private void GivenAccountIsLocked()
        {
            _failedCounter.GetAccountIsLocked(DefaultAccountId).Returns(DefaultIsLocked);
        }


        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private void LogShouldContains()
        {
            _logger.Received(1)
                .Info(Arg.Is<string>(m => m.Contains(DefaultAccountId) && m.Contains(DefaultFailedCount.ToString())));
        }

        private void GivenFailedCount(string accountId, int failedCount)
        {
            _failedCounter.GetFailedCount(accountId).Returns(failedCount);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private static void ShouldBeInValid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }

        private void GivenOtp(string accountId, string opt)
        {
            _otpService.GetCurrentOtp(accountId).Returns(opt);
        }

        private void GivenHash(string password, string hashedPassword)
        {
            _sha256Adapter.Compute(password).Returns(hashedPassword);
        }

        private void GivenPassword(string accountId, string hashedPassword)
        {
            _profileDao.GetPassword(accountId).Returns(hashedPassword);
        }
    }
}