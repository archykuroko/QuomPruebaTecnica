namespace Quom.AssetManagement.Api.Models
{
    public class AssetMovement
    {
        public long Id { get; set; }
        public int AssetId { get; set; }
        public string MovementType { get; set; } = string.Empty;

        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }

        public string? PreviousLocation { get; set; }
        public string? NewLocation { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public int PerformedByUserId { get; set; }
        public string PerformedByUsername { get; set; } = string.Empty;
    }
}