namespace Payment.Application.DTO;

public class PaymentRequest
{
    public Guid BookingId { get; set; }
    public decimal Amount    { get; set; } 
    public string Currency { get; set; } = "CNY";
    public bool? SimulateSuccess { get; set; }
}
