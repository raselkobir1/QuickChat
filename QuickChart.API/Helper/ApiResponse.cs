namespace QuickChart.API.Helper
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static ApiResponse<T> CreateSuccess(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<object> CreateFail(string errorMessage, IEnumerable<string> errors = null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = errorMessage,
                Errors = errors ?? Enumerable.Empty<string>()
            };
        }
    }
}
