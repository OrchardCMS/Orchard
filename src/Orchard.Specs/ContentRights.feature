Feature: Content rights management
    In order to ensure security
    As a root Orchard system operator
    I want only the allowed users to edit the content

Scenario: Administrators can manage a Page
    Given I have installed Orchard
	When I have a user "user1" with roles "Administrator"
    Then "user1" should be able to "publish" a "Page" owned by "user1"
		And "user1" should be able to "edit" a "Page" owned by "user1"

Scenario: Users can't create a Page if they don't have the PublishContent permission
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "EditContent, DeleteContent"
		And I have a user "user1" with roles "CustomRole"
    Then "user1" should not be able to "publish" a "Page" owned by "user1"
		And "user1" should be able to "edit" a "Page" owned by "user1"
		And "user1" should be able to "delete" a "Page" owned by "user1"

Scenario: Users can't edit a Page if they don't have the EditContent permission
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "DeleteContent"
		And I have a user "user1" with roles "CustomRole"
    Then "user1" should not be able to "publish" a "Page" owned by "user1"
		And "user1" should not be able to "edit" a "Page" owned by "user1"
		And "user1" should be able to "delete" a "Page" owned by "user1"

Scenario: Users can create and edit a Page even if they only have the PublishContent permission
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "PublishContent"
		And I have a user "user1" with roles "CustomRole"
    Then "user1" should be able to "publish" a "Page" owned by "user1"
		And "user1" should be able to "edit" a "Page" owned by "user1"
		And "user1" should not be able to "delete" a "Page" owned by "user1"

Scenario: Users can create a Page if they have PublishContent for Page
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "Publish_Page"
		And I have a user "user1" with roles "CustomRole"
    Then "user1" should be able to "publish" a "Page" owned by "user1"
		And "user1" should be able to "edit" a "Page" owned by "user1"
		And "user1" should not be able to "delete" a "Page" owned by "user1"

Scenario: Users can't create a Page for others if they only have PublishContent
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "PublishContent"
		And I have a user "user1" with roles "CustomRole"
		And I have a user "user2" with roles "Administrator"
    Then "user1" should not be able to "publish" a "Page" owned by "user2"
		And "user1" should not be able to "edit" a "Page" owned by "user2"
		And "user1" should not be able to "delete" a "Page" owned by "user2"

Scenario: Users can create a Page for others if they have PublishOthersContent
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "PublishOthersContent"
		And I have a user "user1" with roles "CustomRole"
		And I have a user "user2" with roles "Administrator"
    Then "user1" should be able to "publish" a "Page" owned by "user2"
		And "user1" should be able to "edit" a "Page" owned by "user2"
		And "user1" should not be able to "delete" a "Page" owned by "user2"


Scenario: Users can't create a Page for others if they only have Publish_Page
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "Publish_Page"
		And I have a user "user1" with roles "CustomRole"
		And I have a user "user2" with roles "Administrator"
    Then "user1" should be able to "publish" a "Page" owned by "user2"
		And "user1" should be able to "edit" a "Page" owned by "user2"
		And "user1" should not be able to "delete" a "Page" owned by "user2"

Scenario: Users can create a Page for others if they only have PublishOthers_Page
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "PublishOthers_Page"
		And I have a user "user1" with roles "CustomRole"
		And I have a user "user2" with roles "Administrator"
    Then "user1" should be able to "publish" a "Page" owned by "user2"
		And "user1" should be able to "edit" a "Page" owned by "user2"
		And "user1" should not be able to "delete" a "Page" owned by "user2"

Scenario: Users can delete a Page for others if they only have DeleteOthers_Page
    Given I have installed Orchard
	When I have a role "CustomRole" with permissions "DeleteOthers_Page"
		And I have a user "user1" with roles "CustomRole"
		And I have a user "user2" with roles "Administrator"
    Then "user1" should not be able to "publish" a "Page" owned by "user2"
		And "user1" should not be able to "edit" a "Page" owned by "user2"
		And "user1" should be able to "delete" a "Page" owned by "user2"
