using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Projections.Controllers {
    [ValidateInput(false), Admin]
    public class BindingController : Controller {
        private readonly IRepository<MemberBindingRecord> _repository;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;

        public BindingController(
            IRepository<MemberBindingRecord> repository,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISessionFactoryHolder sessionFactoryHolder) {
            _repository = repository;
            _sessionFactoryHolder = sessionFactoryHolder;
            Shape = shapeFactory;
            Services = services;

            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(BindingIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            // default options
            if (options == null)
                options = new BindingIndexOptions();

            var bindings = _repository.Table;

            switch (options.Filter) {
                case BindingsFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!String.IsNullOrWhiteSpace(options.Search)) {
                bindings = bindings.Where(r => r.DisplayName.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(bindings.Count());

            switch (options.Order) {
                case BindingsOrder.Name:
                    bindings = bindings.OrderBy(u => u.DisplayName);
                    break;
            }

            var results = bindings
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var model = new BindingIndexViewModel {
                Bindings = results.Select(x => new BindingEntry { Binding = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        public ActionResult Select() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();

            var recordBluePrints = _sessionFactoryHolder.GetSessionFactoryParameters().RecordDescriptors;

            var model = new BindingSelectViewModel {
                Records = recordBluePrints.Where(r => IsContentPartRecord(r.Type)).Select(r => new RecordEntry {
                    FullName = r.Type.FullName,
                    Members = r.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => IsValidPropertyType(x.PropertyType)).Select(
                        m => new MemberEntry {
                            Member = m.Name
                        }).ToList()
                }).ToList()
            };

            return View(model);
        }

        public ActionResult Create(string fullName, string member) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();

            var recordBluePrints = _sessionFactoryHolder.GetSessionFactoryParameters().RecordDescriptors;

            var record = recordBluePrints.FirstOrDefault(r => r.Type.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase));

            if(record == null) {
                return HttpNotFound();
            }

            var property = record.Type.GetProperty(member, BindingFlags.Instance | BindingFlags.Public);

            if (property == null) {
                return HttpNotFound();
            }

            var model = new BindingEditViewModel {
                Id = -1,
                FullName = record.Type.FullName,
                Member = property.Name
            };

            return View("Edit", model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(BindingEditViewModel model) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();
        
            if(ModelState.IsValid) {

                _repository.Create(new MemberBindingRecord {
                    Type = model.FullName,
                    Member = model.Member,
                    DisplayName =  model.Display,
                    Description = model.Description
                });

                Services.Notifier.Information(T("Binding created successfully"));

                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();

            var binding = _repository.Get(id);

            if (binding == null) {
                return HttpNotFound();
            }

            var recordBluePrints = _sessionFactoryHolder.GetSessionFactoryParameters().RecordDescriptors;

            var record = recordBluePrints.FirstOrDefault(r => String.Equals(r.Type.FullName, binding.Type, StringComparison.OrdinalIgnoreCase));

            if (record == null) {
                Services.Notifier.Information(T("The record for this binding is no longer available, please remove it."));
                return RedirectToAction("Index");
            }

            var property = record.Type.GetProperty(binding.Member, BindingFlags.Instance | BindingFlags.Public);

            if (property == null) {
                Services.Notifier.Information(T("The member for this binding is no longer available, please remove it."));
                return RedirectToAction("Index");
            }

            var model = new BindingEditViewModel {
                Id = id,
                FullName = record.Type.FullName,
                Member = property.Name,
                Display = binding.DisplayName,
                Description = binding.Description
            };

            return View("Edit", model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult Edit(BindingEditViewModel model) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list bindings")))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid) {

                var binding = _repository.Get(model.Id);

                if (binding == null) {
                    return HttpNotFound();
                }

                binding.DisplayName = model.Display;
                binding.Description = model.Description;

                Services.Notifier.Information(T("Binding updated successfully"));

                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to delete bindings")))
                return new HttpUnauthorizedResult();

            var binding = _repository.Get(id);
            
            if (binding == null) {
                return HttpNotFound();
            }

            _repository.Delete(binding);
            Services.Notifier.Information(T("Binding deleted"));

            return RedirectToAction("Index");
        }

        private static bool IsContentPartRecord(Type type) {
            return typeof(ContentPartRecord).IsAssignableFrom(type) && typeof(ContentPartRecord) != type;
        }
 
        private bool IsValidPropertyType(Type type) {
            return type.IsValueType
                   || type == typeof (string)
                   || (typeof (Nullable).IsAssignableFrom(type) && IsValidPropertyType(type.GetGenericArguments()[0]));
        }
    }
}