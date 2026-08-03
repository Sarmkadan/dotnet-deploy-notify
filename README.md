## AmbientRequestContextTests

The AmbientRequestContextTests class contains tests for the AmbientRequestContext class.

### Current_Getter_CreatesNewContext_WhenNoContextSet
Tests that the Current getter creates a new context when no context is set.

### Current_Getter_ReturnsSameContext_WithinSameLogicalCallContext
Tests that the Current getter returns the same context within the same logical call context.

### Current_Setter_Throws_WhenContextAlreadyExists
Tests that the Current setter throws an exception when the context already exists.

### SetContext_Throws_WhenContextAlreadyExists
Tests that the SetContext method throws an exception when the context already exists.

### ClearContext_RemovesCurrentContext
Tests that the ClearContext method removes the current context.

### Reset_CreatesNewContext
Tests that the Reset method creates a new context.

### Current_ContextFlowsAcrossAwaitBoundary
Tests that the current context flows across await boundaries.

### RequestContextScope_ProperlyRestoresPreviousContext
Tests that the request context scope properly restores the previous context.

### Context_IsIsolatedBetweenParallelTasks
Tests that the context is isolated between parallel tasks.

### ExecuteInContextAsync_ProperlyIsolatesContext
Tests that the ExecuteInContextAsync method properly isolates the context.

### RequestContextScope_ConstructorWithCustomContext_SetsContext
Tests that the request context scope constructor with a custom context sets the context.

### RequestContextScope_Dispose_RestoresPreviousContext
Tests that the request context scope dispose restores the previous context.

Example usage:
```csharp
public class MyClass
{
    private readonly AmbientRequestContext _context;

    public MyClass()
    {
        _context = new AmbientRequestContext();
    }

    public void MyMethod()
    {
        using (var scope = new RequestContextScope(_context))
        {
            // code that uses the context
        }
    }
}
```