using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions {

    public static void EnableChildren(this Transform transform) {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(true);
        }
    }

    public static void DisableChildren(this Transform transform) {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(false);
        }
    }
}
