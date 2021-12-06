using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class PasswordHistoryService : IPasswordHistoryService {
        private readonly IRepository<PasswordHistoryRecord> _historyRepository;
        private readonly IPasswordService _passwordService;

        public PasswordHistoryService(IRepository<PasswordHistoryRecord> historyRepository, IPasswordService passwordService) {
            _historyRepository = historyRepository;
            _passwordService = passwordService;
        }

        public void CreateEntry(UserPart user) {
            _historyRepository.Create(new PasswordHistoryRecord {
                UserPartRecord = user.Record,
                HashAlgorithm = user.HashAlgorithm,
                Password = user.Password,
                PasswordFormat = user.PasswordFormat,
                PasswordSalt = user.PasswordSalt,
                LastPasswordChangeUtc = user.LastPasswordChangeUtc,
            });
        }

        public bool MatchLastPasswords(string plaintextPassword, int howManyPasswords, UserPart user) {
            if (user == null)
                return false;
            var lastPasswords = _historyRepository.Fetch(x => x.UserPartRecord.Id == user.Id).OrderByDescending(x => x.LastPasswordChangeUtc).Take(howManyPasswords);
            foreach (var password in lastPasswords) {
                if (_passwordService.IsMatch(new PasswordContext {
                    PasswordSalt = password.PasswordSalt,
                    HashAlgorithm = password.HashAlgorithm,
                    Password = password.Password,
                    PasswordFormat = password.PasswordFormat
                }, plaintextPassword)) {
                    return true;
                }
            }

            return false;

        }


    }


}