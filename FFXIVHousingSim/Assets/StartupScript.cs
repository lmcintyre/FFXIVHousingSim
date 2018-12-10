using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FFXIVHSLib;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

public class StartupScript : MonoBehaviour
{
	private static bool debug = false;

	void Start()
	{
		DataHandler.defaultGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
		
		DebugTimer timer = new DebugTimer();
		timer.registerEvent("Begin");
		
		DataHandler.territory = Territory.S1H1;
		
		timer.registerEvent("TerritoryLoad");

		Debug.Log("Startupscript finished.");
	}

	// Update is called once per frame
	void Update () {
		
	}
}
