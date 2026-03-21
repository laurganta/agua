namespace CleverEdge
{
    public class Constants
    {
        public const string PIN =
#if UNITY_EDITOR
            "";
#else
            "qasdew";
#endif
    }
}