
namespace SwarmingFleet
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode]
    public static class Utilities
    {
        /// <summary>
        /// 當 <paramref name="obj"/> 為 <see langword="null"/> 時， 引發 <see
        /// cref="ArgumentNullException"/> 例外。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">物件。</param>
        /// <param name="paramName">參數名稱。</param>
        /// <exception cref="ArgumentNullException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfNull<T>(this T obj, string paramName) where T : class
        {
            if (obj is null)
                throw new ArgumentNullException(paramName, $"'{paramName}' can not be null.");
            return obj;
        }

        /// <summary>
        /// 當 <paramref name="str"/> 滿足 <see cref="string.IsNullOrEmpty"/> 的條件時， 引發 <see
        /// cref="ArgumentNullException"/> 例外。
        /// </summary>
        /// <param name="str">字串。</param>
        /// <param name="paramName">參數名稱。</param>
        /// <exception cref="ArgumentNullException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullOrEmpty(this string str, string paramName)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(paramName, $"'{paramName}' can not be null or empty.");
            return str;
        } 

        /// <summary>
        /// 當 <paramref name="str"/> 滿足 <see cref="string.IsNullOrWhiteSpace"/> 的條件時， 引發 <see
        /// cref="ArgumentNullException"/> 例外。
        /// </summary>
        /// <param name="str">字串。</param>
        /// <param name="paramName">參數名稱。</param>
        /// <exception cref="ArgumentNullException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullOrWhiteSpace(this string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(paramName, $"'{paramName}' can not be null or whitespace.");
            return str;
        }
    }
}
