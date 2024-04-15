namespace SharedLibrary.Classes;

using System.Collections.Generic;
using System.Reflection;

public abstract class ConfigClass
{
	internal abstract Dictionary<string, IComparable> Defaults { get; set; }
	public void ResetProperties()
	{
		PropertyInfo[] properties = GetType().GetProperties();

		foreach (PropertyInfo property in properties)
		{
			property.SetValue(this, Defaults[property.Name]);
		}
	}
}

