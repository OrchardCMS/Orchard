﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("DateTime Field")]
    public partial class DateTimeFieldFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "DateTime.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DateTime Field", "  In order to add Date content to my types\r\n  As an administrator\r\n  I want to cr" +
                    "eate, edit and publish DateTime fields", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Creating and using Date fields")]
        public virtual void CreatingAndUsingDateFields()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating and using Date fields", ((string[])(null)));
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
                        "Date of the event"});
            table2.AddRow(new string[] {
                        "Name",
                        "EventDate"});
            table2.AddRow(new string[] {
                        "FieldTypeName",
                        "DateTimeField"});
#line 25
        testRunner.And("I fill in", ((string)(null)), table2, "And ");
#line 30
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
    testRunner.Then("I should see \"The \\\"Date of the event\\\" field has been added.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 35
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 36
    testRunner.Then("I should see \"Date of the event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table3.AddRow(new string[] {
                        "Event.EventDate.Editor.Date",
                        "31/01/2012"});
            table3.AddRow(new string[] {
                        "Event.EventDate.Editor.Time",
                        "12:00 AM"});
#line 37
    testRunner.When("I fill in", ((string)(null)), table3, "When ");
#line 41
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
    testRunner.Then("I should see \"Date of the event could not be parsed as a valid date and time.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 45
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 46
    testRunner.Then("I should see \"Date of the event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table4.AddRow(new string[] {
                        "Event.EventDate.Editor.Date",
                        "01/31/2012"});
            table4.AddRow(new string[] {
                        "Event.EventDate.Editor.Time",
                        "12:00 AM"});
#line 47
    testRunner.When("I fill in", ((string)(null)), table4, "When ");
#line 51
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
    testRunner.Then("I should see \"The Event has been created as a draft.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 54
    testRunner.When("I go to \"Admin/Contents/List\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 55
    testRunner.Then("I should see \"Date of the event:\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 56
        testRunner.And("I should see \"1/31/2012 12:00\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 59
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table5.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Hint",
                        "Enter the date of the event"});
#line 60
        testRunner.And("I fill in", ((string)(null)), table5, "And ");
#line 63
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 64
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 65
    testRunner.Then("I should see \"Enter the date of the event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 68
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table6.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "DateOnly"});
#line 69
        testRunner.And("I fill in", ((string)(null)), table6, "And ");
#line 72
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 73
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 74
    testRunner.Then("I should see \"Event.EventDate.Editor.Date\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 75
        testRunner.And("I should not see \"Event.EventDate.Editor.Time\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 78
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table7.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "TimeOnly"});
#line 79
        testRunner.And("I fill in", ((string)(null)), table7, "And ");
#line 82
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 83
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 84
    testRunner.Then("I should see \"Event.EventDate.Editor.Time\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 85
        testRunner.And("I should not see \"Event.EventDate.Editor.Date\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table8.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "DateAndTime"});
            table8.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Required",
                        "true"});
#line 89
        testRunner.And("I fill in", ((string)(null)), table8, "And ");
#line 93
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 94
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table9.AddRow(new string[] {
                        "Event.EventDate.Editor.Date",
                        "01/31/2012"});
#line 95
        testRunner.And("I fill in", ((string)(null)), table9, "And ");
#line 98
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 99
    testRunner.Then("I should see \"Date of the event is required.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 100
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table10.AddRow(new string[] {
                        "Event.EventDate.Editor.Time",
                        "12:00 AM"});
#line 101
     testRunner.And("I fill in", ((string)(null)), table10, "And ");
#line 104
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 105
    testRunner.Then("I should see \"Date of the event is required.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 108
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table11.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "DateOnly"});
            table11.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Required",
                        "true"});
#line 109
        testRunner.And("I fill in", ((string)(null)), table11, "And ");
#line 113
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 115
    testRunner.Then("I should see \"Event.EventDate.Editor.Date\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 116
    testRunner.When("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 117
    testRunner.Then("I should see \"Date of the event is required.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 120
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table12.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "TimeOnly"});
            table12.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Required",
                        "true"});
#line 121
        testRunner.And("I fill in", ((string)(null)), table12, "And ");
