namespace SharedLibrary.Integrations.GMCM;

/// <summary>
/// Denotes that a GMCM property is to be treated as a percentage range value.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class GMCMPercentageSettingAttribute : Attribute
{
}

