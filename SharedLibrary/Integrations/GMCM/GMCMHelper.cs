namespace SharedLibrary.Integrations.GMCM;

#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SharedLibrary.Classes;
using SharedLibrary.Interfaces.GMCM;
using StardewModdingAPI;

#endregion

internal static class GMCMHelper
{
	internal static IGenericModConfigMenuApi GMCMInterface { get; set; } = null!;
	internal static IModHelper SMAPIHelper { get; set; } = null!;
	internal static IManifest ModManifest { get; set; } = null!;

	public static void Initialise(IGenericModConfigMenuApi api, IModHelper helper, IManifest manifest)
	{
		GMCMInterface = api;
		SMAPIHelper = helper;
		ModManifest = manifest;
	}

	private static void AddSectionTitle()
	{
		GMCMInterface.AddSectionTitle(ModManifest, () => GetI18NTitle(), null);
	}

	private static void Register<TConfig>(TConfig modConfig) 
		where TConfig : ConfigClass, new()
	{
		GMCMInterface.Register(ModManifest, () => modConfig.ResetProperties(), () => SMAPIHelper.WriteConfig(modConfig));
	}

	private static PropertyInfo[] GetProperties<TConfig>(TConfig modConfig)
	{
		var properties = modConfig!.GetType().GetProperties()
			.Where((property) => property.GetCustomAttribute<GMCMIgnoreAttribute>() is null)
			.ToArray();
		return properties;
	}

	private static GMCMIntervalAttribute GetInterval(PropertyInfo property)
	{
		return property.GetCustomAttribute<GMCMIntervalAttribute>()!;
	}

	private static GMCMRangeAttribute GetRange(PropertyInfo property)
	{
		return property.GetCustomAttribute<GMCMRangeAttribute>()!;
	}

	private static Action<TValue> CreateSetter<TValue, TConfig>(PropertyInfo property, TConfig modConfig)
		where TValue : IComparable<TValue>
	{
		return (TValue value) => property.GetSetMethod()!.Invoke(modConfig, new object[] { value! });
	}

	private static Func<TValue> CreateGetter<TValue, TConfig>(PropertyInfo property, TConfig modConfig)
		where TValue : IComparable<TValue>
	{
		return () => (TValue)property.GetGetMethod()!.Invoke(modConfig, null)!;
	}

	private static string FormatPropertyName(PropertyInfo property)
	{
		return JsonNamingPolicy.CamelCase.ConvertName(property.Name);
	}

	private static string GetI18NTitle()
	{
		return SMAPIHelper.Translation.Get("GMCM.title");
	}

	private static string GetI18NName(string propertyName)
	{
		return SMAPIHelper.Translation.Get($"GMCM.{propertyName}");
	}

	private static string Get18NDescription(string propertyName)
	{
		return SMAPIHelper.Translation.Get($"GMCM.{propertyName}.tooltip");
	}

	private static Dictionary<string, Delegate> CreateAccessors<TValue, TConfig>(PropertyInfo property, TConfig modConfig) 
		where TValue : IComparable<TValue>
	{
		Dictionary<string, Delegate> accessors = new()
		{
			{ "getter", CreateGetter<TValue, TConfig>(property, modConfig) },
			{ "setter", CreateSetter<TValue, TConfig>(property, modConfig) }
		};
		return accessors;
	}

	private static void CreateSettings<TConfig>(TConfig modConfig) 
	{
		var properties = GetProperties(modConfig);

		if (properties.Length == 0)
		{
			return;
		}
		
		foreach(PropertyInfo property in properties)
		{
			if (property.PropertyType == typeof(int))
			{
				AddIntSetting(property, modConfig);
				continue;
			}

			if (property.PropertyType == typeof(float))
			{
				AddFloatSetting(property, modConfig);
			}
		}
	}

	private static void AddFloatSetting<TConfig>(PropertyInfo property, TConfig modConfig)
	{
		Dictionary<string, Delegate> accessors = CreateAccessors<float, TConfig>(property, modConfig);
		string propertyName = FormatPropertyName(property);
		GMCMRangeAttribute range = GetRange(property);
		GMCMIntervalAttribute interval = GetInterval(property);

		GMCMInterface.AddNumberOption(
			mod: ModManifest,
			getValue: (Func<float>)accessors["getter"],
			setValue: (Action<float>)accessors["setter"],
			name: () => GetI18NName(propertyName),
			tooltip: () => Get18NDescription(propertyName),
			min: range.Min,
			max: range.Max,
			interval: interval.Value
		);
	}

	private static void AddIntSetting<TConfig>(PropertyInfo property, TConfig modConfig)
	{
		Dictionary<string, Delegate> accessors = CreateAccessors<int, TConfig>(property, modConfig);
		string propertyName = FormatPropertyName(property);
		GMCMRangeAttribute range = GetRange(property);
		GMCMIntervalAttribute interval = GetInterval(property);

		GMCMInterface.AddNumberOption(
			mod: ModManifest,
			getValue: (Func<int>)accessors["getter"], 
			setValue: (Action<int>)accessors["setter"], 
			name: () => GetI18NName(propertyName), 
			tooltip: () => Get18NDescription(propertyName),
			min: (int)range.Min,
			max: (int)range.Max,
			interval: (int)interval.Value
		);
	}

	internal static void Build<TConfig>(TConfig modConfig) 
		where TConfig : ConfigClass, new()
	{
		Register(modConfig);
		AddSectionTitle();
		CreateSettings(modConfig);
	}
}

