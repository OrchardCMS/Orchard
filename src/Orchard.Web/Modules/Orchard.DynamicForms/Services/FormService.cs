using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Models;
using Orchard.DynamicForms.Services.Models;
using Orchard.DynamicForms.ViewModels;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Localization.Services;
using Orchard.Services;

namespace Orchard.DynamicForms.Services {
    public class FormService : IFormService {
        private readonly ILayoutSerializer _serializer;
        private readonly IClock _clock;
        private readonly IRepository<Submission> _submissionRepository;
        private readonly IFormElementEventHandler _elementHandlers;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IBindingManager _bindingManager;
        private readonly IDynamicFormEventHandler _formEventHandler;
        private readonly Lazy<IEnumerable<IElementValidator>> _validators;
        private readonly IDateLocalizationServices _dateLocalizationServices;
        private readonly IOrchardServices _services;
        private readonly ICultureAccessor _cultureAccessor;

        public FormService(
            ILayoutSerializer serializer, 
            IClock clock, 
            IRepository<Submission> submissionRepository, 
            IFormElementEventHandler elementHandlers, 
            IContentDefinitionManager contentDefinitionManager, 
            IBindingManager bindingManager, 
            IDynamicFormEventHandler formEventHandler, 
            Lazy<IEnumerable<IElementValidator>> validators,
            IDateLocalizationServices dateLocalizationServices, 
            IOrchardServices services, 
            ICultureAccessor cultureAccessor) {

            _serializer = serializer;
            _clock = clock;
            _submissionRepository = submissionRepository;
            _elementHandlers = elementHandlers;
            _contentManager = services.ContentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _bindingManager = bindingManager;
            _formEventHandler = formEventHandler;
            _validators = validators;
            _dateLocalizationServices = dateLocalizationServices;
            _services = services;
            _cultureAccessor = cultureAccessor;
        }

        public Form FindForm(LayoutPart layoutPart, string formName = null) {
            var elements = _serializer.Deserialize(layoutPart.LayoutData, new DescribeElementsContext { Content = layoutPart });
            var forms = elements.Flatten().Where(x => x is Form).Cast<Form>();
            return String.IsNullOrWhiteSpace(formName) ? forms.FirstOrDefault() : forms.FirstOrDefault(x => x.Name == formName);
        }

        public IEnumerable<FormElement> GetFormElements(Form form) {
            return form.Elements.Flatten().Where(x => x is FormElement).Cast<FormElement>();
        }

        public IEnumerable<string> GetFormElementNames(Form form) {
            return GetFormElements(form).Select(x => x.Name).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct();
        }

        public NameValueCollection SubmitForm(IContent content, Form form, IValueProvider valueProvider, ModelStateDictionary modelState, IUpdateModel updater) {
            var values = ReadElementValues(form, valueProvider);

            _formEventHandler.Submitted(new FormSubmittedEventContext {
                Content = content,
                Form = form,
                FormService = this,
                ValueProvider = valueProvider,
                Values = values,
                Updater = updater
            });

            _formEventHandler.Validating(new FormValidatingEventContext {
                Content = content,
                Form = form,
                FormService = this,
                Values = values,
                ModelState = modelState,
                ValueProvider = valueProvider,
                Updater = updater
            });

            _formEventHandler.Validated(new FormValidatedEventContext {
                Content = content,
                Form = form,
                FormService = this,
                Values = values,
                ModelState = modelState,
                ValueProvider = valueProvider,
                Updater = updater
            });

            return values;
        }

        public Submission CreateSubmission(string formName, NameValueCollection values) {
            var submission = new Submission {
                FormName = formName,
                CreatedUtc = _clock.UtcNow,
                FormData = values.ToQueryString()
            };

            CreateSubmission(submission);
            return submission;
        }

        public Submission CreateSubmission(Submission submission) {
            _submissionRepository.Create(submission);
            return submission;
        }

        public Submission GetSubmission(int id) {
            return _submissionRepository.Get(id);
        }

