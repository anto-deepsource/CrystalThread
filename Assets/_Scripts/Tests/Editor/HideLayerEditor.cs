using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HideLayerTest))]
public class HideLayerEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        var thisObject = (HideLayerTest)target;
        int layerNumber = thisObject.gameObject.layer;
        LayerMask layerNumberBinary = 1 << layerNumber; // This turns the layer number into the right binary number

        if ( (Tools.visibleLayers & layerNumberBinary) == 0 ) {
            if (GUILayout.Button("Show Layer")) {
                Tools.visibleLayers = Tools.visibleLayers | layerNumberBinary; // This lets us set which layers are visible in the scene view.
                SceneView.RepaintAll(); // We need to repaint the scene
            }
        } else {
            if (GUILayout.Button("Hide Layer")) {
                LayerMask flippedVisibleLayers = ~Tools.visibleLayers;
                Tools.visibleLayers = ~(flippedVisibleLayers | layerNumberBinary); // This lets us set which layers are visible in the scene view.
                SceneView.RepaintAll(); // We need to repaint the scene
            }
        }

        

        
    }
}
