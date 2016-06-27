using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services.Models;
using Orchard.Caching;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;
using Orchard.Settings;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailManager : Component, IAuditTrailManager {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailRepository;
        private readonly IAuditTrailEventProvider _providers;
        private readonly IClock _clock;
        private readonly IAuditTrailEventHandler _auditTrailEventHandlers;
        private readonly IEventDataSerializer _serializer;
        private readonly ICacheManager _cacheManager;
        private readonly ISiteService _siteService;
        private readonly ISignals _signals;
        private readonly IShapeFactory _shapeFactory;
        private readonly IClientHostAddressAccessor _clientHostAddressAccessor;

        public AuditTrailManager(
            IRepository<AuditTrailEventRecord> auditTrailRepository,
            IAuditTrailEventProvider providers,
            IClock clock,
            IAuditTrailEventHandler auditTrailEventHandlers,
            IEventDataSerializer serializer,
            ICacheManager cacheManager,
            ISiteService siteService,
            ISignals signals,
            IShapeFactory shapeFactory, 
            IClientHostAddressAccessor clientHostAddressAccessor) {

            _auditTrailRepository = auditTrailRepository;
            _providers = providers;
            _clock = clock;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _serializer = serializer;
            _cacheManager = cacheManager;
            _siteService = siteService;
            _signals = signals;
            _shapeFactory = shapeFactory;
            _clientHostAddressAccessor = clientHostAddressAccessor;
        }

        public IPageOfItems<AuditTrailEventRecord> GetRecords(
            int page,
            int pageSize,
            Filters filters = null,
            AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending) {

            var query = _auditTrailRepository.Table;

            if (filters != null) {
                var filterContext = new QueryFilterContext(query, filters);

                // Invoke event handlers.
                _auditTrailEventHandlers.Filter(filterContext);

                // Give each provider a chance to modify the query.
                var providersContext = DescribeProviders();
                foreach (var queryFilter in providersContext.QueryFilters) {
                    queryFilter(filterContext);
                }

                query = filterContext.Query;
            }

            switch (orderBy) {
                case AuditTrailOrderBy.EventAscending:
                    query = query.OrderBy(x => x.EventName).ThenByDescending(x => x.Id);
                    break;
                case AuditTrailOrderBy.CategoryAscending:
                    query = query.OrderBy(x => x.Category).ThenByDescending(x => x.Id);
                    break;
                default:
                    query = query.OrderByDescending(x => x.CreatedUtc).ThenByDescending(x => x.Id);
                    break;
            }

            var totalItemCount = query.Count();
            var startIndex = (page - 1) * pageSize;

            query = query.Skip(startIndex);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return new PageOfItems<AuditTrailEventRecord>(query) {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalItemCount
            };
        }

        public AuditTrailEventRecord GetRecord(int id) {
            return _auditTrailRepository.Get(id);
        }

        public dynamic BuildFilterDisplay(Filters filters) {
            var filterDisplay = (dynamic)_shapeFactory.Create("AuditTrailFilter");
            var filterDisplayContext = new DisplayFilterContext(_shapeFactory, filters, filterDisplay);

            // Invoke event handlers.
            _auditTrailEventHandlers.DisplayFilter(filterDisplayContext);

            // Give each provider a chance to provide a filter display.
            var providersContext = DescribeProviders();

            foreach (var action in providersContext.FilterDisplays) {
                action(filterDisplayContext);
            }

            return filterDisplay;
        }

        public AuditTrailEventRecordResult CreateRecord<T>(string eventName, IUser user, IDictionary<string, object> properties = null, IDictionary<string, object> eventData = null, string eventFilterKey = null, string eventFilterData = null) where T : IAuditTrailEventProvider {
            var eventDescriptor = DescribeEvent<T>(eventName);
            if (!IsEventEnabled(eventDescriptor))
                return new AuditTrailEventRecordResult {
                    Record = null,
                    IsDisabled = true
                };

            if (properties == null) properties = new Dictionary<string, object>();
            if (eventData == null) eventData = new Dictionary<string, object>();

            var context = new AuditTrailCreateContext {
                Event = eventName,
                User = user,
                Properties = properties,
                EventData = eventData,
                EventFilterKey = eventFilterKey,
                EventFilterData = eventFilterData
            };

            _auditTrailEventHandlers.Create(context);

            var record = new AuditTrailEventRecord {
                Category = eventDescriptor.CategoryDescriptor.Category,
                EventName = eventName,
                FullEventName = eventDescriptor.Event,
                CreatedUtc = _clock.UtcNow,
                UserName = user != null ? user.UserName : null,
                EventData = _serializer.Serialize(context.EventData),
                EventFilterKey = context.EventFilterKey,
                EventFilterData = context.EventFilterData,
                Comment = context.Comment,
                ClientIpAddress = GetClientAddress()
            };

            _auditTrailRepository.Create(record);
            return new AuditTrailEventRecordResult {
                Record = record,
                IsDisabled = false
            };
        }

        public IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories() {
            var context = DescribeProviders();
            return context.Describe();
        }

        public DescribeContext DescribeProviders() {
            var context = new DescribeContext();
            _providers.Describe(context);
            return context;
        }

        public AuditTrailEventDescriptor DescribeEvent(AuditTrailEventRecord record) {
            return DescribeEvent(record.FullEventName) ?? AuditTrailEventDescriptor.Basic(record);
        }

        public AuditTrailEventDescriptor DescribeEvent<T>(string eventName) where T : IAuditTrailEventProvider {
            var fullyQualifiedEventName = EventNameExtensions.GetFullyQualifiedEventName<T>(eventName);
            return DescribeEvent(fullyQualifiedEventName);
        }

        public AuditTrailEventDescriptor DescribeEvent(string fullyQualifiedEventName) {
            var categoryDescriptors = DescribeCategories();
            var eventDescriptorQuery =
                from c in categoryDescriptors
                from e in c.Events
                where e.Event == fullyQualifiedEventName
                select e;
            var eventDescriptors = eventDescriptorQuery.ToArray();

            return eventDescriptors.FirstOrDefault();
        }

        public IEnumerable<AuditTrailEventRecord> Trim(TimeSpan retentionPeriod) {
            var dateThreshold = (_clock.UtcNow.EndOfDay() - retentionPeriod);
            var query = _auditTrailRepository.Table.Where(x => x.CreatedUtc <= dateThreshold);
            var records = query.ToArray();

            foreach (var record in records) {
                _auditTrailRepository.Delete(record);
            }

            return records;
        }

        public string SerializeProviderConfiguration(IEnumerable<AuditTrailEventSetting> settings) {
            var doc = new XDocument(
                new XElement("Events",
                    settings.Select(x =>
                        new XElement("Event",
                            new XAttribute("Name", x.EventName),
                            new XAttribute("IsEnabled", x.IsEnabled)))));

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        public IEnumerable<AuditTrailEventSetting> DeserializeProviderConfiguration(string data) {
            if (String.IsNullOrWhiteSpace(data))
                return Enumerable.Empty<AuditTrailEventSetting>();

            try {
                var doc = XDocument.Parse(data);
                return doc.Element("Events").Elements("Event").Select(x => new AuditTrailEventSetting {
                    EventName = x.Attr<string>("Name"),
                    IsEnabled = x.Attr<bool>("IsEnabled")
                }).ToArray();

            }
            catch (Exception ex) {
                Logger.Error(ex, "Error occurred during deserialization of audit trail settings.");
            }
            return Enumerable.Empty<AuditTrailEventSetting>();
        }

        private string GetClientAddress() {
            var settings = _siteService.GetSiteSettings().As<AuditTrailSettingsPart>();

            if (!settings.EnableClientIpAddressLogging)
                return null;

            return _clientHostAddressAccessor.GetClientAddress();
        }

        private bool IsEventEnabled(AuditTrailEventDescriptor eventDescriptor) {
            if (eventDescriptor.IsMandatory)
                return true;

            var settingsDictionary = _cacheManager.Get("AuditTrail.EventSettings", context => {
                context.Monitor(_signals.When("AuditTrail.EventSettings"));
                return _siteService.GetSiteSettings().As<AuditTrailSettingsPart>().EventSettings.ToDictionary(x => x.EventName);
            });
            var setting = settingsDictionary.ContainsKey(eventDescriptor.Event) ? settingsDictionary[eventDescriptor.Event] : default(AuditTrailEventSetting);
            return setting != null ? setting.IsEnabled : eventDescriptor.IsEnabledByDefault;
        }
    }
}