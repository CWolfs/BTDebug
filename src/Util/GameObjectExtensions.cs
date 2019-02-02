using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BTDebug;

public static class GameObjectExtensions {
  public static List<GameObject> FindAllContains(this GameObject go, string name) {
    List<GameObject> gameObjects = new List<GameObject>();

    foreach (Transform t in go.transform) {
      if (t.name.Contains(name)) {
        gameObjects.Add(t.gameObject);
      }
    }

    return gameObjects;
  }

  public static List<GameObject> FindAllContainsRecursive(this GameObject go, params string[] names) {
    List<GameObject> gameObjects = new List<GameObject>();

    foreach (Transform t in go.transform) {
      foreach (string checkName in names) {
        if (t.name.Contains(checkName)) {
          gameObjects.Add(t.gameObject);
        }
      }

      gameObjects.AddRange(t.gameObject.FindAllContainsRecursive(names));
    }

    return gameObjects;
  }
}