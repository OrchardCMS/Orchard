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
    [NUnit.Framework.DescriptionAttribute("Boolean Field")]
    public partial class BooleanFieldFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Boolean.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Boolean Field", "    In order to add boolean content to my types\r\n    As an administrator\r\n    I w" +
                    "ant to create, edit and publish boolean fields", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Creating and using Boolean fields")]
        public virtual void CreatingAndUsingBooleanFields()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating and using Boolean fields", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 9
    testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
        testRunner.And("I have installed \"Orchard.Fields\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
    testRunner.When("I go to \"Admin/ContentTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 12
    testRunner.Then("I should see \"<a[^>]*>.*?Create new type</a>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 13
    testRunner.When("I go to \"Admin/ContentTypes/Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table1.AddRow(new string[] {
                        "DisplayName",
                        "Event"});
            table1.AddRow(new string[] {
                        "Name",
                        "Event"});
#line 14
        testRunner.And("I fill in", ((string)(null)), table1, "And ");
#line 18
        testRunner.And("I hit \"Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
        testRunner.And("I go to \"Admin/ContentTypes/\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
    testRunner.Then("I should see \"Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 23
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 24
        testRunner.And("I follow \"Add Field\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table2.AddRow(new string[] {
                        "DisplayName",
                        "Active"});
            table2.AddRow(new string[] {
                        "Name",
                        "Active"});
            table2.AddRow(new string[] {
                        "FieldTypeName",
                        "BooleanField"});
#line 25
        testRunner.And("I fill in", ((string)(null)), table2, "And ");
#line 30
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
    testRunner.Then("I should see \"The \\\"Active\\\" field has been added.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 35
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 36
    testRunner.Then("I should see \"Active\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table3.AddRow(new string[] {
                        "Event.Active.Value",
                        "true"});
#line 37
    testRunner.When("I fill in", ((string)(null)), table3, "When ");
#line 40
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
    testRunner.Then("I should see \"Your Event has been created.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 43
    testRunner.When("I go to \"Admin/Contents/List\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 44
    testRunner.Then("I should see \"Active:\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 45
        testRunner.And("I should see \"Yes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table4.AddRow(new string[] {
                        "Fields[0].BooleanFieldSettings.Hint",
                        "Check if the event is active"});
#line 49
        testRunner.And("I fill in", ((string)(null)), table4, "And ");
#line 52
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
    testRunner.Then("I should see \"Check if the event is active\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 57
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table5.AddRow(new string[] {
                        "Fields[0].BooleanFieldSettings.DefaultValue",
                        "True"});
#line 58
        testRunner.And("I fill in", ((string)(null)), table5, "And ");
#line 61
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 62
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 63
    testRunner.Then("I should see \"checked=\\\"checked\\\"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 66
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table6.AddRow(new string[] {
                        "Fields[0].BooleanFieldSettings.Optional",
                        "false"});
#line 67
        testRunner.And("I fill in", ((string)(null)), table6, "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table7.AddRow(new string[] {
                        "Fields[0].BooleanFieldSettings.NotSetLabel",
                        "May be"});
#line 70
        testRunner.And("I fill in", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table8.AddRow(new string[] {
                        "Fields[0].BooleanFieldSettings.SelectionMode",
                        "Radiobutton"});
#line 73
        testRunner.And("I fill in", ((string)(null)), table8, "And ");
#line 76
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table9.AddRow(new string[] {
                        "Event.Active.Value",
                        ""});
#line 78
        testRunner.And("I fill in", ((string)(null)), table9, "And ");
#line 81
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 82
    testRunner.Then("I should see \"The field Active is mandatory.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
