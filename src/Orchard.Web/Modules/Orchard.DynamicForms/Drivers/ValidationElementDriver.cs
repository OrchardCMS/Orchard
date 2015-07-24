//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Orchard.DynamicForms.Elements;
//using Orchard.DynamicForms.Helpers;
//using Orchard.DynamicForms.Services;
//using Orchard.DynamicForms.Services.Models;
//using Orchard.DynamicForms.ViewModels;
//using Orchard.Forms.Services;
//using Orchard.Layouts.Framework.Display;
//using Orchard.Layouts.Framework.Drivers;
//using Orchard.Layouts.Helpers;

//namespace Orchard.DynamicForms.Drivers {
//    public class ValidationElementDriver : ElementDriver<FormElement> {
//        private readonly IValidationManager _validationManager;
//        private readonly IFormManager _formManager;

//        public ValidationElementDriver(IValidationManager validationManager, IFormManager formManager) {
//            _validationManager = validationManager;
//            _formManager = formManager;
//        }

//        protected override EditorResult OnBuildEditor(FormElement element, ElementEditorContext context) {
//            var validatorNames = element.ValidatorNames.ToArray();

//            var validators = new FieldValidationSettingsViewModel {
//                Validators = _validationManager.GetValidatorsByNames(validatorNames).Select(x => new FieldValidationSettingViewModel() {
//                    Name = x.Name,
//                    SettingsEditor = BuildSettingsEditor(context, x)
//                }).ToList()
//            };

//            if (context.Updater != null) {
//                var viewModel = new FieldValidationSettingsViewModel();
//                if (context.Updater.TryUpdateModel(viewModel, null, null, null)) {
//                    if (viewModel.Validators != null) {
//                        foreach (var validatorModel in viewModel.Validators) {
//                            var validator = validators.Validators.SingleOrDefault(x => x.Name == validatorModel.Name);

//                            if (validator == null)
//                                continue;

//                            validator.Enabled = validatorModel.Enabled;
//                            validator.CustomValidationMessage = validatorModel.CustomValidationMessage;
//                        }
//                    }
//                    validators.ShowValidationMessage = viewModel.ShowValidationMessage;
//                }
//            }

//            // Fetch validation descriptors.
//            foreach (var validator in validators.Validators) {
//                validator.Descriptor = _validationManager.GetValidatorByName(validator.Name);
//            }

//            var validatorsEditor = context.ShapeFactory.EditorTemplate(TemplateName: "Validators", Model: validators);
//            validatorsEditor.Metadata.Position = "Validation:10";

//            return Editor(context, validatorsEditor);
//        }

//        protected override void OnDisplaying(FormElement element, ElementDisplayingContext context) {
//            if (context.DisplayType == "Design" || element.Form == null)
//                return;

//            if (element.Form.EnableClientValidation != true) {
//                context.ElementShape.ClientValidationAttributes = new Dictionary<string, string>();
//                return;
//            }

//            var clientAttributes = new Dictionary<string, string> {
//                {"data-val", "true"}
//            };

//            foreach (var validatorSetting in element.ValidationSettings.Validators.Enabled()) {
//                var validatorDescriptor = _validationManager.GetValidatorByName(validatorSetting.Name);
//                var clientValidationRegistrationContext = new ClientValidationRegistrationContext(element, validatorSetting, validatorDescriptor);
                
//                validatorDescriptor.ClientAttributes(clientValidationRegistrationContext);
//                clientAttributes.Combine(clientValidationRegistrationContext.ClientAttributes);
//            }

//            context.ElementShape.ClientValidationAttributes = clientAttributes;
//        }

//        private dynamic BuildSettingsEditor(ElementEditorContext context, ValidatorDescriptor validatorDescriptor) {
//            if (String.IsNullOrWhiteSpace(validatorDescriptor.SettingsFormName))
//                return null;

//            return _formManager.Bind(_formManager.Build(validatorDescriptor.SettingsFormName), context.ValueProvider);
//        }
//    }
//}