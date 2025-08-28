using System;
namespace ProfessionalSystemUtility
{
    public interface IAuthenticationService { bool ValidatePassword(string password); }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _storedPasswordHash;
        private readonly string _storedSalt;
        public AuthenticationService(IConfigurationProvider configProvider)
        {
            _storedPasswordHash = configProvider.Get("PasswordHash");
            _storedSalt = configProvider.Get("Salt");
            if (string.IsNullOrEmpty(_storedPasswordHash) || string.IsNullOrEmpty(_storedSalt))
            { throw new InvalidOperationException("Password hash or salt is not configured correctly in App.config."); }
        }
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) { return false; }
            string hashOfInputPassword = PasswordHelper.HashPassword(password, _storedSalt);
            return hashOfInputPassword.Equals(_storedPasswordHash);
        }
    }
}
