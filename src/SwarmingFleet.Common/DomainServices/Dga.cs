
namespace SwarmingFleet.DomainServices
{
    using System;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Domain Generation Algorithm (DGA)
    /// </summary>
    public static class Dga
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="rootDomain"></param>
        /// <returns></returns>
        /// <see cref="https://en.wikipedia.org/wiki/Domain_generation_algorithm#Example"/>
        public static string Generate(DateTime date, string rootDomain)
        {
            rootDomain.ThrowIfNullOrWhiteSpace(nameof(rootDomain));

            var domain = new StringBuilder();

            long year = date.Year, month = date.Month, day = date.Day;
            const int DOMAIN_NAME_LEN = 16;
            for (int i = 0; i < DOMAIN_NAME_LEN; i++)
            {
                year = ((year ^ 8 * year) >> 11) ^ ((year & 0xFFFFFFF0) << 17);
                month = ((month ^ 4 * month) >> 25) ^ 16 * (month & 0xFFFFFFF8);
                day = ((day ^ (day << 13)) >> 19) ^ ((day & 0xFFFFFFFE) << 12);
                domain.Append((char)(((year ^ month ^ day) % 25) + 97));
            }

            if (!rootDomain.StartsWith("."))
            {
                domain.Append(".");
            }
            domain.Append(rootDomain);
            return domain.ToString();
        }
    }
} 
