Feature: Media management
    In order to reference images and such from content
    As an author
    I want to upload and manage files in a media folder

Scenario: Media admin is available
    Given I have installed Orchard
        And I have installed "Orchard.Media"
    When I go to "admin/media"
    Then I should see "Manage Media Folders"
        And the status should be 200 "OK"

Scenario: Creating a folder
    Given I have installed Orchard
        And I have installed "Orchard.Media"
    When I go to "admin/media/create"
        And I fill in
            | name | value |
            | Name | Hello World |
        And I hit "Save"		
        And I am redirected
    Then I should see "Manage Media Folders"
        And I should see "Hello World"
        And the status should be 200 "OK"
