using UnityEngine;
 
public static class RectExtensions {
  public static bool Intersects(this Rect r1, Rect r2, out Rect area) {
    area = new Rect();

    if (r2.Overlaps(r1)) {
      float x1 = Mathf.Min(r1.xMax, r2.xMax);
      float x2 = Mathf.Max(r1.xMin, r2.xMin);
      float y1 = Mathf.Min(r1.yMax, r2.yMax);
      float y2 = Mathf.Max(r1.yMin, r2.yMin);
      area.x = Mathf.Min(x1, x2);
      area.y = Mathf.Min(y1, y2);
      area.width = Mathf.Max(0.0f, x1 - x2);
      area.height = Mathf.Max(0.0f, y1 - y2);
      
      return true;
    }

    return false;
  }

  public static Rect GenerateUsableBoundary(this Rect boundaryRec) {
    float mapBorderSize = 50f;
    float mapSize = 2048f;
    Rect edgeOfMapRec = new Rect(0, 0, mapSize - (mapBorderSize * 2f), mapSize - (mapBorderSize * 2f));
    Rect testBounadaryRec = new Rect(boundaryRec.x + 974f, boundaryRec.y + 974f, boundaryRec.width, boundaryRec.height);
    Rect boundaryIntersect;
    testBounadaryRec.Intersects(edgeOfMapRec, out boundaryIntersect);
    return boundaryIntersect;
  }
}