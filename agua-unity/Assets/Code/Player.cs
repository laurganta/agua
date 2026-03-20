namespace CleverEdge
{
    public class Player
    {
        public string PlayerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int AvatarIndex { get; set; }

        public static Player Current { get; set; }
    }
}