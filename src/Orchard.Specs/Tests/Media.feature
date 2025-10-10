Feature: Media management
    In order to reference images and such from content
    As an author
    I want to access the Media Library

Scenario: Media admin is available
    Given I have installed Orchard
    And I have installed "Orchard.MediaLibrary"

    # Accessing the Media Library page
    When I go to "Admin/Orchard.MediaLibrary"
    Then I should see "Media Library"
    And the status should be 200 "OK"
