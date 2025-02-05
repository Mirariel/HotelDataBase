namespace DataBase.Models
{
    public class ReservationRequest
    {
        public int RoomId { get; set; }
        public Customer Customer { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
