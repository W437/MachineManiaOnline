using UnityEngine;

public static class ColorHelper
{
    /// <summary>
    /// Converts a Unity Color to a Hexadecimal string.
    /// </summary>
    /// <param name="color">The Color to convert.</param>
    /// <param name="includeAlpha">Whether to include the alpha channel in the hex string. Default is false.</param>
    /// <returns>Hexadecimal string representing the color.</returns>
    public static string ColorToHex(Color color, bool includeAlpha = false)
    {
        string hex = ColorChannelToHex(color.r) + ColorChannelToHex(color.g) + ColorChannelToHex(color.b);

        if (includeAlpha)
        {
            hex += ColorChannelToHex(color.a);
        }

        return hex;
    }

    /// <summary>
    /// Converts a Hexadecimal string to a Unity Color.
    /// </summary>
    /// <param name="hex">The hex string representing the color.</param>
    /// <returns>Color object.</returns>
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            Debug.LogWarning("Hex string is null or empty. Returning default Color.");
            return Color.black;
        }

        hex = hex.TrimStart('#');

        if (hex.Length != 6 && hex.Length != 8)
        {
            Debug.LogWarning("Hex string length is invalid. Expected 6 or 8 characters. Returning default Color.");
            return Color.black;
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255;

        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }

    /// <summary>
    /// Converts a float color channel to a two-digit hexadecimal string.
    /// </summary>
    /// <param name="channel">The color channel value (0 to 1).</param>
    /// <returns>Two-digit hex string.</returns>
    private static string ColorChannelToHex(float channel)
    {
        int value = Mathf.Clamp(Mathf.RoundToInt(channel * 255), 0, 255);
        return value.ToString("X2");
    }
}
