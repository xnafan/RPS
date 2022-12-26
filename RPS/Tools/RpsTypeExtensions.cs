using RPS.Model;

namespace RPS.Tools
{
    public static class RpsTypeExtensions
    {
        public static bool Beats(this RpsType type, RpsType other)
        {
            return (type == RpsType.Rock && other == RpsType.Scissors)
                || (type == RpsType.Scissors && other == RpsType.Paper)
                || (type == RpsType.Paper && other == RpsType.Rock);
        }
    }
}
