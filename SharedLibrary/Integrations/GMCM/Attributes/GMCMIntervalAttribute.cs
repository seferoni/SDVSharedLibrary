namespace SharedLibrary.Integrations.GMCM;

/// <summary>
/// Defines the interval for a numerical GMCM property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class GMCMIntervalAttribute : Attribute
{
	/// <summary>
	/// The interval value for the targeted numerical GMCM property.
	/// </summary>
	internal float Value { get; set; }

	internal GMCMIntervalAttribute(float interval)
	{
		Value = interval;
	}
}

