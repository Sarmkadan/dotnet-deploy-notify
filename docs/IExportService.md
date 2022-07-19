# IExportService

The `IExportService` interface facilitates the export of notification processing data into various formats, including JSON, CSV, and compressed ZIP archives. Additionally, it provides mechanisms for generating comprehensive analysis reports regarding notification processing activity.

## API

### Members

*   **`ExportService`**: The implementation constructor for the export service.
*   **`Task<string> ExportAsJsonAsync()`**: Asynchronously exports notification data and returns a JSON-formatted string representation.
*   **`Task<string> ExportAsCsvAsync()`**: Asynchronously exports notification data and returns a CSV-formatted string representation.
*   **`Task<byte[]> ExportAsZipAsync()`**: Asynchronously exports notification data and returns a byte array representing the compressed ZIP archive.
*   **`Task SaveToFileAsync()`**: Asynchronously saves generated export data to a specified file system path.
*   **`NotificationReportGenerator`**: Provides access to the generator instance used for creating notification reports.
*   **`NotificationReport GenerateReport()`**: Generates and returns a `NotificationReport` object containing aggregated processing statistics.

### Properties

*   **`int TotalNotifications`**: Gets the total number of notifications processed.
*   **`int SuccessfulCount`**: Gets the count of notifications processed successfully.
*   **`int FailedCount`**: Gets the count of notifications that encountered errors during processing.
*   **`int CancelledCount`**: Gets the count of notifications for which processing was cancelled.
*   **`double AverageDuration`**: Gets the average processing duration across all notifications.
*   **`DateTime GeneratedAt`**: Gets the timestamp indicating when the report data was generated.
*   **`Dictionary<string, int> EnvironmentBreakdown`**: Gets a breakdown of notifications grouped by deployment environment.
*   **`Dictionary<string, int> StatusBreakdown`**: Gets a breakdown of notifications grouped by processing status.
*   **`Dictionary<string, int> ChannelBreakdown`**: Gets a breakdown of notifications grouped by notification channel.
*   **`List<(string Project, int Count)> TopProjects`**: Gets a list of projects with the highest notification volume.

### Overrides

*   **`string ToString()`**: Returns a string representation of the current object state.

## Usage

### Example 1: Exporting Data to JSON
```csharp
public async Task ExportDataAsync(IExportService exportService, string outputPath)
{
    string jsonContent = await exportService.ExportAsJsonAsync();
    await exportService.SaveToFileAsync(outputPath, jsonContent);
}
```

### Example 2: Generating and Utilizing a Report
```csharp
public void PrintReportSummary(IExportService exportService)
{
    var report = exportService.GenerateReport();
    Console.WriteLine($"Report generated at: {report.GeneratedAt}");
    Console.WriteLine($"Total processed: {report.TotalNotifications}");
    Console.WriteLine($"Success rate: {(double)report.SuccessfulCount / report.TotalNotifications:P}");
}
```

## Notes

*   **Asynchronous Operations**: All export methods (`ExportAsJsonAsync`, `ExportAsCsvAsync`, `ExportAsZipAsync`, and `SaveToFileAsync`) follow the asynchronous task pattern. Implementations must ensure that I/O operations do not block the calling thread.
*   **Thread Safety**: Implementations of this interface should be designed with thread safety in mind if they are to be accessed concurrently by multiple background workers or API request threads. State-modifying methods should be synchronized accordingly.
*   **File I/O**: `SaveToFileAsync` assumes appropriate file system permissions are available for the target destination path. It is recommended to handle `IOException` or unauthorized access exceptions at the caller level.
*   **Data Consistency**: The properties returned (e.g., `TotalNotifications`, `StatusBreakdown`) represent a snapshot in time. Consumers should call `GenerateReport()` to obtain a fresh, consistent view of the processing statistics before accessing these properties.
