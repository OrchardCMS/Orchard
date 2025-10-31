namespace Orchard.Security
{
    //TEMP: Add setters, provide default constructor and remove parameterized constructor
    public class CreateUserParams
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _email;
        private readonly string _passwordQuestion;
        private readonly string _passwordAnswer;
        private readonly bool _isApproved;
        private readonly bool _forcePasswordChange;

        public CreateUserParams(string username, string password, string email)
            : this(username, password, email, string.Empty, string.Empty, true, false) { }

        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved)
            : this(username, password, email, passwordQuestion, passwordAnswer, isApproved, false) { }

        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, bool forcePasswordChange)
        {
            _username = username;
            _password = password;
            _email = email;
            _passwordQuestion = passwordQuestion;
            _passwordAnswer = passwordAnswer;
            _isApproved = isApproved;
            _forcePasswordChange = forcePasswordChange;
        }

        public string Username => _username;

        public string Password => _password;

        public string Email => _email;

        public string PasswordQuestion => _passwordQuestion;

        public string PasswordAnswer => _passwordAnswer;

        public bool IsApproved => _isApproved;

        public bool ForcePasswordChange => _forcePasswordChange;
    }
}