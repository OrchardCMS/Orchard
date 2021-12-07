using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class PasswordHistoryService : IPasswordHistoryService {
        private readonly IEnumerable<PasswordHistoryEntry> emptyPasswordHistoryEntries = new List<PasswordHistoryEntry>();
        private readonly IRepository<PasswordHistoryRecord> _historyRepository;
        private readonly IPasswordService _passwordService;

        public PasswordHistoryService(IRepository<PasswordHistoryRecord> historyRepository, IPasswordService passwordService) {
            _historyRepository = historyRepository;
            _passwordService = passwordService;
        }

        public void CreateEntry(PasswordHistoryEntry context) {
            _historyRepository.Create(new PasswordHistoryRecord {
                UserPartRecord = context.User?.As<UserPart>()?.Record,
                HashAlgorithm = context.HashAlgorithm,
                Password = context.Password,
                PasswordFormat = context.PasswordFormat,
                PasswordSalt = context.PasswordSalt,
                LastPasswordChangeUtc = context.LastPasswordChangeUtc,
            });
        }

        public IEnumerable<PasswordHistoryEntry> GetLastPasswords(IUser user, int count) {
            if (user == null)
                return emptyPasswordHistoryEntries;
            var lastPasswords = _historyRepository
                                    .Fetch(x => x.UserPartRecord.Id == user.Id)
                                    .OrderByDescending(x => x.LastPasswordChangeUtc)
                                    .Take(count)
                                    .Select(x => new PasswordHistoryEntry {
                                        Password = x.Password,
                                        PasswordSalt = x.PasswordSalt,
                                        HashAlgorithm = x.HashAlgorithm,
                                        PasswordFormat = x.PasswordFormat,
                                        LastPasswordChangeUtc = x.LastPasswordChangeUtc,
                                        User = user
                                    });
            return lastPasswords;
        }

        public bool PasswordMatchLastOnes(string password, IUser user, int count) {
            if (user == null)
                return false;
            return GetLastPasswords(user, count)
                    .Any(x => _passwordService.IsMatch(x, password));
        }
    }
}