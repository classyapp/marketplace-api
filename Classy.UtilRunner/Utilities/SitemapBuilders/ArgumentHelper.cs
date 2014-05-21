using System;
using System.Diagnostics;
using System.Globalization;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
  /// <summary>
  /// Provides helper methods for asserting arguments.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class provides helper methods for asserting the validity of arguments. It can be used to reduce the number of
  /// laborious <c>if</c>, <c>throw</c> sequences in your code.
  /// </para>
  /// <para>
  /// The <see cref="AssertNotNull"/> method can be used to ensure that arguments are not <see langword="null"/>. The
  /// AssertEnumMember overloads can be used to assert the validity of enumeration arguments.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following code ensures that the <c>name</c> argument is not <c>null</c>:
  /// <code>
  /// public void DisplayDetails(string name)
  /// {
  ///		ArgumentHelper.AssertNotNull(name, "name");
  ///		//now we know that name is not null
  ///		...
  /// }
  /// </code>
  /// </example>
  /// <example>
  /// The following code ensures that the <c>day</c> parameter is a valid member of its enumeration:
  /// <code>
  /// public void DisplayInformation(DayOfWeek day)
  /// {
  ///		ArgumentHelper.AssertEnumMember(day);
  ///		//now we know that day is a valid member of DayOfWeek
  ///		...
  /// }
  /// </code>
  /// </example>
  /// <example>
  /// The following code ensures that the <c>day</c> parameter is either DayOfWeek.Monday or DayOfWeek.Thursday:
  /// <code>
  /// public void DisplayInformation(DayOfWeek day)
  /// {
  ///		ArgumentHelper.AssertEnumMember(day, DayOfWeek.Monday, DayOfWeek.Thursday);
  ///		//now we know that day is either Monday or Thursday
  ///		...
  /// }
  /// </code>
  /// </example>
  /// <example>
  /// The following code ensures that the <c>bindingFlags</c> parameter is either BindingFlags.Public, BindingFlags.NonPublic
  /// or both:
  /// <code>
  /// public void GetInformation(BindingFlags bindingFlags)
  /// {
  ///		ArgumentHelper.AssertEnumMember(bindingFlags, BindingFlags.Public, BindingFlags.NonPublic);
  ///		//now we know that bindingFlags is either Public, NonPublic or both
  ///		...
  /// }
  /// </code>
  /// </example>
  /// <example>
  /// The following code ensures that the <c>bindingFlags</c> parameter is either BindingFlags.Public, BindingFlags.NonPublic,
  /// both or neither (BindingFlags.None):
  /// <code>
  /// public void GetInformation(BindingFlags bindingFlags)
  /// {
  ///		ArgumentHelper.AssertEnumMember(bindingFlags, BindingFlags.Public, BindingFlags.NonPublic, BindingFlags.None);
  ///		//now we know that bindingFlags is either Public, NonPublic, both or neither
  ///		...
  /// }
  /// </code>
  /// </example>
  public static class ArgumentHelper
  {
    /// <summary>
    /// Ensures that <paramref name="arg"/> is not <c>null</c>. If it is, an <see cref="ArgumentNullException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the argument.
    /// </typeparam>
    /// <param name="arg">
    /// The argument to check for <c>null</c>.
    /// </param>
    /// <param name="argName">
    /// The name of the argument.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="arg"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerHidden]
    public static void AssertNotNull<T>(T arg, string argName)
      where T : class
    {
      if (arg == null)
      {
        throw new ArgumentNullException(argName);
      }
    }

    /// <summary>
    /// Ensures that <paramref name="arg"/> is not <c>null</c>. If it is, an <see cref="ArgumentNullException"/> is thrown.
    /// If it is an empty string an <see cref="ArgumentNullException"/> is thrown.
    /// </summary>
    /// <param name="arg">
    /// The argument to check for <c>null</c>.
    /// </param>
    /// <param name="argName">
    /// The name of the argument.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="arg"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="arg"/> is an empty string.
    /// </exception>
    [DebuggerHidden]
    public static void AssertNotEmptyAndNotNull(string arg, string argName)
    {
      if (arg == null)
      {
        throw new ArgumentNullException(argName);
      }
      else if (arg.Trim() == "")
      {
        throw new ArgumentException("Argument must be specified.", argName);
      }
    }

    /// <summary>
    /// Ensures that <paramref name="arg"/> is not <c>null</c>. If it is, an <see cref="ArgumentNullException"/> is thrown.
    /// If it is an empty string an <see cref="ArgumentNullException"/> is thrown.
    /// If it is greater than max length an <see cref="ArgumentOutOfRangeException"/> is thrown.
    /// </summary>
    /// <param name="arg">The argument to check for <c>null</c>.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="maxLength">Max length of argument.</param>
    public static void AssertNotEmptyNotNullAndLength(string arg, string argName, int maxLength)
    {
      AssertNotEmptyAndNotNull(arg, argName);
      if (arg.Length > maxLength)
        throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must be less than or equal to {0}", maxLength));
    }

    /// <summary>
    /// Ensures that <paramref name="arg"/> is within the specifed range. If it is not, an <see cref="ArgumentOutOfRangeException"/> is thrown.
    /// </summary>
    /// <param name="arg">The argument to check if it is out of range.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="minValue">Min length to check for (null if no check).</param>
    /// <param name="maxValue">Max length to check for (null if no check).</param>
    public static void AssertOutOfRange(int arg, string argName, int? minValue, int? maxValue)
    {
      if (minValue.HasValue && (arg < minValue.Value))
        throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must me greater than or equal to {0}", minValue.Value));
      if (maxValue.HasValue && (arg > maxValue.Value))
        throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must me less than or equal to {0}", maxValue.Value));
    }

    /// <summary>
    /// Ensures that <paramref name="arg2"/> is assignable from <paramref name="arg1"/>. If it is not, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <param name="arg1">Argument type to check against.</param>
    /// <param name="arg2">Argument type to check for.</param>
    public static void AssertArgTwoAssignableFromArgOne(object arg1, object arg2)
    {
      if (!arg2.GetType().IsAssignableFrom(arg1.GetType()))
        throw new ArgumentException("Argument two is not assignable from arugment one");
    }

    /// <summary>
    /// Ensures that <paramref name="enumValue"/> is a valid member of the <typeparamref name="TEnum"/> enumeration. If it
    /// is not, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method can be used to validate all publicly-supplied enumeration values. Without such an assertion, it is
    /// possible to cast any <c>int</c> value to the enumeration type and pass it in.
    /// </para>
    /// <para>
    /// This method works for both flags and non-flags enumerations. In the case of a flags enumeration, any combination of
    /// values in the enumeration is accepted. In the case of a non-flags enumeration, <paramref name="enumValue"/> must
    /// be equal to one of the values in the enumeration.
    /// </para>
    /// <para>
    /// This method is generic and quite slow as a result. You should prefer using the
    /// AssertEnumMember(TEnum, params TEnum[]) overload where possible. That overload is both faster and
    /// safer. Faster because it does not incur reflection costs, and safer because you are able to specify the exact
    /// values accepted by your method.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEnum">
    /// The enumeration type.
    /// </typeparam>
    /// <param name="enumValue">
    /// The value of the enumeration.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If <paramref name="enumValue"/> is not a valid member of the <typeparamref name="TEnum"/> enumeration.
    /// </exception>
    [DebuggerHidden]
    public static void AssertEnumMember<TEnum>(TEnum enumValue)
        where TEnum : struct, IConvertible
    {
      if (Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute), false))
      {
        //flag enumeration - we can only get here if TEnum is a valid enumeration type, since the FlagsAttribute can
        //only be applied to enumerations
        bool throwEx;
        long longValue = enumValue.ToInt64(CultureInfo.InvariantCulture);

        if (longValue == 0)
        {
          //only throw if zero isn't defined in the enum
          throwEx = !Enum.IsDefined(typeof(TEnum), 0);
        }
        else
        {
          foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
          {
            longValue &= ~value.ToInt64(CultureInfo.InvariantCulture);
          }

          //throw if there is a value left over after removing all valid values
          throwEx = (longValue != 0);
        }

        if (throwEx)
        {
          throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
            "Enum value '{0}' is not valid for flags enumeration '{1}'.",
            enumValue, typeof(TEnum).FullName));
        }
      }
      else
      {
        //not a flag enumeration
        if (!Enum.IsDefined(typeof(TEnum), enumValue))
        {
          throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
              "Enum value '{0}' is not defined for enumeration '{1}'.",
              enumValue, typeof(TEnum).FullName));
        }
      }
    }

    /// <summary>
    /// Ensures that <paramref name="enumValue"/> is included in the values specified by <paramref name="validValues"/>. If
    /// it is not, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method can be used to ensure that an enumeration argument is valid for the context of the method. It works for
    /// both flags and non-flags enumerations. For flags enumerations, <paramref name="enumValue"/> must be any combination
    /// of values specified by <paramref name="validValues"/>. For non-flags enumerations, <paramref name="enumValue"/>
    /// must be one of the values specified by <paramref name="validValues"/>.
    /// </para>
    /// <para>
    /// This method is much faster than the AssertEnumMember(TEnum) overload. This is because it does not use
    /// reflection to determine the values defined by the enumeration. For this reason you should prefer this method when
    /// validating enumeration arguments.
    /// </para>
    /// <para>
    /// Another reason why this method is prefered is because it allows you to explicitly specify the values that your code
    /// handles. If you use the AssertEnumMember(TEnum) overload and a new value is later added to the
    /// enumeration, the assertion will not fail but your code probably will.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEnum">
    /// The enumeration type.
    /// </typeparam>
    /// <param name="enumValue">
    /// The value of the enumeration.
    /// </param>
    /// <param name="validValues">
    /// An array of all valid values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="validValues"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="enumValue"/> is not present in <paramref name="validValues"/>, or (for flag enumerations) if
    /// <paramref name="enumValue"/> is not some combination of values specified in <paramref name="validValues"/>.
    /// </exception>
    [DebuggerHidden]
    public static void AssertEnumMember<TEnum>(TEnum enumValue, params TEnum[] validValues)
      where TEnum : struct, IConvertible
    {
      AssertNotNull(validValues, "validValues");

      if (Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute), false))
      {
        //flag enumeration
        bool throwEx;
        long longValue = enumValue.ToInt64(CultureInfo.InvariantCulture);

        if (longValue == 0)
        {
          //only throw if zero isn't permitted by the valid values
          throwEx = true;

          foreach (TEnum value in validValues)
          {
            if (value.ToInt64(CultureInfo.InvariantCulture) == 0)
            {
              throwEx = false;
              break;
            }
          }
        }
        else
        {
          foreach (TEnum value in validValues)
          {
            longValue &= ~value.ToInt64(CultureInfo.InvariantCulture);
          }

          //throw if there is a value left over after removing all valid values
          throwEx = (longValue != 0);
        }

        if (throwEx)
        {
          throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
            "Enum value '{0}' is not allowed for flags enumeration '{1}'.",
            enumValue, typeof(TEnum).FullName));
        }
      }
      else
      {
        //not a flag enumeration
        foreach (TEnum value in validValues)
        {
          if (enumValue.Equals(value))
          {
            return;
          }
        }

        //at this point we know an exception is required - however, we want to tailor the message based on whether the
        //specified value is undefined or simply not allowed
        if (!Enum.IsDefined(typeof(TEnum), enumValue))
        {
          throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
              "Enum value '{0}' is not defined for enumeration '{1}'.",
              enumValue, typeof(TEnum).FullName));
        }
        else
        {
          throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
              "Enum value '{0}' is defined for enumeration '{1}' but it is not permitted in this context.",
              enumValue, typeof(TEnum).FullName));
        }
      }
    }
  }
}
