Feature: Web Hosting
  In order to test orchard
  As an integration runner
  I want to verify basic hosting is working

Scenario: Returning static files
  Given I have a clean site based on Simple.Web
  When I go to "Content/Static.txt"  
  Then I should see "Hello world!"
    And the status should be 200 OK

Scenario: Returning static files 2
  Given I have a clean site based on Simple.Web
  When I go to "Content\Static.txt"  
  Then the status should be 200 OK
    And I should see "Hello world!"

Scenario: Returning static files 3
  Given I have a clean site based on Simple.Web
  When I go to "/Static.txt"  
  Then the status should be 200 OK
    And I should see "Hello world!"

Scenario: Returning static files 4
  Given I have a clean site based on Simple.Web
  When I go to "Static.txt"  
  Then the status should be 200 OK
    And I should see "Hello world!"

Scenario: Returning web forms page
  Given I have a clean site based on Simple.Web
  When I go to "Simple/Page.aspx"  
  Then I should see "Hello again"
    And the status should be 200 OK

Scenario: Returning web forms page 2
  Given I have a clean site based on Simple.Web
  When I go to "Simple\Page.aspx"  
  Then the status should be 200 OK
    And I should see "Hello again"

Scenario: Returning a routed request
  Given I have a clean site based on Simple.Web
  When I go to "hello-world"  
  Then the status should be 200 OK
    And I should see "Hello yet again"

Scenario: Following a link
  Given I have a clean site based on Simple.Web
  When I go to "/simple/page.aspx"  
    And I follow "next page"
  Then the status should be 200 OK
    And I should see "Hello yet again"
