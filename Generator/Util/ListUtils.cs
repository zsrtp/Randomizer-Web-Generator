namespace TPRandomizer.Util
{
    using System.Collections.Generic;

    public class ListUtils
    {
        public static bool isEmpty<T>(ICollection<T> list)
        {
            return list == null || list.Count < 1;
        }
    }
}
