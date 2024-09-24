<a href="/com.playeveryware.eos/README.md"><img src="/com.playeveryware.eos/Documentation~/images/PlayEveryWareLogo.gif" alt="README.md" width="5%"/></a>

# UIFriendsMenu.cs

Some scenes in the provided samples have the `friendsTabUI.prefab` included in them. This tab demonstrates the ability to query friends, send social invitations, and display social statuses.

# QueryFriends Consent Requirement

When friends are queried outside of the Epic Social Overlay, only friends who have consented to the application will appear. This could mean that while testing a game with this plugin included, either none of or a subset of your friends may appear in the UI. See [Epic's documentation on Friends List data privacy](https://dev.epicgames.com/docs/epic-account-services/eos-data-privacy-visibility#friends-list) and [Epic's documentation on Authorization and Consent Management](https://dev.epicgames.com/docs/epic-account-services/consent-management) for more information on this restriction.