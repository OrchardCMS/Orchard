﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.0.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Orchard.Specs
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Web Hosting")]
    public partial class WebHostingFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "WebHosting.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Web Hosting", "    In order to test orchard\r\n    As an integration runner\r\n    I want to verify " +
                    "basic hosting is working", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Returning static files")]
        public virtual void ReturningStaticFiles()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Returning static files", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
    testRunner.When("I go to \"Content/Static.txt\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
    testRunner.Then("I should see \"Hello world!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 10
        testRunner.And("the status should be 200 \"OK\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Returning web forms page")]
        public virtual void ReturningWebFormsPage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Returning web forms page", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 14
    testRunner.When("I go to \"Simple/Page.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
    testRunner.Then("I should see \"Hello again\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 16
        testRunner.And("the status should be 200 \"OK\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Returning a routed request")]
        public virtual void ReturningARoutedRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Returning a routed request", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 20
    testRunner.When("I go to \"hello-world\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
    testRunner.Then("the status should be 200 \"OK\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 22
        testRunner.And("I should see \"Hello yet again\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Following a link")]
        public virtual void FollowingALink()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Following a link", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
    testRunner.When("I go to \"/simple/page.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
        testRunner.And("I follow \"next page\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
    testRunner.Then("the status should be 200 \"OK\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 29
        testRunner.And("I should see \"Hello yet again\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Submitting a form with input, default, and hidden fields")]
        public virtual void SubmittingAFormWithInputDefaultAndHiddenFields()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Submitting a form with input, default, and hidden fields", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
        testRunner.And("I am on \"/simple/page.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table1.AddRow(new string[] {
                        "input1",
                        "gamma"});
#line 34
    testRunner.When("I fill in", ((string)(null)), table1, "When ");
#line 37
        testRunner.And("I hit \"Go!\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
    testRunner.Then("I should see \"passthrough1:alpha\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 39
        testRunner.And("I should see \"passthrough2:beta\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 40
        testRunner.And("I should see \"input1:gamma\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cookies follow along your request")]
        public virtual void CookiesFollowAlongYourRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cookies follow along your request", ((string[])(null)));
#line 42
this.ScenarioSetup(scenarioInfo);
#line 43
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 44
    testRunner.When("I go to \"/simple/cookie-set.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 45
        testRunner.And("I go to \"/simple/cookie-show.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
    testRunner.Then("I should see \"foo:bar\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Being redirected")]
        public virtual void BeingRedirected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Being redirected", ((string[])(null)));
#line 48
this.ScenarioSetup(scenarioInfo);
#line 49
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 50
    testRunner.When("I go to \"/simple/redir.aspx\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 51
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
    testRunner.Then("I should see \"Hello again\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Not found modules file")]
        public virtual void NotFoundModulesFile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not found modules file", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 55
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 56
    testRunner.When("I go to \"/Modules/Orchard.Blogs/module.txt\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 57
    testRunner.Then("the status should be 404 \"Not Found\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Not found themes file")]
        public virtual void NotFoundThemesFile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not found themes file", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 60
    testRunner.Given("I have a clean site based on Simple.Web", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 61
    testRunner.When("I go to \"/Themes/Classic/theme.txt\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 62
    testRunner.Then("the status should be 404 \"Not Found\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
