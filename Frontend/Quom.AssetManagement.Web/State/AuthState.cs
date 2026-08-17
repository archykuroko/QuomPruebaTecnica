using Quom.AssetManagement.Web.Models;

namespace Quom.AssetManagement.Web.State
{
    public class AuthState
    {
        public string? Token { get; private set; }

        public int? UserId { get; private set; }

        public string? Username { get; private set; }

        public string? Role { get; private set; }

        public DateTime? ExpiresAt { get; private set; }


        public bool IsAuthenticated =>
            !string.IsNullOrWhiteSpace(Token)
            && ExpiresAt.HasValue
            && ExpiresAt.Value > DateTime.UtcNow;


        public bool IsAdministrator =>
            IsAuthenticated &&
            string.Equals(
                Role?.Trim(),
                "Administrador",
                StringComparison.OrdinalIgnoreCase);


        public bool IsOperator =>
            IsAuthenticated &&
            string.Equals(
                Role?.Trim(),
                "Operador",
                StringComparison.OrdinalIgnoreCase);


        public void SetSession(LoginResponse response)
        {
            Token = response.Token;

            UserId = response.UserId;

            Username = response.Username;

            Role = response.Role?.Trim();

            ExpiresAt = response.ExpiresAt;
        }


        public void RestoreSession(
            StoredAuthSession session)
        {
            Token = session.Token;

            UserId = session.UserId;

            Username = session.Username;

            Role = session.Role;

            ExpiresAt = session.ExpiresAt;
        }


        public void ClearSession()
        {
            Token = null;

            UserId = null;

            Username = null;

            Role = null;

            ExpiresAt = null;
        }
    }
}