        public IPageOfItems<Submission> GetSubmissions(string formName = null, int? skip = null, int? take = null) {
            var query = _submissionRepository.Table;

            if (!String.IsNullOrWhiteSpace(formName))
                query = query.Where(x => x.FormName == formName);

            query = new Orderable<Submission>(query).Desc(x => x.CreatedUtc).Queryable;

            var totalItemCount = query.Count();
            if (skip != null && take.GetValueOrDefault() > 0)
                query = query.Skip(skip.Value).Take(take.GetValueOrDefault());

            return new PageOfItems<Submission>(query) {
                PageNumber = skip.GetValueOrDefault() * take.GetValueOrDefault(),
                PageSize = take ?? Int32.MaxValue,
                TotalItemCount = totalItemCount
            };
        }

        public void DeleteSubmission(Submission submission) {
            _submissionRepository.Delete(submission);
        }

        public int DeleteSubmissions(IEnumerable<int> submissionIds) {
            var submissions = _submissionRepository.Table.Where(x => submissionIds.Contains(x.Id)).ToArray();

            foreach (var submission in submissions) {
                DeleteSubmission(submission);
            }

            return submissions.Length;
        }

        public void ReadElementValues(FormElement element, ReadElementValuesContext context) {
            _elementHandlers.GetElementValue(element, context);
        }

        public NameValueCollection ReadElementValues(Form form, IValueProvider valueProvider) {
            var formElements = GetFormElements(form);
            var values = new NameValueCollection();

            // Let each element provide its values.
            foreach (var element in formElements) {
                var context = new ReadElementValuesContext { ValueProvider = valueProvider };
                ReadElementValues(element, context);

                foreach (var key in from string key in context.Output where !String.IsNullOrWhiteSpace(key) && values[key] == null select key) {
                    var value = context.Output[key];

                    if (form.HtmlEncode)
                        value = HttpUtility.HtmlEncode(value);

                    values.Add(key, value);
                }
            }

            // Collect any remaining form values not handled by any specific element.
            var requestForm = _services.WorkContext.HttpContext.Request.Form;
            var blackList = new[] {"__RequestVerificationToken", "formName", "contentId"};
            foreach (var key in 
                from string key in requestForm 
                where !String.IsNullOrWhiteSpace(key) && !blackList.Contains(key) && values[key] == null 
                select key) {

                values.Add(key, requestForm[key]);
            }

            return values;
        }

