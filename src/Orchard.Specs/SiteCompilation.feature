Feature: The compilation of modules installed in a site
    In order to install on Orchard site
    As a privileged user
    I want to have the modules compiled/installed properly

Scenario: Dynamic compilation can be disabled
    Given I have chosen to load modules with dynamic compilation disabled
        And I have installed Orchard
    When I go to "admin"
    Then I should see "<div id="orchard-version">Orchard v(?:\.\d+){2,4}</div>"

Scenario: Dynamic compilation will kick in if modules are deployed as source files only
    Given I have chosen to deploy modules as source files only
        And I have installed Orchard
    When I go to "admin"
    Then I should see "<div id="orchard-version">Orchard v(?:\.\d+){2,4}</div>"

Scenario: Dynamic compilation can be forced by disabling the precompiled module loader
    Given I have chosen to load modules using dymamic compilation only
        And I have installed Orchard
    When I go to "admin"
    Then I should see "<div id="orchard-version">Orchard v(?:\.\d+){2,4}</div>"
