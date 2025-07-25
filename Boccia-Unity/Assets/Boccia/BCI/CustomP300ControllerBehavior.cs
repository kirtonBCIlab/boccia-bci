using BCIEssentials.StimulusObjects;
using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using Random = System.Random;
using System.Collections.Generic;

namespace BCIEssentials.ControllerBehaviors
{
    using static ContextAwareUtilities;

    public class CustomP300ControllerBehavior : P300ControllerBehavior
    {
        [StartFoldoutGroup("Training Properties")]
        [Tooltip("The time between the end of a sequence and making a selection [sec]")]
        public float trainBufferTime = 0f;

        // Override the user training routine to add buffer time
        protected override IEnumerator RunUserTrainingRoutine()
        {
            // Call the base implementation which handles LSL blocking
            yield return base.RunUserTrainingRoutine();

            // Add the custom buffer time after the base training is complete
            yield return new WaitForSecondsRealtime(trainBufferTime);
        }

        // Provide the camera-visible SPO collection override used by base class
        public new List<SPO> GetCameraVisibleSPOs()
        {
            Camera mainCamera = Camera.main;
            List<SPO> visibleSPOs = new();

            foreach (SPO spo in _selectableSPOs)
            {
                bool isVisible = false;

                // Check UI elements
                if (spo.TryGetComponent(out CanvasRenderer canvasRenderer))
                {
                    // For UI elements, check if they're in the camera's field of view
                    RectTransform rectTransform = spo.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3[] corners = new Vector3[4];
                        rectTransform.GetWorldCorners(corners);
                        isVisible = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), 
                            new Bounds(rectTransform.position, rectTransform.rect.size));
                    }
                }
                // Check 3D renderers
                else if (spo.TryGetComponent(out Renderer renderer))
                {
                    // For 3D objects, check if renderer bounds are visible to the camera
                    isVisible = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), 
                        renderer.bounds);
                }

                if (isVisible)
                {
                    visibleSPOs.Add(spo);
                }
            }

            return visibleSPOs;
        }
    }
}
