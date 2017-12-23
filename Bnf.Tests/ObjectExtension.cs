

namespace Bnf.Tests
{
    public static class ObjectExtension
    {
        public static bool Match(this object x, object y)
        {
            return new EqualityComparer().Equals(x, y);
        }
    }
}
