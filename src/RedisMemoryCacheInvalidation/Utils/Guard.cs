using System;
using System.Collections.Generic;

namespace RedisMemoryCacheInvalidation.Utils
{
    internal static class Guard
    {
        /// <summary>
        /// Ensures the value of the given <paramref name="argumentExpression"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <param name="parameter">instance to test for null</param>
        /// <param name="parameterName">nameof the parameter (for ArgumentNullException)</param>
        public static void NotNull(object parameter, string parameterName)
        {
            if (parameter == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Ensures the value of the given <paramref name="argumentExpression"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <param name="parameter">instance to test for (Generic) Default value</param>
        /// <param name="parameterName">nameof the parameter (for ArgumentNullException)</param>
        public static void NotDefault<T>(T parameter, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(parameter, default(T)))
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Ensures the string value of the given <paramref name="argumentExpression"/> is not null or empty.
        /// Throws <see cref="ArgumentNullException"/> in the first case, or
        /// <see cref="ArgumentException"/> in the latter.
        /// </summary>
        /// <param name="parameter">string to test for Null or empty</param>
        /// <param name="parameterName">nameof the parameter (for ArgumentNullException)</param>
        public static void NotNullOrEmpty(string parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
                throw new ArgumentNullException(parameterName);
        }
    }
}
