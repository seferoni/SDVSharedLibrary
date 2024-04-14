namespace SharedLibrary.Integrations.GMCM;

/// <summary>
/// Denotes that a property is to be ignored by the helper.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class GMCMIgnoreAttribute : Attribute
{
}

