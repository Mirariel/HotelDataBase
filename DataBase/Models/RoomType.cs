namespace DataBase.Models;

public partial class RoomType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public byte Capacity { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}