#line 125
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 126
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 127
    testRunner.Then("I should see \"Event.EventDate.Editor.Date\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 128
    testRunner.When("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 129
    testRunner.Then("I should see \"Date of the event is required.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 132
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table13.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "DateAndTime"});
            table13.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Editor.Date",
                        "01/31/2016"});
            table13.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Editor.Time",
                        "10:00 AM"});
#line 133
        testRunner.And("I fill in", ((string)(null)), table13, "And ");
#line 138
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 139
        testRunner.And("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 140
    testRunner.Then("I should see \"Event.EventDate.Editor.Date\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 141
    testRunner.And("I should see \"Event.EventDate.Editor.Time\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 142
    testRunner.When("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 143
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 144
    testRunner.Then("I should see \"The Event has been created.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 145
    testRunner.When("I go to \"Admin/Contents/List\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 146
    testRunner.Then("I should see \"Date of the event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 147
        testRunner.And("I should see \"1/31/2016 10:00\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Creating and using date time fields in another culture")]
        public virtual void CreatingAndUsingDateTimeFieldsInAnotherCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating and using date time fields in another culture", ((string[])(null)));
#line 149
this.ScenarioSetup(scenarioInfo);
#line 152
    testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 153
        testRunner.And("I have installed \"Orchard.Fields\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 154
        testRunner.And("I have the file \"Content\\orchard.core.po\" in \"Core\\App_Data\\Localization\\fr-FR\\or" +
                    "chard.core.po\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 155
    testRunner.When("I go to \"Admin/ContentTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 156
    testRunner.Then("I should see \"<a[^>]*>.*?Create new type</a>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 157
    testRunner.When("I go to \"Admin/ContentTypes/Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table14.AddRow(new string[] {
                        "DisplayName",
                        "Event"});
            table14.AddRow(new string[] {
                        "Name",
                        "Event"});
#line 158
        testRunner.And("I fill in", ((string)(null)), table14, "And ");
#line 162
        testRunner.And("I hit \"Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 163
        testRunner.And("I go to \"Admin/ContentTypes/\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 164
    testRunner.Then("I should see \"Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 167
    testRunner.When("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 168
        testRunner.And("I follow \"Add Field\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table15.AddRow(new string[] {
                        "DisplayName",
                        "Date of the event"});
            table15.AddRow(new string[] {
                        "Name",
                        "EventDate"});
            table15.AddRow(new string[] {
                        "FieldTypeName",
                        "DateTimeField"});
#line 169
        testRunner.And("I fill in", ((string)(null)), table15, "And ");
#line 174
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 175
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 176
    testRunner.Then("I should see \"The \\\"Date of the event\\\" field has been added.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 179
    testRunner.When("I have \"fr-FR\" as the default culture", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 180
        testRunner.And("I go to \"Admin/ContentTypes/Edit/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table16.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Display",
                        "DateAndTime"});
            table16.AddRow(new string[] {
                        "Fields[EventDate].DateTimeFieldSettings.Required",
                        "true"});
#line 181
        testRunner.And("I fill in", ((string)(null)), table16, "And ");
#line 185
        testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 186
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table17.AddRow(new string[] {
                        "Event.EventDate.Editor.Date",
                        "01/31/2012"});
            table17.AddRow(new string[] {
                        "Event.EventDate.Editor.Time",
                        "12:00 AM"});
#line 187
        testRunner.And("I fill in", ((string)(null)), table17, "And ");
#line 191
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 192
    testRunner.Then("I should see \"Date of the event could not be parsed as a valid date and time\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 193
    testRunner.When("I go to \"Admin/Contents/Create/Event\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table18.AddRow(new string[] {
                        "Event.EventDate.Editor.Date",
                        "31/01/2012"});
            table18.AddRow(new string[] {
                        "Event.EventDate.Editor.Time",
                        "18:00"});
#line 194
        testRunner.And("I fill in", ((string)(null)), table18, "And ");
#line 198
        testRunner.And("I hit \"Save Draft\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 199
        testRunner.And("I am redirected", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 200
    testRunner.Then("I should see \"The Event has been created as a draft.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
