using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Rules", menuName = "Scriptable Objects/Rules")]
public class SO_Rules : ScriptableObject
{
	[Serializable]
	public struct Rule
	{
		[FormerlySerializedAs("isOkay")] public bool inNewBiomeArea;
		public float areaPosition;
		public SO_TileData.TileType title1;
		public SO_TileData.TileType title2;
	}

	public Rule[] Rules;
}
