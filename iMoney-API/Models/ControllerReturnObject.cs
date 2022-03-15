namespace iMoney_API.Models
{
    public class ControllerReturnObject
    {
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public object ReturnObject { get; set; }
    }
}
