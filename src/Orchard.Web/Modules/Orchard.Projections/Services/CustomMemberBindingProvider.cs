using System;
using System.Collections.Generic;
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

        private List<MemberBindingRecord> memberBindings;
        public void GetMemberBindings(BindingBuilder builder) {

            var recordBluePrints = _sessionFactoryHolder.GetSessionFactoryParameters().RecordDescriptors;

            // save this in memory once per request, to avoid hitting the database 5+
            // times per projection per request.
            if (memberBindings == null) {
                memberBindings = _repository.Table.ToList();
            }

            foreach (var member in memberBindings) {
                var record = recordBluePrints.FirstOrDefault(r => String.Equals(r.Type.FullName, member.Type, StringComparison.OrdinalIgnoreCase));

                if (record == null) {
                    continue;
                }

                var property = record.Type.GetProperty(member.Member, BindingFlags.Instance | BindingFlags.Public);
                if (property == null) {
                    continue;
                }

                builder.Add(property, member.DisplayName, member.Description);
            }
        }
    }
}