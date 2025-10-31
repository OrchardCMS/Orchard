namespace Orchard.Security
{
    //TEMP: Add setters, provide default constructor and remove parameterized constructor
    public class CreateUserParams
    {
        public CreateUserParams(string username, string password, string email)
            : this(username, password, email, string.Empty, string.Empty, true, false) { }

        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved)
            : this(username, password, email, passwordQuestion, passwordAnswer, isApproved, false) { }

        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, bool forcePasswordChange)
        {
            Username = username;
            Password = password;
            Email = email;
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
            IsApproved = isApproved;
            ForcePasswordChange = forcePasswordChange;
        }

        public string Username { get; }

        public string Password { get; }

        public string Email { get; }

        public string PasswordQuestion { get; }

        public string PasswordAnswer { get; }

        public bool IsApproved { get; }

        public bool ForcePasswordChange { get; }
    }
}