namespace SharedLibrary.Integrations.GMCM;

#region using directives

using SharedLibrary.Interfaces.GMCM;
using StardewModdingAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

internal class GMCMHelper
{
	internal static IGenericModConfigMenuApi GMCMInterface { get; set; } = null!;
	internal static IModHelper SMAPIHelper { get; set; } = null!;
	internal static IManifest ModManifest { get; set; } = null!;
	GMCMHelper(IGenericModConfigMenuApi api, IModHelper helper, IManifest manifest)
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
		where TConfig : class, new()
	{
		GMCMInterface.Register(ModManifest, () => modConfig = new(), () => SMAPIHelper.WriteConfig(modConfig));
	}

	private static PropertyInfo[] GetProperties<TConfig>(TConfig modConfig)
	{
		var properties = modConfig!.GetType().GetProperties()
			.Where((property) => property.GetCustomAttribute<GMCMIgnoreAttribute>() is null)
			.ToArray();
		return properties;
	}

	private static Action<IComparable> CreateSetter<IComparable, TConfig>(PropertyInfo property, TConfig modConfig)
	{
		return (IComparable value) => property.GetSetMethod()!.Invoke(modConfig, new object[] { value! });
	}

	private static Func<IComparable> CreateGetter<IComparable, TConfig>(PropertyInfo property, TConfig modConfig)
	{
		return () => (IComparable)property.GetGetMethod()!.Invoke(modConfig, null)!;
	}

	private static string GetI18NTitle()
	{
		return SMAPIHelper.Translation.Get("Title");
	}

	private static string GetI18NName(PropertyInfo property)
	{
		return SMAPIHelper.Translation.Get($"{property.Name}");
	}

	private static string Get18NDescription(PropertyInfo property)
	{
		return SMAPIHelper.Translation.Get($"{property.Name}.Tooltip");
	}

	private static Dictionary<string, Delegate> CreateAccessors<TValue, TConfig>(PropertyInfo property, TConfig modConfig) 
		where TValue : IComparable
	{
		Dictionary<string, Delegate> accessors = new()
		{
			{ "getter", CreateGetter<TValue, TConfig>(property, modConfig) },
			{ "setter", CreateSetter<TValue, TConfig>(property, modConfig) }
		};
		return accessors;
	}

	private static void CreateSettings<TConfig>(TConfig modConfig) 
		where TConfig : class, new()
	{
		var properties = GetProperties(modConfig);

		if (properties.Length == 0)
		{
			return;
		}
		// TODO:
	}

	private static void AddIntSetting<TConfig>(PropertyInfo property, TConfig modConfig)
	{
		Dictionary<string, Delegate> accessors = CreateAccessors<int, TConfig>(property, modConfig);
		Dictionary<string, int> range = GetRange<int, TConfig>(property, modConfig);

		GMCMInterface.AddNumberOption(
			mod: ModManifest, 
			getValue: (Func<int>)accessors["getter"], 
			setValue: (Action<int>)accessors["setter"], 
			name: () => GetI18NName(property), 
			tooltip: () => Get18NDescription(property),
			min: range["min"],
			max: range["max"]
		);
	}

	internal static void Build<TConfig>(TConfig modConfig) 
		where TConfig : class, new()
	{
		Register(modConfig);
		AddSectionTitle();
		CreateSettings(modConfig);
	}
}

