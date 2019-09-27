namespace MyConsole
{
    public class Account
    {
        public string Name { get; set; }
    }

    public interface IContext
    {
        Account GetCurrentUser();
        void SetCurrentUser(Account account);
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
}