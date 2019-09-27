﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;

namespace MyConsole
{
    class Program
    {
        private static IAuthentication _authentication;
        private static IContainer _container;

        static void Main(string[] args)
        {
            RegisterContainer();

            Console.WriteLine("who are you?");
            var name = Console.ReadLine();
            var context = _container.Resolve<IContext>();
            context.SetCurrentUser(new Account() {Name = name});

            _authentication = _container.Resolve<IAuthentication>();

            var isValid = _authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeProfile>().As<IProfile>()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(AuditLogInterceptor));

            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();


            builder.RegisterType<AuthenticationService>().As<IAuthentication>()
                   //.EnableClassInterceptors()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(AuditLogInterceptor));

            builder.RegisterType<MyContext>().As<IContext>().SingleInstance();
            builder.RegisterType<AuditLogInterceptor>();

            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogFailedCountDecorator, IAuthentication>();
            //builder.RegisterDecorator<AuditLogDecorator, IAuthentication>();

            //_authentication = new NotificationDecorator(_authentication, _notification);
            //_authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            //_authentication = new LogFailedCountDecorator(_authentication, _logger, _failedCounter);
            var container = builder.Build();
            _container = container;
        }
    }

    public class MyContext : IContext
    {
        private Account _account;

        public Account GetCurrentUser()
        {
            return _account;
        }

        public void SetCurrentUser(Account account)
        {
            _account = account;
        }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Send(string accountId)
        {
            PushMessage($"{nameof(Send)}, accountId:{accountId}");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void ResetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(ResetFailedCount)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}