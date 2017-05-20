namespace FRC.NativeLibraryUtilties
{
    internal static class StringUtil
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;

            for(int i = 0; i < value.Length; i++) {
                if(!char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }
    }
}
