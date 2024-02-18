namespace ConsoleJobScheduler.Core.Domain.Identity.Model
{
    public sealed class User
    {
        private IList<string>? _roles;

        public int Id { get; private set; }

        public string UserName { get; private set; }

        public IList<string> Roles
        {
            get
            {
                _roles ??= new List<string>();

                return _roles.AsReadOnly();
            }
        }

        public User(int id, string userName, IList<string>? roles)
        {
            Id = id;
            UserName = userName;
            _roles = roles;
        }

        public void SetUserId(int id)
        {
            Id = id;
        }
    }
}