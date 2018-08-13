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
		
		DataHandler.territory = Plot.Ward.S1H1;
		
		timer.registerEvent("TerritoryLoad");

		float x = 0.00000004459414f;
		float y = 1.348976f;
		float z = 0.00000004459414f;
		float w = 0.6067699f;

		FFXIVHSLib.Quaternion libTest = new FFXIVHSLib.Quaternion(x, y, z, w);
		Quaternion uTest1 = libTest;
		
		Quaternion test = new Quaternion(x, y, z, w);
		Debug.LogFormat("test lib: {0}, {1}, {2}", libTest.ToVector3().x, libTest.ToVector3().y, libTest.ToVector3().z);
		Debug.LogFormat("test unity: {0}, {1}, {2}", test.eulerAngles.x, test.eulerAngles.y, test.eulerAngles.z);
		Debug.LogFormat("test implicit: {0}, {1}, {2}", uTest1.eulerAngles.x, uTest1.eulerAngles.y, uTest1.eulerAngles.z);
		
		Debug.Log("Startupscript finished.");
	}

	// Update is called once per frame
	void Update () {
		
	}
}
