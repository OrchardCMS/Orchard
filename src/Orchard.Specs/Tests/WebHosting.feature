Feature: Web Hosting
    In order to test orchard
    As an integration runner
    I want to verify basic hosting is working

Scenario: Returning static files
    Given I have a clean site based on Simple.Web
    When I go to "Content/Static.txt"  
    Then I should see "Hello world!"
        And the status should be 200 "OK"

Scenario: Returning web forms page
    Given I have a clean site based on Simple.Web
    When I go to "Simple/Page.aspx"  
    Then I should see "Hello again"
        And the status should be 200 "OK"

Scenario: Returning a routed request
    Given I have a clean site based on Simple.Web
    When I go to "hello-world"  
    Then the status should be 200 "OK"
        And I should see "Hello yet again"

Scenario: Following a link
    Given I have a clean site based on Simple.Web
    When I go to "/simple/page.aspx"  
        And I follow "next page"
    Then the status should be 200 "OK"
        And I should see "Hello yet again"

Scenario: Submitting a form with input, default, and hidden fields
    Given I have a clean site based on Simple.Web
        And I am on "/simple/page.aspx"  
    When I fill in 
        | name | value |
        | input1 | gamma |
        And I hit "Go!"
    Then I should see "passthrough1:alpha"
        And I should see "passthrough2:beta"
        And I should see "input1:gamma"

Scenario: Cookies follow along your request
    Given I have a clean site based on Simple.Web
    When I go to "/simple/cookie-set.aspx"  
        And I go to "/simple/cookie-show.aspx"  
    Then I should see "foo:bar"

Scenario: Being redirected
    Given I have a clean site based on Simple.Web
    When I go to "/simple/redir.aspx"
        And I am redirected
    Then I should see "Hello again"

Scenario: Not found modules file
    Given I have a clean site based on Simple.Web
    When I go to "/Modules/Orchard.Blogs/module.txt"
    Then the status should be 404 "Not Found"

Scenario: Not found themes file
    Given I have a clean site based on Simple.Web
    When I go to "/Themes/Classic/theme.txt"
    Then the status should be 404 "Not Found"