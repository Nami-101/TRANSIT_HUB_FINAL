namespace TransitHub.Models
{
    public enum BookingType
    {
        Train,
        Flight
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum NotificationStatus
    {
        Unread,
        Read
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug
    }

    public enum AdminRole
    {
        SuperAdmin,
        Admin,
        Moderator
    }
}