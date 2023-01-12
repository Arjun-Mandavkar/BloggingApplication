namespace BloggingApplication.Models.Dtos
{
    public class ApiResponseDto
    { 
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        
        public ApiResponseDto(Exception ex)
        {
            isSuccess = false;
            Message = $"{ex.GetType().Name}: {ex.Message}";
        }
        public ApiResponseDto(bool isSuccess, string message)
        {
            this.isSuccess = isSuccess;
            Message = message;
        }
    }
}
