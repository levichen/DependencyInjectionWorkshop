using System;
using System.Linq;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public class AuditLogInterceptor : IInterceptor
    {
        private readonly IContext _context;
        private readonly ILogger _logger;

        public AuditLogInterceptor(IContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!(Attribute.GetCustomAttribute(invocation.Method, typeof(AuditLogAttribute)) is AuditLogAttribute auditLogAttribute))
            {
                invocation.Proceed();
            }
            else
            {
                var currentUser = _context.GetCurrentUser();
                var parameters = string.Join("|", invocation.Arguments.Select(x => (x ?? "").ToString()));

                _logger.Info($"user:{currentUser.Name} invoke with parameters:{parameters}");

                invocation.Proceed();

                var returnValue = invocation.ReturnValue;
                _logger.Info(returnValue.ToString());
            }
        }
    }

    internal class AuditLogDecorator : AuthenticationDecoratorBase
    {
        private readonly IContext _context;
        private readonly ILogger _logger;

        public AuditLogDecorator(IAuthentication authentication, ILogger logger, IContext context) : base(
            authentication)
        {
            _logger = logger;
            _context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var currentUser = _context.GetCurrentUser();
            _logger.Info($"user:{currentUser.Name} invoke with parameters: {accountId} | {password} | {otp}");

            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"isValid:{isValid}");
            return isValid;
        }
    }
}