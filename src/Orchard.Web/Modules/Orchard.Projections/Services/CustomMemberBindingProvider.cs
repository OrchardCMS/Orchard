using System;
using System.Linq;
using System.Reflection;
using Orchard.Data;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class CustomMemberBindingProvider : IMemberBindingProvider {
        private readonly IRepository<MemberBindingRecord> _repository;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;

        public CustomMemberBindingProvider(
            IRepository<MemberBindingRecord> repository,
            ISessionFactoryHolder sessionFactoryHolder) {
            _repository = repository;
            _sessionFactoryHolder = sessionFactoryHolder;
        }

        public void GetMemberBindings(BindingBuilder builder) {

            var recordBluePrints = _sessionFactoryHolder.GetSessionFactoryParameters().RecordDescriptors;

            foreach(var member in _repository.Table.ToList()) {
                var record = recordBluePrints.FirstOrDefault(r => String.Equals(r.Type.FullName, member.Type, StringComparison.OrdinalIgnoreCase));

                if (record == null) {
                    continue;
                }

                var property = record.Type.GetProperty(member.Member, BindingFlags.Instance | BindingFlags.Public);
                if(property == null) {
                    continue;
                }

                builder.Add(property, member.DisplayName, member.Description);
            }
        }
    }
}