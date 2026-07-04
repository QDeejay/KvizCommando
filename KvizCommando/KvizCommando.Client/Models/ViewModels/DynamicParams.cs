namespace KvizCommando.Client.Models.ViewModels
{
    public class DynamicParams
    {
        public Type? BodyComponent { get; set; }
        public Dictionary<string, object?> BodyParameters { get; } = new();
    }
}
