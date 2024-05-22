namespace TPRandomizer.Util
{
    public class StringUtils
    {
        public static bool isEmpty(string str)
        {
            return str == null || str.Length < 1;
        }
    }
}
