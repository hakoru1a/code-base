namespace Shared.SeedWork
{
    /// <summary>
    /// Standard response messages for API operations
    /// </summary>
    public static class ResponseMessages
    {
        // Success Messages
        public const string RetrieveSuccess = "Data retrieved successfully";
        public const string RetrieveItemSuccess = "Item retrieved successfully";
        public const string RetrieveItemsSuccess = "Items retrieved successfully";
        public const string CreateSuccess = "Item created successfully";
        public const string UpdateSuccess = "Item updated successfully";
        public const string DeleteSuccess = "Item deleted successfully";
        public const string OperationSuccess = "Operation completed successfully";

        // Error Messages
        public const string NotFound = "The requested resource was not found";
        public const string BadRequest = "Invalid request data";
        public const string InternalError = "An internal server error occurred";
        public const string ValidationFailed = "Validation failed";
        public const string Unauthorized = "Unauthorized access";
        public const string Forbidden = "Access forbidden";

        // Custom Messages
        public static string ItemNotFound(string itemName, object id) =>
            $"{itemName} with ID {id} was not found";

        public static string ItemCreated(string itemName) =>
            $"{itemName} created successfully";

        public static string ItemUpdated(string itemName) =>
            $"{itemName} updated successfully";

        public static string ItemDeleted(string itemName) =>
            $"{itemName} deleted successfully";
    }
}

