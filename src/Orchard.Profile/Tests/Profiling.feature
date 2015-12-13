Feature: Profiling
    In order to profile the site
    As a developer
    I want to generate a fixed number of repeatable requests

Scenario: Warmup
    Given I am logged in
    When I go to "/admin"
    When I go to "/blog0"
    When I go to "/"

Scenario: Dashboard
    Given I am logged in
    When I go to "/admin" 40 times

Scenario: Hitting blogs
    Given I am logged in
    When I go to "/blog0" 10 times
    When I go to "/blog1" 10 times
    When I go to "/blog2" 10 times
    When I go to "/blog3" 10 times
    When I go to "/blog4" 10 times
     
Scenario: Hitting home page
    //Given I am logged in
    When I go to "/" 10 times
    When I go to "/" 10 times
    When I go to "/" 10 times
    When I go to "/" 10 times
