using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard;
using Orchard.Data;
using Orchard.Email.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Workflows.Models;
using Upgrade.Services;

namespace Upgrade.Controllers {
    [Admin]
    public class MessagingController : Controller {
        private readonly IUpgradeService _upgradeService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<ActivityRecord> _repository;

        public MessagingController(
            IUpgradeService upgradeService,
            IOrchardServices orchardServices,
            IRepository<ActivityRecord> repository) {
            _upgradeService = upgradeService;
            _orchardServices = orchardServices;
            _repository = repository;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var found = false;
            var activityTable = _upgradeService.GetPrefixedTableName("Orchard_Workflows_ActivityRecord");
            if (_upgradeService.TableExists(activityTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + activityTable + " WHERE Name = 'SendEmail'",
                    (reader, connection) => {
                        found = true;
                    });

                if (!found) {
                    _orchardServices.Notifier.Warning(T("This step is unnecessary as no Send Email activities were found."));
                }
            }
            else {
                _orchardServices.Notifier.Warning(T("This step appears unnecessary since it appears Orchard Workflows is not enabled."));
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to upgrade.")))
                return new HttpUnauthorizedResult();

            var activityTable = _upgradeService.GetPrefixedTableName("Orchard_Workflows_ActivityRecord");
            if (_upgradeService.TableExists(activityTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + activityTable + " WHERE Name = 'SendEmail'",
                    (reader, connection) => {
                        var record = _repository.Get((int)reader["Id"]);

                        if (record == null) {
                            return;
                        }

                        var state = JsonConvert.DeserializeAnonymousType(record.State, new {
                            Body = "",
                            Subject = "",
                            Recipient = "",
                            RecipientOther = "",
                        });

                        var newState = new EmailMessage {
                            Body = state.Body,
                            Subject = state.Subject
                        };

                        if (!newState.Body.StartsWith("<p ")) {
                            newState.Body =
                                newState.Body
                                + System.Environment.NewLine;
                        }

                        if (state.Recipient == "owner") {
                            newState.Recipients = "{Content.Author.Email}";
                        }
                        else if (state.Recipient == "author") {
                            newState.Recipients = "{User.Current.Email}";
                        }
                        else if (state.Recipient == "admin") {
                            newState.Recipients = "{Site.SuperUser.Email}";
                        }
                        else if (state.Recipient == "other") {
                            newState.Recipients = state.RecipientOther;
                        }

                        record.State = JsonConvert.SerializeObject(newState);
                    });

                _orchardServices.Notifier.Information(T("Email activities updated successfully"));
            }
            else {
                _orchardServices.Notifier.Warning(T("No email activities were updated."));
            }

            return RedirectToAction("Index");
        }
    }
}
