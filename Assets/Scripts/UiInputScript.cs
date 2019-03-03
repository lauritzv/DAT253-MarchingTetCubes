﻿using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubesProject;
using UnityEngine;
using UnityEngine.UI;

namespace MarchingCubesProject
{
    public class UiInputScript : MonoBehaviour
    {
        private VoxelScript VoxCreation;
        [SerializeField] private InputField widthField;
        [SerializeField] private InputField heigthField;
        [SerializeField] private InputField lengthField;
        [SerializeField] private InputField filenameField;
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Slider isoSlider;
        [SerializeField] private Dropdown mObject;
        [SerializeField] private Toggle autoupdToggle;

        [SerializeField] private float defaultSphereIso = 0f;
        [SerializeField] private float defaultFractalIso = 0f;
        [SerializeField] private float defaultDicomIso = 0f;
        [SerializeField] private Vector3 defaultGridSize = new Vector3(32f,32f,32f);

        private List<float> defaultIsos = new List<float>();
        private bool _autoUpdate;
        private MeshScript _mscript;

        void Start()
        {

            if (VoxCreation == null)
                VoxCreation = GetComponent<VoxelScript>();

            if (_mscript == null)
                _mscript = GetComponent<MeshScript>();

            defaultIsos.Add(defaultSphereIso);
            defaultIsos.Add(defaultFractalIso);
            defaultIsos.Add(defaultDicomIso);
            _autoUpdate = autoupdToggle.isOn;

            ResetToDefaultValues();

            if (_autoUpdate)
                UpdatePushed();
        }
        private void ResetToDefaultValues()
        {
            scaleSlider.value = 1f;
            isoSlider.value = defaultIsos[(int)VoxCreation.MObject];
            VoxCreation.Iso = defaultIsos[(int)VoxCreation.MObject];
            print("MObject intvalue:" + defaultIsos[(int)VoxCreation.MObject]);
            widthField.text = defaultGridSize.x.ToString();
            VoxCreation.Width = Mathf.RoundToInt(defaultGridSize.x);
            heigthField.text = defaultGridSize.y.ToString();
            VoxCreation.Height = Mathf.RoundToInt(defaultGridSize.y);
            lengthField.text = defaultGridSize.z.ToString();
            VoxCreation.Length = Mathf.RoundToInt(defaultGridSize.z);
            VoxCreation.NewVoxelsNeeded = true;
        }

        public void UpdatePushed()
        {
            VoxCreation.UpdateButtonPushed();
            ScaleSliderChanged(scaleSlider.value);
        }

        public void WidthChanged(string text)
        {
            VoxCreation.Width = Mathf.Clamp(int.Parse(text), 3, 354);
            widthField.text = VoxCreation.Width.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Width);
            if (_autoUpdate)
                UpdatePushed();
        }

        public void HeigthChanged(string text)
        {
            VoxCreation.Height = Mathf.Clamp(int.Parse(text), 3, 354);
            heigthField.text = VoxCreation.Height.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            if (_autoUpdate)
                UpdatePushed();
        }

        public void LengthChanged(string text)
        {
            VoxCreation.Length = Mathf.Clamp(int.Parse(text), 3, 354);
            lengthField.text = VoxCreation.Length.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            if (_autoUpdate)
                UpdatePushed();
        }

        public void IsoSliderChanged(float value)
        {
            VoxCreation.Iso = value;
            print("iso changed to: " + VoxCreation.Iso);
            if (_autoUpdate)
                UpdatePushed();
        }

        public void ScaleSliderChanged(float value)
        {
            int maxDim = Mathf.Max(VoxCreation.Width, VoxCreation.Height, VoxCreation.Length);
            transform.localScale = new Vector3(
                maxDim * value / VoxCreation.Width,
                maxDim * value / VoxCreation.Height,
                maxDim * value / VoxCreation.Length);
        }

        public void ResetButtonPushed()
        {
            ResetToDefaultValues();
            if (_autoUpdate)
                UpdatePushed();
        }

        public void MObjectDropdownChanged(int value)
        {
            VoxCreation.MObject = (MARCHING_OBJECT) value; // 0: Sphere, 1: Fractal, 2: DicomScan
            isoSlider.value = defaultIsos[value];
            VoxCreation.Iso = defaultIsos[value];

            VoxCreation.NewVoxelsNeeded = true;
            UpdatePushed();
        }

        public void TetrasSelected(bool on)
        {
            VoxCreation.Mode = @on ? MARCHING_MODE.Tetrahedron : MARCHING_MODE.Cubes;
            UpdatePushed();
        }
        public void ToggleAutoUpdate(bool on)
        {
            _autoUpdate = @on ? true : false;
            if (on) UpdatePushed();
        }

        public void WriteObjButtonPushed()
        {
            string filename = filenameField.text;
            string savePath = Application.dataPath + @"/../save/";

            if (FilenameScript.IsValidFilename(filename) && !FilenameScript.FilenameExists(savePath, filename))
                _mscript.MeshToFile(savePath, filename);

            else print("Filename is invalid or already taken.");
        }
    }
}
