using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net.Appender;
using log4net.Core;
using log4net.ObjectRenderer;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace Orchard.Logging {
    /// <summary>
    /// Initializes the log4net environment using an XML DOM.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configures a <see cref="Hierarchy"/> using an XML DOM.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public class OrchardXmlHierarchyConfigurator {
        private enum ConfigUpdateMode {
            Merge,
            Overwrite
        }

        // String constants used while parsing the XML data
        private const string ConfigurationTag = "log4net";
        private const string RendererTag = "renderer";
        private const string AppenderTag = "appender";
        private const string AppenderRefTag = "appender-ref";
        private const string ParamTag = "param";

        // TODO: Deprecate use of category tags
        private const string CategoryTag = "category";
        // TODO: Deprecate use of priority tag
        private const string PriorityTag = "priority";

        private const string LoggerTag = "logger";
        private const string NameAttr = "name";
        private const string TypeAttr = "type";
        private const string ValueAttr = "value";
        private const string RootTag = "root";
        private const string LevelTag = "level";
        private const string RefAttr = "ref";
        private const string AdditivityAttr = "additivity";
        private const string ThresholdAttr = "threshold";
        private const string ConfigDebugAttr = "configDebug";
        private const string InternalDebugAttr = "debug";
        private const string ConfigUpdateModeAttr = "update";
        private const string RenderingTypeAttr = "renderingClass";
        private const string RenderedTypeAttr = "renderedClass";

        // flag used on the level element
        private const string Inherited = "inherited";

        /// <summary>
        /// key: appenderName, value: appender.
        /// </summary>
        private Hashtable _appenderBag;

        /// <summary>
        /// The Hierarchy being configured.
        /// </summary>
        private readonly Hierarchy _hierarchy;

        /// <summary>
        /// The snapshot of the environment variables at configuration time, or null if an error has occured during querying them.
        /// </summary>
        private IDictionary _environmentVariables;

        /// <summary>
        /// Construct the configurator for a hierarchy
        /// </summary>
        /// <param name="hierarchy">The hierarchy to build.</param>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="OrchardXmlHierarchyConfigurator" /> class
        /// with the specified <see cref="Hierarchy" />.
        /// </para>
        /// </remarks>
        public OrchardXmlHierarchyConfigurator(Hierarchy hierarchy) {
            _hierarchy = hierarchy;
            _appenderBag = new Hashtable();
        }

        /// <summary>
        /// Configure the hierarchy by parsing a DOM tree of XML elements.
        /// </summary>
        /// <param name="element">The root element to parse.</param>
        /// <remarks>
        /// <para>
        /// Configure the hierarchy by parsing a DOM tree of XML elements.
        /// </para>
        /// </remarks>
        public void Configure(XmlElement element) {
            if (element == null || _hierarchy == null) {
                return;
            }

            var rootElementName = element.LocalName;

            if (rootElementName != ConfigurationTag) {
                LogLog.Error("XmlHierarchyConfigurator: Xml element is - not a <" + ConfigurationTag + "> element.");
                return;
            }

            if (!LogLog.InternalDebugging) {
                // Look for a debug attribute to enable internal debug
                var debugAttribute = element.GetAttribute(InternalDebugAttr);
                LogLog.Debug("XmlHierarchyConfigurator: " + InternalDebugAttr + " attribute [" + debugAttribute + "].");

                if (debugAttribute.Length > 0 && debugAttribute != "null") {
                    LogLog.InternalDebugging = OptionConverter.ToBoolean(debugAttribute, true);
                }
                else {
                    LogLog.Debug("XmlHierarchyConfigurator: Ignoring " + InternalDebugAttr + " attribute.");
                }

                var confDebug = element.GetAttribute(ConfigDebugAttr);
                if (confDebug.Length > 0 && confDebug != "null") {
                    LogLog.Warn("XmlHierarchyConfigurator: The \"" + ConfigDebugAttr + "\" attribute is deprecated.");
                    LogLog.Warn("XmlHierarchyConfigurator: Use the \"" + InternalDebugAttr + "\" attribute instead.");
                    LogLog.InternalDebugging = OptionConverter.ToBoolean(confDebug, true);
                }
            }

            // Default mode is merge
            var configUpdateMode = ConfigUpdateMode.Merge;

            // Look for the config update attribute
            var configUpdateModeAttribute = element.GetAttribute(ConfigUpdateModeAttr);
            if (!String.IsNullOrEmpty(configUpdateModeAttribute)) {
                // Parse the attribute
                try {
                    configUpdateMode = (ConfigUpdateMode)OptionConverter.ConvertStringTo(typeof(ConfigUpdateMode), configUpdateModeAttribute);
                }
                catch {
                    LogLog.Error("XmlHierarchyConfigurator: Invalid " + ConfigUpdateModeAttr + " attribute value [" + configUpdateModeAttribute + "]");
                }
            }

            // IMPL: The IFormatProvider argument to Enum.ToString() is deprecated in .NET 2.0
            LogLog.Debug("XmlHierarchyConfigurator: Configuration update mode [" + configUpdateMode + "].");

            // Only reset configuration if overwrite flag specified
            if (configUpdateMode == ConfigUpdateMode.Overwrite) {
                // Reset to original unset configuration
                _hierarchy.ResetConfiguration();
                LogLog.Debug("XmlHierarchyConfigurator: Configuration reset before reading config.");
            }

            // The Log4netFactory/OrchardXmlConfigurator/OrchardXmlHierarchyConfigurator can be refactored later
            // so we call HostEnvironment.IsFullTrust
            if (AppDomain.CurrentDomain.IsHomogenous && AppDomain.CurrentDomain.IsFullyTrusted)
                _environmentVariables = System.Environment.GetEnvironmentVariables();
            else {
                _environmentVariables = null;
            }

            /* Building Appender objects, placing them in a local namespace
               for future reference */

            /* Process all the top level elements */

            foreach (XmlNode currentNode in element.ChildNodes) {
                if (currentNode.NodeType == XmlNodeType.Element) {
                    var currentElement = (XmlElement)currentNode;

                    if (currentElement.LocalName == LoggerTag) {
                        ParseLogger(currentElement);
                    }
                    else if (currentElement.LocalName == CategoryTag) {
                        // TODO: deprecated use of category
                        ParseLogger(currentElement);
                    }
                    else if (currentElement.LocalName == RootTag) {
                        ParseRoot(currentElement);
                    }
                    else if (currentElement.LocalName == RendererTag) {
                        ParseRenderer(currentElement);
                    }
                    else if (currentElement.LocalName == AppenderTag) {
                        // We ignore appenders in this pass. They will
                        // be found and loaded if they are referenced.
                    }
                    else {
                        // Read the param tags and set properties on the hierarchy
                        SetParameter(currentElement, _hierarchy);
                    }
                }
            }

            // Lastly set the hierarchy threshold
            string thresholdStr = element.GetAttribute(ThresholdAttr);
            LogLog.Debug("XmlHierarchyConfigurator: Hierarchy Threshold [" + thresholdStr + "]");
            if (thresholdStr.Length > 0 && thresholdStr != "null") {
                var thresholdLevel = (Level)ConvertStringTo(typeof(Level), thresholdStr);
                if (thresholdLevel != null) {
                    _hierarchy.Threshold = thresholdLevel;
                }
                else {
                    LogLog.Warn("XmlHierarchyConfigurator: Unable to set hierarchy threshold using value [" + thresholdStr + "] (with acceptable conversion types)");
                }
            }

            // Done reading config
        }

        /// <summary>
        /// Parse appenders by IDREF.
        /// </summary>
        /// <param name="appenderRef">The appender ref element.</param>
        /// <returns>The instance of the appender that the ref refers to.</returns>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents an appender and return 
        /// the appender.
        /// </para>
        /// </remarks>
        protected IAppender FindAppenderByReference(XmlElement appenderRef) {
            var appenderName = appenderRef.GetAttribute(RefAttr);

            IAppender appender = (IAppender)_appenderBag[appenderName];
            if (appender != null) {
                return appender;
            }
            // Find the element with that id
            XmlElement element = null;

            if (!String.IsNullOrEmpty(appenderName)) {
                foreach (XmlElement curAppenderElement in appenderRef.OwnerDocument.GetElementsByTagName(AppenderTag)) {
                    if (curAppenderElement.GetAttribute("name") != appenderName) {
                        continue;
                    }
                    element = curAppenderElement;
                    break;
                }
            }

            if (element == null) {
                LogLog.Error("XmlHierarchyConfigurator: No appender named [" + appenderName + "] could be found.");
                return null;
            }
            appender = ParseAppender(element);
            if (appender != null) {
                _appenderBag[appenderName] = appender;
            }
            return appender;
        }

        /// <summary>
        /// Parses an appender element.
        /// </summary>
        /// <param name="appenderElement">The appender element.</param>
        /// <returns>The appender instance or <c>null</c> when parsing failed.</returns>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents an appender and return
        /// the appender instance.
        /// </para>
        /// </remarks>
        protected IAppender ParseAppender(XmlElement appenderElement) {
            var appenderName = appenderElement.GetAttribute(NameAttr);
            var typeName = appenderElement.GetAttribute(TypeAttr);

            LogLog.Debug("XmlHierarchyConfigurator: Loading Appender [" + appenderName + "] type: [" + typeName + "]");
            try {
                var appender = (IAppender)Activator.CreateInstance(SystemInfo.GetTypeFromString(typeName, true, true));
                appender.Name = appenderName;

                foreach (XmlNode currentNode in appenderElement.ChildNodes) {
                    /* We're only interested in Elements */
                    if (currentNode.NodeType == XmlNodeType.Element) {
                        var currentElement = (XmlElement)currentNode;

                        // Look for the appender ref tag
                        if (currentElement.LocalName == AppenderRefTag) {
                            var refName = currentElement.GetAttribute(RefAttr);

                            var appenderContainer = appender as IAppenderAttachable;
                            if (appenderContainer != null) {
                                LogLog.Debug("XmlHierarchyConfigurator: Attaching appender named [" + refName + "] to appender named [" + appender.Name + "].");

                                var referencedAppender = FindAppenderByReference(currentElement);
                                if (referencedAppender != null) {
                                    appenderContainer.AddAppender(referencedAppender);
                                }
                            }
                            else {
                                LogLog.Error("XmlHierarchyConfigurator: Requesting attachment of appender named [" + refName + "] to appender named [" + appender.Name + "] which does not implement log4net.Core.IAppenderAttachable.");
                            }
                        }
                        else {
                            // For all other tags we use standard set param method
                            SetParameter(currentElement, appender);
                        }
                    }
                }

                var optionHandler = appender as IOptionHandler;
                if (optionHandler != null) {
                    optionHandler.ActivateOptions();
                }

                LogLog.Debug("XmlHierarchyConfigurator: Created Appender [" + appenderName + "]");
                return appender;
            }
            catch (Exception ex) {
                // Yes, it's ugly.  But all exceptions point to the same problem: we can't create an Appender

                LogLog.Error("XmlHierarchyConfigurator: Could not create Appender [" + appenderName + "] of type [" + typeName + "]. Reported error follows.", ex);
                return null;
            }
        }

        /// <summary>
        /// Parses a logger element.
        /// </summary>
        /// <param name="loggerElement">The logger element.</param>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents a logger.
        /// </para>
        /// </remarks>
        protected void ParseLogger(XmlElement loggerElement) {
            // Create a new log4net.Logger object from the <logger> element.
            var loggerName = loggerElement.GetAttribute(NameAttr);

            LogLog.Debug("XmlHierarchyConfigurator: Retrieving an instance of log4net.Repository.Logger for logger [" + loggerName + "].");
            var log = _hierarchy.GetLogger(loggerName) as Logger;

            // Setting up a logger needs to be an atomic operation, in order
            // to protect potential log operations while logger
            // configuration is in progress.
            if (log == null) {
                return;
            }
            lock (log) {
                var additivity = OptionConverter.ToBoolean(loggerElement.GetAttribute(AdditivityAttr), true);

                LogLog.Debug("XmlHierarchyConfigurator: Setting [" + log.Name + "] additivity to [" + additivity + "].");
                log.Additivity = additivity;
                ParseChildrenOfLoggerElement(loggerElement, log, false);
            }
        }

        /// <summary>
        /// Parses the root logger element.
        /// </summary>
        /// <param name="rootElement">The root element.</param>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents the root logger.
        /// </para>
        /// </remarks>
        protected void ParseRoot(XmlElement rootElement) {
            var root = _hierarchy.Root;
            // logger configuration needs to be atomic
            lock (root) {
                ParseChildrenOfLoggerElement(rootElement, root, true);
            }
        }

        /// <summary>
        /// Parses the children of a logger element.
        /// </summary>
        /// <param name="catElement">The category element.</param>
        /// <param name="log">The logger instance.</param>
        /// <param name="isRoot">Flag to indicate if the logger is the root logger.</param>
        /// <remarks>
        /// <para>
        /// Parse the child elements of a &lt;logger&gt; element.
        /// </para>
        /// </remarks>
        protected void ParseChildrenOfLoggerElement(XmlElement catElement, Logger log, bool isRoot) {
            // Remove all existing appenders from log. They will be
            // reconstructed if need be.
            log.RemoveAllAppenders();

            foreach (XmlNode currentNode in catElement.ChildNodes) {
                if (currentNode.NodeType == XmlNodeType.Element) {
                    var currentElement = (XmlElement)currentNode;

                    if (currentElement.LocalName == AppenderRefTag) {
                        var appender = FindAppenderByReference(currentElement);
                        var refName = currentElement.GetAttribute(RefAttr);
                        if (appender != null) {
                            LogLog.Debug("XmlHierarchyConfigurator: Adding appender named [" + refName + "] to logger [" + log.Name + "].");
                            log.AddAppender(appender);
                        }
                        else {
                            LogLog.Error("XmlHierarchyConfigurator: Appender named [" + refName + "] not found.");
                        }
                    }
                    else if (currentElement.LocalName == LevelTag || currentElement.LocalName == PriorityTag) {
                        ParseLevel(currentElement, log, isRoot);
                    }
                    else {
                        SetParameter(currentElement, log);
                    }
                }
            }

            var optionHandler = log as IOptionHandler;
            if (optionHandler != null) {
                optionHandler.ActivateOptions();
            }
        }

        /// <summary>
        /// Parses an object renderer.
        /// </summary>
        /// <param name="element">The renderer element.</param>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents a renderer.
        /// </para>
        /// </remarks>
        protected void ParseRenderer(XmlElement element) {
            var renderingClassName = element.GetAttribute(RenderingTypeAttr);
            var renderedClassName = element.GetAttribute(RenderedTypeAttr);

            LogLog.Debug("XmlHierarchyConfigurator: Rendering class [" + renderingClassName + "], Rendered class [" + renderedClassName + "].");
            var renderer = (IObjectRenderer)OptionConverter.InstantiateByClassName(renderingClassName, typeof(IObjectRenderer), null);
            if (renderer == null) {
                LogLog.Error("XmlHierarchyConfigurator: Could not instantiate renderer [" + renderingClassName + "].");
                return;
            }
            try {
                _hierarchy.RendererMap.Put(SystemInfo.GetTypeFromString(renderedClassName, true, true), renderer);
            }
            catch (Exception e) {
                LogLog.Error("XmlHierarchyConfigurator: Could not find class [" + renderedClassName + "].", e);
            }
        }

        /// <summary>
        /// Parses a level element.
        /// </summary>
        /// <param name="element">The level element.</param>
        /// <param name="log">The logger object to set the level on.</param>
        /// <param name="isRoot">Flag to indicate if the logger is the root logger.</param>
        /// <remarks>
        /// <para>
        /// Parse an XML element that represents a level.
        /// </para>
        /// </remarks>
        protected void ParseLevel(XmlElement element, Logger log, bool isRoot) {
            var loggerName = log.Name;
            if (isRoot) {
                loggerName = "root";
            }

            var levelStr = element.GetAttribute(ValueAttr);
            LogLog.Debug("XmlHierarchyConfigurator: Logger [" + loggerName + "] Level string is [" + levelStr + "].");

            if (Inherited == levelStr) {
                if (isRoot) {
                    LogLog.Error("XmlHierarchyConfigurator: Root level cannot be inherited. Ignoring directive.");
                }
                else {
                    LogLog.Debug("XmlHierarchyConfigurator: Logger [" + loggerName + "] level set to inherit from parent.");
                    log.Level = null;
                }
            }
            else {
                log.Level = log.Hierarchy.LevelMap[levelStr];
                if (log.Level == null) {
                    LogLog.Error("XmlHierarchyConfigurator: Undefined level [" + levelStr + "] on Logger [" + loggerName + "].");
                }
                else {
                    LogLog.Debug("XmlHierarchyConfigurator: Logger [" + loggerName + "] level set to [name=\"" + log.Level.Name + "\",value=" + log.Level.Value + "].");
                }
            }
        }

        /// <summary>
        /// Sets a parameter on an object.
        /// </summary>
        /// <param name="element">The parameter element.</param>
        /// <param name="target">The object to set the parameter on.</param>
        /// <remarks>
        /// The parameter name must correspond to a writable property
        /// on the object. The value of the parameter is a string,
        /// therefore this function will attempt to set a string
        /// property first. If unable to set a string property it
        /// will inspect the property and its argument type. It will
        /// attempt to call a static method called <c>Parse</c> on the
        /// type of the property. This method will take a single
        /// string argument and return a value that can be used to
        /// set the property.
        /// </remarks>
        protected void SetParameter(XmlElement element, object target) {
            // Get the property name
            var name = element.GetAttribute(NameAttr);

            // If the name attribute does not exist then use the name of the element
            if (element.LocalName != ParamTag || String.IsNullOrEmpty(name)) {
                name = element.LocalName;
            }

            // Look for the property on the target object
            var targetType = target.GetType();
            Type propertyType = null;

            MethodInfo methInfo = null;

            // Try to find a writable property
            var propInfo = targetType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (propInfo != null && propInfo.CanWrite) {
                // found a property
                propertyType = propInfo.PropertyType;
            }
            else {
                propInfo = null;

                // look for a method with the signature Add<property>(type)
                methInfo = FindMethodInfo(targetType, name);

                if (methInfo != null) {
                    propertyType = methInfo.GetParameters()[0].ParameterType;
                }
            }

            if (propertyType == null) {
                LogLog.Error("XmlHierarchyConfigurator: Cannot find Property [" + name + "] to set object on [" + target + "]");
            }
            else {
                string propertyValue = null;

                if (element.GetAttributeNode(ValueAttr) != null) {
                    propertyValue = element.GetAttribute(ValueAttr);
                }
                else if (element.HasChildNodes) {
                    // Concatenate the CDATA and Text nodes together
                    foreach (XmlNode childNode in element.ChildNodes) {
                        if (childNode.NodeType == XmlNodeType.CDATA || childNode.NodeType == XmlNodeType.Text) {
                            if (propertyValue == null) {
                                propertyValue = childNode.InnerText;
                            }
                            else {
                                propertyValue += childNode.InnerText;
                            }
                        }
                    }
                }

                if (propertyValue != null) {
                    if (_environmentVariables != null) {
                        // Expand environment variables in the string.
                        propertyValue = OptionConverter.SubstituteVariables(propertyValue, _environmentVariables);
                    }

                    Type parsedObjectConversionTargetType = null;

                    // Check if a specific subtype is specified on the element using the 'type' attribute
                    var subTypeString = element.GetAttribute(TypeAttr);
                    if (!String.IsNullOrEmpty(subTypeString)) {
                        // Read the explicit subtype
                        try {
                            var subType = SystemInfo.GetTypeFromString(subTypeString, true, true);

                            LogLog.Debug("XmlHierarchyConfigurator: Parameter [" + name + "] specified subtype [" + subType.FullName + "]");

                            if (!propertyType.IsAssignableFrom(subType)) {
                                // Check if there is an appropriate type converter
                                if (OptionConverter.CanConvertTypeTo(subType, propertyType)) {
                                    // Must re-convert to the real property type
                                    parsedObjectConversionTargetType = propertyType;

                                    // Use sub type as intermediary type
                                    propertyType = subType;
                                }
                                else {
                                    LogLog.Error("XmlHierarchyConfigurator: Subtype [" + subType.FullName + "] set on [" + name + "] is not a subclass of property type [" + propertyType.FullName + "] and there are no acceptable type conversions.");
                                }
                            }
                            else {
                                // The subtype specified is found and is actually a subtype of the property
                                // type, therefore we can switch to using this type.
                                propertyType = subType;
                            }
                        }
                        catch (Exception ex) {
                            LogLog.Error("XmlHierarchyConfigurator: Failed to find type [" + subTypeString + "] set on [" + name + "]", ex);
                        }
                    }

                    // Now try to convert the string value to an acceptable type
                    // to pass to this property.

                    var convertedValue = ConvertStringTo(propertyType, propertyValue);

                    // Check if we need to do an additional conversion
                    if (convertedValue != null && parsedObjectConversionTargetType != null) {
                        LogLog.Debug("XmlHierarchyConfigurator: Performing additional conversion of value from [" + convertedValue.GetType().Name + "] to [" + parsedObjectConversionTargetType.Name + "]");
                        convertedValue = OptionConverter.ConvertTypeTo(convertedValue, parsedObjectConversionTargetType);
                    }

                    if (convertedValue != null) {
                        if (propInfo != null) {
                            // Got a converted result
                            LogLog.Debug("XmlHierarchyConfigurator: Setting Property [" + propInfo.Name + "] to " + convertedValue.GetType().Name + " value [" + convertedValue + "]");

                            try {
                                // Pass to the property
                                propInfo.SetValue(target, convertedValue, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
                            }
                            catch (TargetInvocationException targetInvocationEx) {
                                LogLog.Error("XmlHierarchyConfigurator: Failed to set parameter [" + propInfo.Name + "] on object [" + target + "] using value [" + convertedValue + "]", targetInvocationEx.InnerException);
                            }
                        }
                        else if (methInfo != null) {
                            // Got a converted result
                            LogLog.Debug("XmlHierarchyConfigurator: Setting Collection Property [" + methInfo.Name + "] to " + convertedValue.GetType().Name + " value [" + convertedValue + "]");

                            try {
                                // Pass to the property
                                methInfo.Invoke(target, BindingFlags.InvokeMethod, null, new[] { convertedValue }, CultureInfo.InvariantCulture);
                            }
                            catch (TargetInvocationException targetInvocationEx) {
                                LogLog.Error("XmlHierarchyConfigurator: Failed to set parameter [" + name + "] on object [" + target + "] using value [" + convertedValue + "]", targetInvocationEx.InnerException);
                            }
                        }
                    }
                    else {
                        LogLog.Warn("XmlHierarchyConfigurator: Unable to set property [" + name + "] on object [" + target + "] using value [" + propertyValue + "] (with acceptable conversion types)");
                    }
                }
                else {
                    object createdObject;

                    if (propertyType == typeof(string) && !HasAttributesOrElements(element)) {
                        // If the property is a string and the element is empty (no attributes
                        // or child elements) then we special case the object value to an empty string.
                        // This is necessary because while the String is a class it does not have
                        // a default constructor that creates an empty string, which is the behavior
                        // we are trying to simulate and would be expected from CreateObjectFromXml
                        createdObject = "";
                    }
                    else {
                        // No value specified
                        Type defaultObjectType = null;
                        if (IsTypeConstructible(propertyType)) {
                            defaultObjectType = propertyType;
                        }

                        createdObject = CreateObjectFromXml(element, defaultObjectType, propertyType);
                    }

                    if (createdObject == null) {
                        LogLog.Error("XmlHierarchyConfigurator: Failed to create object to set param: " + name);
                    }
                    else {
                        if (propInfo != null) {
                            // Got a converted result
                            LogLog.Debug("XmlHierarchyConfigurator: Setting Property [" + propInfo.Name + "] to object [" + createdObject + "]");

                            try {
                                // Pass to the property
                                propInfo.SetValue(target, createdObject, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
                            }
                            catch (TargetInvocationException targetInvocationEx) {
                                LogLog.Error("XmlHierarchyConfigurator: Failed to set parameter [" + propInfo.Name + "] on object [" + target + "] using value [" + createdObject + "]", targetInvocationEx.InnerException);
                            }
                        }
                        else if (methInfo != null) {
                            // Got a converted result
                            LogLog.Debug("XmlHierarchyConfigurator: Setting Collection Property [" + methInfo.Name + "] to object [" + createdObject + "]");

                            try {
                                // Pass to the property
                                methInfo.Invoke(target, BindingFlags.InvokeMethod, null, new[] { createdObject }, CultureInfo.InvariantCulture);
                            }
                            catch (TargetInvocationException targetInvocationEx) {
                                LogLog.Error("XmlHierarchyConfigurator: Failed to set parameter [" + methInfo.Name + "] on object [" + target + "] using value [" + createdObject + "]", targetInvocationEx.InnerException);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test if an element has no attributes or child elements
        /// </summary>
        /// <param name="element">the element to inspect</param>
        /// <returns><c>true</c> if the element has any attributes or child elements, <c>false</c> otherwise</returns>
        private static bool HasAttributesOrElements(XmlElement element) {
            return element.ChildNodes.Cast<XmlNode>().Any(node => node.NodeType == XmlNodeType.Attribute || node.NodeType == XmlNodeType.Element);
        }

        /// <summary>
        /// Test if a <see cref="Type"/> is constructible with <c>Activator.CreateInstance</c>.
        /// </summary>
        /// <param name="type">the type to inspect</param>
        /// <returns><c>true</c> if the type is creatable using a default constructor, <c>false</c> otherwise</returns>
        private static bool IsTypeConstructible(Type type) {
            if (type.IsClass && !type.IsAbstract) {
                var defaultConstructor = type.GetConstructor(new Type[0]);
                if (defaultConstructor != null && !defaultConstructor.IsAbstract && !defaultConstructor.IsPrivate) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Look for a method on the <paramref name="targetType"/> that matches the <paramref name="name"/> supplied
        /// </summary>
        /// <param name="targetType">the type that has the method</param>
        /// <param name="name">the name of the method</param>
        /// <returns>the method info found</returns>
        /// <remarks>
        /// <para>
        /// The method must be a public instance method on the <paramref name="targetType"/>.
        /// The method must be named <paramref name="name"/> or "Add" followed by <paramref name="name"/>.
        /// The method must take a single parameter.
        /// </para>
        /// </remarks>
        private static MethodInfo FindMethodInfo(Type targetType, string name) {
            var requiredMethodNameA = name;
            var requiredMethodNameB = "Add" + name;

            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var methInfo in methods) {
                if (!methInfo.IsStatic) {
                    if (string.Compare(methInfo.Name, requiredMethodNameA, true, CultureInfo.InvariantCulture) == 0 ||
                        string.Compare(methInfo.Name, requiredMethodNameB, true, CultureInfo.InvariantCulture) == 0) {
                        // Found matching method name

                        // Look for version with one arg only
                        var methParams = methInfo.GetParameters();
                        if (methParams.Length == 1) {
                            return methInfo;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Converts a string value to a target type.
        /// </summary>
        /// <param name="type">The type of object to convert the string to.</param>
        /// <param name="value">The string value to use as the value of the object.</param>
        /// <returns>
        /// <para>
        /// An object of type <paramref name="type"/> with value <paramref name="value"/> or 
        /// <c>null</c> when the conversion could not be performed.
        /// </para>
        /// </returns>
        protected object ConvertStringTo(Type type, string value) {
            // Hack to allow use of Level in property
            if (typeof(Level) == type) {
                // Property wants a level
                var levelValue = _hierarchy.LevelMap[value];

                if (levelValue == null) {
                    LogLog.Error("XmlHierarchyConfigurator: Unknown Level Specified [" + value + "]");
                }

                return levelValue;
            }
            return OptionConverter.ConvertStringTo(type, value);
        }

        /// <summary>
        /// Creates an object as specified in XML.
        /// </summary>
        /// <param name="element">The XML element that contains the definition of the object.</param>
        /// <param name="defaultTargetType">The object type to use if not explicitly specified.</param>
        /// <param name="typeConstraint">The type that the returned object must be or must inherit from.</param>
        /// <returns>The object or <c>null</c></returns>
        /// <remarks>
        /// <para>
        /// Parse an XML element and create an object instance based on the configuration
        /// data.
        /// </para>
        /// <para>
        /// The type of the instance may be specified in the XML. If not
        /// specified then the <paramref name="defaultTargetType"/> is used
        /// as the type. However the type is specified it must support the
        /// <paramref name="typeConstraint"/> type.
        /// </para>
        /// </remarks>
        protected object CreateObjectFromXml(XmlElement element, Type defaultTargetType, Type typeConstraint) {
            Type objectType;

            // Get the object type
            var objectTypeString = element.GetAttribute(TypeAttr);
            if (String.IsNullOrEmpty(objectTypeString)) {
                if (defaultTargetType == null) {
                    LogLog.Error("XmlHierarchyConfigurator: Object type not specified. Cannot create object of type [" + typeConstraint.FullName + "]. Missing Value or Type.");
                    return null;
                }
                // Use the default object type
                objectType = defaultTargetType;
            }
            else {
                // Read the explicit object type
                try {
                    objectType = SystemInfo.GetTypeFromString(objectTypeString, true, true);
                }
                catch (Exception ex) {
                    LogLog.Error("XmlHierarchyConfigurator: Failed to find type [" + objectTypeString + "]", ex);
                    return null;
                }
            }

            var requiresConversion = false;

            // Got the object type. Check that it meets the typeConstraint
            if (typeConstraint != null) {
                if (!typeConstraint.IsAssignableFrom(objectType)) {
                    // Check if there is an appropriate type converter
                    if (OptionConverter.CanConvertTypeTo(objectType, typeConstraint)) {
                        requiresConversion = true;
                    }
                    else {
                        LogLog.Error("XmlHierarchyConfigurator: Object type [" + objectType.FullName + "] is not assignable to type [" + typeConstraint.FullName + "]. There are no acceptable type conversions.");
                        return null;
                    }
                }
            }

            // Create using the default constructor
            object createdObject = null;
            try {
                createdObject = Activator.CreateInstance(objectType);
            }
            catch (Exception createInstanceEx) {
                LogLog.Error("XmlHierarchyConfigurator: Failed to construct object of type [" + objectType.FullName + "] Exception: " + createInstanceEx);
            }

            // Set any params on object
            foreach (var currentNode in element.ChildNodes.Cast<XmlNode>().Where(currentNode => currentNode.NodeType == XmlNodeType.Element)) {
                SetParameter((XmlElement)currentNode, createdObject);
            }

            // Check if we need to call ActivateOptions
            var optionHandler = createdObject as IOptionHandler;
            if (optionHandler != null) {
                optionHandler.ActivateOptions();
            }

            // Ok object should be initialized

            return requiresConversion ? OptionConverter.ConvertTypeTo(createdObject, typeConstraint) : createdObject;
        }
    }
}
