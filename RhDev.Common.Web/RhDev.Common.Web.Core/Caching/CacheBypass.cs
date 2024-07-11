using System;

namespace RhDev.Common.Web.Core.Caching
{
    /// <summary>
    /// Vyřazuje cache z činnosti pro všechna volání uvnitř using.
    /// </summary>
    /// <remarks>
    /// Cache, nad kterou se CacheBypass volá, se musí dotázat na propertu IsActive, 
    /// aby nad ní bylo možné implementaci CacheBypass volat.
    /// </remarks>
    public class CacheBypass : IDisposable
    {
        [ThreadStatic]
        private static bool isActive;

        public CacheBypass()
        {
            isActive = true;
        }

        /// <summary>
        /// Indikuje, že se nemá použít cachovaná hodnota, pokud je true.
        /// </summary>
        public static bool IsActive => isActive;

        public void Dispose()
        {
            isActive = false;
        }
    }
}
