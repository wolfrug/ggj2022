using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("m_slotName", "m_timeOfSave", "m_version", "m_game", "m_slotIndex", "m_savedKeys")]
	public class ES3UserType_SaveGameSlot : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3UserType_SaveGameSlot() : base(typeof(SaveGameSlot)){ Instance = this; priority = 1;}


		public override void Write(object obj, ES3Writer writer)
		{
			var instance = (SaveGameSlot)obj;
			
			writer.WriteProperty("m_slotName", instance.m_slotName, ES3Type_string.Instance);
			writer.WriteProperty("m_timeOfSave", instance.m_timeOfSave, ES3Type_string.Instance);
			writer.WriteProperty("m_version", instance.m_version, ES3Type_float.Instance);
			writer.WriteProperty("m_game", instance.m_game, ES3Type_string.Instance);
			writer.WriteProperty("m_slotIndex", instance.m_slotIndex, ES3Type_int.Instance);
			writer.WriteProperty("m_savedKeys", instance.m_savedKeys, ES3Type_StringArray.Instance);
		}

		public override object Read<T>(ES3Reader reader)
		{
			var instance = new SaveGameSlot();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "m_slotName":
						instance.m_slotName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "m_timeOfSave":
						instance.m_timeOfSave = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "m_version":
						instance.m_version = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "m_game":
						instance.m_game = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "m_slotIndex":
						instance.m_slotIndex = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "m_savedKeys":
						instance.m_savedKeys = reader.Read<System.String[]>(ES3Type_StringArray.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}


	public class ES3UserType_SaveGameSlotArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SaveGameSlotArray() : base(typeof(SaveGameSlot[]), ES3UserType_SaveGameSlot.Instance)
		{
			Instance = this;
		}
	}
}