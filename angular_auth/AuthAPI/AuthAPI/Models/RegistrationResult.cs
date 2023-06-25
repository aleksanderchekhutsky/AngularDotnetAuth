namespace AuthAPI.Models
{
    public class RegistrationResult
    {
        public RegistrationResult()
        {
                
        }
        public RegistrationResult(bool succes, string message)
        {
            Success = succes;
            Message = message;
                
        }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; internal set; }
    }
    
}
