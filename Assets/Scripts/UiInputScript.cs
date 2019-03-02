using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubesProject;
using UnityEngine;
using UnityEngine.UI;

namespace MarchingCubesProject
{
    public class UiInputScript : MonoBehaviour
    {
        [SerializeField] private VoxelScript VoxCreation;
        [SerializeField] private InputField widthField;
        [SerializeField] private InputField heigthField;
        [SerializeField] private InputField lengthField;
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Slider isoSlider;
        [SerializeField] private Dropdown mObject;

        [SerializeField] private float defaultSphereIso = 0f;
        [SerializeField] private float defaultFractalIso = 0f;
        [SerializeField] private float defaultDicomIso = 0f;
        [SerializeField] private Vector3 defaultGridSize = new Vector3(32f,32f,32f);

        private List<float> defaultIsos = new List<float>();

        void Start()
        {
            if (VoxCreation == null)
            {
                VoxCreation = GetComponent<VoxelScript>();
            }
            defaultIsos.Add(defaultSphereIso);
            defaultIsos.Add(defaultFractalIso);
            defaultIsos.Add(defaultDicomIso);

            ResetToDefaultValues();
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
        }

        public void WidthChanged(string text)
        {
            VoxCreation.Width = Mathf.Clamp(int.Parse(text), 3, 354);
            widthField.text = VoxCreation.Width.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Width);
        }

        public void HeigthChanged(string text)
        {
            VoxCreation.Height = Mathf.Clamp(int.Parse(text), 3, 354);
            heigthField.text = VoxCreation.Height.ToString();
            VoxCreation.NewVoxelsNeeded = true;
        }

        public void LengthChanged(string text)
        {
            VoxCreation.Length = Mathf.Clamp(int.Parse(text), 3, 354);
            lengthField.text = VoxCreation.Length.ToString();
            VoxCreation.NewVoxelsNeeded = true;
        }

        public void IsoSliderChanged(float value)
        {
            VoxCreation.Iso = value;
            print("iso changed to: " + VoxCreation.Iso);
            UpdatePushed();
        }

        public void ScaleSliderChanged(float value)
        {
            transform.localScale = new Vector3(value, value, value);
        }

        public void ResetButtonPushed()
        {
            ResetToDefaultValues();
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
    }
}
