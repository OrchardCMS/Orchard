Feature: Multiple tenant management
    In order to host several isolated web applications
    As a root Orchard system operator
    I want to create and manage tenant configurations

Scenario: Default site is listed 
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy"
    Then I should see "List of Site&#39;s Tenants"
        And I should see "<h3>\s*Default\s*</h3>"
        And the status should be 200 "OK"

Scenario: New tenant fields are required
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I hit "Save"
    Then I should see "is required"

Scenario: A new tenant is created
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlPrefix | scott |
        And I hit "Save"
        And I am redirected
    Then I should see "<h3>\s*Scott\s*</h3>"
        And the status should be 200 "OK"
        
Scenario: A new tenant is created with uninitialized state
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlPrefix | scott |
        And I hit "Save"
        And I am redirected
    Then I should see "<li class="tenant Uninitialized">"
        And the status should be 200 "OK"

Scenario: A new tenant goes to the setup screen
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
        And I hit "Save"
        And I go to "/Setup" on host scott.example.org
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And the status should be 200 "OK"

Scenario: Several tenants are configured and go to setup screen
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott1 |
            | RequestUrlHost | scott1.example.org |
        And I hit "Save"
        And I am redirected
        And I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott2 |
            | RequestUrlHost | scott2.example.org |
        And I hit "Save"
        And I am redirected
        And I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott3 |
            | RequestUrlHost | scott3.example.org |
        And I hit "Save"
        And I am redirected
        And I go to "/Setup" on host scott1.example.org
        And I go to "/Setup" on host scott2.example.org
        And I go to "/Setup" on host scott3.example.org
    Then I should see "Welcome to Orchard"

Scenario: A new tenant with preconfigured database goes to the setup screen
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
            | DataProvider | SqlCe |
        And I hit "Save"
        And I am redirected
        And I go to "/Setup" on host scott.example.org
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And I should not see "SQL Server Compact"
        And the status should be 200 "OK"

Scenario: A new tenant runs the setup
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
        And I hit "Save"
        And I go to "/Setup" on host scott.example.org
        And I fill in 
            | name | value |
            | SiteName | Scott Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
            And I go to "/"
    Then I should see "Scott Site"
        And I should see "Welcome"
        
Scenario: An existing initialized tenant cannot have its database option cleared
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
        And I hit "Save"
        And I go to "/Setup" on host scott.example.org
        And I fill in 
            | name | value |
            | SiteName | Scott Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
        And I go to "/Admin/MultiTenancy/Edit/Scott" on host localhost
    Then I should see "<h1 id="page-title">Edit Tenant</h1>"
        And I should see "<h2>Scott</h2>"
        And I should not see "Allow the tenant to set up the database"

Scenario: Default tenant cannot be disabled
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy"
    Then I should not see "<form action="/Admin/MultiTenancy/disable""

Scenario: A running tenant can be disabled
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
        And I hit "Save"
        And I go to "/Setup" on host scott.example.org
        And I fill in 
            | name | value |
            | SiteName | Scott Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
        And I go to "/Admin/MultiTenancy" on host localhost
        And I hit "Suspend"
        And I am redirected
    Then I should see "<form action="/Admin/MultiTenancy/Enable""

Scenario: A running tenant which is disabled can be enabled
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
    When I go to "Admin/MultiTenancy/Add"
        And I fill in 
            | name | value |
            | Name | Scott |
            | RequestUrlHost | scott.example.org |
        And I hit "Save"
        And I go to "/Setup" on host scott.example.org
        And I fill in 
            | name | value |
            | SiteName | Scott Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
        And I go to "/Admin/MultiTenancy" on host localhost
        And I hit "Suspend"
        And I am redirected
        And I hit "Resume"
        And I am redirected
    Then I should see "<form action="/Admin/MultiTenancy/Disable""

Scenario: Listing tenants from command line
    Given I have installed Orchard
        And I have installed "Orchard.MultiTenancy"
        And I have tenant "Alpha" on "example.org" as "New-site-name"
    When I execute >tenant list
    Then I should see "Name: Alpha"
        And I should see "Request URL host: example.org"
