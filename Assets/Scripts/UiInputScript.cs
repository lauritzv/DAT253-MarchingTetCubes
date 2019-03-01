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
        public VoxelScript VoxCreation;
        public InputField widthField;
        public InputField heigthField;
        public InputField lengthField;
        public Slider scaleSlider;
        public Slider isoSlider;
        public Dropdown mObject;

        public float defaultSphereIso = 0f;
        public float defaultFractalIso = 0f;
        public float defaultDicomIso = 1.1f;

        private List<float> defaultIsos = new List<float>();

        // Start is called before the first frame update
        void Start()
        {
            if (VoxCreation == null)
            {
                VoxCreation = GetComponent<VoxelScript>();
            }
            defaultIsos.Add(defaultSphereIso);
            defaultIsos.Add(defaultFractalIso);
            defaultIsos.Add(defaultDicomIso);
        }

        public void UpdatePushed()
        {
            VoxCreation.UpdateButtonPushed();
        }

        public void WidthChanged(string text)
        {
            VoxCreation.Width = Mathf.Clamp(int.Parse(text), 3, 128);
            widthField.text = VoxCreation.Width.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Width);
        }

        public void HeigthChanged(string text)
        {
            VoxCreation.Height = Mathf.Clamp(int.Parse(text), 3, 128);
            heigthField.text = VoxCreation.Height.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Height);
        }

        public void LengthChanged(string text)
        {
            VoxCreation.Length = Mathf.Clamp(int.Parse(text), 3, 128);
            lengthField.text = VoxCreation.Length.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Length);
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
            //VoxCreation.NewVoxelsNeeded = true;
            //print("radius changed to: " + VoxCreation.Radius);
        }

        public void ResetButtonPushed()
        {
            scaleSlider.value = 1f;
            isoSlider.value = defaultIsos[(int)VoxCreation.MObject];
            VoxCreation.Iso = defaultIsos[(int) VoxCreation.MObject];
            print("MObject intvalue:" + defaultIsos[(int)VoxCreation.MObject]);
            widthField.text = "16";
            VoxCreation.Width = 16;
            heigthField.text = "16";
            VoxCreation.Height = 16;
            lengthField.text = "16";
            VoxCreation.Length = 16;
            VoxCreation.NewVoxelsNeeded = true;
            UpdatePushed();
        }

        public void MObjectDropdownChanged(int value)
        {
            switch (value)
            {
                case 0:
                    VoxCreation.MObject = MARCHING_OBJECT.Sphere;
                    break;
                case 1:
                    VoxCreation.MObject = MARCHING_OBJECT.Fractal;
                    break;
                case 2:
                    VoxCreation.MObject = MARCHING_OBJECT.DicomScan;
                    break;
            }
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
