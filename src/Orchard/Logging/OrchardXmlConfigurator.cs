using System;
using System.IO;
using System.Reflection;
using System.Xml;

using log4net;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace Orchard.Logging {
    public class OrchardXmlConfigurator {

        /// <summary>
        /// Private constructor
        /// </summary>
        private OrchardXmlConfigurator() {
        }

        /// <summary>
        /// Configures log4net using the specified configuration file.
        /// </summary>
        /// <param name="configFilename">The name of the XML file to load the configuration from.</param>
        /// <remarks>
        /// <para>
        /// The configuration file must be valid XML. It must contain
        /// at least one element called <c>log4net</c> that holds
        /// the log4net configuration data.
        /// </para>
        /// <para>
        /// The log4net configuration file can possible be specified in the application's
        /// configuration file (either <c>MyAppName.exe.config</c> for a
        /// normal application on <c>Web.config</c> for an ASP.NET application).
        /// </para>
        /// <para>
        /// The first element matching <c>&lt;configuration&gt;</c> will be read as the 
        /// configuration. If this file is also a .NET .config file then you must specify 
        /// a configuration section for the <c>log4net</c> element otherwise .NET will 
        /// complain. Set the type for the section handler to <see cref="System.Configuration.IgnoreSectionHandler"/>, for example:
        /// <code lang="XML" escaped="true">
        /// <configSections>
        ///		<section name="log4net" type="System.Configuration.IgnoreSectionHandler" />
        ///	</configSections>
        /// </code>
        /// </para>
        /// <example>
        /// The following example configures log4net using a configuration file, of which the 
        /// location is stored in the application's configuration file :
        /// </example>
        /// <code lang="C#">
        /// using log4net.Config;
        /// using System.IO;
        /// using System.Configuration;
        /// 
        /// ...
        /// 
        /// XmlConfigurator.Configure(ConfigurationSettings.AppSettings["log4net-config-file"]);
        /// </code>
        /// <para>
        /// In the <c>.config</c> file, the path to the log4net can be specified like this :
        /// </para>
        /// <code lang="XML" escaped="true">
        /// <configuration>
        ///		<appSettings>
        ///			<add key="log4net-config-file" value="log.config"/>
        ///		</appSettings>
        ///	</configuration>
        /// </code>
        /// </remarks>
        public static void Configure(string configFilename) {
            Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFilename);
        }

        /// <summary>
        /// Configures log4net using the specified configuration data stream.
        /// </summary>
        /// <param name="configStream">A stream to load the XML configuration from.</param>
        /// <remarks>
        /// <para>
        /// The configuration data must be valid XML. It must contain
        /// at least one element called <c>log4net</c> that holds
        /// the log4net configuration data.
        /// </para>
        /// <para>
        /// Note that this method will NOT close the stream parameter.
        /// </para>
        /// </remarks>
        public static void Configure(Stream configStream) {
            Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configStream);
        }

        /// <summary>
        /// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
        /// file.
        /// </summary>
        /// <param name="repository">The repository to configure.</param>
        /// <param name="configFilename">The name of the XML file to load the configuration from.</param>
        /// <remarks>
        /// <para>
        /// The configuration file must be valid XML. It must contain
        /// at least one element called <c>log4net</c> that holds
        /// the configuration data.
        /// </para>
        /// <para>
        /// The log4net configuration file can possible be specified in the application's
        /// configuration file (either <c>MyAppName.exe.config</c> for a
        /// normal application on <c>Web.config</c> for an ASP.NET application).
        /// </para>
        /// <para>
        /// The first element matching <c>&lt;configuration&gt;</c> will be read as the 
        /// configuration. If this file is also a .NET .config file then you must specify 
        /// a configuration section for the <c>log4net</c> element otherwise .NET will 
        /// complain. Set the type for the section handler to <see cref="System.Configuration.IgnoreSectionHandler"/>, for example:
        /// <code lang="XML" escaped="true">
        /// <configSections>
        ///		<section name="log4net" type="System.Configuration.IgnoreSectionHandler" />
        ///	</configSections>
        /// </code>
        /// </para>
        /// <example>
        /// The following example configures log4net using a configuration file, of which the 
        /// location is stored in the application's configuration file :
        /// </example>
        /// <code lang="C#">
        /// using log4net.Config;
        /// using System.IO;
        /// using System.Configuration;
        /// 
        /// ...
        /// 
        /// XmlConfigurator.Configure(ConfigurationSettings.AppSettings["log4net-config-file"]);
        /// </code>
        /// <para>
        /// In the <c>.config</c> file, the path to the log4net can be specified like this :
        /// </para>
        /// <code lang="XML" escaped="true">
        /// <configuration>
        ///		<appSettings>
        ///			<add key="log4net-config-file" value="log.config"/>
        ///		</appSettings>
        ///	</configuration>
        /// </code>
        /// </remarks>
        public static void Configure(ILoggerRepository repository, string configFilename) {
            LogLog.Debug("XmlConfigurator: configuring repository [" + repository.Name + "] using file [" + configFilename + "]");

            if (String.IsNullOrWhiteSpace(configFilename)) {
                LogLog.Error("XmlConfigurator: Configure called with null 'configFilename' parameter");
            }
            else {
                // Have to use File.Exists() rather than configFile.Exists()
                // because configFile.Exists() caches the value, not what we want.
                if (File.Exists(configFilename)) {
                    // Open the file for reading
                    FileStream fs = null;

                    // Try hard to open the file
                    for (var retry = 5; --retry >= 0; ) {
                        try {
                            fs = new FileStream(configFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                            break;
                        }
                        catch (IOException ex) {
                            if (retry == 0) {
                                LogLog.Error("XmlConfigurator: Failed to open XML config file [" + configFilename + "]", ex);

                                // The stream cannot be valid
                                fs = null;
                            }

                            System.Threading.Thread.Sleep(250);
                        }
                    }

                    if (fs != null) {
                        try {
                            // Load the configuration from the stream
                            Configure(repository, fs);
                        }
                        finally {
                            // Force the file closed whatever happens
                            fs.Close();
                        }
                    }
                }
                else {
                    LogLog.Debug("XmlConfigurator: config file [" + configFilename + "] not found. Configuration unchanged.");
                }
            }
        }

        /// <summary>
        /// Configures the <see cref="ILoggerRepository"/> using the specified configuration 
        /// file.
        /// </summary>
        /// <param name="repository">The repository to configure.</param>
        /// <param name="configStream">The stream to load the XML configuration from.</param>
        /// <remarks>
        /// <para>
        /// The configuration data must be valid XML. It must contain
        /// at least one element called <c>log4net</c> that holds
        /// the configuration data.
        /// </para>
        /// <para>
        /// Note that this method will NOT close the stream parameter.
        /// </para>
        /// </remarks>
        public static void Configure(ILoggerRepository repository, Stream configStream) {
            LogLog.Debug("XmlConfigurator: configuring repository [" + repository.Name + "] using stream");

            if (configStream == null) {
                LogLog.Error("XmlConfigurator: Configure called with null 'configStream' parameter");
            }
            else {
                // Load the config file into a document
                var doc = new XmlDocument();
                try {
                    // Create a text reader for the file stream
                    var xmlReader = new XmlTextReader(configStream) { DtdProcessing = DtdProcessing.Parse };

                    // Specify that the reader should not perform validation
                    var settings = new XmlReaderSettings { ValidationType = ValidationType.None };

                    // load the data into the document
                    doc.Load(xmlReader);
                }
                catch (Exception ex) {
                    LogLog.Error("XmlConfigurator: Error while loading XML configuration", ex);

                    // The document is invalid
                    doc = null;
                }

                if (doc != null) {
                    LogLog.Debug("XmlConfigurator: loading XML configuration");

                    // Configure using the 'log4net' element
                    var configNodeList = doc.GetElementsByTagName("log4net");
                    if (configNodeList.Count == 0) {
                        LogLog.Debug("XmlConfigurator: XML configuration does not contain a <log4net> element. Configuration Aborted.");
                    }
                    else if (configNodeList.Count > 1) {
                        LogLog.Error("XmlConfigurator: XML configuration contains [" + configNodeList.Count + "] <log4net> elements. Only one is allowed. Configuration Aborted.");
                    }
                    else {
                        ConfigureFromXml(repository, configNodeList[0] as XmlElement);
                    }
                }
            }
        }

        /// <summary>
        /// Configures the specified repository using a <c>log4net</c> element.
        /// </summary>
        /// <param name="repository">The hierarchy to configure.</param>
        /// <param name="element">The element to parse.</param>
        /// <remarks>
        /// <para>
        /// Loads the log4net configuration from the XML element
        /// supplied as <paramref name="element"/>.
        /// </para>
        /// <para>
        /// This method is ultimately called by one of the Configure methods 
        /// to load the configuration from an <see cref="XmlElement"/>.
        /// </para>
        /// </remarks>
        private static void ConfigureFromXml(ILoggerRepository repository, XmlElement element) {
            if (element == null) {
                LogLog.Error("XmlConfigurator: ConfigureFromXml called with null 'element' parameter");
            }
            else if (repository == null) {
                LogLog.Error("XmlConfigurator: ConfigureFromXml called with null 'repository' parameter");
            }
            else {
                LogLog.Debug("XmlConfigurator: Configuring Repository [" + repository.Name + "]");

                //
                // Since we're not reinventing the whole Hierarchy class from log4net to add out optimizations to XmlRepositoryConfigurator
                // we've to check the neccessary casts here to be able to complete the configuration.
                //

                // Needed to fire configuration changed event
                var repositorySkeleton = repository as LoggerRepositorySkeleton;

                // Needed to XmlHierarchyConfigurator
                var hierarchy = repository as Hierarchy;

                if (repositorySkeleton == null || hierarchy == null) {
                    LogLog.Warn("XmlConfigurator: Repository [" + repository + "] does not support the XmlConfigurator");
                }
                else {
                    var configurator = new OrchardXmlHierarchyConfigurator(hierarchy);

                    // Copy the xml data into the root of a new document
                    // this isolates the xml config data from the rest of
                    // the document
                    var newDoc = new XmlDocument();
                    var newElement = (XmlElement)newDoc.AppendChild(newDoc.ImportNode(element, true));

                    // Pass the configurator the config element
                    configurator.Configure(newElement);

                    repositorySkeleton.RaiseConfigurationChanged(EventArgs.Empty);
                }
            }
        }
    }
}
