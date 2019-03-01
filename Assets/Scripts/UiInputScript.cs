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
        public Slider radiusSlider;
        public Slider isoSlider;
        public Dropdown mObject;


        // Start is called before the first frame update
        void Start()
        {
            if (VoxCreation == null)
            {
                VoxCreation = GetComponent<VoxelScript>();
            }
        }

        public void UpdatePushed()
        {
            VoxCreation.UpdateButtonPushed();
        }

        public void WidthChanged()
        {
            VoxCreation.Width = Mathf.Clamp(int.Parse(widthField.text), 3, 64);
            widthField.text = VoxCreation.Width.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Width);
        }

        public void HeigthChanged()
        {
            VoxCreation.Height = Mathf.Clamp(int.Parse(heigthField.text), 3, 64);
            heigthField.text = VoxCreation.Height.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Height);
        }

        public void LengthChanged()
        {
            VoxCreation.Length = Mathf.Clamp(int.Parse(lengthField.text), 3, 64);
            lengthField.text = VoxCreation.Length.ToString();
            VoxCreation.NewVoxelsNeeded = true;
            print("width changed to: " + VoxCreation.Length);
        }

        public void IsoSliderChanged()
        {
            VoxCreation.Iso = isoSlider.value;
            print("iso changed to: " + VoxCreation.Iso);
            UpdatePushed();
        }

        public void RadiusSliderChanged()
        {
            VoxCreation.Radius = radiusSlider.value;
            VoxCreation.NewVoxelsNeeded = true;
            print("radius changed to: " + VoxCreation.Radius);
        }

        public void ResetButtonPushed()
        {
            radiusSlider.value = 8f;
            isoSlider.value = 0f;
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

            VoxCreation.NewVoxelsNeeded = true;
            UpdatePushed();
        }

        public void TetrasSelected(bool on)
        {
            if (on)
            {
                VoxCreation.Mode = MARCHING_MODE.Tetrahedron;
            }
            else VoxCreation.Mode = MARCHING_MODE.Cubes;
            UpdatePushed();
        }
    }
}
