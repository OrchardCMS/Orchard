namespace Orchard.Security {
    //TEMP: Add setters, provide default constructor and remove parameterized constructor
    public class CreateUserParams {
        private readonly string _username;
        private readonly string _password;
        private readonly string _email;
        private readonly string _passwordQuestion;
        private readonly string _passwordAnswer;
        private readonly bool _isApproved;

        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved) {
            _username = username;
            _password = password;
            _email = email;
            _passwordQuestion = passwordQuestion;
            _passwordAnswer = passwordAnswer;
            _isApproved = isApproved;
        }

        public string Username {
            get { return _username; }
        }

        public string Password {
            get { return _password; }
        }

        public string Email {
            get { return _email; }
        }

        public string PasswordQuestion {
            get { return _passwordQuestion; }
        }

        public string PasswordAnswer {
            get { return _passwordAnswer; }
        }

        public bool IsApproved {
            get { return _isApproved; }
        }
    }
}