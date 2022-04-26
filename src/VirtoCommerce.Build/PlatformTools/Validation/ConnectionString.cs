namespace PlatformTools.Validation
{
    public abstract class ConnectionString
    {
        protected string _connectionString;
        public ConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
        public abstract string Validate();
        public virtual bool IsEmpty()
        {
            return string.IsNullOrEmpty(_connectionString);
        }
        public virtual string GetConnectionString()
        {
            return _connectionString;
        }
    }
}
