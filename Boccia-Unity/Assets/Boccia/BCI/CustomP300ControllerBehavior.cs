using BCIEssentials.Controllers;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    public class CustomP300ControllerBehavior : P300ControllerBehavior
    {
        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.GraphBP)
        {
            switch (populationMethod)
            {
                case SpoPopulationMethod.Predefined:
                    // Keep current list contents.
                    break;
                case SpoPopulationMethod.Children:
                    Debug.LogWarning("Populating by children is not yet implemented");
                    break;
                default:
                case SpoPopulationMethod.Tag:
                    _objectIDtoSPODict.Clear();
                    base.PopulateObjectList(populationMethod);
                    break;
                case SpoPopulationMethod.GraphBP:
                    _objectIDtoSPODict.Clear();
                    base.PopulateObjectList(populationMethod);

                    // Context-aware graph partitioning needs at least 2 nodes.
                    if (_selectableSPOs.Count < 2)
                    {
                        Debug.LogWarning($"[PopulateObjectList] GraphBP mode: Only {_selectableSPOs.Count} camera-visible SPOs. " +
                            "ContextAwareMultiFlash requires at least 2 visible objects. Please ensure both target and non-target objects are visible to the camera.");
                    }
                    break;
            }
        }

        public override void SelectSPO(int objectID, bool stopStimulusRun = false)
        {
            int ResolveIndex(int prediction) => prediction;

            if (_selectableSPOs.Count == 0)
            {
                Debug.LogWarning("[SelectSPO] No objects to select. Total available: 0");
                return;
            }

            int index = ResolveIndex(objectID);
            if (index < 0 || index >= _selectableSPOs.Count || _selectableSPOs[index] == null)
            {
                Debug.LogWarning($"[SelectSPO] Prediction {objectID} invalid for current pool. Repopulating and retrying...");
                PopulateObjectList(myPopMethod);

                index = ResolveIndex(objectID);
                if (index < 0 || index >= _selectableSPOs.Count || _selectableSPOs[index] == null)
                {
                    Debug.LogError($"[SelectSPO] Unable to resolve prediction {objectID} to a live SPO by index. Available count: {_selectableSPOs.Count}");
                    return;
                }
            }

            var spo = _selectableSPOs[index];
            Debug.Log($"[SelectSPO] Selected SPO '{spo.gameObject.name}' by pool index {index}.");
            spo.Select();
            LastSelectedSPO = spo;

            if (stopStimulusRun)
            {
                StopStimulusRun();
            }
        }
    }
}
