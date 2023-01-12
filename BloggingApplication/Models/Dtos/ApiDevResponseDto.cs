namespace BloggingApplication.Models.Dtos
{
    public class ApiDevResponseDto
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string ExtraDetails { get; set; }

        public ApiDevResponseDto(Exception ex)
        {
            Message = $"{ex.GetType().Name}: {ex.Message}";
            ExtraDetails = ex.ToString();
        }
        public ApiDevResponseDto(bool isSuccess, string message, string extraDetails)
        {
            this.isSuccess = isSuccess;
            Message = message;
            ExtraDetails = extraDetails;
        }
    }
}