        public DataTable GenerateDataTable(IEnumerable<Submission> submissions) {
            var records = submissions.Select(x => Tuple.Create(x, x.ToNameValues())).ToArray();
            var columnNames = new HashSet<string>();
            var dataTable = new DataTable();

            foreach (var key in 
                from record in records 
                from string key in record.Item2 where !columnNames.Contains(key) 
                where !String.IsNullOrWhiteSpace(key)
                select key) {
                columnNames.Add(key);
            }

            dataTable.Columns.Add("Id");
            dataTable.Columns.Add("Created");
            foreach (var columnName in columnNames) {
                dataTable.Columns.Add(columnName);
            }

            foreach (var record in records) {
                var dataRow = dataTable.NewRow();

                dataRow["Id"] = record.Item1.Id;
                dataRow["Created"] = _dateLocalizationServices.ConvertToSiteTimeZone(record.Item1.CreatedUtc).ToString(_cultureAccessor.CurrentCulture);
                foreach (var columnName in columnNames) {
                    var value = record.Item2[columnName];
                    dataRow[columnName] = value;
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public ContentItem CreateContentItem(Form form, IValueProvider valueProvider) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(form.FormBindingContentType);

            if (contentTypeDefinition == null)
                return null;

            var contentItem = _contentManager.New(contentTypeDefinition.Name);

            // Create the version record before updating fields to prevent those field values from being lost when invoking Create.
            // If Create is invoked while VersionRecord is null, a new VersionRecord will be created, wiping out our field values.
            contentItem.VersionRecord = new ContentItemVersionRecord {
                ContentItemRecord = new ContentItemRecord(),
                Number = 1,
                Latest = true,
                Published = true
            };

            var lookup = _bindingManager.DescribeBindingsFor(contentTypeDefinition);
            var formElements = GetFormElements(form);

            foreach (var element in formElements) {
                var context = new ReadElementValuesContext { ValueProvider = valueProvider };
                ReadElementValues(element, context);

                var value = context.Output[element.Name];
                var bindingSettings = element.Data.GetModel<FormBindingSettings>(null);

                if (bindingSettings != null) {
                    foreach (var partBindingSettings in bindingSettings.Parts) {
                        InvokePartBindings(contentItem, lookup, partBindingSettings, value);

                        foreach (var fieldBindingSettings in partBindingSettings.Fields) {
                            InvokeFieldBindings(contentItem, lookup, partBindingSettings, fieldBindingSettings, value);
                        }
                    }
                }
            }

            var contentTypeSettings = contentTypeDefinition.Settings.GetModel<ContentTypeSettings>();
            _contentManager.Create(contentItem, VersionOptions.Draft);

            if (form.Publication == "Publish" || !contentTypeSettings.Draftable) {
                _contentManager.Publish(contentItem);
            }
            
            return contentItem;
        }

        public IEnumerable<IElementValidator> GetValidators<TElement>() where TElement : FormElement {
            return GetValidators(typeof(TElement));
        }

        public IEnumerable<IElementValidator> GetValidators(FormElement element) {
            return GetValidators(element.GetType());
        }

        public IEnumerable<IElementValidator> GetValidators(Type elementType) {
            return _validators.Value.Where(x => IsFormElementType(x, elementType)).ToArray();
        }

        public IEnumerable<IElementValidator> GetValidators() {
            return _validators.Value.ToArray();
        }

        public void RegisterClientValidationAttributes(FormElement element, RegisterClientValidationAttributesContext context) {
            _elementHandlers.RegisterClientValidation(element, context);
        }

        private static void InvokePartBindings(
            ContentItem contentItem, 
            IEnumerable<ContentPartBindingDescriptor> lookup, 
            PartBindingSettings partBindingSettings,
            string value) {

            var part = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == partBindingSettings.Name);
            if (part == null)
                return;

            var partBindingDescriptors = lookup.Where(x => x.Part.PartDefinition.Name == partBindingSettings.Name);
            var partBindingsQuery =
                from descriptor in partBindingDescriptors
                from bindingContext in descriptor.BindingContexts
                where bindingContext.ContextName == part.PartDefinition.Name
                from binding in bindingContext.Bindings
                select binding;
            var partBindings = partBindingsQuery.ToArray();

            foreach (var binding in partBindingSettings.Bindings.Where(x => x.Enabled)) {
                var localBinding = binding;
                foreach (var partBinding in partBindings.Where(x => x.Name == localBinding.Name)) {
                    partBinding.Setter.DynamicInvoke(contentItem, part, value);
                }
            }
        }

        private static void InvokeFieldBindings(
            ContentItem contentItem,
            IEnumerable<ContentPartBindingDescriptor> lookup,
            PartBindingSettings partBindingSettings,
            FieldBindingSettings fieldBindingSettings,
            string value) {

            var part = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == partBindingSettings.Name);
            if (part == null)
                return;

            var field = part.Fields.FirstOrDefault(x => x.Name == fieldBindingSettings.Name);
            if (field == null)
                return;

            var fieldBindingDescriptorsQuery = 
                from partBindingDescriptor in lookup
                where partBindingDescriptor.Part.PartDefinition.Name == partBindingSettings.Name
                from fieldBindingDescriptor in partBindingDescriptor.FieldBindings
                where fieldBindingDescriptor.Field.Name == fieldBindingSettings.Name
                select fieldBindingDescriptor;
            var fieldBindingDescriptors = fieldBindingDescriptorsQuery.ToArray();
            var fieldBindingsQuery =
                from descriptor in fieldBindingDescriptors
                from bindingContext in descriptor.BindingContexts
                where bindingContext.ContextName == field.FieldDefinition.Name
                from binding in bindingContext.Bindings
                select binding;
            var fieldBindings = fieldBindingsQuery.ToArray();

            foreach (var binding in fieldBindingSettings.Bindings.Where(x => x.Enabled)) {
                var localBinding = binding;
                foreach (var fieldBinding in fieldBindings.Where(x => x.Name == localBinding.Name)) {
                    fieldBinding.Setter.DynamicInvoke(contentItem, field, value);
                }
            }
        }

        private static bool IsFormElementType(IElementValidator validator, Type elementType) {
            var validatorType = validator.GetType();
            var validatorElementType = validatorType.BaseType.GenericTypeArguments[0];
            return validatorElementType == elementType || validatorElementType.IsAssignableFrom(elementType);
        }
    }
}