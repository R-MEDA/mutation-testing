namespace Domain
{
    public abstract record Event(Guid StreamId)
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}