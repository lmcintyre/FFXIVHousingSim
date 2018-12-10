using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Xsl;
using Cinemachine;
using FFXIVHSLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

[RequireComponent(typeof(Camera))]
public class CameraHandler : MonoBehaviour
{
    public CinemachineFreeLook FreeLook;

    //Keep track of where we are
    private Plot _centeredPlot;
    private GameObject _center;
    private Vector3 _defaultLook = new Vector3(90, 0, 0);

    public Territory _territory;
    private int _plotView = 1;
    private bool _subdiv;
    
    void Start()
    {
        UpdatePlotView();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_plotView != 1)
                _plotView--;
            UpdatePlotView();
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_plotView != 30)
                _plotView++;
            UpdatePlotView();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _subdiv = !_subdiv;
            UpdatePlotView();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FreeLook.m_XAxis.m_InputAxisName = "Mouse X";
            FreeLook.m_YAxis.m_InputAxisName = "Mouse Y";

            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            FreeLook.m_XAxis.m_InputAxisName = "";
            FreeLook.m_YAxis.m_InputAxisName = "";

            FreeLook.m_XAxis.m_InputAxisValue = 0f;
            FreeLook.m_YAxis.m_InputAxisValue = 0f;
    
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            float mwheel = -1 * Input.mouseScrollDelta.y;
            CinemachineFreeLook.Orbit[] orbits = FreeLook.m_Orbits;
            if (orbits[0].m_Radius + mwheel > 3 && orbits[0].m_Radius + mwheel < 70)
            {
                for (int i = 0; i < orbits.Length - 1; i++)
                {
                    orbits[i].m_Height += mwheel;
                    orbits[i].m_Radius += mwheel;
                }
            }
        }
    }
    
    private void UpdatePlotView()
    {
        _centeredPlot = DataHandler.GetPlot(_territory, _plotView, _subdiv);
        Debug.LogFormat("Plot {0} selected at {1} {2} {3}", _centeredPlot.index,
            _centeredPlot.position.x, _centeredPlot.position.y, _centeredPlot.position.z);

        if (_center != null)
            Destroy(_center);
        _center = new GameObject();
        _center.transform.position = _centeredPlot.position;
        FreeLook.LookAt = _center.transform;
        FreeLook.Follow = _center.transform;
        
        transform.position = _centeredPlot.position + new Vector3(0, 50, 0);
        transform.rotation = Quaternion.Euler(_defaultLook);
    }
}