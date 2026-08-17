namespace Quom.AssetManagement.Web.Models
{
    public class ReturnAssetRequest
    {
        public string ReturnCondition { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}