namespace Clients.Enums;

public enum Gender
{
    M,  // Masculino
    F,  // Femenino
    O   // Otro
}

public static class GenderExtensions
{
    /// <summary>
    /// Obtiene el nombre en español del género (igual que en JAVA)
    /// Coincide con Gender.java de la API JAVA
    /// </summary>
    public static string GetDisplayName(this Gender gender) => gender switch
    {
        Gender.M => "Varón",
        Gender.F => "Mujer",
        Gender.O => "Otro",
        _ => "Otro"
    };

    /// <summary>
    /// Obtiene la ruta de imagen por defecto para el género
    /// Coincide con Gender.java de la API JAVA: default/male.png, default/female.png, default/other.png
    /// </summary>
    public static string GetDefaultImagePath(this Gender gender) => gender switch
    {
        Gender.M => "default/male.png",
        Gender.F => "default/female.png",
        Gender.O => "default/other.png",
        _ => "default/other.png"
    };

    /// <summary>
    /// Verifica si una ruta de imagen es una imagen por defecto (protegida)
    /// </summary>
    public static bool IsDefaultImagePath(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        var defaultImages = new[] { "male.png", "female.png", "other.png", "default/male.png", "default/female.png", "default/other.png" };
        return defaultImages.Contains(imagePath.ToLowerInvariant());
    }

    /// <summary>
    /// Verifica si una imagen es una imagen por defecto (solo por nombre)
    /// </summary>
    public static bool IsDefaultImage(string? imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return false;

        var defaultImages = new[] { "male.png", "female.png", "other.png" };

        // Extraer solo el nombre de la ruta completa
        string fileName = imageName.Contains("/") 
            ? imageName.Substring(imageName.LastIndexOf("/") + 1) 
            : imageName;

        return defaultImages.Contains(fileName.ToLowerInvariant());
    }

    /// <summary>
    /// Convierte string a Gender enum
    /// </summary>
    public static Gender FromString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return Gender.O;

        return value.ToUpperInvariant() switch
        {
            "M" or "MASCULINO" or "MALE" => Gender.M,
            "F" or "FEMENINO" or "FEMALE" => Gender.F,
            "O" or "OTRO" or "OTHER" => Gender.O,
            _ => Gender.O
        };
    }

    /// <summary>
    /// Convierte display name a Gender enum
    /// </summary>
    public static Gender FromDisplayName(string? displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return Gender.O;

        return displayName.ToLowerInvariant() switch
        {
            "masculino" or "male" => Gender.M,
            "femenino" or "female" => Gender.F,
            "otro" or "other" => Gender.O,
            _ => Gender.O
        };
    }
}
