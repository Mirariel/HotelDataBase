namespace DataBase.Models
{
    public class ServiceOrderRequest
    {
        public int ServiceId { get; set; }
        public int CustomerId { get; set; }
        public int ReservationId { get; set; }
        public DateTime SelectedDate { get; set; }
        public string SelectedTime { get; set; }
    }
}
