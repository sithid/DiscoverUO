using System.Text;

namespace DiscoverUO.Web.Shared
{
    public static class DebugLogger
    {
        public static void LogInfo(string[] toLog )
        {
            Console.WriteLine();

            StringBuilder builder = new StringBuilder();

            for( int i = 0; i < toLog.Length; i++ )
            {
                if (i < (toLog.Length - 1))
                    builder.Append($"{toLog[i]}::");
                else
                    builder.Append($"{toLog[i]}");
            }

            Console.WriteLine( builder.ToString() );
        }
    }
}
