public class RentalProcessIndexViewModel
{
    public List<RentalProcessDto> Rentals { get; set; } = new List<RentalProcessDto>();

    public int? SelectedStatus { get; set; }
}