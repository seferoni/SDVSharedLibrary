namespace SharedLibrary.Integrations.GMCM;

/// <summary>
/// Defines the minimum and maximum value of a GMCM property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class GMCMRangeAttribute : Attribute
{
	internal float Min { get; set; }

	internal float Max { get; set; }

	internal GMCMRangeAttribute(float min, float max)
	{
		Min = min;
		Max = max;
	}
}

