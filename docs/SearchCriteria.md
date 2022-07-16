# SearchCriteria
The `SearchCriteria` type is used to define the parameters for searching deployment notifications. It provides a flexible way to filter and retrieve notifications based on various criteria such as project name, version, status, and more. This type is essential for implementing efficient and targeted searches in the `dotnet-deploy-notify` project.

## API
The `SearchCriteria` type has the following public members:
* `ProjectName`: A string representing the name of the project to search for. Can be null.
* `Version`: A string representing the version of the project to search for. Can be null.
* `Status`: A `BuildStatus` enum value representing the status of the deployment to search for. Can be null.
* `TargetEnvironment`: An `Environment` enum value representing the target environment of the deployment to search for. Can be null.
* `BranchName`: A string representing the name of the branch to search for. Can be null.
* `CommitAuthor`: A string representing the author of the commit to search for. Can be null.
* `CreatedAfter`: A `DateTime` representing the earliest creation time to search for. Can be null.
* `CreatedBefore`: A `DateTime` representing the latest creation time to search for. Can be null.
* `MinimumPriority`: A `NotificationPriority` enum value representing the minimum priority of the notifications to search for. Can be null.
* `Channels`: A list of `NotificationChannel` enum values representing the channels to search for. Can be null.
* `MessageContains`: A string representing the text to search for in the notification messages. Can be null.
* `Limit`: An integer representing the maximum number of results to return.
* `Offset`: An integer representing the offset from the start of the result set.
* `Items`: A list of type `T` representing the search results.
* `Total`: An integer representing the total number of results.
* `Returned`: An integer representing the number of results returned.
* `NotificationSearchEngine`: An instance of the search engine used to perform the search.
* `Search`: A `SearchResult` of type `DeploymentNotification` representing the result of the search.
* `FullTextSearch`: A `SearchResult` of type `DeploymentNotification` representing the result of the full-text search.

## Usage
Here are two examples of using the `SearchCriteria` type:
```csharp
// Example 1: Search for notifications with a specific project name and status
var criteria = new SearchCriteria
{
    ProjectName = "MyProject",
    Status = BuildStatus.Success
};
var results = criteria.Search;
foreach (var notification in results.Items)
{
    Console.WriteLine(notification.Message);
}

// Example 2: Search for notifications with a specific message content and minimum priority
var criteria2 = new SearchCriteria
{
    MessageContains = "error",
    MinimumPriority = NotificationPriority.High
};
var results2 = criteria2.FullTextSearch;
foreach (var notification in results2.Items)
{
    Console.WriteLine(notification.Message);
}
```

## Notes
When using the `SearchCriteria` type, note that the `Limit` and `Offset` properties can be used to implement pagination. The `CreatedAfter` and `CreatedBefore` properties can be used to filter notifications by creation time. The `MinimumPriority` property can be used to filter notifications by priority. The `Channels` property can be used to filter notifications by channel. The `NotificationSearchEngine` instance is used to perform the search, and the `Search` and `FullTextSearch` properties return the results of the search. The `SearchCriteria` type is not thread-safe, and concurrent access to its properties may result in unexpected behavior. Additionally, the `Search` and `FullTextSearch` methods may throw exceptions if the search engine encounters an error.
